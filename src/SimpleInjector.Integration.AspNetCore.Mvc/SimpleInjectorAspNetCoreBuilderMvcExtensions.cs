// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector.Integration.AspNetCore;
    using SimpleInjector.Integration.AspNetCore.Mvc;

    /// <summary>
    /// Extension methods for <see cref="SimpleInjectorAspNetCoreBuilder"/> that allow integrating
    /// Simple Injector with ASP.NET Core MVC page models and tag helpers.
    /// </summary>
    public static class SimpleInjectorAspNetCoreBuilderMvcExtensions
    {
        /// <summary>
        /// Registers all application's page models in Simple Injector and instructs ASP.NET Core to let
        /// Simple Injector create those page models.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <returns>The supplied <paramref name="builder"/> instance.</returns>
        public static SimpleInjectorAspNetCoreBuilder AddPageModelActivation(
              this SimpleInjectorAspNetCoreBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            ApplicationPartManager manager =
                GetApplicationPartManager(builder.Services, nameof(AddPageModelActivation));

            RegisterPageModels(builder.Container, manager);

            builder.Services.AddSingleton<IPageModelActivatorProvider>(
                new SimpleInjectorPageModelActivatorProvider(builder.Container));

            return builder;
        }

        /// <summary>
        /// Registers the application's tag helpers that in Simple Injector and instructs ASP.NET Core to let
        /// Simple Injector create those tag helpers.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <param name="applicationTypeSelector">An optional predicate that instructs Simple Injector which
        /// tag helpers to register and construct and which tag helpers to be instructed by ASP.NET Core
        /// itself. When ommitted, Simple Injector will skip all tag helpers in the 'Microsoft.' namespace.
        /// </param>
        /// <returns>The supplied <paramref name="builder"/> instance.</returns>
        public static SimpleInjectorAspNetCoreBuilder AddTagHelperActivation(
            this SimpleInjectorAspNetCoreBuilder builder,
            Predicate<Type>? applicationTypeSelector = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            // There are tag helpers OOTB in MVC. Letting the application container try to create them will
            // fail because of the dependencies these tag helpers have. This means that OOTB tag helpers need
            // to remain created by the framework's DefaultTagHelperActivator, hence the selector predicate.
            Predicate<Type> selector = applicationTypeSelector ??
                (type => !type.GetTypeInfo().Namespace.StartsWith("Microsoft")
                    && !type.GetTypeInfo().Name.Contains("__Generated__"));

            var manager = GetApplicationPartManager(builder.Services, nameof(AddTagHelperActivation));

            ServiceDescriptor tagHelperDescriptor = GetTagHelperActivatorDescriptor(builder.Services);

            builder.Container.RegisterTagHelpers(manager, selector);

            builder.Services.AddSingleton<ITagHelperActivator>(p => new SimpleInjectorTagHelperActivator(
                builder.Container,
                selector,
                (ITagHelperActivator)p.GetInstance(tagHelperDescriptor)));

            return builder;
        }

        private static ServiceDescriptor GetTagHelperActivatorDescriptor(IServiceCollection services)
        {
            var descriptor = FindServiceDescriptor(services, typeof(ITagHelperActivator));

            if (descriptor is null)
            {
                throw new InvalidOperationException(
                    $"A registration for the {typeof(ITagHelperActivator).FullName} is missing from the " +
                    "ASP.NET Core configuration system. This is most likely caused by a missing call to " +
                    "either services.AddRazorPages(), services.AddControllersWithViews(), " +
                    "services.AddViewLocalization(), or services.AddRazorViewEngine() as part of the " +
                    "ConfigureServices(IServiceCollection) method of the Startup class. A call to one of " +
                    "those methods will ensure the registration of the ITagHelperActivator. The default " +
                    "ITagHelperActivator registration is used as a fallback.");
            }
            else if (descriptor.Lifetime != ServiceLifetime.Singleton)
            {
                throw new InvalidOperationException(
                    $"Although a registration for {typeof(ITagHelperActivator).FullName} exists in the " +
                    "ASP.NET Core configuration system, the registration is not added as Singleton. " +
                    $"Instead the registration exists as {descriptor.Lifetime}. This makes it impossible " +
                    $"Simple Injector's {nameof(AddTagHelperActivation)} method requires the default " +
                    "ITagHelperActivator to be a Singleton. This is most likely because " +
                    "ITagHelperActivator was overridden by you or a third-party library. Make sure that " +
                    "you use one of the AddSingleton overloads to register the default ITagHelperActivator.");
            }
            else
            {
                return descriptor;
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

        private static void RegisterTagHelpers(
            this Container container, ApplicationPartManager manager, Predicate<Type> applicationTypeSelector)
        {
            // TagHelpers should always have a transient lifestyle, because a single chtml can have the same
            // tag applied many times, but all should be a seperate instance. See #761.
            var tagHelperLifestyle = Lifestyle.Transient;

            var tagHelperTypes =
                from part in manager.ApplicationParts.OfType<IApplicationPartTypeProvider>()
                from type in part.Types
                where typeof(ITagHelper).IsAssignableFrom(type)
                where !type.IsAbstract
                where !type.IsGenericTypeDefinition
                where applicationTypeSelector(type)
                select type;

            RegisterConcreteTypes(container, tagHelperTypes, tagHelperLifestyle);
        }

        private static void RegisterPageModels(Container container, ApplicationPartManager manager)
        {
            // As far as I can see, page models must inherit from the PageModel class.
            var pageModelTypes =
                from part in manager.ApplicationParts.OfType<IApplicationPartTypeProvider>()
                from type in part.Types
                where type.IsSubclassOf(typeof(PageModel))
                where !type.IsAbstract
                where !type.IsGenericTypeDefinition
                select type;

            RegisterConcreteTypes(container, pageModelTypes);
        }

        private static void RegisterConcreteTypes(
            this Container container, IEnumerable<Type> types, Lifestyle? lifestyle = null)
        {
            foreach (Type type in types.ToArray())
            {
                container.AddRegistration(type, CreateConcreteRegistration(container, type, lifestyle));
            }
        }

        private static Registration CreateConcreteRegistration(
            Container container, Type concreteType, Lifestyle? lifestyle) =>
            (lifestyle ?? container.Options.LifestyleSelectionBehavior.SelectLifestyle(concreteType))
                .CreateRegistration(concreteType, container);

        private static ServiceDescriptor? FindServiceDescriptor(IServiceCollection services, Type serviceType)
        {
            // In case there are multiple descriptors for a given type, .NET Core will use the last
            // descriptor when one instance is resolved. We will have to get this last one as well.
            ServiceDescriptor? descriptor = services.LastOrDefault(d => d.ServiceType == serviceType);

            if (descriptor is null && serviceType.GetTypeInfo().IsGenericType)
            {
                // In case the registration is made as open-generic type, the previous query will return
                // null, and we need to go find the last open generic registration for the service type.
                var serviceTypeDefinition = serviceType.GetTypeInfo().GetGenericTypeDefinition();
                descriptor = services.LastOrDefault(d => d.ServiceType == serviceTypeDefinition);
            }

            return descriptor;
        }

        private static object GetInstance(this IServiceProvider provider, ServiceDescriptor descriptor) =>
            descriptor.ImplementationInstance != null
                ? descriptor.ImplementationInstance
                : descriptor.ImplementationType != null
                    ? ActivatorUtilities.GetServiceOrCreateInstance(provider, descriptor.ImplementationType)
                    : descriptor.ImplementationFactory(provider);
    }
}