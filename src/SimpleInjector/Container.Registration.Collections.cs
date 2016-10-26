#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2015 Simple Injector Contributors
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
    using System.Linq;
    using SimpleInjector.Decorators;
    using SimpleInjector.Internals;
    using SimpleInjector.Lifestyles;

#if !PUBLISH
    /// <summary>Methods for registration of collections.</summary>
#endif
    public partial class Container
    {
        /// <summary>
        /// Registers a dynamic (container uncontrolled) collection of elements of type 
        /// <typeparamref name="TService"/>. A call to <see cref="GetAllInstances{T}"/> will return the 
        /// <paramref name="containerUncontrolledCollection"/> itself, and updates to the collection will be 
        /// reflected in the result. If updates are allowed, make sure the collection can be iterated safely 
        /// if you're running a multi-threaded application.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="containerUncontrolledCollection">The container-uncontrolled collection to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when a <paramref name="containerUncontrolledCollection"/>
        /// for <typeparamref name="TService"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="containerUncontrolledCollection"/> is a null
        /// reference.</exception>
        public void RegisterCollection<TService>(IEnumerable<TService> containerUncontrolledCollection)
            where TService : class
        {
            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));
            Requires.IsNotNull(containerUncontrolledCollection, nameof(containerUncontrolledCollection));

            this.RegisterContainerUncontrolledCollection(typeof(TService), containerUncontrolledCollection);
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
        public void RegisterCollection<TService>(params TService[] singletons) where TService : class
        {
            Requires.IsNotNull(singletons, nameof(singletons));
            Requires.DoesNotContainNullValues(singletons, nameof(singletons));

            if (typeof(TService) == typeof(Type) && singletons.Any())
            {
                throw new ArgumentException(
                    StringResources.RegisterCollectionCalledWithTypeAsTService(singletons.Cast<Type>()),
                    nameof(TService));
            }

            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));

            var singletonRegistrations =
                from singleton in singletons
                select SingletonLifestyle.CreateSingleInstanceRegistration(typeof(TService), singleton, this,
                    singleton.GetType());

            this.RegisterCollection(typeof(TService), singletonRegistrations);
        }

        /// <summary>
        /// Registers a collection of <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the resolved collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// registered, i.e the resolved collection is deterministic.   
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
        /// registered, i.e the resolved collection is deterministic.   
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
        /// registered, i.e the resolved collection is deterministic.   
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
        public void RegisterCollection(Type serviceType, IEnumerable<Type> serviceTypes)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(serviceTypes, nameof(serviceTypes));

            // Make a copy for correctness and performance.
            serviceTypes = serviceTypes.ToArray();

            Requires.DoesNotContainNullValues(serviceTypes, nameof(serviceTypes));
            Requires.ServiceIsAssignableFromImplementations(serviceType, serviceTypes, nameof(serviceTypes),
                typeCanBeServiceType: true);
            Requires.DoesNotContainOpenGenericTypesWhenServiceTypeIsNotGeneric(serviceType, serviceTypes,
                nameof(serviceTypes));
            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(serviceType, serviceTypes, 
                nameof(serviceTypes));

            this.RegisterCollectionInternal(serviceType, serviceTypes);
        }

        /// <summary>
        /// Registers a collection of <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the resolved collection of <paramref name="serviceType"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// registered, i.e the resolved collection is deterministic.   
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
        public void RegisterCollection(Type serviceType, IEnumerable<Registration> registrations)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(registrations, nameof(registrations));

            // Make a copy for performance and correctness.
            registrations = registrations.ToArray();

            Requires.DoesNotContainNullValues(registrations, nameof(registrations));
            Requires.AreRegistrationsForThisContainer(this, registrations, nameof(registrations));
            Requires.ServiceIsAssignableFromImplementations(serviceType, registrations, nameof(registrations), 
                typeCanBeServiceType: true);
            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(serviceType, registrations, 
                nameof(registrations));

            this.RegisterCollectionInternal(serviceType, registrations);
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
        public void RegisterCollection(Type serviceType, IEnumerable containerUncontrolledCollection)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(containerUncontrolledCollection, nameof(containerUncontrolledCollection));
            Requires.IsNotOpenGenericType(serviceType, nameof(serviceType));
            Requires.IsNotAnAmbiguousType(serviceType, nameof(serviceType));

            try
            {
                this.RegisterContainerUncontrolledCollection(serviceType,
                    containerUncontrolledCollection.Cast<object>());
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                throw new ArgumentException(
                    StringResources.UnableToResolveTypeDueToSecurityConfiguration(serviceType, ex) +
                    Environment.NewLine + "paramName: " + nameof(serviceType), ex);
            }
        }

        // This method is internal to prevent the main API of the framework from being 'polluted'. The
        // SimpleInjector.Advanced.AdvancedExtensions.AppendToCollection extension method enabled public
        // exposure.
        internal void AppendToCollectionInternal(Type itemType, Registration registration)
        {
            this.RegisterCollectionInternal(itemType,
                new[] { ContainerControlledItem.CreateFromRegistration(registration) },
                appending: true);
        }

        internal void AppendToCollectionInternal(Type itemType, Type implementationType)
        {
            // NOTE: The supplied serviceTypes can be opened, partially-closed, closed, non-generic or even
            // abstract.
            this.RegisterCollectionInternal(itemType,
                new[] { ContainerControlledItem.CreateFromType(implementationType) },
                appending: true);
        }

        private void RegisterCollectionInternal(Type itemType, IEnumerable<Registration> registrations)
        {
            var controlledItems = registrations.Select(ContainerControlledItem.CreateFromRegistration).ToArray();

            this.RegisterCollectionInternal(itemType, controlledItems);
        }

        private void RegisterCollectionInternal(Type itemType, IEnumerable<Type> serviceTypes)
        {
            // NOTE: The supplied serviceTypes can be opened, partially-closed, closed, non-generic or even
            // abstract.
            var controlledItems = serviceTypes.Select(ContainerControlledItem.CreateFromType).ToArray();
            this.RegisterCollectionInternal(itemType, controlledItems);
        }

        private void RegisterCollectionInternal(Type itemType, ContainerControlledItem[] controlledItems, 
            bool appending = false)
        {
            this.ThrowWhenContainerIsLockedOrDisposed();

            this.RegisterGenericContainerControlledCollection(itemType, controlledItems, appending);
        }

        private void RegisterGenericContainerControlledCollection(Type itemType,
            ContainerControlledItem[] controlledItems, bool appending)
        {
            CollectionResolver resolver = this.GetContainerControlledResolver(itemType);

            resolver.AddControlledRegistrations(itemType, controlledItems, append: appending);
        }

        private void RegisterGenericContainerUncontrolledCollection(Type itemType, IEnumerable collection)
        {
            var resolver = this.GetContainerUncontrolledResolver(itemType);

            var producer = SingletonLifestyle.CreateUncontrolledCollectionProducer(itemType, collection, this);

            resolver.RegisterUncontrolledCollection(itemType, producer);
        }

        private void RegisterContainerUncontrolledCollection<T>(Type itemType,
            IEnumerable<T> containerUncontrolledCollection)
        {
            IEnumerable readOnlyCollection = containerUncontrolledCollection.MakeReadOnly();
            IEnumerable castedCollection = Helpers.CastCollection(readOnlyCollection, itemType);

            this.RegisterGenericContainerUncontrolledCollection(itemType, castedCollection);
        }

        private CollectionResolver GetContainerControlledResolver(Type itemType)
        {
            return this.GetCollectionResolver(itemType, containerControlled: true);
        }

        private CollectionResolver GetContainerUncontrolledResolver(Type itemType)
        {
            return this.GetCollectionResolver(itemType, containerControlled: false);
        }

        private CollectionResolver GetCollectionResolver(Type itemType, bool containerControlled)
        {
            Type key = GetRegistrationKey(itemType);

            return this.collectionResolvers.GetValueOrDefault(key)
                ?? this.CreateAndAddCollectionResolver(key, containerControlled);
        }

        private CollectionResolver CreateAndAddCollectionResolver(Type openGenericServiceType, bool controlled)
        {
            var resolver = controlled
                ? (CollectionResolver)new ContainerControlledCollectionResolver(this, openGenericServiceType)
                : (CollectionResolver)new ContainerUncontrolledCollectionResolver(this, openGenericServiceType);

            this.collectionResolvers.Add(openGenericServiceType, resolver);

            this.ResolveUnregisteredType += resolver.ResolveUnregisteredType;
            this.Verifying += resolver.TriggerUnregisteredTypeResolutionOnAllClosedCollections;

            return resolver;
        }
    }
}