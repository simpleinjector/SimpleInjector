// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;

    /// <summary>TODO</summary>
    public class ImplementationRegistrationResult : IImplementationRegistrationResult
    {
        private readonly Container container;

        internal ImplementationRegistrationResult(
            Container container, Type serviceType, Type implementationType)
        {
            this.container = container;
            this.ServiceType = serviceType;
            this.ImplementationType = implementationType;
        }

        /// <inheritdoc />
        Container IRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ServiceType { get; }

        /// <inheritdoc />
        public Type ImplementationType { get; }
    }
}