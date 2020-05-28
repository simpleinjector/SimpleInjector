// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;
    using System.Collections.Generic;

    /// <summary>TODO</summary>
    public class TypeAppendCollectionRegistrationResult : ITypesCollectionRegistrationResult
    {
        private readonly Container container;

        internal TypeAppendCollectionRegistrationResult(Container container, Type elementType, Type appendedType)
        {
            this.container = container;
            this.ElementType = elementType;
            this.AppendedType = appendedType;
        }

        /// <inheritdoc />
        Container ICollectionRegistrationResult.Container => this.container;

        /// <inheritdoc />
        public Type ElementType { get; }

        /// <summary>TODO</summary>
        public Type AppendedType { get; }

        /// <inheritdoc />
        IEnumerable<Type> ITypesCollectionRegistrationResult.Types => new[] { this.AppendedType };
    }
}