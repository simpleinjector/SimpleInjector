// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Integration.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Mvc.Razor.Internal;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension methods for integrating Simple Injector with ASP.NET Core MVC applications.
    /// </summary>
    public static class SimpleInjectorAspNetCoreMvcIntegrationExtensions
    {
        /// <summary>
        /// Registers a custom <see cref="SimpleInjectorTagHelperActivator"/> that allows the resolval of
        /// tag helpers using the <paramref name="container"/>. In case no <paramref name="applicationTypeSelector"/>
        /// is supplied, the custom tag helper activator will forward the creation of tag helpers that are not
        /// located in a "Microsoft*" namespace to Simple Injector.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> the custom tag helper activator should
        /// be registered in.</param>
        /// <param name="container">The container tag helpers should be resolved from.</param>
        /// <param name="applicationTypeSelector">An optional predicate that allows specifying which types
        /// should be resolved by Simple Injector (true) and which should be resolved by the framework (false).
        /// When not specified, all tag helpers whose namespace does not start with "Microsoft" will be forwarded
        /// to the Simple Injector container.</param>
        [Obsolete(
            "Please use " + nameof(SimpleInjectorAspNetCoreBuilderMvcExtensions) + "." +
            nameof(SimpleInjectorAspNetCoreBuilderMvcExtensions.AddTagHelperActivation) + " instead—e.g. " +
            "'services.AddSimpleInjector(container, options => options." +
            nameof(SimpleInjectorAspNetCoreBuilderMvcExtensions.AddTagHelperActivation) + "());'. " +
            "Please see https://simpleinjector.org/aspnetcore for more details. " +
            "Will be treated as an error from version 4.9. Will be removed in version 5.0.",
            error: false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void AddSimpleInjectorTagHelperActivation(
            this IServiceCollection services,
            Container container,
            Predicate<Type>? applicationTypeSelector = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            // There are tag helpers OOTB in MVC. Letting the application container try to create them will fail
            // because of the dependencies these tag helpers have. This means that OOTB tag helpers need to remain
            // created by the framework's DefaultTagHelperActivator, hence the selector predicate.
            Predicate<Type> selector =
                applicationTypeSelector ?? (type => !type.GetTypeInfo().Namespace.StartsWith("Microsoft"));

            services.AddSingleton<ITagHelperActivator>(p => new SimpleInjectorTagHelperActivator(
                container,
                selector,
                new DefaultTagHelperActivator(p.GetRequiredService<ITypeActivatorCache>())));
        }

        /// <summary>
        /// Registers the ASP.NET Core MVC controller instances that are defined in the application through
        /// the <see cref="ApplicationPartManager"/>.
        /// </summary>
        /// <param name="container">The container the controllers should be registered in.</param>
        /// <param name="applicationBuilder">The ASP.NET object that holds the application's configuration.
        /// </param>
        [Obsolete(
            "Please use " + nameof(SimpleInjectorAspNetCoreBuilderMvcExtensions) + "." +
            nameof(SimpleInjectorAspNetCoreBuilderMvcExtensions.AddPageModelActivation) + " instead—e.g. " +
            "'services.AddSimpleInjector(container, options => options." +
            nameof(SimpleInjectorAspNetCoreBuilderMvcExtensions.AddPageModelActivation) + "());'. " +
            "Please see https://simpleinjector.org/aspnetcore for more details. " +
            "Will be treated as an error from version 4.9. Will be removed in version 5.0.",
            error: false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void RegisterPageModels(
            this Container container, IApplicationBuilder applicationBuilder)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            var manager = applicationBuilder.ApplicationServices.GetService<ApplicationPartManager>();

            if (manager == null)
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

            // As far as I can see, page models must inherit from the PageModel class.
            var pageModelTypes =
                from part in manager.ApplicationParts.OfType<IApplicationPartTypeProvider>()
                from type in part.Types
                where type.IsSubclassOf(typeof(PageModel))
                where !type.IsAbstract
                where !type.IsGenericTypeDefinition
                select type;

            RegisterPageModelTypes(container, pageModelTypes);
        }

        private static void RegisterPageModelTypes(this Container container, IEnumerable<Type> types)
        {
            foreach (Type type in types.ToArray())
            {
                container.AddRegistration(type, CreateConcreteRegistration(container, type));
            }
        }

        private static Registration CreateConcreteRegistration(Container container, Type concreteType)
        {
            var lifestyle = container.Options.LifestyleSelectionBehavior.SelectLifestyle(concreteType);

            return lifestyle.CreateRegistration(concreteType, container);
        }
    }
}