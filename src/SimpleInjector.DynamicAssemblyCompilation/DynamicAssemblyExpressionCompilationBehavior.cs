// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.DynamicAssemblyCompilation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Defines the container's behavior for compiling expressions into delegates that are placed in a dynamic
    /// assembly (whenever possible).
    /// </summary>
    public sealed class DynamicAssemblyExpressionCompilationBehavior : IExpressionCompilationBehavior
    {
        // The 5000th delegate and up will not use dynamic assembly compilation to prevent memory leaks when
        // the user accidentally keeps creating new InstanceProducer instances. This means there can be a
        // multitude of registrations, since not all registrations will trigger delegate compilation.
        private const long DynamicAssemblyCompilationThreshold = 5000;

        private static readonly ModuleBuilder Builder =
            AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("SimpleInjector.DAC.Compiled"),
                AssemblyBuilderAccess.Run)
                .DefineDynamicModule("SimpleInjector.DAC.CompiledModule");

        private static long dynamicClassCounter;

        private interface IDelegateBuilder
        {
            Delegate BuildDelegate();
        }

        /// <inheritdoc />
        public Delegate Compile(Expression expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            // HACK: Prevent "JIT Compiler encountered an internal limitation" exception while running in
            // the debugger with VS2013 (See work item 20904).
            if (!Debugger.IsAttached)
            {
                // Stop doing dynamic assembly compilation once we reached the threshold.
                if (Interlocked.Read(ref dynamicClassCounter) < DynamicAssemblyCompilationThreshold)
                {
                    // Dynamic assembly compilation doesn't work when compiling internal types.
                    if (!ExpressionNeedsAccessToInternals(expression))
                    {
                        try
                        {
                            var @delegate = CompileInDynamicAssembly(expression);

                            // Test the creation. Since we're using a dynamically created assembly, we can't
                            // create every delegate we can create using expression.Compile(), so we need to
                            // test this.
                            JitCompileDelegate(@delegate);

                            return @delegate;
                        }
                        catch
                        {
                            // When JITting fails, we just fall back to the core behavior.
                        }
                    }
                }
            }

            return this.CompileWithoutDynamicAssembly(expression);
        }

        private Delegate CompileWithoutDynamicAssembly(Expression expression)
        {
            // Turn the expression into a Lambda
            var lambda = expression as LambdaExpression
                ?? Expression.Lambda(typeof(Func<>).MakeGenericType(expression.Type), expression);

            return lambda.Compile();
        }

        private static Delegate CompileLambdaInDynamicAssembly(LambdaExpression lambda)
        {
            const string MethodName = "Lambda";
            string typeName = "DynamicProducer" + GetNextDynamicClassId();

            TypeBuilder typeBuilder;

            lock (Builder)
            {
                typeBuilder = Builder.DefineType(typeName, TypeAttributes.Public);
            }

            MethodBuilder methodBuilder =
                typeBuilder.DefineMethod(MethodName, MethodAttributes.Static | MethodAttributes.Public);

            lambda.CompileToMethod(methodBuilder);

            Type type = typeBuilder.CreateType();

            return Delegate.CreateDelegate(lambda.Type, type.GetMethod(MethodName), true);
        }

        private static bool ExpressionNeedsAccessToInternals(Expression expression)
        {
            // This doesn't find all possible cases, but get's us close enough.
            return InternalsFinderVisitor.ContainsInternals(expression);
        }

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void JitCompileDelegate(Delegate @delegate)
        {
            RuntimeHelpers.PrepareDelegate(@delegate);
        }

        private static Delegate CompileInDynamicAssembly(Expression expression)
        {
            if (expression is LambdaExpression lambda)
            {
                return CompileLambdaInDynamicAssembly(lambda);
            }
            else
            {
                ConstantExpression[] constantExpressions = GetConstants(expression).Distinct().ToArray();

                if (constantExpressions.Length > 0)
                {
                    return CompileInDynamicAssemblyAsClosure(expression, constantExpressions);
                }
                else
                {
                    return CompileInDynamicAssemblyAsStatic(expression);
                }
            }
        }

        private static Delegate CompileInDynamicAssemblyAsClosure(
            Expression expression, ConstantExpression[] constantExpressions)
        {
            // ConstantExpressions can't be compiled to a delegate using a MethodBuilder. We will have
            // to replace them to something that can be compiled: an object[] with constants.
            var constantsParameter = Expression.Parameter(typeof(object[]), "constants");

            var replacedExpression =
                ReplaceConstantsWithArrayLookup(expression, constantExpressions, constantsParameter);

            var lambda = Expression.Lambda(
                typeof(Func<,>).MakeGenericType(typeof(object[]), expression.Type),
                replacedExpression,
                constantsParameter);

            Delegate create = CompileLambdaInDynamicAssembly(lambda);

            // Test the creation. Since we're using a dynamically created assembly, we can't create every
            // delegate we can create using expression.Compile(), so we need to test this.
            JitCompileDelegate(create);

            object[] constants = constantExpressions.Select(constant => constant.Value).ToArray();

            var builder = (IDelegateBuilder)Activator.CreateInstance(
                typeof(ConstantsClosure<>).MakeGenericType(expression.Type),
                new object[] { create, constants });

            return builder.BuildDelegate();
        }

        private static Delegate CompileInDynamicAssemblyAsStatic(Expression expression)
        {
            LambdaExpression lambda = Expression.Lambda(
                typeof(Func<>).MakeGenericType(expression.Type),
                expression,
                new ParameterExpression[0]);

            return CompileLambdaInDynamicAssembly(lambda);
        }

        private static Expression ReplaceConstantsWithArrayLookup(
            Expression expression, ConstantExpression[] constants, ParameterExpression constantsParameter)
        {
            return ConstantArrayIndexizerVisitor.ReplaceConstantsWithArrayIndexes(
                expression, constants, constantsParameter);
        }

        private static List<ConstantExpression> GetConstants(Expression expression) =>
            ConstantFinderVisitor.FindConstants(expression);

        private static long GetNextDynamicClassId() => Interlocked.Increment(ref dynamicClassCounter);

        internal sealed class ConstantsClosure<TResult> : IDelegateBuilder
        {
            private readonly Delegate func;

            public ConstantsClosure(Func<object[], TResult> create, object[] constants) =>
                this.func = new Func<TResult>(() => create(constants));

            public Delegate BuildDelegate() => this.func;
        }
    }
}