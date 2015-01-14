#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014-2015 Simple Injector Contributors
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
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Filters;
    using SimpleInjector.Advanced;
    using SimpleInjector.Integration.WebApi;

    /// <summary>
    /// Extension methods for integrating Simple Injector with ASP.NET Web API applications.
    /// </summary>
    public static class SimpleInjectorWebApiExtensions
    {
        private static readonly Lifestyle LifestyleWithDisposal = new WebApiRequestLifestyle(true);
        private static readonly Lifestyle LifestyleNoDisposal = new WebApiRequestLifestyle(false);

        private static bool httpRequestMessageTrackingEnabled;

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TConcrete"/> will be returned within the
        /// Web API request. When the Web API request ends and 
        /// <typeparamref name="TConcrete"/> implements <see cref="IDisposable"/>, the cached instance will be 
        /// disposed.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TConcrete"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TConcrete"/> is a type
        /// that can not be created by the container.</exception>
        public static void RegisterWebApiRequest<TConcrete>(this Container container)
            where TConcrete : class
        {
            Requires.IsNotNull(container, "container");

            container.Register<TConcrete, TConcrete>(LifestyleWithDisposal);
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TImplementation"/> will be returned  will 
        /// be returned within the Web API request. When the Web API request ends and 
        /// <typeparamref name="TImplementation"/> implements <see cref="IDisposable"/>, the cached instance 
        /// will be disposed.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <typeparamref name="TImplementation"/> 
        /// type is not a type that can be created by the container.
        /// </exception>
        public static void RegisterWebApiRequest<TService, TImplementation>(
            this Container container)
            where TImplementation : class, TService
            where TService : class
        {
            Requires.IsNotNull(container, "container");

            container.Register<TService, TImplementation>(LifestyleWithDisposal);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the lifetime of a Web API request. When the Web API
        /// request ends, and the cached instance implements <see cref="IDisposable"/>, that cached instance 
        /// will be disposed.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the <paramref name="container"/>, or <paramref name="instanceCreator"/> are
        /// null references.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the
        /// <typeparamref name="TService"/> has already been registered.</exception>
        public static void RegisterWebApiRequest<TService>(this Container container,
            Func<TService> instanceCreator)
            where TService : class
        {
            RegisterWebApiRequest<TService>(container, instanceCreator, disposeWhenScopeEnds: true);
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TConcrete"/> will be returned for
        /// each Web API request. When the Web API request ends, and <typeparamref name="TConcrete"/> 
        /// implements <see cref="IDisposable"/>, the cached instance will be disposed.
        /// Scopes can be nested, and each scope gets its own instance.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="disposeWhenScopeEnds">If set to <c>true</c> the cached instance will be
        /// disposed at the end of its lifetime.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TConcrete"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TConcrete"/> is a type
        /// that can not be created by the container.</exception>
        public static void RegisterWebApiRequest<TConcrete>(this Container container,
            bool disposeWhenScopeEnds)
            where TConcrete : class, IDisposable
        {
            Requires.IsNotNull(container, "container");

            container.Register<TConcrete, TConcrete>(GetLifestyle(disposeWhenScopeEnds));
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TImplementation"/> will be returned for
        /// the duration of a single Web API request. When the Web API request ends, 
        /// <paramref name="disposeWhenScopeEnds"/> is set to <b>true</b>, and the cached instance
        /// implements <see cref="IDisposable"/>, that cached instance will be disposed.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="disposeWhenScopeEnds">If set to <c>true</c> the cached instance will be
        /// disposed at the end of its lifetime.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <typeparamref name="TImplementation"/> 
        /// type is not a type that can be created by the container.
        /// </exception>
        public static void RegisterWebApiRequest<TService, TImplementation>(
            this Container container, bool disposeWhenScopeEnds)
            where TImplementation : class, TService, IDisposable
            where TService : class
        {
            Requires.IsNotNull(container, "container");

            container.Register<TService, TImplementation>(GetLifestyle(disposeWhenScopeEnds));
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the lifetime of single Web API request. When the Web API
        /// request ends, <paramref name="disposeWhenScopeEnds"/> is set to <b>true</b>, and the cached 
        /// instance implements <see cref="IDisposable"/>, that cached instance will be disposed.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <param name="disposeWhenScopeEnds">If set to <c>true</c> the cached instance will be
        /// disposed at the end of its lifetime.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the <paramref name="container"/>, or <paramref name="instanceCreator"/> are
        /// null references.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the
        /// <typeparamref name="TService"/> has already been registered.</exception>
        public static void RegisterWebApiRequest<TService>(this Container container,
            Func<TService> instanceCreator, bool disposeWhenScopeEnds)
            where TService : class
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(instanceCreator, "instanceCreator");

            container.Register<TService>(instanceCreator, GetLifestyle(disposeWhenScopeEnds));
        }

        /// <summary>Registers a <see cref="IFilterProvider"/> that allows filter attributes to go through the
        /// Simple Injector pipeline (https://simpleinjector.org/pipel). This allows any registered property to be 
        /// injected if a custom <see cref="IPropertySelectionBehavior"/> in configured in the container, and 
        /// allows any<see cref="Container.RegisterInitializer">initializers</see> to be called on those 
        /// attributes. 
        /// <b>Please note that attributes are cached by Web API, so only dependencies should be injected that
        /// have the singleton lifestyle.</b>
        /// </summary>
        /// <param name="container">The container that should be used.</param>
        /// <param name="configuration">The <see cref="HttpConfiguration"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null 
        /// reference (Nothing in VB).</exception>
        public static void RegisterWebApiFilterProvider(this Container container, HttpConfiguration configuration)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(configuration, "configuration");

            configuration.Services.RemoveAll(typeof(IFilterProvider), 
                provider => provider is ActionDescriptorFilterProvider);

            var filterProvider = new SimpleInjectorActionDescriptorFilterProvider(container);

            configuration.Services.Add(typeof(IFilterProvider), filterProvider);

            container.SetItem(typeof(SimpleInjectorActionDescriptorFilterProvider), filterProvider);
        }
        
        /// <summary>
        /// Registers the Web API <see cref="IHttpController"/> types that available for the application. This
        /// method uses the configured <see cref="IAssembliesResolver"/> and 
        /// <see cref="IHttpControllerTypeResolver"/> to determine which controller types to register.
        /// </summary>
        /// <param name="container">The container the controllers should be registered in.</param>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use to get the Controller
        /// types to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null 
        /// reference (Nothing in VB).</exception>
        public static void RegisterWebApiControllers(this Container container, HttpConfiguration configuration)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(configuration, "configuration");

            var controllerTypes = GetControllerTypesFromConfiguration(configuration);

            controllerTypes.ForEach(type => container.Register(type, type));
        }

        /// <summary>
        /// Makes the current <see cref="T:System.Net.Http.HttpRequestMessage" /> resolvable by calling
        /// <see cref="GetCurrentHttpRequestMessage(Container)">GetCurrentHttpRequestMessage</see>.
        /// </summary>
        /// <param name="container">The container instance for which HttpRequestMessageTracking should be
        /// enabled.</param>
        /// <param name="configuration">The application's configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference 
        /// (Nothing in VB).</exception>
        public static void EnableHttpRequestMessageTracking(this Container container, 
            HttpConfiguration configuration)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(configuration, "configuration");

            if (!configuration.MessageHandlers.OfType<SimpleInjectorHttpRequestMessageHandler>().Any())
            {
                configuration.MessageHandlers.Add(new SimpleInjectorHttpRequestMessageHandler(container));
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
        /// is a null reference (Nothing in VB).</exception>
        public static HttpRequestMessage GetCurrentHttpRequestMessage(this Container container)
        {
            Requires.IsNotNull(container, "container");

            if (!httpRequestMessageTrackingEnabled)
            {
                throw new InvalidOperationException(
                    "Resolving the current HttpRequestMessage has not been enabled. Make sure " +
                    "container.EnableHttpRequestMessageTracking(GlobalConfiguration.Configuration) has " + 
                    "been called during startup.");
            }

            return SimpleInjectorHttpRequestMessageProvider.CurrentMessage;
        }

        private static List<Type> GetControllerTypesFromConfiguration(HttpConfiguration configuration)
        {
            IAssembliesResolver assembliesResolver = GetAssembliesResolver(configuration);

            IHttpControllerTypeResolver typeResolver = GetHttpControllerTypeResolver(configuration);

            return typeResolver.GetControllerTypes(assembliesResolver).ToList();
        }

        private static IAssembliesResolver GetAssembliesResolver(HttpConfiguration configuration)
        {
            try
            {
                return configuration.Services.GetAssembliesResolver();
            }
            catch (Exception ex)
            {
                // For a still unknown reason, the Services.GetAssembliesResolver can throw an exception.
                // See: https://stackoverflow.com/questions/27927199
                string message = string.Format(
                    "There was an error retrieving the {0}. {1}",
                    typeof(IAssembliesResolver).FullName,
                    ex.Message);

                throw new InvalidOperationException(message, ex);
            }
        }

        private static IHttpControllerTypeResolver GetHttpControllerTypeResolver(HttpConfiguration configuration)
        {
            try
            {
                return configuration.Services.GetHttpControllerTypeResolver();
            }
            catch (Exception ex)
            {
                string message = string.Format(
                    "There was an error retrieving the {0}. {1}",
                    typeof(IHttpControllerTypeResolver).FullName,
                    ex.Message);

                throw new InvalidOperationException(message, ex);
            }
        }

        private static Lifestyle GetLifestyle(bool dispose)
        {
            return dispose ? LifestyleWithDisposal : LifestyleNoDisposal;
        }
    }
}