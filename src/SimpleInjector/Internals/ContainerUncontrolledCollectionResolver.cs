// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    // This resolver allows multiple registrations for the same generic type to be combined in case the generic
    // type is variant.
    internal sealed class ContainerUncontrolledCollectionResolver : CollectionResolver
    {
        internal ContainerUncontrolledCollectionResolver(Container container, Type openGenericServiceType)
            : base(container, openGenericServiceType)
        {
        }

        internal override void AddControlledRegistrations(
            Type serviceType, ContainerControlledItem[] registrations, bool append)
        {
            if (append)
            {
                throw new NotSupportedException(
                    StringResources.AppendingRegistrationsToContainerUncontrolledCollectionsIsNotSupported(
                        serviceType));
            }
            else
            {
                throw new NotSupportedException(
                    StringResources.MixingRegistrationsWithControlledAndUncontrolledIsNotSupported(
                        serviceType, controlled: true));
            }
        }

        internal override void RegisterUncontrolledCollection(Type serviceType, InstanceProducer producer)
        {
            this.AddRegistrationGroup(RegistrationGroup.CreateForUncontrolledProducer(serviceType, producer));
        }

        protected override InstanceProducer BuildCollectionProducer(Type closedServiceType)
        {
            InstanceProducer[] producers = this.GetAssignableProducers(closedServiceType);

            return producers.Length <= 1
                ? producers.FirstOrDefault()
                : this.CombineProducersToOne(closedServiceType, producers);
        }

        protected override Type[] GetAllKnownClosedServiceTypes()
        {
            var closedServiceTypes =
                from registrationGroup in this.RegistrationGroups
                select registrationGroup.ServiceType;

            return closedServiceTypes.Distinct().ToArray();
        }

        private InstanceProducer[] GetAssignableProducers(Type closedServiceType)
        {
            var producers =
                from registrationGroup in this.RegistrationGroups
                where closedServiceType.IsAssignableFrom(registrationGroup.ServiceType)
                select registrationGroup.UncontrolledProducer;

            return producers.ToArray();
        }

        private InstanceProducer CombineProducersToOne(Type closedServiceType, InstanceProducer[] producers)
        {
            IEnumerable instanceStream =
                from producer in producers
                from instances in (IEnumerable<object>)producer.GetInstance()
                select instances;

            instanceStream = Helpers.CastCollection(instanceStream, closedServiceType);

            var registration = SingletonLifestyle.CreateUncontrolledCollectionRegistration(
                closedServiceType,
                instanceStream,
                this.Container);

            Type collectionType = typeof(IEnumerable<>).MakeGenericType(closedServiceType);

            return new InstanceProducer(collectionType, registration);
        }
    }
}