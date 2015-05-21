#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2014 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Internals
{
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
    using SimpleInjector.Advanced;

    internal static partial class CompilationHelpers
    {
        // The 5000th delegate and up will not use dynamic assembly compilation to prevent memory leaks when
        // the user accidentally keeps creating new InstanceProducer instances. This means there can be a
        // multitude of registrations, since not all registrations will trigger delegate compilation.
        private const long DynamicAssemblyCompilationThreshold = 5000;

        private static long dynamicClassCounter;

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

        private static Func<TResult> CompileInDynamicAssembly<TResult>(Expression expression)
        {
            ConstantExpression[] constantExpressions = GetConstants(expression).Distinct().ToArray();

            if (constantExpressions.Any())
            {
                return CompileInDynamicAssemblyAsClosure<TResult>(expression, constantExpressions);
            }
            else
            {
                return CompileInDynamicAssemblyAsStatic<TResult>(expression);
            }
        }

        private static Func<TResult> CompileInDynamicAssemblyAsClosure<TResult>(Expression expression,
            ConstantExpression[] constantExpressions)
        {
            // ConstantExpressions can't be compiled to a delegate using a MethodBuilder. We will have
            // to replace them to something that can be compiled: an object[] with constants.
            var constantsParameter = Expression.Parameter(typeof(object[]), "constants");

            var replacedExpression =
                ReplaceConstantsWithArrayLookup(expression, constantExpressions, constantsParameter);

            var lambda = Expression.Lambda<Func<object[], TResult>>(replacedExpression, constantsParameter);

            Func<object[], TResult> create = CompileDelegateInDynamicAssembly(lambda);

            // Test the creation. Since we're using a dynamically created assembly, we can't create every
            // delegate we can create using expression.Compile(), so we need to test this.
            JitCompileDelegate(create);

            object[] constants = constantExpressions.Select(constant => constant.Value).ToArray();

            return () => create(constants);
        }

        private static Func<TResult> CompileInDynamicAssemblyAsStatic<TResult>(Expression expression)
        {
            Expression<Func<TResult>> lambda =
                Expression.Lambda<Func<TResult>>(expression, Helpers.Array<ParameterExpression>.Empty);

            return CompileDelegateInDynamicAssembly(lambda);
        }

        private static Expression ReplaceConstantsWithArrayLookup(Expression expression,
            ConstantExpression[] constants, ParameterExpression constantsParameter)
        {
            return ConstantArrayIndexizerVisitor.ReplaceConstantsWithArrayIndexes(expression,
                constants, constantsParameter);
        }

        private static TDelegate CompileDelegateInDynamicAssembly<TDelegate>(Expression<TDelegate> lambda)
        {
            return (TDelegate)(object)CompileLambdaInDynamicAssembly(lambda,
                "DynamicInstanceProducer" + GetNextDynamicClassId(), "GetInstance");
        }

        private static List<ConstantExpression> GetConstants(Expression expression)
        {
            return ConstantFinderVisitor.FindConstants(expression);
        }

        private static long GetNextDynamicClassId()
        {
            return Interlocked.Increment(ref dynamicClassCounter);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Not all delegates can be JITted. We fall back to the slower expression.Compile " +
                            "in that case.")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static partial void TryCompileInDynamicAssembly<TResult>(Expression expression,
            ref Func<TResult> compiledLambda)
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
                    var @delegate = CompileInDynamicAssembly<TResult>(expression);

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
    }
}