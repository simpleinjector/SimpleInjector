// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;
    using System.Collections.Generic;

    /// <summary>TODO</summary>
    public interface ITypesCollectionRegistrationResult : ICollectionRegistrationResult
    {
        /// <summary>TODO</summary>
        IEnumerable<Type> Types { get; }
    }
}