// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;
    using System.Collections.Generic;

    /// <summary>TODO</summary>
    public class RegistrationAppendCollectionRegistrationResult : IRegistrationsCollectionRegistrationResult
    {
        private readonly Container container;

        internal RegistrationAppendCollectionRegistrationResult(
            Container container, Type elementType, Registration registration)
        {
            this.container = container;
            this.ElementType = elementType;
            this.Registration = registration;
        }

        /// <inheritdoc />
        Container ICollectionRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ElementType { get; }

        /// <summary>TODO</summary>
        public Registration Registration { get; }

        /// <inheritdoc />
        IEnumerable<Registration> IRegistrationsCollectionRegistrationResult.Registrations =>
            new[] { this.Registration };
    }
}