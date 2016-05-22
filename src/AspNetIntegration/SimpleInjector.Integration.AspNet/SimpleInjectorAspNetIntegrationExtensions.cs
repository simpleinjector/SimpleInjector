#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015-2016 Simple Injector Contributors
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
    using System.Linq;
    using System.Reflection;
    using Diagnostics;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector.Extensions.ExecutionContextScoping;

    /// <summary>
    /// Extension methods for integrating Simple Injector with ASP.NET applications.
    /// </summary>
    public static class SimpleInjectorAspNetIntegrationExtensions
    {
        /// <summary>Wraps an ASP.NET request in a execution context scope.</summary>
        /// <param name="applicationBuilder">The ASP.NET application builder instance that references all
        /// framework components.</param>
        /// <param name="container"></param>
        public static void UseSimpleInjectorAspNetRequestScoping(this IApplicationBuilder applicationBuilder, 
            Container container)
        {
            Requires.IsNotNull(applicationBuilder, nameof(applicationBuilder));
            Requires.IsNotNull(container, nameof(container));

            applicationBuilder.Use(async (context, next) =>
            {
                using (container.BeginExecutionContextScope())
                {
                    await next();
                }
            });
        }

        /// <summary>
        /// Cross-wires a registration made in the ASP.NET configuration into Simple Injector with the
        /// <see cref="Lifestyle.Transient">Transient</see> lifestyle, to allow that instance to be injected 
        /// into application components.
        /// </summary>
        /// <typeparam name="TService">The type of the ASP.NET abstraction to cross-wire.</typeparam>
        /// <param name="container">The container to cross-wire that registration in.</param>
        /// <param name="applicationBuilder">The ASP.NET application builder instance that references all
        /// framework components.</param>
        public static void CrossWire<TService>(this Container container, IApplicationBuilder applicationBuilder)
            where TService : class
        {
            // Always use the transient lifestyle, because we have no clue what the lifestyle in ASP.NET is,
            // and scoped and singleton lifestyles will dispose instances, while ASP.NET controls them.
            var registration = Lifestyle.Transient.CreateRegistration(
                applicationBuilder.ApplicationServices.GetRequiredService<TService>,
                container);

            // Prevent Simple Injector from throwing exceptions when the service type is disposable (yuck!).
            // Implementing IDisposable on abstractions is a serious design flaw, but ASP.NET does it anyway :-(
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "Owned by ASP.NET");

            container.AddRegistration(typeof(TService), registration);
        }

        /// <summary>
        /// Registers the ASP.NET controller instances that are defined in the application.
        /// </summary>
        /// <param name="container">The container the controllers should be registered in.</param>
        /// <param name="applicationBuilder">The ASP.NET object that holds the 
        /// <see cref="IControllerTypeProvider"/> that allows retrieving the application's controller types.
        /// </param>
        public static void RegisterAspNetControllers(this Container container,
            IApplicationBuilder applicationBuilder)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(applicationBuilder, nameof(applicationBuilder));

            var provider = applicationBuilder.ApplicationServices.GetRequiredService<IControllerTypeProvider>();

            RegisterAspNetControllers(container, provider);
        }

        /// <summary>
        /// Registers the ASP.NET controller types using the supplied 
        /// <paramref name="controllerTypeProvider"/>.
        /// </summary>
        /// <param name="container">The container the controllers should be registered in.</param>
        /// <param name="controllerTypeProvider">The provider that contains the list of controllers to 
        /// register.</param>
        public static void RegisterAspNetControllers(this Container container,
            IControllerTypeProvider controllerTypeProvider)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(controllerTypeProvider, nameof(controllerTypeProvider));

            var controllerTypes = controllerTypeProvider.ControllerTypes.Select(t => t.AsType());

            RegisterControllerTypes(container, controllerTypes);
        }

        /// <summary>
        /// Registers the ASP.NET view component instances that are defined in the application.
        /// </summary>
        /// <param name="container">The container the controllers should be registered in.</param>
        /// <param name="applicationBuilder">The ASP.NET object that holds the 
        /// <see cref="IControllerTypeProvider"/> that allows retrieving the application's controller types.
        /// </param>
        public static void RegisterAspNetViewComponents(this Container container,
            IApplicationBuilder applicationBuilder)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(applicationBuilder, nameof(applicationBuilder));

            IServiceProvider serviceProvider = applicationBuilder.ApplicationServices;
            var componentProvider = serviceProvider.GetRequiredService<IViewComponentDescriptorProvider>();

            RegisterAspNetViewComponents(container, componentProvider);
        }

        /// <summary>
        /// Registers the ASP.NET view component types using the supplied 
        /// <paramref name="viewComponentDescriptorProvider"/>.
        /// </summary>
        /// <param name="container">The container the controllers should be registered in.</param>
        /// <param name="viewComponentDescriptorProvider">The provider that contains the list of view
        /// components to register.</param>
        public static void RegisterAspNetViewComponents(this Container container,
            IViewComponentDescriptorProvider viewComponentDescriptorProvider)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(viewComponentDescriptorProvider, nameof(viewComponentDescriptorProvider));

            var componentTypes = viewComponentDescriptorProvider
                .GetViewComponents()
                .Select(description => description.TypeInfo.AsType());

            RegisterViewComponentTypes(container, componentTypes);
        }

        private static void RegisterControllerTypes(this Container container, IEnumerable<Type> types)
        {
            foreach (Type type in types.ToArray())
            {
                var registration = CreateConcreteRegistration(container, type);

                if (typeof(IDisposable).IsAssignableFrom(type))
                {
                    registration.SuppressDiagnosticWarning(
                        DiagnosticType.DisposableTransientComponent, "ASP.NET disposes controllers.");
                }

                // TODO: Ensure disposal of controllers; Since we override IControllerActivator, nothing is
                // disposed anymore.
                container.AddRegistration(type, registration);
            }
        }

        private static void RegisterViewComponentTypes(this Container container, IEnumerable<Type> types)
        {
            foreach (Type type in types.ToArray())
            {
                container.AddRegistration(type, CreateConcreteRegistration(container, type));
            }
        }

        private static Registration CreateConcreteRegistration(Container container, Type concreteType)
        {
            var lifestyle = 
                container.Options.LifestyleSelectionBehavior.SelectLifestyle(concreteType, concreteType);

            return lifestyle.CreateRegistration(concreteType, container);
        }
    }
}