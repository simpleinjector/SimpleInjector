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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleInjector
{
    /// <summary>
    /// The container. Create an instance of this type for registration of dependencies.
    /// </summary>
    public partial class Container
    {
        private readonly object locker = new object();

        private readonly List<InstanceInitializer> instanceInitializers = new List<InstanceInitializer>();

        private Dictionary<Type, IInstanceProducer> registrations = new Dictionary<Type, IInstanceProducer>(40);

        // This dictionary is only used for validation. After validation is gets erased.
        private Dictionary<Type, IEnumerable> collectionsToValidate = new Dictionary<Type, IEnumerable>();

        private bool locked;

        private EventHandler<UnregisteredTypeEventArgs> resolveUnregisteredType;

        /// <summary>Initializes a new instance of the <see cref="Container"/> class.</summary>
        public Container()
        {
        }

        internal Dictionary<Type, IInstanceProducer> Registrations
        {
            get { return this.registrations; }
        }

        /// <summary>
        /// Returns an array with the current registrations. This list contains all explicitly registered
        /// types, and all implictly registered instances. Implicit registrations are  all concrete 
        /// unregistered types that have been requested, all types that have been resolved using
        /// unregistered type resolution (using the <see cref="ResolveUnregisteredType"/> event), and
        /// requested unregistered collections. Note that the result of this method may change over time, 
        /// because of these implicit registrations. Because of this, users should not depend on this method
        /// as a reliable source of resolving instances. This method is provided for debugging purposes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A call to this method locks the container. No new registrations can be made after a call to this 
        /// method.
        /// </para>
        /// <para>
        /// <b>Note:</b> This method is <i>not</i> guaranteed to always return the same <b>IInstanceProducer</b>
        /// instance for a given <see cref="Type"/>. It will however either always return <b>null</b> or
        /// always return a producer that is able to return the expected instance. Because of this, do not
        /// compare sets of instances returned by different calls to <see cref="GetCurrentRegistrations"/>
        /// by reference. The way of comparing lists is by the actual type. The type of each instance is
        /// guaranteed to be unique in the returned list.
        /// </para>
        /// </remarks>
        /// <returns>An array of <see cref="IInstanceProducer"/> instances.</returns>
        public IInstanceProducer[] GetCurrentRegistrations()
        {
            var snapshot = this.registrations;

            // We must lock, because not locking could lead to race conditions.
            this.LockContainer();

            return snapshot.Values.ToArray();
        }

        /// <summary>Wrapper for instance initializer Action delegates.</summary>
        private sealed class InstanceInitializer
        {
            internal Type ServiceType { get; set; }

            internal object Action { get; set; }
        }
    }
}