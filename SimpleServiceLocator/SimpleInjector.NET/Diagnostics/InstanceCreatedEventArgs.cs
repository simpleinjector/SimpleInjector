#region Copyright (c) 2013 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2013 S. van Deursen
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

namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Represents the method that will handle an <see cref="InstanceCreatedEventArgs"/> event.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e"> An <see cref="InstanceCreatedEventArgs"/> that contains the event data.</param>
#if !SILVERLIGHT
    [Serializable]
#endif
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "e",
        Justification = "This is the event arguments and the convention is to name it 'e'.")]
    public delegate void InstanceCreatedEventHandler(InstanceProducer sender, InstanceCreatedEventArgs e);
    
    /// <summary>
    /// Provides data for and interaction with the <see cref="Container.InstanceCreated">InstanceCreated</see> 
    /// event of the <see cref="Container"/>.
    /// </summary>
#if DEBUG
    [DebuggerDisplay("InstanceCreatedEventArgs (" + 
        "ImplementationType: {SimpleInjector.Helpers.ToFriendlyName(Registration.ImplementationType)} )")]
#endif
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
        Justification = "We can't inherit from EventArgs, but this structure represents an EventArgs.")]
    public struct InstanceCreatedEventArgs : IEquatable<InstanceCreatedEventArgs>
    {
        // NOTE: Because of performance considerations, this type has been made a struct. This prevents Simple
        // Injector from creating an extra reference type (the InstanceCreatedEventArgs) every time an instance
        // is created. This would cause extra pressure on the GC.
        private readonly Registration registration;
        private readonly object instance;

        internal InstanceCreatedEventArgs(Registration registration, object instance)
        {
            this.registration = registration;
            this.instance = instance;
        }

        /// <summary>Gets the <see cref="Registration"/> that triggered the event.</summary>
        /// <value>The <see cref="Registration"/>.</value>
        public Registration Registration
        {
            get { return this.registration; }
        }

        /// <summary>Gets the created instance.</summary>
        /// <value>The created instance.</value>
        public object Instance
        {
            get { return this.instance; }
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return
                (this.registration == null ? 0 : this.registration.GetHashCode()) ^
                (this.instance == null ? 0 : this.instance.GetHashCode());
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>True if the current object is equal to the other parameter; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is InstanceCreatedEventArgs))
            {
                return false;
            }

            return this.Equals((InstanceCreatedEventArgs)obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(InstanceCreatedEventArgs other)
        {
            return this == other;
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="InstanceCreatedEventArgs"/> objects are equal.
        /// </summary>
        /// <param name="first">The first object to compare.</param>
        /// <param name="second">The second object to compare.</param>
        /// <returns>True if a and b are equal; otherwise, false.</returns>
        public static bool operator ==(InstanceCreatedEventArgs first, InstanceCreatedEventArgs second)
        {
            return object.ReferenceEquals(first.registration, first.registration) &&
                object.ReferenceEquals(second.instance, second.instance);
        }

        /// <summary>
        /// Indicates whether the values of two specified  <see cref="InstanceCreatedEventArgs"/>  objects are 
        /// not equal.
        /// </summary>
        /// <param name="first">The first object to compare.</param>
        /// <param name="second">The second object to compare.</param>
        /// <returns>True if a and b are not equal; otherwise, false.</returns>
        public static bool operator !=(InstanceCreatedEventArgs first, InstanceCreatedEventArgs second)
        {
            return !(first == second);
        }
    }
}