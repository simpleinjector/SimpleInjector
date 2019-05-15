// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Integration.AspNetCore;
    using SimpleInjector.Integration.AspNetCore.Mvc;

    /// <summary>
    /// Extension methods for <see cref="SimpleInjectorAspNetCoreBuilder"/> that allow integrating
    /// Simple Injector with ASP.NET Core MVC controllers and view components.
    /// </summary>
    public static class SimpleInjectorAspNetCoreBuilderMvcCoreExtensions
    {
        /// <summary>
        /// Registers all application's controllers in Simple Injector and instructs ASP.NET Core to let
        /// Simple Injector create those controllers.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <returns>The supplied <paramref name="builder"/> instance.</returns>
        public static SimpleInjectorAspNetCoreBuilder AddControllerActivation(
            this SimpleInjectorAspNetCoreBuilder builder)
        {
            Requires.IsNotNull(builder, nameof(builder));

            ApplicationPartManager manager = GetApplicationPartManager(
                builder.Services,
                nameof(AddControllerActivation));

            RegisterMvcControllers(builder.Container, manager);

            builder.Services.AddSingleton<IControllerActivator>(
                new SimpleInjectorControllerActivator(builder.Container));

            return builder;
        }

        /// <summary>
        /// Registers all application's view components in Simple Injector and instructs ASP.NET Core to let
        /// Simple Injector create those view components.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <returns>The supplied <paramref name="builder"/> instance.</returns>
        public static SimpleInjectorAspNetCoreBuilder AddViewComponentActivation(
            this SimpleInjectorAspNetCoreBuilder builder)
        {
            Requires.IsNotNull(builder, nameof(builder));

            RegisterViewComponentTypes(builder);

            builder.Services.AddSingleton<IViewComponentActivator>(
                new SimpleInjectorViewComponentActivator(builder.Container));

            return builder;
        }

        private static ApplicationPartManager GetApplicationPartManager(
            this IServiceCollection services, string methodName)
        {
            ServiceDescriptor descriptor = services
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

        private static void RegisterMvcControllers(Container container, ApplicationPartManager manager)
        {
            var feature = new ControllerFeature();
            manager.PopulateFeature(feature);
            var controllerTypes = feature.Controllers.Select(t => t.AsType());

            RegisterControllerTypes(container, controllerTypes);
        }

        private static void RegisterControllerTypes(this Container container, IEnumerable<Type> types)
        {
            foreach (Type type in types.ToArray())
            {
                var registration = CreateConcreteRegistration(container, type);

                // Microsoft.AspNetCore.Mvc.Controller implements IDisposable (which is a design flaw).
                // This will cause false positives in Simple Injector's diagnostic services, so we suppress
                // this warning in case the registered type doesn't override Dispose from Controller.
                if (ShouldSuppressDisposingControllers(type))
                {
                    registration.SuppressDiagnosticWarning(
                        DiagnosticType.DisposableTransientComponent,
                            "Derived type doesn't override Dispose, so it can be safely ignored.");
                }

                container.AddRegistration(type, registration);
            }
        }

        // The user should be warned when he implements IDisposable on a non-controller derivative,
        // and otherwise only if he has overridden Controller.Dispose(bool).
        private static bool ShouldSuppressDisposingControllers(Type controllerType) =>
            TypeInheritsFromController(controllerType)
                && GetProtectedDisposeMethod(controllerType).DeclaringType == typeof(Controller);

        private static bool TypeInheritsFromController(Type controllerType) =>
            typeof(Controller).GetTypeInfo().IsAssignableFrom(controllerType);

        private static MethodInfo GetProtectedDisposeMethod(Type controllerType)
        {
            foreach (var method in controllerType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                // if method == 'protected void Dispose(bool)'
                if (!method.IsPrivate
                    && !method.IsPublic
                    && method.ReturnType == typeof(void)
                    && method.Name == "Dispose"
                    && method.GetParameters().Length == 1
                    && method.GetParameters()[0].ParameterType == typeof(bool))
                {
                    return method;
                }
            }

            return null;
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

        private static Registration CreateConcreteRegistration(Container container, Type concreteType) =>
            container.Options.LifestyleSelectionBehavior
                .SelectLifestyle(concreteType)
                .CreateRegistration(concreteType, container);
    }
}