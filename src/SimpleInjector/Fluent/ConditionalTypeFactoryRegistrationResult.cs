// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;
    using SimpleInjector.Advanced;

    /// <summary>TODO</summary>
    public class ConditionalTypeFactoryRegistrationResult : ApiObject, IConditionalRegistrationResult
    {
        private readonly Container container;

        internal ConditionalTypeFactoryRegistrationResult(
            Container container,
            Type serviceType,
            Func<TypeFactoryContext, Type> implementationTypeFactory,
            Lifestyle lifestyle,
            Predicate<PredicateContext> predicate)
        {
            this.container = container;
            this.ServiceType = serviceType;
            this.Predicate = predicate;
            this.Lifestyle = lifestyle;
            this.ImplementationTypeFactory = implementationTypeFactory;
        }

        /// <inheritdoc />
        Container IRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ServiceType { get; }

        /// <inheritdoc />
        public Predicate<PredicateContext> Predicate { get; }

        /// <summary>TODO</summary>
        public Lifestyle Lifestyle { get; }

        /// <summary>TODO</summary>
        public Func<TypeFactoryContext, Type> ImplementationTypeFactory { get; }
    }
}