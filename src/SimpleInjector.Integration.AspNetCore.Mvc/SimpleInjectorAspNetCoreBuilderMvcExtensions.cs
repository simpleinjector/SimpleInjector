#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2019 Simple Injector Contributors
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

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Mvc.Razor.Internal;
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
            Predicate<Type> applicationTypeSelector = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            // There are tag helpers OOTB in MVC. Letting the application container try to create them will
            // fail because of the dependencies these tag helpers have. This means that OOTB tag helpers need
            // to remain created by the framework's DefaultTagHelperActivator, hence the selector predicate.
            applicationTypeSelector = applicationTypeSelector ??
                (type => !type.GetTypeInfo().Namespace.StartsWith("Microsoft"));

            var manager = GetApplicationPartManager(builder.Services, nameof(AddTagHelperActivation));

            builder.Container.RegisterTagHelpers(manager, applicationTypeSelector);

            builder.Services.AddSingleton<ITagHelperActivator>(p => new SimpleInjectorTagHelperActivator(
                builder.Container,
                applicationTypeSelector,
                new DefaultTagHelperActivator(p.GetRequiredService<ITypeActivatorCache>())));

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

        private static void RegisterTagHelpers(
            this Container container, ApplicationPartManager manager, Predicate<Type> applicationTypeSelector)
        {
            var pageModelTypes =
                from part in manager.ApplicationParts.OfType<IApplicationPartTypeProvider>()
                from type in part.Types
                where typeof(ITagHelper).IsAssignableFrom(type)
                where !type.IsAbstract
                where !type.IsGenericTypeDefinition
                where applicationTypeSelector(type)
                select type;

            RegisterConcreteTypes(container, pageModelTypes);
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

        private static void RegisterConcreteTypes(this Container container, IEnumerable<Type> types)
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