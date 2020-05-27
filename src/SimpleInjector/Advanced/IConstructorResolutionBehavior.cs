// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Defines the container's behavior for finding a suitable constructor for the creation of a type.
    /// Set the <see cref="ContainerOptions.ConstructorResolutionBehavior">ConstructorResolutionBehavior</see>
    /// property of the container's <see cref="Container.Options"/> property to change the default behavior
    /// of the container.
    /// </summary>
    public interface IConstructorResolutionBehavior
    {
        /// <summary>
        /// Gets the given <paramref name="implementationType"/>'s constructor that can be used by the
        /// container to create that instance. In case no suitable constructor can be found, <b>null</b> is
        /// returned and the <paramref name="errorMessage"/> will contain the reason why the resolution failed.
        /// </summary>
        /// <param name="implementationType">Type of the implementation to find a suitable constructor for.</param>
        /// <param name="errorMessage">The reason why the resolution failed.</param>
        /// <returns>The <see cref="ConstructorInfo"/> or null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="implementationType"/> is null.
        /// </exception>
        ConstructorInfo? TryGetConstructor(Type implementationType, out string? errorMessage);
    }
}