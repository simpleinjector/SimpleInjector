#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2018 Simple Injector Contributors
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using SimpleInjector.Internals;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Contains methods for registering and creating collections in the <see cref="Container"/>.
    /// </summary>
    public sealed class ContainerCollectionRegistrator
    {
        private readonly Container container;

        internal ContainerCollectionRegistrator(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            this.container = container;
        }

        /// <summary>
        /// Creates a collection of 
        /// all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <typeparamref name="TService"/>
        /// with a default lifestyle and register them as a collection of <typeparamref name="TService"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// </summary>
        /// <typeparam name="TService">The element type of the collections to register. This can be either
        /// a non-generic, closed-generic or open-generic type.</typeparam>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        /// <returns>A collection that acts as stream, and calls back into the container to resolve instances
        /// every time the collection is enumerated.</returns>
        public IEnumerable<TService> Create<TService>(params Assembly[] assemblies) where TService : class
        {
            return this.Create<TService>((IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Creates a collection of 
        /// all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <typeparamref name="TService"/>
        /// with a default lifestyle and register them as a collection of <typeparamref name="TService"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// </summary>
        /// <typeparam name="TService">The element type of the collections to register. This can be either
        /// a non-generic, closed-generic or open-generic type.</typeparam>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        /// <returns>A collection that acts as stream, and calls back into the container to resolve instances
        /// every time the collection is enumerated.</returns>
        public IEnumerable<TService> Create<TService>(IEnumerable<Assembly> assemblies) where TService : class
        {
            Requires.IsNotNull(assemblies, nameof(assemblies));

            var compositesExcluded = new TypesToRegisterOptions { IncludeComposites = false };
            var types = this.container.GetTypesToRegister(typeof(TService), assemblies, compositesExcluded);

            return this.Create<TService>(types);
        }

        /// <summary>
        /// Creates a collection of <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated.
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A collection that acts as stream, and calls back into the container to resolve instances
        /// every time the collection is enumerated.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceTypes"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <typeparamref name="TService"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        public IEnumerable<TService> Create<TService>(params Type[] serviceTypes) where TService : class
        {
            return this.Create<TService>((IEnumerable<Type>)serviceTypes);
        }

        /// <summary>
        /// Creates a collection of <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated.
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A collection that acts as stream, and calls back into the container to resolve instances
        /// every time the collection is enumerated.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceTypes"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <typeparamref name="TService"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        public IEnumerable<TService> Create<TService>(IEnumerable<Type> serviceTypes) where TService : class
        {
            return this.CreateInternal<TService>(serviceTypes);
        }

        /// <summary>
        /// Creates a collection of <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A collection that acts as stream, and calls back into the container to resolve instances
        /// every time the collection is enumerated.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="registrations"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="registrations"/> contains a null
        /// (Nothing in VB) element or when <typeparamref name="TService"/> is not assignable from any of the
        /// types supplied by the given <paramref name="registrations"/> instances.
        /// </exception>
        public IEnumerable<TService> Create<TService>(params Registration[] registrations) where TService : class
        {
            return this.Create<TService>((IEnumerable<Registration>)registrations);
        }

        /// <summary>
        /// Creates a collection of <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A collection that acts as stream, and calls back into the container to resolve instances
        /// every time the collection is enumerated.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="registrations"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="registrations"/> contains a null
        /// (Nothing in VB) element or when <typeparamref name="TService"/> is not assignable from any of the
        /// types supplied by the given <paramref name="registrations"/> instances.
        /// </exception>
        public IEnumerable<TService> Create<TService>(IEnumerable<Registration> registrations) 
            where TService : class
        {
            return this.CreateInternal<TService>(registrations);
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of a collection of 
        /// all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <typeparamref name="TService"/>
        /// with a default lifestyle and register them as a collection of <typeparamref name="TService"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// The collection's instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        public Registration CreateRegistration<TService>(params Assembly[] assemblies) where TService : class
        {
            return this.CreateRegistration<TService>((IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of a collection of 
        /// all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <typeparamref name="TService"/>
        /// with a default lifestyle and register them as a collection of <typeparamref name="TService"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// The collection's instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        public Registration CreateRegistration<TService>(IEnumerable<Assembly> assemblies)
            where TService : class
        {
            Requires.IsNotNull(assemblies, nameof(assemblies));

            var compositesExcluded = new TypesToRegisterOptions { IncludeComposites = false };
            var types = this.container.GetTypesToRegister(typeof(TService), assemblies, compositesExcluded);

            return this.CreateRegistration<TService>(types);
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of a collection of 
        /// <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceTypes"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <typeparamref name="TService"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        public Registration CreateRegistration<TService>(params Type[] serviceTypes) where TService : class
        {
            return this.CreateRegistration<TService>((IEnumerable<Type>)serviceTypes);
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of a collection of 
        /// <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceTypes"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <typeparamref name="TService"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        public Registration CreateRegistration<TService>(IEnumerable<Type> serviceTypes)
            where TService : class
        {
            // This might seem backwards, but CreateInternal<T> also creates a Producer/Registration pair to
            // ensure the collection is correctly verified, and we can make use of this existing pair.
            return this.CreateInternal<TService>(serviceTypes).ParentProducer.Registration;
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of a collection of 
        /// <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="registrations"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="registrations"/> contains a null
        /// (Nothing in VB) element or when <typeparamref name="TService"/> is not assignable from any of the
        /// types supplied by the given <paramref name="registrations"/> instances.
        /// </exception>
        public Registration CreateRegistration<TService>(params Registration[] registrations)
            where TService : class
        {
            return this.CreateRegistration<TService>((IEnumerable<Registration>)registrations);
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of a collection of 
        /// <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="registrations"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="registrations"/> contains a null
        /// (Nothing in VB) element or when <typeparamref name="TService"/> is not assignable from any of the
        /// types supplied by the given <paramref name="registrations"/> instances.
        /// </exception>
        public Registration CreateRegistration<TService>(IEnumerable<Registration> registrations)
            where TService : class
        {
            return this.CreateInternal<TService>(registrations).ParentProducer.Registration;
        }

        /// <summary>
        /// Allows appending new registrations to existing registrations made using one of the
        /// <b>RegisterCollection</b> overloads.
        /// </summary>
        /// <param name="serviceType">The service type of the collection.</param>
        /// <param name="registration">The registration to append.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceType"/> is not a
        /// reference type, is open generic, or ambiguous.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the container is locked.</exception>
        /// <exception cref="NotSupportedException">Thrown when the method is called for a registration
        /// that is made with one of the <b>RegisterCollection</b> overloads that accepts a dynamic collection
        /// (an <b>IEnumerable</b> or <b>IEnumerable&lt;TService&gt;</b>).</exception>
        public void AppendTo(Type serviceType, Registration registration)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(registration, nameof(registration));

            Requires.IsReferenceType(serviceType, nameof(serviceType));
            Requires.IsNotAnAmbiguousType(serviceType, nameof(serviceType));

            Requires.IsRegistrationForThisContainer(this.container, registration, nameof(registration));
            Requires.ServiceOrItsGenericTypeDefinitionIsAssignableFromImplementation(serviceType,
                registration.ImplementationType, nameof(registration));

            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(serviceType, new[] { registration },
                nameof(registration));

            this.container.AppendToCollectionInternal(serviceType, registration);
        }

        /// <summary>
        /// Allows appending new registrations to existing registrations made using one of the
        /// <b>RegisterCollection</b> overloads.
        /// </summary>
        /// <param name="serviceType">The service type of the collection.</param>
        /// <param name="implementationType">The implementation type to append.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceType"/> is not a
        /// reference type, or ambiguous.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the container is locked.</exception>
        /// <exception cref="NotSupportedException">Thrown when the method is called for a registration
        /// that is made with one of the <b>RegisterCollection</b> overloads that accepts a dynamic collection
        /// (an <b>IEnumerable</b> or <b>IEnumerable&lt;TService&gt;</b>).</exception>
        public void AppendTo(Type serviceType, Type implementationType)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(implementationType, nameof(implementationType));

            Requires.IsReferenceType(serviceType, nameof(serviceType));
            Requires.IsNotAnAmbiguousType(serviceType, nameof(serviceType));

            Requires.ServiceOrItsGenericTypeDefinitionIsAssignableFromImplementation(serviceType,
                implementationType, nameof(implementationType));

            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(serviceType,
                new[] { implementationType }, nameof(implementationType));

            this.container.AppendToCollectionInternal(serviceType, implementationType);
        }

        private ContainerControlledCollection<TService> CreateInternal<TService>(
            IEnumerable<Type> serviceTypes)
            where TService : class
        {
            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));
            Requires.IsNotNull(serviceTypes, nameof(serviceTypes));

            // Make a copy for correctness and performance.
            serviceTypes = serviceTypes.ToArray();

            Requires.DoesNotContainNullValues(serviceTypes, nameof(serviceTypes));
            Requires.ServiceIsAssignableFromImplementations(typeof(TService), serviceTypes, nameof(serviceTypes),
                typeCanBeServiceType: true);
            Requires.DoesNotContainOpenGenericTypesWhenServiceTypeIsNotGeneric(typeof(TService), serviceTypes,
                nameof(serviceTypes));
            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(typeof(TService), serviceTypes,
                nameof(serviceTypes));

            var collection = new ContainerControlledCollection<TService>(this.container);

            collection.AppendAll(serviceTypes);

            this.RegisterForVerification(collection);

            return collection;
        }

        private ContainerControlledCollection<TService> CreateInternal<TService>(
            IEnumerable<Registration> registrations)
            where TService : class
        {
            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));
            Requires.IsNotNull(registrations, nameof(registrations));

            Requires.DoesNotContainNullValues(registrations, nameof(registrations));
            Requires.AreRegistrationsForThisContainer(this.container, registrations, nameof(registrations));
            Requires.ServiceIsAssignableFromImplementations(typeof(TService), registrations, nameof(registrations),
                typeCanBeServiceType: true);
            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(typeof(TService), registrations,
                nameof(registrations));

            var collection = new ContainerControlledCollection<TService>(this.container);

            collection.AppendAll(registrations);

            this.RegisterForVerification(collection);

            return collection;
        }

        private void RegisterForVerification<TService>(ContainerControlledCollection<TService> collection)
        {
            // By creating a Producer, Simple Injector will automatically register it as 'external producer',
            // which allows it to be verified. To prevent memory leaks however, this external producer is
            // linked using a WeakReference to allow it to be GCed. To prevent this from happening, while
            // the application keeps referencing the collection, we let the collection reference the producer.
            collection.ParentProducer =
                SingletonLifestyle.CreateControlledCollectionProducer(collection, this.container);
        }
    }
}