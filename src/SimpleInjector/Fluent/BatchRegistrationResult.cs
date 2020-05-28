// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>TODO</summary>
    public class BatchRegistrationResult : IRegistrationResult
    {
        private readonly Container container;

        internal BatchRegistrationResult(
            Container container, Type serviceType, IEnumerable<Mapping> registrations)
        {
            this.container = container;
            this.ServiceType = serviceType;
            this.Registrations = registrations.ToArray();
        }

        /// <inheritdoc />
        Container IRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ServiceType { get; }

        /// <summary>TODO</summary>
        public Mapping[] Registrations { get; }

        /// <summary>TODO</summary>
        public class Mapping
        {
            internal Mapping(Type serviceType, Registration registration)
            {
                this.ServiceType = serviceType;
                this.Registration = registration;
            }

            /// <summary>TODO</summary>
            public Type ServiceType { get; }

            /// <summary>TODO</summary>
            public Registration Registration { get; }
        }
    }
}