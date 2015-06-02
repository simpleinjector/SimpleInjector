#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014-2015 Simple Injector Contributors
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

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector;
    using SimpleInjector.Decorators;
    using SimpleInjector.Lifestyles;

    internal abstract class CollectionResolver
    {
        private readonly List<RegistrationGroup> registrationGroups;
        private readonly Dictionary<Type, Registration> lifestyleRegistrationCache =
            new Dictionary<Type, Registration>();

        private bool verified;

        protected CollectionResolver(Container container, Type openGenericServiceType)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(openGenericServiceType, "openGenericServiceType");

            this.Container = container;
            this.OpenGenericServiceType = openGenericServiceType;
            this.registrationGroups = new List<RegistrationGroup>();
        }

        protected Type OpenGenericServiceType { get; private set; }

        protected Container Container { get; private set; }

        internal static CollectionResolver Create(Container container, Type openGenericServiceType,
            bool containerControlled)
        {
            return containerControlled
                ? (CollectionResolver)new ContainerControlledCollectionResolver(container, openGenericServiceType)
                : (CollectionResolver)new ContainerUncontrolledCollectionResolver(container, openGenericServiceType);
        }

        internal virtual void AddControlledRegistrations(Type serviceType,
            ContainerControlledItem[] registrations, bool append, bool allowOverridingRegistrations)
        {
            throw new InvalidOperationException(
                StringResources.MixingRegistrationsWithControlledAndUncontrolledIsNotSupported(serviceType,
                    controlled: true));
        }

        internal virtual void RegisterUncontrolledCollection(Type serviceType, IEnumerable collection,
            bool allowOverridingRegistrations)
        {
            throw new InvalidOperationException(
                StringResources.MixingRegistrationsWithControlledAndUncontrolledIsNotSupported(serviceType,
                    controlled: false));
        }

        internal void ResolveUnregisteredType(object sender, UnregisteredTypeEventArgs e)
        {
            if (typeof(IEnumerable<>).IsGenericTypeDefinitionOf(e.UnregisteredServiceType))
            {
                Type closedServiceType = e.UnregisteredServiceType.GetGenericArguments().Single();

                if (this.OpenGenericServiceType.IsGenericTypeDefinitionOf(closedServiceType))
                {
                    Registration registration;

                    if (this.TryGetContainerControlledRegistrationFromCache(closedServiceType, out registration))
                    {
                        e.Register(registration);
                    }
                }
            }
        }

        internal void TriggerUnregisteredTypeResolutionOnAllClosedCollections()
        {
            if (!this.verified)
            {
                this.verified = true;

                foreach (Type closedServiceType in this.GetAllKnownClosedServiceTypes())
                {
                    // When registering a generic collection, the container keeps track of all open and closed
                    // elements in the resolver. This resolver allows unregistered type resolution and this 
                    // allows all closed versions of the collection to be resolved. But if we only used 
                    // unregistered type resolution, this could cause these registrations to be hidden from 
                    // the verification mechanism in case the collections are root types in the application. 
                    // This could cause the container to verify, while still failing at runtime when resolving 
                    // a collection. So by explicitly resolving the known closed-generic versions here, we
                    // ensure that all non-generic registrations (and because of that, most open-generic 
                    // registrations as well) will be validated.
                    this.Container.GetRegistration(typeof(IEnumerable<>).MakeGenericType(closedServiceType));
                }
            }
        }

        protected abstract Type[] GetAllKnownClosedServiceTypes();

        protected abstract Registration BuildCollectionRegistration(Type closedServiceType);

        protected void AddRegistrationGroup(bool allowOverridingRegistrations, RegistrationGroup group)
        {
            if (!group.Appended)
            {
                if (allowOverridingRegistrations)
                {
                    this.RemoveRegistrationsToOverride(group.ServiceType);
                }

                this.CheckForOverlappingRegistrations(group.ServiceType);
            }

            this.registrationGroups.Add(group);
        }

        private bool TryGetContainerControlledRegistrationFromCache(Type closedServiceType,
            out Registration registration)
        {
            lock (this.lifestyleRegistrationCache)
            {
                if (!this.lifestyleRegistrationCache.TryGetValue(closedServiceType, out registration))
                {
                    registration = this.BuildCollectionRegistration(closedServiceType);

                    if (registration != null)
                    {
                        this.lifestyleRegistrationCache[closedServiceType] = registration;
                    }
                }

                // If there are no implementations, no registration need to be made.
                return registration != null;
            }
        }

        private void RemoveRegistrationsToOverride(Type serviceType)
        {
            this.registrationGroups.RemoveAll(group => group.ServiceType == serviceType || group.Appended);
        }

        private void CheckForOverlappingRegistrations(Type serviceType)
        {
            var overlappingGroups = this.GetOverlappingGroupsFor(serviceType);

            if (overlappingGroups.Any())
            {
                if (!serviceType.ContainsGenericParameters &&
                    overlappingGroups.Any(group => group.ServiceType == serviceType))
                {
                    throw new InvalidOperationException(
                        StringResources.CollectionTypeAlreadyRegistered(serviceType));
                }

                throw new InvalidOperationException(
                    StringResources.MixingCallsToRegisterCollectionIsNotSupported(serviceType));
            }
        }

        private IEnumerable<RegistrationGroup> GetOverlappingGroupsFor(Type serviceType)
        {
            return
                from registrationGroup in this.registrationGroups
                where !registrationGroup.Appended
                where registrationGroup.ServiceType == serviceType
                    || serviceType.ContainsGenericParameters
                    || registrationGroup.ServiceType.ContainsGenericParameters
                select registrationGroup;
        }

        protected sealed class RegistrationGroup
        {
            internal Type ServiceType { get; set; }

            internal ContainerControlledItem[] ControlledItems { get; set; }

            internal IEnumerable UncontrolledCollection { get; set; }

            internal bool Appended { get; set; }
        }

        // This class is similar to the OpenGenericRegistrationExtensions.UnregisteredAllOpenGenericResolver class
        // (which is used by RegisterAllOpenGeneric), but this class (used by RegisterCollection) behaves differently. 
        // This implementation forwards requests for types back to the container (using the 
        // ContainerControlledCollection<T>) to allow lifestyles of individual registrations to be overridden and 
        // this allows abstractions to be used as types. This isn't supported by RegisterAllOpenGeneric and 
        // changing this would be a breaking change. That's why we need both of them.
        private sealed class ContainerControlledCollectionResolver : CollectionResolver
        {
            internal ContainerControlledCollectionResolver(Container container, Type openGenericServiceType)
                : base(container, openGenericServiceType)
            {
            }

            internal override void AddControlledRegistrations(Type serviceType,
                ContainerControlledItem[] registrations, bool append, bool allowOverridingRegistrations)
            {
                this.AddRegistrationGroup(allowOverridingRegistrations, new RegistrationGroup
                {
                    ServiceType = serviceType,
                    ControlledItems = registrations,
                    Appended = append
                });
            }

            protected override Registration BuildCollectionRegistration(Type closedServiceType)
            {
                ContainerControlledItem[] closedGenericImplementations =
                    this.GetClosedContainerControlledItemsFor(closedServiceType);

                IContainerControlledCollection collection = DecoratorHelpers.CreateContainerControlledCollection(
                    closedServiceType, this.Container);

                collection.AppendAll(closedGenericImplementations);

                return DecoratorHelpers.CreateRegistrationForContainerControlledCollection(closedServiceType,
                    collection, this.Container);
            }

            protected override Type[] GetAllKnownClosedServiceTypes()
            {
                var closedServiceTypes =
                    from registrationGroup in this.registrationGroups
                    from item in registrationGroup.ControlledItems
                    let implementation = item.ImplementationType
                    where !implementation.ContainsGenericParameters
                    from service in implementation.GetBaseTypesAndInterfacesFor(this.OpenGenericServiceType)
                    select service;

                return closedServiceTypes.Distinct().ToArray();
            }

            private ContainerControlledItem[] GetClosedContainerControlledItemsFor(Type closedGenericServiceType)
            {
                return Helpers.GetClosedGenericImplementationsFor(closedGenericServiceType,
                    this.GetItemsFor(closedGenericServiceType));
            }

            private IEnumerable<ContainerControlledItem> GetItemsFor(Type closedGenericServiceType)
            {
                return
                    from registrationGroup in this.registrationGroups
                    where registrationGroup.ServiceType.ContainsGenericParameters ||
                        closedGenericServiceType.IsAssignableFrom(registrationGroup.ServiceType)
                    from item in registrationGroup.ControlledItems
                    select item;
            }
        }

        // This resolver allows multiple registrations for the same generic type to be combined in case the generic
        // type is variant.
        private sealed class ContainerUncontrolledCollectionResolver : CollectionResolver
        {
            internal ContainerUncontrolledCollectionResolver(Container container, Type openGenericServiceType)
                : base(container, openGenericServiceType)
            {
            }

            internal override void RegisterUncontrolledCollection(Type serviceType, IEnumerable collection,
                bool allowOverridingRegistrations)
            {
                this.AddRegistrationGroup(allowOverridingRegistrations, new RegistrationGroup
                {
                    ServiceType = serviceType,
                    UncontrolledCollection = collection,
                });
            }

            protected override Registration BuildCollectionRegistration(Type closedServiceType)
            {
                var collections = (
                    from registrationGroup in this.registrationGroups
                    where closedServiceType.IsAssignableFrom(registrationGroup.ServiceType)
                    select registrationGroup.UncontrolledCollection)
                    .ToArray();

                if (!collections.Any())
                {
                    return null;
                }

                IEnumerable collection = Helpers.ConcatCollections(closedServiceType, collections);

                return SingletonLifestyle.CreateUncontrolledCollectionRegistration(closedServiceType,
                    collection, this.Container);
            }

            protected override Type[] GetAllKnownClosedServiceTypes()
            {
                var closedServiceTypes =
                    from registrationGroup in this.registrationGroups
                    select registrationGroup.ServiceType;

                return closedServiceTypes.Distinct().ToArray();
            }
        }
    }
}