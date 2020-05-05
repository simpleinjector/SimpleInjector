// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Integration.WebApi;

    /// <summary>
    /// Extension methods for integrating Simple Injector with ASP.NET Web API applications.
    /// </summary>
    public static partial class SimpleInjectorWebApiExtensions
    {
        private static bool httpRequestMessageTrackingEnabled;

        /// <summary>
        /// Registers the Web API <see cref="IHttpController"/> types that available for the application. This
        /// method uses the configured <see cref="IAssembliesResolver"/> and
        /// <see cref="IHttpControllerTypeResolver"/> to determine which controller types to register.
        /// </summary>
        /// <param name="container">The container the controllers should be registered in.</param>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use to get the Controller
        /// types to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null
        /// reference.</exception>
        public static void RegisterWebApiControllers(
            this Container container, HttpConfiguration configuration)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(configuration, nameof(configuration));

            IEnumerable<Assembly> assemblies = GetAvailableApplicationAssemblies(configuration);

            RegisterWebApiControllers(container, configuration, assemblies);
        }

        /// <summary>
        /// Registers the Web API <see cref="IHttpController"/> types that available for the application. This
        /// method uses the configured <see cref="IHttpControllerTypeResolver"/> to determine which controller
        /// types to register.
        /// </summary>
        /// <param name="container">The container the controllers should be registered in.</param>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use to get the Controller
        /// types to register.</param>
        /// <param name="assemblies">The assemblies to search.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null
        /// reference.</exception>
        public static void RegisterWebApiControllers(
            this Container container, HttpConfiguration configuration, params Assembly[] assemblies)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(configuration, nameof(configuration));
            Requires.IsNotNull(assemblies, nameof(assemblies));

            container.RegisterWebApiControllers(configuration, (IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Registers the Web API <see cref="IHttpController"/> types that available for the application. This
        /// method uses the configured <see cref="IHttpControllerTypeResolver"/> to determine which controller
        /// types to register.
        /// </summary>
        /// <param name="container">The container the controllers should be registered in.</param>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use to get the Controller
        /// types to register.</param>
        /// <param name="assemblies">The assemblies to search.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.
        /// </exception>
        public static void RegisterWebApiControllers(
            this Container container, HttpConfiguration configuration, IEnumerable<Assembly> assemblies)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(configuration, nameof(configuration));
            Requires.IsNotNull(assemblies, nameof(assemblies));

            IAssembliesResolver assembliesResolver = new AssembliesResolver(assemblies);

            var controllerTypes = GetControllerTypesFromConfiguration(configuration, assembliesResolver);

            foreach (Type controllerType in controllerTypes)
            {
                Registration registration = Lifestyle.Transient.CreateRegistration(controllerType, container);

                if (typeof(ApiController).IsAssignableFrom(controllerType))
                {
                    // Suppress the Disposable Transient Component warning, because Web API's controller factory
                    // ensures correct disposal of controllers.
                    registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent,
                        justification:
                            "Web API registers controllers for disposal when the request ends during the " +
                            "call to ApiController.ExecuteAsync.");
                }

                container.AddRegistration(controllerType, registration);
            }
        }

        /// <summary>
        /// Makes the current <see cref="T:System.Net.Http.HttpRequestMessage" /> resolvable by calling
        /// <see cref="GetCurrentHttpRequestMessage(Container)">GetCurrentHttpRequestMessage</see>.
        /// </summary>
        /// <param name="container">The container instance for which HttpRequestMessageTracking should be
        /// enabled.</param>
        /// <param name="configuration">The application's configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.
        /// </exception>
        public static void EnableHttpRequestMessageTracking(
            this Container container, HttpConfiguration configuration)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(configuration, nameof(configuration));

            if (!configuration.MessageHandlers.OfType<SimpleInjectorHttpRequestMessageHandler>().Any())
            {
                configuration.MessageHandlers.Insert(0, new SimpleInjectorHttpRequestMessageHandler());
            }

            httpRequestMessageTrackingEnabled = true;
        }

        /// <summary>
        /// Retrieves the <see cref="HttpRequestMessage"/> instance for the current request.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>The <see cref="HttpRequestMessage"/> for the current request.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this method is called before
        /// <see cref="EnableHttpRequestMessageTracking"/> is called.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> argument
        /// is a null reference.</exception>
        public static HttpRequestMessage? GetCurrentHttpRequestMessage(this Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            if (!httpRequestMessageTrackingEnabled)
            {
                throw new InvalidOperationException(
                    "Resolving the current HttpRequestMessage has not been enabled. Make sure " +
                    "container.EnableHttpRequestMessageTracking(GlobalConfiguration.Configuration) has " +
                    "been called during startup.");
            }

            return SimpleInjectorHttpRequestMessageProvider.CurrentMessage;
        }

        private static IEnumerable<Assembly> GetAvailableApplicationAssemblies(HttpConfiguration configuration)
        {
            IAssembliesResolver assembliesResolver = GetRegisteredAssembliesResolver(configuration);

            try
            {
                return assembliesResolver.GetAssemblies();
            }
            catch (InvalidOperationException ex)
            {
                if (!ex.Message.Contains("pre-start"))
                {
                    throw;
                }

                throw new InvalidOperationException(
                    ex.Message + " " +
                    "Please note that the RegisterWebApiControllers(Container, HttpConfiguration) overload " +
                    "makes use of the configured IAssembliesResolver. Web API's default IAssembliesResolver " +
                    "uses the System.Web.Compilation.BuildManager, which can't be used in the pre-start " +
                    "initialization phase or outside the context of ASP.NET (e.g. when running unit tests). " +
                    "Either make sure you call the RegisterWebApiControllers method at a later point in " +
                    "time, register a custom IAssembliesResolver that does not depend on the BuildManager, " +
                    "or supply a list of assemblies manually using the " +
                    "RegisterWebApiControllers(Container, HttpConfiguration, IEnumerable<Assembly>) " +
                    "overload.",
                    ex);
            }
        }

        private static List<Type> GetControllerTypesFromConfiguration(HttpConfiguration configuration,
            IAssembliesResolver assembliesResolver)
        {
            IHttpControllerTypeResolver typeResolver = GetHttpControllerTypeResolver(configuration);

            return typeResolver.GetControllerTypes(assembliesResolver).ToList();
        }

        private static IAssembliesResolver GetRegisteredAssembliesResolver(HttpConfiguration configuration)
        {
            try
            {
                return configuration.Services.GetAssembliesResolver();
            }
            catch (Exception ex)
            {
                // See: https://stackoverflow.com/questions/27927199
                string message = string.Format(CultureInfo.InvariantCulture,
                    "There was an error retrieving the {0}. Are you missing a binding redirect? {1}",
                    typeof(IAssembliesResolver).FullName,
                    ex.Message);

                throw new InvalidOperationException(message, ex);
            }
        }

        private static IHttpControllerTypeResolver GetHttpControllerTypeResolver(
            HttpConfiguration configuration)
        {
            try
            {
                return configuration.Services.GetHttpControllerTypeResolver()
                    ?? throw new InvalidOperationException(
                        "An IHttpControllerTypeResolver instance is missing from the HttpConfiguration" +
                        ".Services.");
            }
            catch (Exception ex)
            {
                string message = string.Format(CultureInfo.InvariantCulture,
                    "There was an error retrieving the {0}. Are you missing a binding redirect? {1}",
                    typeof(IHttpControllerTypeResolver).FullName,
                    ex.Message);

                throw new InvalidOperationException(message, ex);
            }
        }

        private sealed class AssembliesResolver : IAssembliesResolver
        {
            private readonly ICollection<Assembly> assemblies;

            public AssembliesResolver(IEnumerable<Assembly> assemblies)
            {
                this.assemblies = assemblies.ToList().AsReadOnly();
            }

            public ICollection<Assembly> GetAssemblies() => this.assemblies;
        }
    }
}