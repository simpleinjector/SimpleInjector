#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2016 Simple Injector Contributors
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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// An instance of this type will be supplied to the <see cref="System.Predicate{T}" />
    /// delegate that is that is supplied to the 
    /// <see cref="SimpleInjector.Container.RegisterInitializer(Action{InstanceInitializationData}, Predicate{InitializerContext})">RegisterInitializer</see>
    /// overload that takes this delegate. This type contains contextual information about the creation and it 
    /// allows the user to examine the given instance to decide whether the instance should be initialized or 
    /// not.
    /// </summary>
    [DebuggerDisplay(nameof(InitializerContext) + " ({" + nameof(DebuggerDisplay) + ", nq})")]
    public class InitializerContext
    {
        internal InitializerContext(Registration registration)
        {
            Requires.IsNotNull(registration, nameof(registration));

            this.Registration = registration;
        }

        /// <summary>
        /// Gets a null reference. This property has been deprecated.
        /// </summary>
        /// <value>The null (Nothing in VB).</value>
        [Obsolete("The Producer property has been deprecated. Please use Registration instead.", error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public InstanceProducer Producer { get; }

        /// <summary>
        /// Gets the <see cref="Registration"/> that is responsible for the initialization of the created
        /// instance.
        /// </summary>
        /// /// <value>The <see cref="Registration"/>.</value>
        public Registration Registration { get; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "This method is called by the debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebuggerDisplay =>
            string.Format(CultureInfo.InvariantCulture,
                "Registration.ImplementationType: {0}",
                this.Registration.ImplementationType.ToFriendlyName());
    }
}