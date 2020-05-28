// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;
    using SimpleInjector.Advanced;

    /// <summary>TODO</summary>
    public class DecoratorTypeFactoryRegistrationResult : ApiObject
        , IRegistrationResult
    {
        private readonly Container container;

        internal DecoratorTypeFactoryRegistrationResult(
            Container container,
            Type serviceType,
            Func<DecoratorPredicateContext, Type> decoratorTypeFactory,
            Lifestyle lifestyle,
            Predicate<DecoratorPredicateContext>? predicate = null)
        {
            this.container = container;
            this.ServiceType = serviceType;
            this.DecoratorTypeFactory = decoratorTypeFactory;
            this.Lifestyle = lifestyle;
            this.Predicate = predicate;
        }

        /// <inheritdoc />
        Container IRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ServiceType { get; }

        /// <summary>TODO</summary>
        public Func<DecoratorPredicateContext, Type> DecoratorTypeFactory { get; }

        /// <summary>TODO</summary>
        public Lifestyle Lifestyle { get; }

        /// <inheritdoc />
        public Predicate<DecoratorPredicateContext>? Predicate { get; }
    }

}