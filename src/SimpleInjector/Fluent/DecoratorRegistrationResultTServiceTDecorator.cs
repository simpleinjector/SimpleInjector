// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;
    using SimpleInjector.Advanced;

    /// <summary>TODO</summary>
    public class DecoratorRegistrationResult<TService, TDecorator> : ApiObject
        , IRegistrationResult<TService, TDecorator>
        , IDecoratorRegistrationResult
    {
        private readonly Container container;

        internal DecoratorRegistrationResult(
            Container container, Predicate<DecoratorPredicateContext>? predicate = null)
        {
            this.container = container;
            this.Predicate = predicate;
        }

        /// <inheritdoc />
        Container IRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ServiceType => typeof(TService);

        /// <inheritdoc />
        Type IImplementationRegistrationResult.ImplementationType => typeof(TDecorator);

        /// <inheritdoc />
        public Type DecoratorType => typeof(TDecorator);

        /// <inheritdoc />
        public Predicate<DecoratorPredicateContext>? Predicate { get; }
    }

}