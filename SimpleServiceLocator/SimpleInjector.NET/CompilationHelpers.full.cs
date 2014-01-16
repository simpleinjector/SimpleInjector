#region Copyright (c) 2013 Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
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

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;

    internal static partial class CompilationHelpers
    {
        private static long dynamicClassCounter;

        internal static Delegate CompileLambdaInDynamicAssembly(Container container, LambdaExpression lambda,
            string typeName, string methodName)
        {
            System.Reflection.Emit.TypeBuilder typeBuilder =
                container.ModuleBuilder.DefineType(typeName, TypeAttributes.Public);

            System.Reflection.Emit.MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName,
                MethodAttributes.Static | MethodAttributes.Public);

            lambda.CompileToMethod(methodBuilder);

            Type type = typeBuilder.CreateType();

            return Delegate.CreateDelegate(lambda.Type, type.GetMethod(methodName), true);
        }

        // This doesn't find all possible cases, but get's us close enough.
        internal static bool ExpressionNeedsAccessToInternals(Expression expression)
        {
            var visitor = new InternalUseFinderVisitor();
            visitor.Visit(expression);
            return visitor.NeedsAccessToInternals;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "We can skip the exception, because we call the fallbackDelegate.")]
        private static Func<object> CompileAndExecuteInDynamicAssemblyWithFallback(Container container,
            Expression expression, out object createdInstance)
        {
            try
            {
                var @delegate = CompileInDynamicAssembly(container, expression);

                // Test the creation. Since we're using a dynamically created assembly, we can't create every
                // delegate we can create using expression.Compile(), so we need to test this. We need to 
                // store the created instance because we are not allowed to ditch that instance.
                createdInstance = @delegate();

                return @delegate;
            }
            catch
            {
                // The fallback. Here we don't execute the lambda, because this would mean that when the
                // execution fails the lambda is not returned and the compiled delegate would never be cached,
                // forcing a compilation hit on each call.
                createdInstance = null;
                return CompileLambda(expression);
            }
        }

        private static Func<object> CompileInDynamicAssembly(Container container, Expression expression)
        {
            ConstantExpression[] constantExpressions = GetConstants(expression).Distinct().ToArray();

            if (constantExpressions.Any())
            {
                return CompileInDynamicAssemblyAsClosure(container, expression, constantExpressions);
            }
            else
            {
                return CompileInDynamicAssemblyAsStatic(container, expression);
            }
        }

        private static Func<object> CompileInDynamicAssemblyAsClosure(Container container,
            Expression originalExpression, ConstantExpression[] constantExpressions)
        {
            // ConstantExpressions can't be compiled to a delegate using a MethodBuilder. We will have
            // to replace them to something that can be compiled: an object[] with constants.
            var constantsParameter = Expression.Parameter(typeof(object[]), "constants");

            var replacedExpression =
                ReplaceConstantsWithArrayLookup(originalExpression, constantExpressions, constantsParameter);

            var lambda = Expression.Lambda<Func<object[], object>>(replacedExpression, constantsParameter);

            Func<object[], object> create = CompileDelegateInDynamicAssembly(container, lambda);

            object[] contants = constantExpressions.Select(c => c.Value).ToArray();

            return () => create(contants);
        }

        private static Func<object> CompileInDynamicAssemblyAsStatic(Container container, Expression expression)
        {
            var lambda = Expression.Lambda<Func<object>>(expression, new ParameterExpression[0]);

            return CompileDelegateInDynamicAssembly(container, lambda);
        }

        private static Expression ReplaceConstantsWithArrayLookup(Expression expression,
            ConstantExpression[] constants, ParameterExpression constantsParameter)
        {
            var indexizer = new ConstantArrayIndexizerVisitor(constants, constantsParameter);

            return indexizer.Visit(expression);
        }

        private static TDelegate CompileDelegateInDynamicAssembly<TDelegate>(Container container,
            Expression<TDelegate> lambda)
        {
            return (TDelegate)(object)CompileLambdaInDynamicAssembly(container, lambda,
                "DynamicInstanceProducer" + GetNextDynamicClassId(), "GetInstance");
        }

        private static List<ConstantExpression> GetConstants(Expression expression)
        {
            var constantFinder = new ConstantFinderVisitor();

            constantFinder.Visit(expression);

            return constantFinder.Constants;
        }

        private static long GetNextDynamicClassId()
        {
            return Interlocked.Increment(ref dynamicClassCounter);
        }

        static partial void TryCompileAndExecuteInDynamicAssembly(Container container,
            Expression expression, ref Func<object> compiledLambda, ref object createdInstance)
        {
            createdInstance = null;
            compiledLambda = null;

            // In the common case, the developer will/should only create a single container during the 
            // lifetime of the application (this is the recommended approach). In this case, we can optimize
            // the perf by compiling delegates in an dynamic assembly. We can't do this when the developer 
            // creates many containers, because this will create a memory leak (dynamic assemblies are never 
            // unloaded). We might however relax this constraint and optimize the first N container instances.
            // (where N is configurable)
            if (container.Options.EnableDynamicAssemblyCompilation &&
                !ExpressionNeedsAccessToInternals(expression))
            {
                compiledLambda =
                    CompileAndExecuteInDynamicAssemblyWithFallback(container, expression, out createdInstance);
            }
        }

        private sealed class InternalUseFinderVisitor : ExpressionVisitor
        {
            private bool partOfAssigmnent;

            public bool NeedsAccessToInternals { get; private set; }

            public override Expression Visit(Expression node)
            {
                return base.Visit(node);
            }

            protected override Expression VisitNew(NewExpression node)
            {
                this.MayAccessExpression(node.Constructor.IsPublic && IsPublic(node.Constructor.DeclaringType));

                return base.VisitNew(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                this.MayAccessExpression(node.Method.IsPublic && IsPublic(node.Method.DeclaringType));

                return base.VisitMethodCall(node);
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (node.NodeType == ExpressionType.Assign)
                {
                    bool oldValue = this.partOfAssigmnent;

                    try
                    {
                        this.partOfAssigmnent = true;

                        return base.VisitBinary(node);
                    }
                    finally
                    {
                        this.partOfAssigmnent = oldValue;
                    }
                }

                return base.VisitBinary(node);
            }

            protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
            {
                return base.VisitMemberAssignment(node);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var property = node.Member as PropertyInfo;

                if (node.NodeType == ExpressionType.MemberAccess && property != null)
                {
                    bool canDoPublicAssign = partOfAssigmnent && property.GetSetMethod() != null;
                    bool canDoPublicRead = !partOfAssigmnent && property.GetGetMethod() != null;

                    this.MayAccessExpression(IsPublic(property.DeclaringType) &&
                        (canDoPublicAssign || canDoPublicRead));
                }

                return base.VisitMember(node);
            }

            private void MayAccessExpression(bool mayAccess)
            {
                if (!mayAccess)
                {
                    this.NeedsAccessToInternals = true;
                }
            }

            private static bool IsPublic(Type type)
            {
                return type.IsPublic || type.IsNestedPublic;
            }
        }

        private sealed class ConstantFinderVisitor : ExpressionVisitor
        {
            internal readonly List<ConstantExpression> Constants = new List<ConstantExpression>();

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (!node.Type.IsPrimitive)
                {
                    this.Constants.Add(node);
                }

                return base.VisitConstant(node);
            }
        }

        private sealed class ConstantArrayIndexizerVisitor : ExpressionVisitor
        {
            private readonly List<ConstantExpression> constantExpressions;
            private readonly ParameterExpression constantsParameter;

            public ConstantArrayIndexizerVisitor(ConstantExpression[] constantExpressions,
                ParameterExpression constantsParameter)
            {
                this.constantExpressions = constantExpressions.ToList();
                this.constantsParameter = constantsParameter;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                int index = this.constantExpressions.IndexOf(node);

                if (index >= 0)
                {
                    return Expression.Convert(
                        Expression.ArrayIndex(
                            this.constantsParameter,
                            Expression.Constant(index, typeof(int))),
                        node.Type);
                }

                return base.VisitConstant(node);
            }
        }
    }
}