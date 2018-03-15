namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using SimpleInjector.Internals;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Contains methods for registering and creating collections in the <see cref="Container"/>.
    /// </summary>
    public class ContainerCollectionRegistrator
    {
        private readonly Container container;

        internal ContainerCollectionRegistrator(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            this.container = container;
        }

        /// <summary>
        /// Creates a collection of <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated.
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// registered, i.e the resolved collection is deterministic.   
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
        /// registered, i.e the resolved collection is deterministic.   
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

        /// <summary>
        /// Creates a collection of <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// registered, i.e the resolved collection is deterministic.   
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
        /// service types supplied by the given <paramref name="registrations"/> instances.
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
        /// registered, i.e the resolved collection is deterministic.   
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
        /// service types supplied by the given <paramref name="registrations"/> instances.
        /// </exception>
        public IEnumerable<TService> Create<TService>(IEnumerable<Registration> registrations) where TService : class
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