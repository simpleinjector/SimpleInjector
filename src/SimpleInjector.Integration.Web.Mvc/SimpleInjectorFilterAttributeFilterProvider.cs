// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.Web.Mvc
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    internal sealed class SimpleInjectorFilterAttributeFilterProvider : FilterAttributeFilterProvider
    {
        private readonly ConcurrentDictionary<Type, Registration> registrations =
            new ConcurrentDictionary<Type, Registration>();

        private readonly Func<Type, Registration> registrationFactory;

        // Supply false for cacheAttributeInstances, because otherwise attributes become singletons and this
        // has can cause all sorts of concurrency bugs, because attributes will becomes singletons.
        internal SimpleInjectorFilterAttributeFilterProvider(Container container)
            : base(cacheAttributeInstances: false)
        {
            this.Container = container;

            this.registrationFactory =
                concreteType => Lifestyle.Transient.CreateRegistration(concreteType, container);
        }

        internal Container Container { get; }

        public override IEnumerable<Filter> GetFilters(
            ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            Filter[] filters = this.GetBaseFilters(controllerContext, actionDescriptor);

            foreach (var filter in filters)
            {
                this.InitializeInstance(filter.Instance);
            }

            return filters;
        }

        private Filter[] GetBaseFilters(
            ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            IEnumerable<Filter> filters = base.GetFilters(controllerContext, actionDescriptor);

            return (filters as Filter[]) ?? filters.ToArray();
        }

        private void InitializeInstance(object instance)
        {
            Registration registration =
                this.registrations.GetOrAdd(instance.GetType(), this.registrationFactory);

            registration.InitializeInstance(instance);
        }
    }
}