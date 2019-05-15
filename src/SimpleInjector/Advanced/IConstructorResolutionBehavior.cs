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
        /// container to create that instance.
        /// </summary>
        /// <param name="implementationType">Type of the implementation to find a suitable constructor for.</param>
        /// <returns>
        /// The <see cref="ConstructorInfo"/>. This method never returns null.
        /// </returns>
        /// <exception cref="ActivationException">Thrown when no suitable constructor could be found.</exception>
        ConstructorInfo GetConstructor(Type implementationType);
    }
}