// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
#if NET45
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;

    internal static partial class CompilationHelpers
    {
        // The 5000th delegate and up will not use dynamic assembly compilation to prevent memory leaks when
        // the user accidentally keeps creating new InstanceProducer instances. This means there can be a
        // multitude of registrations, since not all registrations will trigger delegate compilation.
        private const long DynamicAssemblyCompilationThreshold = 5000;

        private static long dynamicClassCounter;

        private interface IDelegateBuilder
        {
            Delegate BuildDelegate();
        }

        internal static Delegate CompileLambdaInDynamicAssembly(LambdaExpression lambda, string typeName,
            string methodName)
        {
            TypeBuilder typeBuilder = Container.ModuleBuilder.DefineType(typeName, TypeAttributes.Public);

            MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName,
                MethodAttributes.Static | MethodAttributes.Public);

            lambda.CompileToMethod(methodBuilder);

            Type type = typeBuilder.CreateType();

            return Delegate.CreateDelegate(lambda.Type, type.GetMethod(methodName), true);
        }

        internal static bool ExpressionNeedsAccessToInternals(Expression expression)
        {
            // This doesn't find all possible cases, but get's us close enough.
            return InternalsFinderVisitor.ContainsInternals(expression);
        }

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void JitCompileDelegate(Delegate @delegate)
        {
            RuntimeHelpers.PrepareDelegate(@delegate);
        }

        private static Delegate CompileInDynamicAssembly(Type resultType, Expression expression)
        {
            ConstantExpression[] constantExpressions = GetConstants(expression).Distinct().ToArray();

            if (constantExpressions.Any())
            {
                return CompileInDynamicAssemblyAsClosure(resultType, expression, constantExpressions);
            }
            else
            {
                return CompileInDynamicAssemblyAsStatic(resultType, expression);
            }
        }

        private static Delegate CompileInDynamicAssemblyAsClosure(Type resultType, Expression expression,
            ConstantExpression[] constantExpressions)
        {
            // ConstantExpressions can't be compiled to a delegate using a MethodBuilder. We will have
            // to replace them to something that can be compiled: an object[] with constants.
            var constantsParameter = Expression.Parameter(typeof(object[]), "constants");

            var replacedExpression =
                ReplaceConstantsWithArrayLookup(expression, constantExpressions, constantsParameter);

            var lambda = Expression.Lambda(
                typeof(Func<,>).MakeGenericType(typeof(object[]), resultType),
                replacedExpression,
                constantsParameter);

            Delegate create = CompileDelegateInDynamicAssembly(lambda);

            // Test the creation. Since we're using a dynamically created assembly, we can't create every
            // delegate we can create using expression.Compile(), so we need to test this.
            JitCompileDelegate(create);

            object[] constants = constantExpressions.Select(constant => constant.Value).ToArray();

            var builder = (IDelegateBuilder)Activator.CreateInstance(
                typeof(ConstantsClosure<>).MakeGenericType(resultType),
                new object[] { create, constants });

            return builder.BuildDelegate();
        }

        private static Delegate CompileInDynamicAssemblyAsStatic(Type resultType, Expression expression)
        {
            LambdaExpression lambda = Expression.Lambda(
                typeof(Func<>).MakeGenericType(resultType),
                expression,
                Helpers.Array<ParameterExpression>.Empty);

            return CompileDelegateInDynamicAssembly(lambda);
        }

        private static Expression ReplaceConstantsWithArrayLookup(Expression expression,
            ConstantExpression[] constants, ParameterExpression constantsParameter)
        {
            return ConstantArrayIndexizerVisitor.ReplaceConstantsWithArrayIndexes(expression,
                constants, constantsParameter);
        }

        private static Delegate CompileDelegateInDynamicAssembly(LambdaExpression lambda) =>
            CompileLambdaInDynamicAssembly(lambda,
                "DynamicInstanceProducer" + GetNextDynamicClassId(), "GetInstance");

        private static List<ConstantExpression> GetConstants(Expression expression) =>
            ConstantFinderVisitor.FindConstants(expression);

        private static long GetNextDynamicClassId() => Interlocked.Increment(ref dynamicClassCounter);

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Not all delegates can be JITted. We fall back to the slower expression.Compile " +
                            "in that case.")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static partial void TryCompileInDynamicAssembly(Type resultType, Expression expression,
            ref Delegate? compiledLambda)
        {
            // HACK: Prevent "JIT Compiler encountered an internal limitation" exception while running in
            // the debugger with VS2013 (See work item 20904).
            if (Debugger.IsAttached)
            {
                return;
            }

            if (Interlocked.Read(ref dynamicClassCounter) >= DynamicAssemblyCompilationThreshold)
            {
                // Stop doing dynamic assembly compilation.
                return;
            }

            compiledLambda = null;

            if (!ExpressionNeedsAccessToInternals(expression))
            {
                try
                {
                    var @delegate = CompileInDynamicAssembly(resultType, expression);

                    // Test the creation. Since we're using a dynamically created assembly, we can't create
                    // every delegate we can create using expression.Compile(), so we need to test this.
                    JitCompileDelegate(@delegate);

                    compiledLambda = @delegate;
                }
                catch
                {
                }
            }
        }

        public class ConstantsClosure<TResult> : IDelegateBuilder
        {
            private readonly Func<object[], TResult> create;
            private readonly object[] constants;

            public ConstantsClosure(Func<object[], TResult> create, object[] constants)
            {
                this.create = create;
                this.constants = constants;
            }

            public TResult GetInstance() => this.create(this.constants);

            public Delegate BuildDelegate() => new Func<TResult>(this.GetInstance);
        }
    }
#endif
}