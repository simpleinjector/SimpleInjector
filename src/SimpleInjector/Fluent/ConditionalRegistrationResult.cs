// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;
    using SimpleInjector.Advanced;

    /// <summary>TODO</summary>
    public class ConditionalRegistrationResult : ApiObject
        , IConditionalRegistrationResult
        , IRegistrationResult
    {
        private readonly Container container;

        internal ConditionalRegistrationResult(
            Container container,
            Type serviceType,
            Type implementationType,
            Predicate<PredicateContext> predicate)
        {
            this.container = container;
            this.ServiceType = serviceType;
            this.Predicate = predicate;
            this.ImplementationType = implementationType;
        }

        /// <inheritdoc />
        Container IRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ServiceType { get; }

        /// <inheritdoc />
        public Predicate<PredicateContext> Predicate { get; }

        /// <summary>TODO</summary>
        public Type ImplementationType { get; }
    }
}