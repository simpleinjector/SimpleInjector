// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System.Linq.Expressions;
    using System.Reflection;

    // An Overridden parameter prevents the Registration class from calling back into the container to build
    // an expression for the given constructor parameter. Instead the Registration will
    internal struct OverriddenParameter
    {
        // The parameter to ignore.
        internal readonly ParameterInfo Parameter;

        // The place holder to temporarily inject into the constructor instead so the complete expression can
        // go through the interception pipeline.
        internal readonly ConstantExpression PlaceHolder;

        // The final expression that will replace the place holder after the expression went through the
        // interception pipeline.
        internal readonly Expression Expression;

        // The producer of the dependency. This is used to build up the relationships collection and is used
        // for diagnostics.
        internal readonly InstanceProducer Producer;

        internal OverriddenParameter(ParameterInfo parameter, Expression expression, InstanceProducer producer)
        {
            this.Parameter = parameter;
            this.Expression = expression;
            this.Producer = producer;

            // A placeholder is a fake expression that we inject into the NewExpression. After the
            // NewExpression is created, it is ran through the ExpressionBuilding interception. By using
            // placeholders instead of the real overridden expressions we prevent those expressions from
            // being processed twice by the ExpressionBuilding event (since we expect the supplied expressions
            // to already be processed). After the event has ran we replace the placeholders with the real
            // expressions again (using an ExpressionVisitor).
            this.PlaceHolder = System.Linq.Expressions.Expression.Constant(null, parameter.ParameterType);
        }
    }
}