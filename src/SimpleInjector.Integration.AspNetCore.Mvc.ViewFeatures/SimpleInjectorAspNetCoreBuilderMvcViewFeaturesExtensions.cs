// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector.Integration.AspNetCore;
    using SimpleInjector.Integration.AspNetCore.Mvc;

    /// <summary>
    /// Extension methods for <see cref="SimpleInjectorAspNetCoreBuilder"/> that allow integrating
    /// Simple Injector with ASP.NET Core MVC view components.
    /// </summary>
    public static class SimpleInjectorAspNetCoreBuilderMvcViewFeaturesExtensions
    {
        /// <summary>
        /// Registers all application's view components in Simple Injector and instructs ASP.NET Core to let
        /// Simple Injector create those view components.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <returns>The supplied <paramref name="builder"/> instance.</returns>
        public static SimpleInjectorAspNetCoreBuilder AddViewComponentActivation(
            this SimpleInjectorAspNetCoreBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            RegisterViewComponentTypes(builder);

            builder.Services.AddSingleton<IViewComponentActivator>(
                new SimpleInjectorViewComponentActivator(builder.Container));

            return builder;
        }

        private static void RegisterViewComponentTypes(SimpleInjectorAspNetCoreBuilder builder)
        {
            var container = builder.Container;

            ApplicationPartManager manager = GetApplicationPartManager(
               builder.Services,
               nameof(AddViewComponentActivation));

            var feature = new ViewComponentFeature();
            manager.PopulateFeature(feature);
            var viewComponentTypes = feature.ViewComponents.Select(info => info.AsType());

            foreach (Type type in viewComponentTypes.ToArray())
            {
                container.AddRegistration(type, CreateConcreteRegistration(container, type));
            }
        }

        private static ApplicationPartManager GetApplicationPartManager(
            this IServiceCollection services, string methodName)
        {
            ServiceDescriptor? descriptor = services
                .LastOrDefault(d => d.ServiceType == typeof(ApplicationPartManager));

            if (descriptor is null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "A registration for the {0} is missing from the ASP.NET Core configuration " +
                        "system. This is most likely caused by a missing call to services.AddMvcCore() or " +
                        "services.AddMvc() as part of the ConfigureServices(IServiceCollection) method of " +
                        "the Startup class. A call to one of those methods will ensure the registration " +
                        "of the {1}.",
                        typeof(ApplicationPartManager).FullName,
                        typeof(ApplicationPartManager).Name));
            }
            else if (descriptor.ImplementationInstance is null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Although a registration for {0} exists in the ASP.NET Core configuration system, " +
                        "the registration is not added as an existing instance. This makes it impossible " +
                        "for Simple Injector's {1} method to get this instance from ASP.NET Core's " +
                        "IServiceCollection. This is most likely because {2} was overridden by you or a " +
                        "third-party library. Make sure that you use the AddSingleton overload that takes " +
                        "in an existing instance—i.e. call " +
                        "services.AddSingleton<{2}>(new {2}()).",
                        typeof(ApplicationPartManager).FullName,
                        methodName,
                        typeof(ApplicationPartManager).Name));
            }
            else
            {
                return (ApplicationPartManager)descriptor.ImplementationInstance;
            }
        }

        private static Registration CreateConcreteRegistration(Container container, Type concreteType) =>
            container.Options.LifestyleSelectionBehavior
                .SelectLifestyle(concreteType)
                .CreateRegistration(concreteType, container);
    }
}