// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;
    using SimpleInjector.Advanced;

    /// <summary>TODO</summary>
    public class ConditionalRegistrationResult<TService> : ApiObject, IConditionalRegistrationResult<TService>
    {
        private readonly Container container;

        internal ConditionalRegistrationResult(
            Container container, Predicate<PredicateContext> predicate)
        {
            this.container = container;
            this.Predicate = predicate;
        }

        /// <inheritdoc />
        Container IRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ServiceType => typeof(TService);

        /// <inheritdoc />
        public Predicate<PredicateContext> Predicate { get; }
    }
}