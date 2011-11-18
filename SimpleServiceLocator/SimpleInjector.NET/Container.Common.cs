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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using SimpleInjector.InstanceProducers;

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
        
        private EventHandler<UnregisteredTypeEventArgs> resolveUnregisteredType = (s, e) => { };
        
        private Dictionary<Type, PropertyInjector> propertyInjectorCache =
            new Dictionary<Type, PropertyInjector>();

        /// <summary>Initializes a new instance of the <see cref="Container"/> class.</summary>
        public Container()
        {
            this.RegisterSingle<Container>(this);
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
        /// <returns>An array of <see cref="InstanceProducer"/> instances.</returns>
        public IInstanceProducer[] GetCurrentRegistrations()
        {
            var snapshot = this.registrations;

            // We must lock, because not locking could lead to race conditions.
            this.LockContainer();

            return snapshot.Values.ToArray();
        }

        /// <summary>
        /// Determines whether the specified System.Object is equal to the current System.Object.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current System.Object.</param>
        /// <returns>
        /// True if the specified System.Object is equal to the current System.Object; otherwise, false.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>Returns the hash code of the current instance.</summary>
        /// <returns>The hash code of the current instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the <see cref="Container"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the <see cref="Container"/>.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <summary>Gets the <see cref="System.Type"/> of the current instance.</summary>
        /// <returns>The <see cref="System.Type"/> instance that represents the exact runtime 
        /// type of the current instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification =
            "This FxCop warning is valid, but this method is used to be able to attach an " +
            "EditorBrowsableAttribute to the GetType method, which will hide the method when the user " +
            "browses the methods of the Container class with IntelliSense. The GetType method has " +
            "no value for the user who will only use this class for registration.")]
        public new Type GetType()
        {
            return base.GetType();
        }

        /// <summary>Wrapper for instance initializer Action delegates.</summary>
        private sealed class InstanceInitializer
        {
            internal Type ServiceType { get; set; }

            internal object Action { get; set; }
        }
    }
}