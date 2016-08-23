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
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal sealed class InternalsFinderVisitor : ExpressionVisitor
    {
        private bool partOfAssigmnent;
        private bool needsAccessToInternals;

        private InternalsFinderVisitor()
        {
        }

        public static bool ContainsInternals(Expression node)
        {
            var visitor = new InternalsFinderVisitor();
            visitor.Visit(node);
            return visitor.needsAccessToInternals;
        }

        public override Expression Visit(Expression node) => base.Visit(node);

        internal void MayAccessExpression(bool mayAccess)
        {
            if (!mayAccess)
            {
                this.needsAccessToInternals = true;
            }
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            this.MayAccessExpression(IsPublic(node.Type));

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

        private static bool IsPublic(Type type) => GetTypeAndDeclaringTypes(type).All(IsPublicInternal);

        private static bool IsPublicInternal(Type type) =>
            (type.IsNested ? type.IsNestedPublic() : type.IsPublic())
            && (!type.IsGenericType() || type.GetGenericArguments().All(IsPublic));

        private static IEnumerable<Type> GetTypeAndDeclaringTypes(Type type)
        {
            yield return type;

            while (type.DeclaringType != null)
            {
                yield return type.DeclaringType;

                type = type.DeclaringType;
            }
        }
    }
}