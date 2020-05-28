// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;

    /// <summary>TODO</summary>
    public class InstanceRegistrationResult : IRegistrationResult
    {
        private readonly Container container;

        internal InstanceRegistrationResult(
            Container container, Type serviceType, object instance, Registration registration)
        {
            this.container = container;
            this.ServiceType = serviceType;
            this.Instance = instance;
            this.Registration = registration;
        }

        /// <inheritdoc />
        Container IRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ServiceType { get; }

        /// <summary>TODO</summary>
        public object Instance { get; }

        /// <summary>TODO</summary>
        public Registration Registration { get; }
    }
}