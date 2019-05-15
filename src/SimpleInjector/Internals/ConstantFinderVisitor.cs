// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal sealed class ConstantFinderVisitor : ExpressionVisitor
    {
        private readonly List<ConstantExpression> constants = new List<ConstantExpression>();

        private ConstantFinderVisitor()
        {
        }

        public static List<ConstantExpression> FindConstants(Expression node)
        {
            var visitor = new ConstantFinderVisitor();
            visitor.Visit(node);
            return visitor.constants;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (!node.Type.IsPrimitive())
            {
                this.constants.Add(node);
            }

            return base.VisitConstant(node);
        }
    }
}