// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;
    using System.Collections.Generic;

    /// <summary>TODO</summary>
    public class RegistrationsCollectionRegistrationResult : IRegistrationsCollectionRegistrationResult
    {
        private readonly Container container;

        internal RegistrationsCollectionRegistrationResult(
            Container container, Type elementType, IEnumerable<Registration> registrations)
        {
            this.container = container;
            this.ElementType = elementType;
            this.Registrations = registrations;
        }

        /// <inheritdoc />
        Container ICollectionRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ElementType { get; }

        /// <inheritdoc />
        public IEnumerable<Registration> Registrations { get; }
    }
}