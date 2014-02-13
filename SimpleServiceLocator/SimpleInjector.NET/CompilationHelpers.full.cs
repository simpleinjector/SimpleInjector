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

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
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

        // This doesn't find all possible cases, but get's us close enough.
        internal static bool ExpressionNeedsAccessToInternals(Expression expression)
        {
            var visitor = new InternalUseFinderVisitor();
            visitor.Visit(expression);
            return visitor.NeedsAccessToInternals;
        }

        [SecuritySafeCritical]
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
                Expression.Lambda<Func<TResult>>(expression, new ParameterExpression[0]);

            return CompileDelegateInDynamicAssembly(lambda);
        }

        private static Expression ReplaceConstantsWithArrayLookup(Expression expression,
            ConstantExpression[] constants, ParameterExpression constantsParameter)
        {
            var indexizer = new ConstantArrayIndexizerVisitor(constants, constantsParameter);

            return indexizer.Visit(expression);
        }

        private static TDelegate CompileDelegateInDynamicAssembly<TDelegate>(Expression<TDelegate> lambda)
        {
            return (TDelegate)(object)CompileLambdaInDynamicAssembly(lambda,
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

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Not all delegates can be JITted. We fallback to the slower expression.Compile " +
                            "in that case.")]
        static partial void TryCompileInDynamicAssembly<TResult>(Expression expression, 
            ref Func<TResult> compiledLambda)
        {
            compiledLambda = null;

            if (!ExpressionNeedsAccessToInternals(expression))
            {
                try
                {
                    var @delegate = CompileInDynamicAssembly<TResult>(expression);

                    // Test the creation. Since we're using a dynamically created assembly, we can't create every
                    // delegate we can create using expression.Compile(), so we need to test this.
                    JitCompileDelegate(@delegate);

                    compiledLambda = @delegate;
                }
                catch
                {
                }
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

            internal void MayAccessExpression(bool mayAccess)
            {
                if (!mayAccess)
                {
                    this.NeedsAccessToInternals = true;
                }
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                this.MayAccessExpression(node.Type.IsPublic);

                return base.VisitConstant(node);
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
                    bool canDoPublicAssign = this.partOfAssigmnent && property.GetSetMethod() != null;
                    bool canDoPublicRead = !this.partOfAssigmnent && property.GetGetMethod() != null;

                    this.MayAccessExpression(IsPublic(property.DeclaringType) &&
                        (canDoPublicAssign || canDoPublicRead));
                }

                return base.VisitMember(node);
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