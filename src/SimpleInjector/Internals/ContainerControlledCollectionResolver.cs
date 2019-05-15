// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector;
    using SimpleInjector.Decorators;

    internal sealed class ContainerControlledCollectionResolver : CollectionResolver
    {
        internal ContainerControlledCollectionResolver(Container container, Type openGenericServiceType)
            : base(container, openGenericServiceType)
        {
        }

        internal override void RegisterUncontrolledCollection(Type serviceType, InstanceProducer producer)
        {
            throw new NotSupportedException(
                StringResources.MixingRegistrationsWithControlledAndUncontrolledIsNotSupported(serviceType,
                    controlled: false));
        }

        internal override void AddControlledRegistrations(
            Type serviceType, ContainerControlledItem[] registrations, bool append)
        {
            var group = RegistrationGroup.CreateForControlledItems(serviceType, registrations, append);
            this.AddRegistrationGroup(group);
        }

        protected override InstanceProducer BuildCollectionProducer(Type closedServiceType)
        {
            ContainerControlledItem[] closedGenericImplementations =
                this.GetClosedContainerControlledItemsFor(closedServiceType);

            IContainerControlledCollection collection =
                ControlledCollectionHelper.CreateContainerControlledCollection(
                    closedServiceType, this.Container);

            collection.AppendAll(closedGenericImplementations);

            var collectionType = typeof(IEnumerable<>).MakeGenericType(closedServiceType);

            return new InstanceProducer(
                serviceType: collectionType,
                registration: collection.CreateRegistration(collectionType, this.Container));
        }

        protected override Type[] GetAllKnownClosedServiceTypes() => (
            from registrationGroup in this.RegistrationGroups
            from item in registrationGroup.ControlledItems
            let implementation = item.ImplementationType
            where !implementation.ContainsGenericParameters()
            from service in implementation.GetBaseTypesAndInterfacesFor(this.ServiceType)
            select service)
            .Distinct()
            .ToArray();

        private ContainerControlledItem[] GetClosedContainerControlledItemsFor(Type serviceType)
        {
            var items = this.GetItemsFor(serviceType);

            return serviceType.IsGenericType()
                ? Types.GetClosedGenericImplementationsFor(serviceType, items)
                : items.ToArray();
        }

        private IEnumerable<ContainerControlledItem> GetItemsFor(Type closedGenericServiceType) =>
            from registrationGroup in this.RegistrationGroups
            where registrationGroup.ServiceType.ContainsGenericParameters() ||
                closedGenericServiceType.IsAssignableFrom(registrationGroup.ServiceType)
            from item in registrationGroup.ControlledItems
            select item;
    }
}