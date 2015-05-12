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
    using System.Linq;
    using System.Reflection;
    using SimpleInjector.Advanced;
    using SimpleInjector.Extensions;
    using SimpleInjector.Extensions.Decorators;
    using SimpleInjector.Lifestyles;

    /// <summary>Defines the accessibility of the types to search.</summary>
    public enum AccessibilityOption
    {
        /// <summary>Load both public as internal types from the given assemblies.</summary>
        AllTypes = 0,

        /// <summary>Only load publicly exposed types from the given assemblies.</summary>
        PublicTypesOnly = 1,
    }

#if !PUBLISH
    /// <summary>Methods for registration of collections.</summary>
#endif
    public partial class Container
    {
        /// <summary>
        /// Registers a dynamic (container uncontrolled) collection of elements of type 
        /// <typeparamref name="TService"/>. A call to <see cref="GetAllInstances{T}"/> will return the 
        /// <paramref name="collection"/> itself, and updates to the collection will be reflected in the 
        /// result. If updates are allowed, make sure the collection can be iterated safely if you're running 
        /// a multi-threaded application.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="collection">The collection to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when a <paramref name="collection"/>
        /// for <typeparamref name="TService"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is a null
        /// reference.</exception>
        public void RegisterAll<TService>(IEnumerable<TService> collection) where TService : class
        {
            Requires.IsNotAnAmbiguousType(typeof(TService), "TService");
            Requires.IsNotNull(collection, "collection");

            this.ThrowWhenCollectionTypeAlreadyRegistered(typeof(TService));

            var readOnlyCollection = collection.MakeReadOnly();

            var registration = Lifestyle.Singleton.CreateRegistration(
                typeof(IEnumerable<TService>), () => readOnlyCollection, this);

            registration.IsCollection = true;

            // This is a container uncontrolled collection
            this.AddRegistration(typeof(IEnumerable<TService>), registration);
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
        public void RegisterAll<TService>(params TService[] singletons) where TService : class
        {
            Requires.IsNotAnAmbiguousType(typeof(TService), "TService");
            Requires.IsNotNull(singletons, "singletons");
            Requires.DoesNotContainNullValues(singletons, "singletons");

            var collection = DecoratorHelpers.CreateContainerControlledCollection(typeof(TService), this);

            collection.AppendAll(
                from instance in singletons
                select SingletonLifestyle.CreateSingleRegistration(typeof(TService), instance, this,
                    instance.GetType()));

            this.RegisterContainerControlledCollection(typeof(TService), collection);
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
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <typeparamref name="TService"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        public void RegisterAll<TService>(params Type[] serviceTypes) where TService : class
        {
            this.RegisterAll(typeof(TService), serviceTypes);
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
        public void RegisterAll<TService>(IEnumerable<Type> serviceTypes) where TService : class
        {
            this.RegisterAll(typeof(TService), serviceTypes);
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
        public void RegisterAll(Type serviceType, IEnumerable<Type> serviceTypes)
        {
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(serviceTypes, "serviceTypes");

            // Make a copy for correctness and performance.
            Type[] serviceTypesCopy = serviceTypes.ToArray();

            Requires.DoesNotContainNullValues(serviceTypesCopy, "serviceTypes");
            Requires.ServiceIsAssignableFromImplementations(serviceType, serviceTypesCopy, "serviceTypes",
                typeCanBeServiceType: true);
            Requires.DoesNotContainOpenGenericTypesWhenServiceTypeIsNotGeneric(serviceType, serviceTypesCopy,
                "serviceTypes");
            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(serviceType, serviceTypesCopy, "serviceTypes");

            this.RegisterCollectionInternal(serviceType, serviceTypesCopy);
        }

        /// <summary>
        /// Registers a collection of <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the resolved collection of <paramref name="serviceType"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// registered, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="registrations"/> contains a null
        /// (Nothing in VB) element, the <paramref name="serviceType"/> is a generic type definition, or when 
        /// <paramref name="serviceType"/> is
        /// not assignable from one of the given <paramref name="registrations"/> elements.
        /// </exception>
        public void RegisterAll(Type serviceType, IEnumerable<Registration> registrations)
        {
            Requires.IsNotNull(registrations, "registrations");

            this.RegisterAll(serviceType, registrations.ToArray());
        }

        /// <summary>
        /// Registers a collection of <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the resolved collection of <paramref name="serviceType"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// registered, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="registrations"/> contains a null
        /// (Nothing in VB) element, the <paramref name="serviceType"/> is a generic type definition, or when 
        /// <paramref name="serviceType"/> is
        /// not assignable from one of the given <paramref name="registrations"/> elements.
        /// </exception>
        public void RegisterAll(Type serviceType, params Registration[] registrations)
        {
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(registrations, "registrations");
            Requires.IsNotOpenGenericType(serviceType, "serviceType");

            // Make a copy for correctness.
            registrations = registrations.ToArray();

            Requires.DoesNotContainNullValues(registrations, "registrations");
            Requires.AreRegistrationsForThisContainer(this, registrations, "registrations");
            Requires.ServiceIsAssignableFromImplementations(serviceType, registrations, "registrations",
                typeCanBeServiceType: true);
            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(serviceType, registrations, "registrations");

            this.RegisterCollectionInternal(serviceType, registrations);
        }

        /// <summary>
        /// Registers a dynamic (container uncontrolled) collection of elements of type 
        /// <paramref name="serviceType"/>. A call to <see cref="GetAllInstances{T}"/> will return the 
        /// <paramref name="collection"/> itself, and updates to the collection will be reflected in the 
        /// result. If updates are allowed, make sure the collection can be iterated safely if you're running 
        /// a multi-threaded application.
        /// </summary>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="collection">The collection of items to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null 
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> represents an
        /// open generic type.</exception>
        public void RegisterAll(Type serviceType, IEnumerable collection)
        {
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(collection, "collection");
            Requires.IsNotOpenGenericType(serviceType, "serviceType");
            Requires.IsNotAnAmbiguousType(serviceType, "serviceType");

            try
            {
                this.RegisterAllInternal(serviceType, collection);
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                throw new ArgumentException(
                    StringResources.UnableToResolveTypeDueToSecurityConfiguration(serviceType, ex) +
                    "\nparamName: " + "serviceType", ex);
            }
        }

        /// <summary>
        /// Registers all concrete, non-generic types (both public and internal) that are defined in the given
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
        public void RegisterAll<TService>(params Assembly[] assemblies) where TService : class
        {
            this.RegisterAll(typeof(TService), (IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Registers all concrete, non-generic types (both public and internal) that are defined in the given
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
        public void RegisterAll<TService>(IEnumerable<Assembly> assemblies) where TService : class
        {
            this.RegisterAll(typeof(TService), assemblies);
        }

        /// <summary>
        /// Registers all concrete, non-generic types that match the given <paramref name="accessibility"/> 
        /// that are defined in the given set of <paramref name="assemblies"/> and that implement the given 
        /// <typeparamref name="TService"/> with a default lifestyle and register them as a collection of 
        /// <typeparamref name="TService"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// </summary>
        /// <typeparam name="TService">The element type of the collections to register. This can be either
        /// a non-generic, closed-generic or open-generic type.</typeparam>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        public void RegisterAll<TService>(AccessibilityOption accessibility, params Assembly[] assemblies)
            where TService : class
        {
            this.RegisterAll(typeof(TService), accessibility, (IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Registers all concrete, non-generic types that match the given <paramref name="accessibility"/> 
        /// that are defined in the given set of <paramref name="assemblies"/> and that implement the given 
        /// <typeparamref name="TService"/> with a default lifestyle and register them as a collection of 
        /// <typeparamref name="TService"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// </summary>
        /// <typeparam name="TService">The element type of the collections to register. This can be either
        /// a non-generic, closed-generic or open-generic type.</typeparam>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        public void RegisterAll<TService>(AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
            where TService : class
        {
            this.RegisterAll(typeof(TService), accessibility, assemblies);
        }

        /// <summary>
        /// Registers all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <paramref name="serviceType"/> 
        /// with a default lifestyle and register them as a collection of <paramref name="serviceType"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// </summary>
        /// <param name="serviceType">The element type of the collections to register. This can be either
        /// a non-generic, closed-generic or open-generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        public void RegisterAll(Type serviceType, params Assembly[] assemblies)
        {
            this.RegisterAll(serviceType, (IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Registers all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <paramref name="serviceType"/> 
        /// with a default lifestyle and register them as a collection of <paramref name="serviceType"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// </summary>
        /// <param name="serviceType">The element type of the collections to register. This can be either
        /// a non-generic, closed-generic or open-generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        public void RegisterAll(Type serviceType, IEnumerable<Assembly> assemblies)
        {
            var types = this.GetTypesToRegisterInternal(serviceType, AccessibilityOption.AllTypes, assemblies);

            this.RegisterAll(serviceType, types);
        }

        /// <summary>
        /// Registers all concrete, non-generic types that match the given <paramref name="accessibility"/> 
        /// that are defined in the given set of <paramref name="assemblies"/> and that implement the given 
        /// <paramref name="serviceType"/> with a default lifestyle and register them as a collection of 
        /// <paramref name="serviceType"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// </summary>
        /// <param name="serviceType">The element type of the collections to register. This can be either
        /// a non-generic, closed-generic or open-generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        public void RegisterAll(Type serviceType, AccessibilityOption accessibility, params Assembly[] assemblies)
        {
            this.RegisterAll(serviceType, accessibility, (IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Registers all concrete, non-generic types that match the given <paramref name="accessibility"/> 
        /// that are defined in the given set of <paramref name="assemblies"/> and that implement the given 
        /// <paramref name="serviceType"/> with a default lifestyle and register them as a collection of 
        /// <paramref name="serviceType"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// </summary>
        /// <param name="serviceType">The element type of the collections to register. This can be either
        /// a non-generic, closed-generic or open-generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        public void RegisterAll(Type serviceType, AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            var types = this.GetTypesToRegisterInternal(serviceType, accessibility, assemblies);

            this.RegisterAll(serviceType, types);
        }
        
        // This method is internal to prevent the main API of the framework from being 'polluted'. The
        // SimpleInjector.Advanced.AdvancedExtensions.AppendToCollection extension method enabled public
        // exposure.
        internal void AppendToCollectionInternal(Type serviceType, Registration registration)
        {
            this.RegisterCollectionInternal(serviceType,
                new[] { new ContainerControlledItem(registration) },
                appending: true);
        }

        internal void AppendToCollectionInternal(Type serviceType, Type implementationType)
        {
            // NOTE: The supplied serviceTypes can be opened, partially-closed, closed, non-generic or even
            // abstract.
            this.RegisterCollectionInternal(serviceType,
                new[] { new ContainerControlledItem(implementationType) },
                appending: true);
        }

        private void RegisterCollectionInternal(Type serviceType, Registration[] registrations)
        {
            this.RegisterCollectionInternal(serviceType,
                registrations.Select(r => new ContainerControlledItem(r)).ToArray());
        }

        private void RegisterCollectionInternal(Type serviceType, Type[] serviceTypes)
        {
            // NOTE: The supplied serviceTypes can be opened, partially-closed, closed, non-generic or even
            // abstract.
            this.RegisterCollectionInternal(serviceType,
                serviceTypes.Select(type => new ContainerControlledItem(type)).ToArray());
        }

        private void RegisterCollectionInternal(Type serviceType, ContainerControlledItem[] registrations,
            bool appending = false)
        {
            this.ThrowWhenContainerIsLocked();

            if (serviceType.IsGenericType)
            {
                this.RegisterGenericCollection(serviceType, registrations, appending);
            }
            else
            {
                this.RegisterNonGenericCollection(serviceType, registrations, appending);
            }
        }

        private void RegisterGenericCollection(Type serviceType, ContainerControlledItem[] registrations,
            bool appending)
        {
            ContainerControlledCollectionResolver resolver = this.GetUnregisteredAllResolver(serviceType);

            resolver.AddRegistrations(serviceType, registrations,
                append: appending,
                allowOverridingRegistrations: this.Options.AllowOverridingRegistrations);
        }

        private ContainerControlledCollectionResolver GetUnregisteredAllResolver(Type serviceType)
        {
            ContainerControlledCollectionResolver resolver;

            Type openGenericServiceType = serviceType.GetGenericTypeDefinition();

            if (!this.registerAllResolvers.TryGetValue(openGenericServiceType, out resolver))
            {
                this.ThrowWhenTypeAlreadyRegistered(typeof(IEnumerable<>).MakeGenericType(serviceType));

                resolver = new ContainerControlledCollectionResolver(this, openGenericServiceType);

                this.ResolveUnregisteredType += resolver.ResolveUnregisteredType;
                this.Verifying += resolver.TriggerUnregisteredTypeResolutionOnAllClosedCollections;

                this.registerAllResolvers.Add(openGenericServiceType, resolver);
            }

            return resolver;
        }

        private void RegisterNonGenericCollection(Type serviceType, ContainerControlledItem[] registrations,
            bool appending)
        {
            if (appending)
            {
                this.AppendToNonGenericCollection(serviceType, registrations);
            }
            else
            {
                this.RegisterContainerControlledCollection(serviceType, registrations);
            }
        }

        private void AppendToNonGenericCollection(Type serviceType, ContainerControlledItem[] registrations)
        {
            Type enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            bool collectionRegistered = this.producers.ContainsKey(enumerableServiceType);

            if (collectionRegistered)
            {
                this.AppendToExistingNonGenericCollection(serviceType, registrations);
            }
            else
            {
                this.RegisterContainerControlledCollection(serviceType, registrations);
            }
        }

        private void AppendToExistingNonGenericCollection(Type serviceType,
            ContainerControlledItem[] registrations)
        {
            Type enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);

            var producer = this.producers[enumerableServiceType];

            IContainerControlledCollection instance =
                DecoratorHelpers.ExtractContainerControlledCollectionFromRegistration(producer.Registration);

            if (instance == null)
            {
                ThrowAppendingRegistrationsToContainerUncontrolledCollectionsIsNotSupported(serviceType);
            }

            instance.AppendAll(registrations);
        }

        private static void ThrowAppendingRegistrationsToContainerUncontrolledCollectionsIsNotSupported(
            Type serviceType)
        {
            string exceptionMessage =
                StringResources.AppendingRegistrationsToContainerUncontrolledCollectionsIsNotSupported(
                    serviceType);

            throw new NotSupportedException(exceptionMessage);
        }

        private void RegisterContainerControlledCollection(Type serviceType,
            ContainerControlledItem[] registrations)
        {
            IContainerControlledCollection collection =
                DecoratorHelpers.CreateContainerControlledCollection(serviceType, this);

            collection.AppendAll(registrations);

            this.RegisterContainerControlledCollection(serviceType, collection);
        }
        
        private void RegisterContainerControlledCollection(Type serviceType,
            IContainerControlledCollection collection)
        {
            this.ThrowWhenCollectionTypeAlreadyRegistered(serviceType);

            var registration = DecoratorHelpers.CreateRegistrationForContainerControlledCollection(serviceType,
                collection, this);

            this.AddRegistration(typeof(IEnumerable<>).MakeGenericType(serviceType), registration);
        }

        private void RegisterAllInternal(Type serviceType, IEnumerable collection)
        {
            IEnumerable readOnlyCollection = collection.Cast<object>().MakeReadOnly();

            IEnumerable castedCollection = Helpers.CastCollection(readOnlyCollection, serviceType);

            this.ThrowWhenCollectionTypeAlreadyRegistered(serviceType);

            Type enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);

            var registration =
                SingletonLifestyle.CreateSingleRegistration(enumerableServiceType, castedCollection, this);

            registration.IsCollection = true;

            // This is a container-uncontrolled collection
            this.AddRegistration(enumerableServiceType, registration);
        }
        
        private IEnumerable<Type> GetTypesToRegisterInternal(Type serviceType, AccessibilityOption accessibility, 
            IEnumerable<Assembly> assemblies)
        {
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsValidEnum(accessibility, "accessibility");
            Requires.IsNotNull(assemblies, "assemblies");

            bool includeInternals = accessibility == AccessibilityOption.AllTypes;

            return
                from assembly in assemblies.Distinct()
                where !assembly.IsDynamic
                from type in ExtensionHelpers.GetTypesFromAssembly(assembly, includeInternals)
                where ExtensionHelpers.IsConcreteType(type)
                where ExtensionHelpers.ServiceIsAssignableFromImplementation(serviceType, type)
                where !DecoratorHelpers.IsDecorator(this, serviceType, type)
                select type;
        }
    }
}