// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;
    using System.Collections.Generic;

    /// <summary>TODO</summary>
    public class TypesCollectionRegistrationResult : ITypesCollectionRegistrationResult
    {
        private readonly Container container;

        internal TypesCollectionRegistrationResult(
            Container container, Type elementType, IEnumerable<Type> types)
        {
            this.container = container;
            this.ElementType = elementType;
            this.Types = types;
        }

        /// <inheritdoc />
        Container ICollectionRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ElementType { get; }

        /// <inheritdoc />
        public IEnumerable<Type> Types { get; }
    }
}