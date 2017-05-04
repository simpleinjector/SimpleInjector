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
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Diagnostics;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension methods for integrating Simple Injector with ASP.NET Core MVC applications.
    /// </summary>
    public static class SimpleInjectorAspNetCoreMvcIntegrationExtensions
    {
        /// <summary>
        /// Registers the ASP.NET Core MVC controller instances that are defined in the application through
        /// the <see cref="ApplicationPartManager"/>.
        /// </summary>
        /// <param name="container">The container the controllers should be registered in.</param>
        /// <param name="applicationBuilder">The ASP.NET object that holds the application's configuration.
        /// </param>
        public static void RegisterMvcControllers(this Container container,
            IApplicationBuilder applicationBuilder)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(applicationBuilder, nameof(applicationBuilder));

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

            var feature = new ControllerFeature();
            manager.PopulateFeature(feature);

            RegisterControllerTypes(container, feature.Controllers.Select(t => t.AsType()));
        }

        /// <summary>
        /// Registers the ASP.NET Core MVC view component instances that are defined in the application.
        /// </summary>
        /// <param name="container">The container the controllers should be registered in.</param>
        /// <param name="applicationBuilder">The ASP.NET object that holds the 
        /// <see cref="IViewComponentDescriptorProvider"/> that allows retrieving the application's controller types.
        /// </param>
        public static void RegisterMvcViewComponents(this Container container,
            IApplicationBuilder applicationBuilder)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(applicationBuilder, nameof(applicationBuilder));

            IServiceProvider serviceProvider = applicationBuilder.ApplicationServices;
            var componentProvider = serviceProvider.GetService<IViewComponentDescriptorProvider>();

            if (componentProvider == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "A registration for the {0} is missing from the ASP.NET Core configuration " +
                        "system. Make sure it is registered or pass it in using the " +
                        "RegisterMvcViewComponents overload that accepts {1}. You can ensure that {1} is " +
                        "registered by either calling .AddMvc() or .AddViews() on the IServiceCollection " +
                        "class in the ConfigureServices method. Do note that calling .AddMvcCore() will " +
                        "not result in a registered {1}.",
                    typeof(IViewComponentDescriptorProvider).FullName,
                    typeof(IViewComponentDescriptorProvider).Name));
            }

            RegisterMvcViewComponents(container, componentProvider);
        }

        /// <summary>
        /// Registers the ASP.NET view component types using the supplied 
        /// <paramref name="viewComponentDescriptorProvider"/>.
        /// </summary>
        /// <param name="container">The container the controllers should be registered in.</param>
        /// <param name="viewComponentDescriptorProvider">The provider that contains the list of view
        /// components to register.</param>
        public static void RegisterMvcViewComponents(this Container container,
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

                // Microsoft.AspNetCore.Mvc.Controller implements IDisposable (which is a design flaw).
                // This will cause false positives in Simple Injector's diagnostic services, so we suppress
                // this warning in case the registered type doesn't override Dispose from Controller.
                if (ShouldSuppressDisposableTransientComponent(type))
                {
                    registration.SuppressDiagnosticWarning(
                        DiagnosticType.DisposableTransientComponent,
                            "Derived type doesn't override Dispose, so it can be safely ignored.");
                }

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
            var lifestyle = container.Options.LifestyleSelectionBehavior.SelectLifestyle(concreteType);

            return lifestyle.CreateRegistration(concreteType, container);
        }

        // The user should be warned when he implements IDisposable on a non-controller derivative,
        // and otherwise only if he has overridden Controller.Dispose(bool).
        private static bool ShouldSuppressDisposableTransientComponent(Type controllerType) =>
            TypeInheritsFromController(controllerType)
                ? GetProtectedDisposeMethod(controllerType).DeclaringType == typeof(Controller)
                : false;

        private static bool TypeInheritsFromController(Type controllerType) =>
            typeof(Controller).GetTypeInfo().IsAssignableFrom(controllerType);

        private static MethodInfo GetProtectedDisposeMethod(Type controllerType)
        {
            foreach (var method in controllerType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                // if method == 'protected void Dispose(bool)'
                if (
                    !method.IsPrivate && !method.IsPublic
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
    }
}