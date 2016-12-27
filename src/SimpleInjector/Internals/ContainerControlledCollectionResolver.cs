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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
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

        internal override void AddControlledRegistrations(Type serviceType,
            ContainerControlledItem[] registrations, bool append)
        {
            var group = RegistrationGroup.CreateForControlledItems(serviceType, registrations, append);
            this.AddRegistrationGroup(group);
        }

        protected override InstanceProducer BuildCollectionProducer(Type closedServiceType)
        {
            ContainerControlledItem[] closedGenericImplementations =
                this.GetClosedContainerControlledItemsFor(closedServiceType);

            IContainerControlledCollection collection = DecoratorHelpers.CreateContainerControlledCollection(
                closedServiceType, this.Container);

            collection.AppendAll(closedGenericImplementations);

            return new InstanceProducer(
                typeof(IEnumerable<>).MakeGenericType(closedServiceType),
                DecoratorHelpers.CreateRegistrationForContainerControlledCollection(closedServiceType,
                    collection, this.Container));
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