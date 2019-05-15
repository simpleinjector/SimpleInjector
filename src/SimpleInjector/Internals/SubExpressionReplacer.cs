// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System.Linq.Expressions;

    // Searches an expression for a specific sub expression and replaces that sub expression with a
    // different supplied expression.
    internal sealed class SubExpressionReplacer : ExpressionVisitor
    {
        private readonly ConstantExpression subExpressionToFind;
        private readonly Expression replacementExpression;

        private SubExpressionReplacer(ConstantExpression subExpressionToFind,
            Expression replacementExpression)
        {
            this.subExpressionToFind = subExpressionToFind;
            this.replacementExpression = replacementExpression;
        }

        internal static Expression Replace(
            Expression expressionToAlter, ConstantExpression nodeToFind, Expression replacementNode)
        {
            var visitor = new SubExpressionReplacer(nodeToFind, replacementNode);

            return visitor.Visit(expressionToAlter);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            return node == this.subExpressionToFind ? this.replacementExpression : base.VisitConstant(node);
        }
    }
}