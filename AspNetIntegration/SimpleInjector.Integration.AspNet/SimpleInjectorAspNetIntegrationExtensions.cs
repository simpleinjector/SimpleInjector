#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015 Simple Injector Contributors
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
    using Diagnostics;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Mvc.Controllers;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension methods for integrating Simple Injector with ASP.NET applications.
    /// </summary>
    public static class SimpleInjectorAspNetIntegrationExtensions
    {
        private static readonly Predicate<Type> AllTypes = type => true;

        /// <summary>
        /// Cross-wires a registration made in the ASP.NET configuration into Simple Injector, to allow
        /// that instance to be injected into application components.
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
        /// <param name="assemblies">The assemblies to search.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/> or 
        /// <paramref name="applicationBuilder" /> are null references (Nothing in VB).</exception>
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

            RegisterAspNetControllers(container, controllerTypes);
        }

        /// <summary>
        /// Registers the supplied list of ASP.NET controller types.
        /// </summary>
        /// <param name="container">The container the controllers should be registered in.</param>
        /// <param name="controllerTypes">The controller types to register.</param>
        public static void RegisterAspNetControllers(this Container container, IEnumerable<Type> controllerTypes)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(controllerTypes, nameof(controllerTypes));

            foreach (Type type in controllerTypes.ToArray())
            {
                Registration registration = CreateControllerRegistration(container, type);

                container.AddRegistration(type, registration);
            }
        }

        private static Registration CreateControllerRegistration(Container container, Type type)
        {
            var lifestyle = container.Options.LifestyleSelectionBehavior.SelectLifestyle(type, type);

            var registration = lifestyle.CreateRegistration(type, container);

            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent,
                "ASP.NET disposes controllers.");

            return registration;
        }
    }
}