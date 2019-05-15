// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Linq.Expressions;

    internal sealed class ExpressionRegistration : Registration
    {
        private readonly Expression expression;

        internal ExpressionRegistration(Expression expression, Container container)
            : this(expression, GetImplementationTypeFor(expression), GetLifestyleFor(expression), container)
        {
        }

        internal ExpressionRegistration(
            Expression expression, Type implementationType, Lifestyle lifestyle, Container container)
            : base(lifestyle, container)
        {
            Requires.IsNotNull(expression, nameof(expression));
            Requires.IsNotNull(implementationType, nameof(implementationType));

            this.expression = expression;
            this.ImplementationType = implementationType;
        }

        public override Type ImplementationType { get; }
        public override Expression BuildExpression() => this.expression;

        private static Lifestyle GetLifestyleFor(Expression expression) =>
            expression is ConstantExpression ? Lifestyle.Singleton :
            expression is NewExpression ? Lifestyle.Transient :
            Lifestyle.Unknown;

        private static Type GetImplementationTypeFor(Expression expression) =>
            expression is NewExpression newExpression
                ? newExpression.Constructor.DeclaringType
                : expression.Type;
    }
}