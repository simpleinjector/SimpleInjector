// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;

    /// <summary>
    /// Defines the container's behavior for selecting the lifestyle for a registration in case no lifestyle
    /// is explicitly supplied.
    /// Set the <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see> 
    /// property of the container's <see cref="Container.Options"/> property to change the default behavior 
    /// of the container. By default, when no lifestyle is explicitly supplied, the 
    /// <see cref="Lifestyle.Transient">Transient</see> lifestyle is used.
    /// </summary>
    public interface ILifestyleSelectionBehavior
    {
        /// <summary>Selects the lifestyle based on the supplied type information.</summary>
        /// <param name="implementationType">Type of the implementation to that is registered.</param>
        /// <returns>The suited <see cref="Lifestyle"/> for the given type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either one of the arguments is a null reference.</exception>
        Lifestyle SelectLifestyle(Type implementationType);
    }
}