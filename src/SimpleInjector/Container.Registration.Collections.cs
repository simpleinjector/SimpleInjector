#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2018 Simple Injector Contributors
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

namespace SimpleInjector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

#if !PUBLISH
    /// <summary>Methods for registration of collections.</summary>
#endif
    public partial class Container
    {
        /// <summary>
        /// Registers a dynamic (container-uncontrolled) collection of elements of type 
        /// <typeparamref name="TService"/>. A call to <see cref="GetAllInstances{T}"/> will return the 
        /// <paramref name="containerUncontrolledCollection"/> itself, and updates to the collection will be 
        /// reflected in the result. If updates are allowed, make sure the collection can be iterated safely 
        /// if you're running a multi-threaded application.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.
        /// </typeparam>
        /// <param name="containerUncontrolledCollection">The container-uncontrolled collection to register.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when a 
        /// <paramref name="containerUncontrolledCollection"/> for <typeparamref name="TService"/> has already
        /// been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="containerUncontrolledCollection"/> is a null reference.
        /// </exception>
        [Obsolete("Please use Container." + nameof(Container.Collection) + "." +
            nameof(ContainerCollectionRegistrator.Register) + " instead. " +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        public void RegisterCollection<TService>(IEnumerable<TService> containerUncontrolledCollection)
            where TService : class
        {
            this.Collection.Register(containerUncontrolledCollection);
        }

        /// <summary>
        /// Registers a collection of singleton elements of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="singletons">The collection to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when a <paramref name="singletons"/>
        /// for <typeparamref name="TService"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="singletons"/> is a null
        /// reference.</exception>
        /// <exception cref="ArgumentException">Thrown when one of the elements of <paramref name="singletons"/>
        /// is a null reference.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly",
            Justification = "TService is the name of the generic type argument. So this warning is a false positive.")]
        [Obsolete("Please use Container." + nameof(Container.Collection) + "." +
            nameof(ContainerCollectionRegistrator.Register) + " instead. " +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        public void RegisterCollection<TService>(params TService[] singletons) where TService : class
        {
            this.Collection.Register<TService>(singletons);
        }

        /// <summary>
        /// Registers a collection of <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the resolved collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceTypes"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <typeparamref name="TService"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        [Obsolete("Please use Container." + nameof(Container.Collection) + "." +
            nameof(ContainerCollectionRegistrator.Register) + " instead. " +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        public void RegisterCollection<TService>(IEnumerable<Type> serviceTypes) where TService : class
        {
            this.RegisterCollection(typeof(TService), serviceTypes);
        }

        /// <summary>
        /// Registers a collection of <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the resolved collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="registrations"/> contains a null
        /// (Nothing in VB) element or when <typeparamref name="TService"/> is not assignable from any of the
        /// service types supplied by the given <paramref name="registrations"/> instances.
        /// </exception>
        [Obsolete("Please use Container." + nameof(Container.Collection) + "." +
            nameof(ContainerCollectionRegistrator.Register) + " instead. " +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        public void RegisterCollection<TService>(IEnumerable<Registration> registrations)
            where TService : class
        {
            this.RegisterCollection(typeof(TService), registrations);
        }

        /// <summary>
        /// Registers a collection of <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the resolved collection of <paramref name="serviceType"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <paramref name="serviceType"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        [Obsolete("Please use Container." + nameof(Container.Collection) + "." +
            nameof(ContainerCollectionRegistrator.Register) + " instead. " +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        public void RegisterCollection(Type serviceType, IEnumerable<Type> serviceTypes)
        {
            this.Collection.Register(serviceType, serviceTypes);
        }

        /// <summary>
        /// Registers a collection of <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the resolved collection of <paramref name="serviceType"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <param name="serviceType">The base type or interface for elements in the collection. This can be
        /// an a non-generic type, closed generic type or generic type definition.</param>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="registrations"/> contains a null
        /// (Nothing in VB) element or when <paramref name="serviceType"/> is not assignable from any of the
        /// service types supplied by the given <paramref name="registrations"/> instances.
        /// </exception>
        [Obsolete("Please use Container." + nameof(Container.Collection) + "." +
            nameof(ContainerCollectionRegistrator.Register) + " instead. " +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        public void RegisterCollection(Type serviceType, IEnumerable<Registration> registrations)
        {
            this.Collection.Register(serviceType, registrations);
        }

        /// <summary>
        /// Registers a dynamic (container uncontrolled) collection of elements of type 
        /// <paramref name="serviceType"/>. A call to <see cref="GetAllInstances{T}"/> will return the 
        /// <paramref name="containerUncontrolledCollection"/> itself, and updates to the collection will be 
        /// reflected in the result. If updates are allowed, make sure the collection can be iterated safely 
        /// if you're running a multi-threaded application.
        /// </summary>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="containerUncontrolledCollection">The collection of items to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null 
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> represents an
        /// open generic type.</exception>
        [Obsolete("Please use Container." + nameof(Container.Collection) + "." +
            nameof(ContainerCollectionRegistrator.Register) + " instead. " +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        public void RegisterCollection(Type serviceType, IEnumerable containerUncontrolledCollection)
        {
            this.Collection.Register(serviceType, containerUncontrolledCollection);
        }
    }
}