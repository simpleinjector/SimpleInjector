// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;

    /// <summary>TODO</summary>
    public class RegistrationResult<TService> : IRegistrationResult<TService>, IRegistrationRegistrationResult
    {
        private readonly Container container;

        internal RegistrationResult(Container container, Registration registration)
        {
            this.container = container;
            this.Registration = registration;
        }

        /// <inheritdoc />
        Container IRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ServiceType => typeof(TService);

        /// <inheritdoc />
        public Type ImplementationType => this.Registration.ImplementationType;

        /// <summary>TODO</summary>
        public Registration Registration { get; }
    }
}