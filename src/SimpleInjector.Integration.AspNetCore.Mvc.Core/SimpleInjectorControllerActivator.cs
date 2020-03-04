// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.AspNetCore.Mvc
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;

    /// <summary>Controller activator for Simple Injector.</summary>
    public sealed class SimpleInjectorControllerActivator : IControllerActivator
    {
        private readonly ConcurrentDictionary<Type, InstanceProducer?> controllerProducers =
            new ConcurrentDictionary<Type, InstanceProducer?>();

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

            InstanceProducer? producer =
                this.controllerProducers.GetOrAdd(controllerType, this.GetControllerProducer);

            if (producer is null)
            {
                const string AddControllerActivationMethod =
                    nameof(SimpleInjectorAspNetCoreBuilderMvcCoreExtensions.AddControllerActivation);

                throw new InvalidOperationException(
                    $"For the {nameof(SimpleInjectorControllerActivator)} to function properly, it " +
                    $"requires all controllers to be registered explicitly, but a registration for " +
                    $"{controllerType.ToFriendlyName()} is missing. To find the controllers to register, " +
                    $"Simple Injector's {AddControllerActivationMethod} method makes use of ASP.NET Core's " +
                    $"Application Parts. The most likely cause of this missing controller is, therefore, " +
                    $"that {controllerType.ToFriendlyName()}'s assembly, namely " +
                    $"{controllerType.Assembly.GetName().Name}, is not registered as application part. For " +
                    $"more information about configuring application parts, please consult the official " +
                    $"Microsoft documentation at: " +
                    $"https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts.");
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
        private InstanceProducer? GetControllerProducer(Type controllerType) =>
            this.container.GetCurrentRegistrations().SingleOrDefault(r => r.ServiceType == controllerType);
    }
}