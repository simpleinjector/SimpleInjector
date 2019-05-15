// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.ServiceCollection
{
    using System;

    /// <summary>
    /// Allows access to the request's <see cref="IServiceProvider"/> instance.
    /// This interface is used by Simple Injector and allow it to resolve transient and scoped services from the
    /// framework's <see cref="IServiceProvider"/> through cross wiring.
    /// </summary>
    public interface IServiceProviderAccessor
    {
        /// <summary>
        /// Gets the current <see cref="IServiceProvider"/> for the current scope or request.
        /// This operation will never return null.
        /// </summary>
        /// <value>An <see cref="IServiceProvider"/> instance.</value>
        IServiceProvider Current { get; }
    }
}