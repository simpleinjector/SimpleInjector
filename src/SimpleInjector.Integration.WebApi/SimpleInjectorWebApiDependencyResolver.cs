// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dependencies;
    using Lifestyles;

    /// <summary>
    /// Provides additional options for creating the <see cref="SimpleInjectorWebApiDependencyResolver"/>.
    /// </summary>
    public enum DependencyResolverScopeOption
    {
        /// <summary>
        /// When <see cref="IDependencyResolver.BeginScope"/> is called, an ambient
        /// <see cref="AsyncScopedLifestyle"/> scope is used, if one already exists. Otherwise, it
        /// creates a new <see cref="AsyncScopedLifestyle"/> scope before returning.
        /// This is the default value.
        /// </summary>
        UseAmbientScope = 0,

        /// <summary>
        /// A new <see cref="AsyncScopedLifestyle"/> scope  is always created by
        /// <see cref="IDependencyResolver.BeginScope"/> before returning.
        /// </summary>
        RequiresNew = 1
    }

    /// <summary>Simple Injector <see cref="IDependencyResolver"/> implementation.</summary>
    /// <example>
    /// The following example shows the usage of the <b>SimpleInjectorWebApiDependencyResolver</b> in an
    /// Web API application:
    /// <code lang="cs"><![CDATA[
    /// using System.Web.Http;
    /// using SimpleInjector;
    /// using SimpleInjector.Integration.WebApi;
    ///
    /// public static class WebApiConfig
    /// {
    ///     public static void Register(HttpConfiguration config)
    ///     {
    ///         var container = new Container();
    ///
    ///         // Make the container registrations, example:
    ///         // container.Register<IUserRepository, SqlUserRepository>();
    ///
    ///         container.RegisterWebApiControllers(config);
    ///         container.RegisterWebApiFilterProvider(config);
    ///
    ///         // Create a new SimpleInjectorDependencyResolver that wraps the,
    ///         // container, and register that resolver in MVC.
    ///
    ///         container.Verify();
    ///
    ///         config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
    ///
    ///         config.Routes.MapHttpRoute(
    ///             name: "DefaultApi",
    ///             routeTemplate: "api/{controller}/{id}",
    ///             defaults: new { id = RouteParameter.Optional }
    ///         );
    ///     }
    /// }
    /// ]]></code>
    /// The previous example show the use of the
    /// <see cref="SimpleInjectorWebApiExtensions.RegisterWebApiControllers(Container, HttpConfiguration)">RegisterWebApiControllers</see>
    /// extension methods and how the <b>SimpleInjectorWebApiDependencyResolver</b> can be used to set the created
    /// <see cref="Container"/> instance as default dependency resolver in Web API.
    /// </example>
    public sealed class SimpleInjectorWebApiDependencyResolver : IDependencyResolver
    {
        private readonly AsyncScopedLifestyle scopedLifestyle = new AsyncScopedLifestyle();
        private readonly Container container;
        private readonly DependencyResolverScopeOption scopeOption;
        private readonly Scope? scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorWebApiDependencyResolver"/> class with
        /// the default scope option (i.e. to use an ambient <see cref="AsyncScopedLifestyle"/>
        /// scope if one already exists).
        /// </summary>
        /// <param name="container">The container.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> parameter is
        /// a null reference.</exception>
        public SimpleInjectorWebApiDependencyResolver(Container container)
            : this(container, DependencyResolverScopeOption.UseAmbientScope)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorWebApiDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="scopeOption">The scoping option.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> parameter is
        /// a null reference.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when the
        /// <paramref name="scopeOption"/> contains an invalid value.</exception>
        public SimpleInjectorWebApiDependencyResolver(Container container,
            DependencyResolverScopeOption scopeOption)
            : this(container, beginScope: false)
        {
            Requires.IsNotNull(container, nameof(container));

            if (scopeOption < DependencyResolverScopeOption.UseAmbientScope ||
                scopeOption > DependencyResolverScopeOption.RequiresNew)
            {
                throw new System.ComponentModel.InvalidEnumArgumentException(
                    "scopeOption", (int)scopeOption, typeof(DependencyResolverScopeOption));
            }

            this.scopeOption = scopeOption;
        }

        private SimpleInjectorWebApiDependencyResolver(Container container, bool beginScope)
        {
            this.container = container;

            if (beginScope)
            {
                this.scope = AsyncScopedLifestyle.BeginScope(container);
            }
        }

        private IServiceProvider ServiceProvider => this.container;

        /// <summary>Starts a resolution scope.</summary>
        /// <returns>The dependency scope.</returns>
        IDependencyScope IDependencyResolver.BeginScope()
        {
            bool beginScope = this.scopeOption == DependencyResolverScopeOption.RequiresNew ||
                this.scopedLifestyle.GetCurrentScope(this.container) is null;

            return new SimpleInjectorWebApiDependencyResolver(this.container, beginScope);
        }

        /// <summary>Retrieves a service from the scope.</summary>
        /// <param name="serviceType">The service to be retrieved.</param>
        /// <returns>The retrieved service.</returns>
        object IDependencyScope.GetService(Type serviceType)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));

            // By calling GetInstance instead of GetService when resolving a controller, we prevent the
            // container from returning null when the controller isn't registered explicitly and can't be
            // created because of an configuration error. GetInstance will throw a descriptive exception
            // instead. Not doing this will cause Web API to throw a non-descriptive "Make sure that the
            // controller has a parameterless public constructor" exception.
            if (!serviceType.IsAbstract && typeof(IHttpController).IsAssignableFrom(serviceType))
            {
                return this.container.GetInstance(serviceType);
            }

            return this.ServiceProvider.GetService(serviceType);
        }

        /// <summary>Retrieves a collection of services from the scope.</summary>
        /// <param name="serviceType">The collection of services to be retrieved.</param>
        /// <returns>The retrieved collection of services.</returns>
        IEnumerable<object> IDependencyScope.GetServices(Type serviceType)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));

            Type collectionType = typeof(IEnumerable<>).MakeGenericType(serviceType);

            // The IDependencyResolver doesn't state what is expected from the returned enumerable. We,
            // therefore, simply assume it is correct to return a stream.
            var services = (IEnumerable<object>)this.ServiceProvider.GetService(collectionType);

            // NOTE: The contract of IDependencyScope isn't very clear, but Web API will break when null
            // is returned.
            return services ?? Enumerable.Empty<object>();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        public void Dispose()
        {
            // NOTE: Dispose is called by Web API outside the context of the CallContext in which it was
            // created (which is fucking awful btw and should be considered a design flaw).
            this.scope?.Dispose();
        }
    }
}