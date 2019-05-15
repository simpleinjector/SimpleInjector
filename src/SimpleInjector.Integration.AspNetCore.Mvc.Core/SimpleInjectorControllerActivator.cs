// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.AspNetCore.Mvc
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;

    /// <summary>Controller activator for Simple Injector.</summary>
    public sealed class SimpleInjectorControllerActivator : IControllerActivator
    {
        private readonly ConcurrentDictionary<Type, InstanceProducer> controllerProducers =
            new ConcurrentDictionary<Type, InstanceProducer>();

        private readonly Container container;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorControllerActivator"/> class.
        /// </summary>
        /// <param name="container">The container instance.</param>
        public SimpleInjectorControllerActivator(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            this.container = container;
        }

        /// <summary>Creates a controller.</summary>
        /// <param name="context">The Microsoft.AspNet.Mvc.ActionContext for the executing action.</param>
        /// <returns>A new controller instance.</returns>
        public object Create(ControllerContext context)
        {
            Type controllerType = context.ActionDescriptor.ControllerTypeInfo.AsType();

            var producer = this.controllerProducers.GetOrAdd(controllerType, this.GetControllerProducer);

            if (producer == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "For the {0} to function properly, it requires all controllers to be registered explicitly " +
                        "in Simple Injector, but a registration for {1} is missing. To ensure all controllers are " +
                        "registered properly, call the RegisterMvcControllers extension method on the Container " +
                        "from within your Startup.Configure method while supplying the IApplicationBuilder " +
                        "instance, e.g. \"this.container.RegisterMvcControllers(app);\".{2}" +
                        "Full controller name: {3}.",
                        typeof(SimpleInjectorControllerActivator).Name,
                        controllerType.ToFriendlyName(),
                        Environment.NewLine,
                        controllerType.FullName));
            }

            return producer.GetInstance();
        }

        /// <summary>Releases the controller.</summary>
        /// <param name="context">The Microsoft.AspNet.Mvc.ActionContext for the executing action.</param>
        /// <param name="controller">The controller instance.</param>
        public void Release(ControllerContext context, object controller)
        {
        }

        // By searching through the current registrations, we ensure that the controller is not auto-registered, because
        // that might cause it to be resolved from ASP.NET Core, in case auto cross-wiring is enabled.
        private InstanceProducer GetControllerProducer(Type controllerType) =>
            this.container.GetCurrentRegistrations().SingleOrDefault(r => r.ServiceType == controllerType);
    }
}