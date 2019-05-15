// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

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

                this.MayAccessExpression(IsPublic(property.DeclaringType)
                    && (canDoPublicAssign || canDoPublicRead));
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