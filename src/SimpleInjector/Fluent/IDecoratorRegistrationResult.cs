// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Fluent
{
    using System;

    /// <summary>TODO</summary>
    public interface IDecoratorRegistrationResult : IRegistrationResult
    {
        /// <summary>TODO</summary>
        Type DecoratorType { get; }

        /// <summary>TODO</summary>
        Predicate<DecoratorPredicateContext>? Predicate { get; }
    }

}