#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2016 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

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