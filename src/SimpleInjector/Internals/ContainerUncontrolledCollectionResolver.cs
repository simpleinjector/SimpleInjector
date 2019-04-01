#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015 Simple Injector Contributors
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