// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System.Linq.Expressions;

    // Searches an expression for a specific sub expression and replaces that sub expression with a
    // different supplied expression.
    internal sealed class SubExpressionReplacer(
        ConstantExpression subExpressionToFind, Expression replacementExpression)
        : ExpressionVisitor
    {
        internal static Expression Replace(
            Expression expressionToAlter, ConstantExpression nodeToFind, Expression replacementNode)
        {
            return new SubExpressionReplacer(nodeToFind, replacementNode).Visit(expressionToAlter);
        }

        protected override Expression VisitConstant(ConstantExpression node) =>
            node == subExpressionToFind ? replacementExpression : base.VisitConstant(node);
    }
}