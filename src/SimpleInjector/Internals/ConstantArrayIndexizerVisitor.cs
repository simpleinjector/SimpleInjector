// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal sealed class ConstantArrayIndexizerVisitor : ExpressionVisitor
    {
        private readonly List<ConstantExpression> constantExpressions;
        private readonly ParameterExpression constantsParameter;

        private ConstantArrayIndexizerVisitor(ConstantExpression[] constantExpressions,
            ParameterExpression constantsParameter)
        {
            this.constantExpressions = constantExpressions.ToList();
            this.constantsParameter = constantsParameter;
        }

        public static Expression ReplaceConstantsWithArrayIndexes(
            Expression node, ConstantExpression[] constantExpressions, ParameterExpression constantsParameter)
        {
            var visitor = new ConstantArrayIndexizerVisitor(constantExpressions, constantsParameter);

            return visitor.Visit(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            int index = this.constantExpressions.IndexOf(node);

            return index >= 0
                ? this.CreateArrayIndexerExpression(node, index)
                : base.VisitConstant(node);
        }

        private UnaryExpression CreateArrayIndexerExpression(ConstantExpression node, int index) =>
            Expression.Convert(
                Expression.ArrayIndex(
                    this.constantsParameter,
                    Expression.Constant(index, typeof(int))),
                node.Type);
    }
}