// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Linq.Expressions;

    internal sealed class DefaultExpressionCompilationBehavior : IExpressionCompilationBehavior
    {
        public Delegate Compile(Expression expression)
        {
            Requires.IsNotNull(expression, nameof(expression));

            // Turn the expression into a Lambda
            var lambda = expression as LambdaExpression
                ?? Expression.Lambda(typeof(Func<>).MakeGenericType(expression.Type), expression);

            return lambda.Compile();
        }
    }
}