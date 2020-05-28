// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;

    /// <summary>TODO</summary>
    public class RegistrationResult : IRegistrationRegistrationResult
    {
        private readonly Container container;

        internal RegistrationResult(Container container, Type serviceType, Registration registration)
        {
            this.container = container;
            this.ServiceType = serviceType;
            this.Registration = registration;
        }

        /// <inheritdoc />
        Container IRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ServiceType { get; }

        /// <inheritdoc />
        public Type ImplementationType => this.Registration.ImplementationType;

        /// <inheritdoc />
        public Registration Registration { get; }
    }
}