#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2010 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
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
    /// Defines the container's behavior for verifying the constructor that will be picked by the
    /// <see cref="IConstructorResolutionBehavior"/>.
    /// </summary>
    public interface IConstructorVerificationBehavior
    {
        /// <summary>Verifies the specified <paramref name="parameter"/>.</summary>
        /// <param name="parameter">The parameter.</param>
        /// <exception cref="ActivationException">Thrown when the <paramref name="parameter"/> cannot be 
        /// used for auto wiring.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="parameter"/> is a
        /// null reference.</exception>
        void Verify(ParameterInfo parameter);
    }
}