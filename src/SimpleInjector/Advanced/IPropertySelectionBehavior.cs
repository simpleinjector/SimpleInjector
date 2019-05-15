// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Defines the container's behavior for selecting properties to inject during the creation of a type.
    /// Set the <see cref="ContainerOptions.PropertySelectionBehavior">PropertySelectionBehavior</see> 
    /// property of the container's <see cref="Container.Options"/> property to change the default behavior 
    /// of the container. By default, no properties will be injected by the container.
    /// </summary>
    public interface IPropertySelectionBehavior
    {
        /// <summary>
        /// Determines whether a property should be injected by the container upon creation of its type.
        /// </summary>
        /// <param name="implementationType">
        /// The type being created for which the property should be injected. Note that this might a
        /// different type than the type on which the property is declared (which might be a base class).</param>
        /// <param name="propertyInfo">The property to check.</param>
        /// <returns>True when the property should be injected.</returns>
        bool SelectProperty(Type implementationType, PropertyInfo propertyInfo);
    }
}