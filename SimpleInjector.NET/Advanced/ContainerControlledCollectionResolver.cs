#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014 Simple Injector Contributors
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

namespace SimpleInjector.Advanced
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector;
    using SimpleInjector.Extensions;
    using SimpleInjector.Extensions.Decorators;

    // This class is similar to the OpenGenericRegistrationExtensions.UnregisteredAllOpenGenericResolver class
    // (which is used by RegisterAllOpenGeneric), but this class (used by RegisterAll) behaves differently. 
    // This implementation forwards requests for types back to the container (using the 
    // ContainerControlledCollection<T>) to allow lifestyles of individual registrations to be overridden and 
    // this allows abstractions to be used as types. This isn't supported by RegisterAllOpenGeneric and 
    // changing this would be a breaking change. That's why we need both of them.
    internal sealed class ContainerControlledCollectionResolver
    {
        internal readonly Type OpenGenericServiceType;
        internal readonly Container Container;

        private readonly List<RegistrationGroup> registrationGroups = new List<RegistrationGroup>();

        private readonly Dictionary<Type, Registration> lifestyleRegistrationCache =
            new Dictionary<Type, Registration>();

        internal ContainerControlledCollectionResolver(Container container, Type opengenericServiceType)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(opengenericServiceType, "opengenericServiceType");

            this.Container = container;
            this.OpenGenericServiceType = opengenericServiceType;
        }

        internal void AddRegistrations(Type serviceType, ContainerControlledItem[] registrations,
            bool append, bool allowOverridingRegistrations)
        {
            if (!append)
            {
                if (allowOverridingRegistrations)
                {
                    this.RemoveRegistrationsToOverride(serviceType);
                }

                this.CheckForOverlappingRegistrations(serviceType);
            }

            this.registrationGroups.Add(new RegistrationGroup 
            {
                ServiceType = serviceType, 
                Items = registrations,
                Appended = append
            });
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

        internal Type[] GetAllKnownClosedServiceTypes()
        {
            var closedServiceTypes =
                from registrationGroup in this.registrationGroups
                from item in registrationGroup.Items
                let implementation = item.ImplementationType
                where !implementation.ContainsGenericParameters
                from service in implementation.GetBaseTypesAndInterfacesFor(this.OpenGenericServiceType)
                select service;

            return closedServiceTypes.Distinct().ToArray();
        }

        internal Type[] GetClosedGenericImplementationsFor(Type closedGenericServiceType)
        {
            return ExtensionHelpers.GetClosedGenericImplementationsFor(
                closedGenericServiceType,
                from item in this.GetItemsFor(closedGenericServiceType)
                select item.ImplementationType);
        }

        internal ContainerControlledItem[] GetClosedContainerControlledItemsFor(Type closedGenericServiceType)
        {
            // NOTE: Performance bottleneck in 2.7
            return ExtensionHelpers.GetClosedGenericImplementationsFor(closedGenericServiceType,
                this.GetItemsFor(closedGenericServiceType));
        }

        private IEnumerable<ContainerControlledItem> GetItemsFor(Type closedGenericServiceType)
        {
            return
                from registrationGroup in this.registrationGroups
                where registrationGroup.ServiceType.ContainsGenericParameters ||
                    closedGenericServiceType.IsAssignableFrom(registrationGroup.ServiceType)
                from item in registrationGroup.Items
                select item;
        }

        private bool TryGetContainerControlledRegistrationFromCache(Type closedServiceType,
            out Registration registration)
        {
            lock (this.lifestyleRegistrationCache)
            {
                Type[] closedGenericImplementations =
                    this.GetClosedGenericImplementationsFor(closedServiceType);

                if (!this.lifestyleRegistrationCache.TryGetValue(closedServiceType, out registration))
                {
                    registration = this.BuildContainerControlledRegistration(closedServiceType,
                        closedGenericImplementations);

                    this.lifestyleRegistrationCache[closedServiceType] = registration;
                }

                // If there are no implementations, no registration need to be made.
                return closedGenericImplementations.Any();
            }
        }

        private Registration BuildContainerControlledRegistration(Type closedServiceType,
            Type[] closedGenericImplementations)
        {
            IContainerControlledCollection collection = DecoratorHelpers.CreateContainerControlledCollection(
                closedServiceType, this.Container);

            collection.AppendAll(closedGenericImplementations);

            return DecoratorHelpers.CreateRegistrationForContainerControlledCollection(closedServiceType,
                collection, this.Container);
        }

        private void RemoveRegistrationsToOverride(Type serviceType)
        {
            this.registrationGroups.RemoveAll(group => group.ServiceType == serviceType || group.Appended);
        }

        private void CheckForOverlappingRegistrations(Type serviceType)
        {
            var overlappingGroups =
                from registrationGroup in this.registrationGroups
                where !registrationGroup.Appended
                where registrationGroup.ServiceType == serviceType 
                    || serviceType.ContainsGenericParameters
                    || registrationGroup.ServiceType.ContainsGenericParameters
                select registrationGroup;

            if (overlappingGroups.Any())
            {
                throw new InvalidOperationException(
                    StringResources.MixingCallsToRegisterAllIsNotSupported(serviceType));
            }
        }

        private class RegistrationGroup
        {
            internal Type ServiceType { get; set; }

            internal ContainerControlledItem[] Items { get; set; }

            internal bool Appended { get; set; }
        }
    }
}