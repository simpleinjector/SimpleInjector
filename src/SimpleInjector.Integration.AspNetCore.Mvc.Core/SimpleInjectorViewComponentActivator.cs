// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.AspNetCore.Mvc
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ViewComponents;

    /// <summary>
    /// View component activator for Simple Injector.
    /// </summary>
    public sealed class SimpleInjectorViewComponentActivator : IViewComponentActivator
    {
        private readonly ConcurrentDictionary<Type, InstanceProducer> viewComponentProducers =
            new ConcurrentDictionary<Type, InstanceProducer>();

        private readonly Container container;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorViewComponentActivator"/> class.
        /// </summary>
        /// <param name="container">The container instance.</param>
        public SimpleInjectorViewComponentActivator(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            this.container = container;
        }

        /// <summary>Creates a view component.</summary>
        /// <param name="context">The <see cref="ViewComponentContext"/> for the executing <see cref="ViewComponent"/>.</param>
        /// <returns>A view component instance.</returns>
        public object Create(ViewComponentContext context)
        {
            Type viewComponentType = context.ViewComponentDescriptor.TypeInfo.AsType();

            var producer = this.viewComponentProducers.GetOrAdd(viewComponentType, this.GetViewComponentProducer);

            if (producer == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "For the {0} to function properly, it requires all view components to be registered explicitly " +
                        "in Simple Injector, but a registration for {1} is missing. To ensure all view components are " +
                        "registered properly, call the RegisterMvcViewComponents extension method on the Container " +
                        "from within your Startup.Configure method while supplying the IApplicationBuilder " +
                        "instance, e.g. \"this.container.RegisterMvcViewComponents(app);\".{2}" +
                        "Full view component name: {3}.",
                        typeof(SimpleInjectorViewComponentActivator).Name,
                        viewComponentType.ToFriendlyName(),
                        Environment.NewLine,
                        viewComponentType.FullName));
            }

            return producer.GetInstance();
        }

        /// <summary>Releases the view component.</summary>
        /// <param name="context">The <see cref="ViewComponentContext"/> associated with the viewComponent.</param>
        /// <param name="viewComponent">The view component to release.</param>
        public void Release(ViewComponentContext context, object viewComponent)
        {
            // No-op.
        }

        // By searching through the current registrations, we ensure that the component is not auto-registered, because
        // that might cause it to be resolved from ASP.NET Core, in case auto cross-wiring is enabled.
        private InstanceProducer GetViewComponentProducer(Type viewComponentType) =>
            this.container.GetCurrentRegistrations().SingleOrDefault(r => r.ServiceType == viewComponentType);
    }
}