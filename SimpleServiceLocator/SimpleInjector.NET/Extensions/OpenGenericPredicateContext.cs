#region Copyright (c) 2013 S. van Deursen
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

namespace SimpleInjector.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// An instance of this type will be supplied to the <see cref="Predicate{T}"/>
    /// delegate that is that is supplied to the 
    /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric(Container,Type,Type,Lifestyle,Predicate{OpenGenericPredicateContext})">RegisterOpenGeneric</see>
    /// overload that takes this delegate. This type contains information about the open generic service that is about
    /// to be created and it allows the user to examine the given instance to decide whether this implementation should
    /// be created or not.
    /// </summary>
    /// <remarks>
    /// Please see the 
    /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric(Container,Type,Type,Lifestyle,Predicate{OpenGenericPredicateContext})">RegisterOpenGeneric</see>
    /// method for more information.
    /// </remarks>
    [DebuggerDisplay("OpenGenericPredicateContext ({DebuggerDisplay,nq})")]
    public sealed class OpenGenericPredicateContext
    {
        internal OpenGenericPredicateContext(Type serviceType, Type implementationType, bool handled)
        {
            this.ServiceType = serviceType;
            this.ImplementationType = implementationType;
            this.Handled = handled;
        }

        /// <summary>Gets the closed generic service type that is to be created.</summary>
        /// <value>The closed generic service type.</value>
        public Type ServiceType { get; private set; }

        /// <summary>Gets the closed generic implementation type that will be created by the container.</summary>
        /// <value>The implementation type.</value>
        public Type ImplementationType { get; private set; }

        /// <summary>Gets a value indicating whether a previous <b>RegisterOpenGeneric</b> registration has already
        /// been applied for the given <see cref="ServiceType"/>.</summary>
        /// <value>The indication whether the event has been handled.</value>
        public bool Handled { get; private set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "This method is called by the debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "ServiceType = {0}, ImplementationType = {1}, Handled = {2}",
                    this.ServiceType.ToFriendlyName(),
                    this.ImplementationType.ToFriendlyName(),
                    this.Handled);
            }
        }
    }
}