// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;
    using SimpleInjector.Advanced;

    /// <summary>TODO</summary>
    public class ConditionalRegistrationRegistrationResult : ApiObject, IConditionalRegistrationResult
    {
        private readonly Container container;

        internal ConditionalRegistrationRegistrationResult(
            Container container,
            Type serviceType,
            Registration registration,
            Predicate<PredicateContext> predicate)
        {
            this.container = container;
            this.ServiceType = serviceType;
            this.Registration = registration;
            this.Predicate = predicate;
        }

        /// <inheritdoc />
        Container IRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ServiceType { get; }

        /// <inheritdoc />
        public Predicate<PredicateContext> Predicate { get; }

        /// <summary>TODO</summary>
        public Registration Registration { get; }
    }
}