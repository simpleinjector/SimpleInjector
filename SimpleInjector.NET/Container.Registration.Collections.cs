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
    using SimpleInjector.Advanced;
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
            Requires.IsNotAnAmbiguousType(typeof(TService), "TService");
            Requires.IsNotNull(containerUncontrolledCollection, "containerUncontrolledCollection");

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
            Requires.IsNotNull(singletons, "singletons");
            Requires.DoesNotContainNullValues(singletons, "singletons");

            if (typeof(TService) == typeof(Type) && singletons.Any())
            {
                throw new ArgumentException(
                    StringResources.RegisterCollectionCalledWithTypeAsTService(singletons.Cast<Type>()),
                    "TService");
            }

            Requires.IsNotAnAmbiguousType(typeof(TService), "TService");

            var registrations =
                from singleton in singletons
                select SingletonLifestyle.CreateSingleInstanceRegistration(typeof(TService), singleton, this,
                    singleton.GetType());

            this.RegisterCollection(typeof(TService), registrations);
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
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(serviceTypes, "serviceTypes");

            // Make a copy for correctness and performance.
            serviceTypes = serviceTypes.ToArray();

            Requires.DoesNotContainNullValues(serviceTypes, "serviceTypes");
            Requires.ServiceIsAssignableFromImplementations(serviceType, serviceTypes, "serviceTypes",
                typeCanBeServiceType: true);
            Requires.DoesNotContainOpenGenericTypesWhenServiceTypeIsNotGeneric(serviceType, serviceTypes,
                "serviceTypes");
            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(serviceType, serviceTypes, "serviceTypes");

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
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(registrations, "registrations");

            // Make a copy for performance and correctness.
            registrations = registrations.ToArray();

            Requires.DoesNotContainNullValues(registrations, "registrations");
            Requires.AreRegistrationsForThisContainer(this, registrations, "registrations");
            Requires.ServiceIsAssignableFromImplementations(serviceType, registrations, "registrations", typeCanBeServiceType: true);
            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(serviceType, registrations, "registrations");

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
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(containerUncontrolledCollection, "containerUncontrolledCollection");
            Requires.IsNotOpenGenericType(serviceType, "serviceType");
            Requires.IsNotAnAmbiguousType(serviceType, "serviceType");

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
                    "\nparamName: " + "serviceType", ex);
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
            this.RegisterCollectionInternal(itemType,
                registrations.Select(ContainerControlledItem.CreateFromRegistration).ToArray());
        }

        private void RegisterCollectionInternal(Type itemType, IEnumerable<Type> serviceTypes)
        {
            // NOTE: The supplied serviceTypes can be opened, partially-closed, closed, non-generic or even
            // abstract.
            var controlledItems = serviceTypes.Select(ContainerControlledItem.CreateFromType).ToArray();
            this.RegisterCollectionInternal(itemType, controlledItems);
        }

        private void RegisterCollectionInternal(Type itemType, ContainerControlledItem[] registrations,
            bool appending = false)
        {
            this.ThrowWhenContainerIsLocked();

            if (itemType.IsGenericType)
            {
                this.RegisterGenericContainerControlledCollection(itemType, registrations, appending);
            }
            else
            {
                this.RegisterNonGenericCollection(itemType, registrations, appending);
            }
        }

        private void RegisterGenericContainerControlledCollection(Type itemType, 
            ContainerControlledItem[] registrations, bool appending)
        {
            CollectionResolver resolver = this.GetContainerControlledResolver(itemType);

            resolver.AddControlledRegistrations(itemType, registrations,
                append: appending,
                allowOverridingRegistrations: this.Options.AllowOverridingRegistrations);
        }

        private void RegisterNonGenericCollection(Type itemType, ContainerControlledItem[] registrations,
            bool appending)
        {
            if (appending)
            {
                this.AppendToNonGenericCollection(itemType, registrations);
            }
            else
            {
                this.RegisterContainerControlledCollection(itemType, registrations);
            }
        }

        private void AppendToNonGenericCollection(Type itemType, ContainerControlledItem[] registrations)
        {
            Type enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(itemType);
            bool collectionRegistered = this.producers.ContainsKey(enumerableServiceType);

            if (collectionRegistered)
            {
                this.AppendToExistingNonGenericCollection(itemType, registrations);
            }
            else
            {
                this.RegisterContainerControlledCollection(itemType, registrations);
            }
        }

        private void AppendToExistingNonGenericCollection(Type itemType, ContainerControlledItem[] registrations)
        {
            Type enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);

            var producer = this.producers[enumerableType];

            IContainerControlledCollection instance =
                DecoratorHelpers.ExtractContainerControlledCollectionFromRegistration(producer.Registration);

            if (instance == null)
            {
                ThrowAppendingRegistrationsToContainerUncontrolledCollectionsIsNotSupported(itemType);
            }

            instance.AppendAll(registrations);
        }

        private void RegisterGenericContainerUncontrolledCollection(Type itemType, IEnumerable collection)
        {
            var resolver = this.GetContainerUncontrolledResolver(itemType);

            resolver.RegisterUncontrolledCollection(itemType, collection, 
                allowOverridingRegistrations: this.Options.AllowOverridingRegistrations);
        }

        private void RegisterContainerControlledCollection(Type serviceType,
            ContainerControlledItem[] registrations)
        {
            IContainerControlledCollection collection =
                DecoratorHelpers.CreateContainerControlledCollection(serviceType, this);

            collection.AppendAll(registrations);

            this.RegisterContainerControlledCollection(serviceType, collection);
        }
        
        private void RegisterContainerControlledCollection(Type itemType, 
            IContainerControlledCollection collection)
        {
            this.ThrowWhenCollectionTypeAlreadyRegistered(itemType);

            Registration registration = 
                DecoratorHelpers.CreateRegistrationForContainerControlledCollection(itemType, collection, this);

            Type enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);

            this.AddRegistration(enumerableType, registration);
        }

        private void RegisterContainerUncontrolledCollection<T>(Type itemType, 
            IEnumerable<T> containerUncontrolledCollection)
        {
            this.ThrowWhenCollectionTypeAlreadyRegistered(itemType);

            IEnumerable readOnlyCollection = containerUncontrolledCollection.MakeReadOnly();
            IEnumerable castedCollection = Helpers.CastCollection(readOnlyCollection, itemType);

            if (itemType.IsGenericType)
            {
                this.RegisterGenericContainerUncontrolledCollection(itemType, containerUncontrolledCollection);
            }
            else
            {
                var registration =
                    SingletonLifestyle.CreateUncontrolledCollectionRegistration(itemType, castedCollection, this);

                Type enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);

                this.AddRegistration(enumerableType, registration);
            }
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
            CollectionResolver resolver;

            Type openGenericServiceType = itemType.GetGenericTypeDefinition();

            if (!this.collectionResolvers.TryGetValue(openGenericServiceType, out resolver))
            {
                this.ThrowWhenTypeAlreadyRegistered(typeof(IEnumerable<>).MakeGenericType(itemType));

                resolver = CollectionResolver.Create(this, openGenericServiceType, containerControlled);

                this.ResolveUnregisteredType += resolver.ResolveUnregisteredType;
                this.Verifying += resolver.TriggerUnregisteredTypeResolutionOnAllClosedCollections;

                this.collectionResolvers.Add(openGenericServiceType, resolver);
            }

            return resolver;
        }

        private void ThrowWhenCollectionTypeAlreadyRegistered(Type itemType)
        {
            if (!this.Options.AllowOverridingRegistrations &&
                this.producers.ContainsKey(typeof(IEnumerable<>).MakeGenericType(itemType)))
            {
                throw new InvalidOperationException(
                    StringResources.CollectionTypeAlreadyRegistered(itemType));
            }
        }

        private static void ThrowAppendingRegistrationsToContainerUncontrolledCollectionsIsNotSupported(
            Type serviceType)
        {
            string exceptionMessage =
                StringResources.AppendingRegistrationsToContainerUncontrolledCollectionsIsNotSupported(
                    serviceType);

            throw new NotSupportedException(exceptionMessage);
        }
    }
}