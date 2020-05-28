// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;
    using SimpleInjector.Advanced;

    /// <summary>TODO</summary>
    public class DecoratorRegistrationResult : ApiObject
        , IRegistrationResult
        , IDecoratorRegistrationResult
    {
        private readonly Container container;

        internal DecoratorRegistrationResult(
            Container container,
            Type serviceType,
            Type decoratorType,
            Predicate<DecoratorPredicateContext>? predicate = null)
        {
            this.container = container;
            this.ServiceType = serviceType;
            this.DecoratorType = decoratorType;
            this.Predicate = predicate;
        }

        /// <inheritdoc />
        Container IRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ServiceType { get; }

        /// <inheritdoc />
        public Type DecoratorType { get; }

        /// <inheritdoc />
        public Predicate<DecoratorPredicateContext>? Predicate { get; }
    }

}