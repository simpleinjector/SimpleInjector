#region Copyright (c) 2013 Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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

    /// <summary>
    /// Contains data that can be used to initialize a created instance. This data includes the actual
    /// created <see cref="Instance"/> and the <see cref="Context"/> information about the created instance.
    /// </summary>
    public struct InstanceInitializationData : IEquatable<InstanceInitializationData>
    {
        // NOTE: Because of performance considerations, this type has been made a struct. This prevents Simple
        // Injector from creating an extra reference type every time an instance is created. This would cause 
        // extra pressure on the GC.
        private readonly InitializationContext context;
        private readonly object instance;

        internal InstanceInitializationData(InitializationContext context, object instance)
        {
            this.context = context;
            this.instance = instance;
        }

        /// <summary>Gets the <see cref="InitializationContext"/> with contextual information about the 
        /// created instance.</summary>
        /// <value>The <see cref="InitializationContext"/>.</value>
        public InitializationContext Context
        {
            get { return this.context; }
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
                (this.context == null ? 0 : this.context.GetHashCode()) ^
                (this.instance == null ? 0 : this.instance.GetHashCode());
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>True if the current object is equal to the other parameter; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is InstanceInitializationData))
            {
                return false;
            }

            return this.Equals((InstanceInitializationData)obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(InstanceInitializationData other)
        {
            return this == other;
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="InstanceInitializationData"/> objects are equal.
        /// </summary>
        /// <param name="first">The first object to compare.</param>
        /// <param name="second">The second object to compare.</param>
        /// <returns>True if a and b are equal; otherwise, false.</returns>
        public static bool operator ==(InstanceInitializationData first, InstanceInitializationData second)
        {
            return object.ReferenceEquals(first.context, second.context) &&
                object.ReferenceEquals(first.instance, second.instance);
        }

        /// <summary>
        /// Indicates whether the values of two specified  <see cref="InstanceInitializationData"/>  objects are 
        /// not equal.
        /// </summary>
        /// <param name="first">The first object to compare.</param>
        /// <param name="second">The second object to compare.</param>
        /// <returns>True if a and b are not equal; otherwise, false.</returns>
        public static bool operator !=(InstanceInitializationData first, InstanceInitializationData second)
        {
            return !(first == second);
        }
    }
}