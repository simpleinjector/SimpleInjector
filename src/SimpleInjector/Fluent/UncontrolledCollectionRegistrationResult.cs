// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;
    using System.Collections;

    /// <summary>TODO</summary>
    public class UncontrolledCollectionRegistrationResult : ICollectionRegistrationResult
    {
        private readonly Container container;

        internal UncontrolledCollectionRegistrationResult(
            Container container, Type elementType, IEnumerable collection)
        {
            this.container = container;
            this.ElementType = elementType;
        }

        /// <inheritdoc />
        Container ICollectionRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ElementType { get; }
    }
}