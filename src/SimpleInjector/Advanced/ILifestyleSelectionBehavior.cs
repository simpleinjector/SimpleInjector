#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014-2016 Simple Injector Contributors
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