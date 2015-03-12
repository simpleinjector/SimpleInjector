#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2015 Simple Injector Contributors
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

namespace SimpleInjector.Integration.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dependencies;
    using SimpleInjector.Extensions.ExecutionContextScoping;

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
    /// <see cref="SimpleInjectorWebApiExtensions.RegisterWebApiControllers(Container, HttpConfiguration)">RegisterWebApiControllers</see> and
    /// <see cref="SimpleInjectorWebApiExtensions.RegisterWebApiFilterProvider">RegisterWebApiFilterProvider</see>
    /// extension methods and how the <b>SimpleInjectorWebApiDependencyResolver</b> can be used to set the created
    /// <see cref="Container"/> instance as default dependency resolver in Web API.
    /// </example>
    public sealed class SimpleInjectorWebApiDependencyResolver : IDependencyResolver
    {
        private readonly Container container;
        private readonly Scope scope;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorWebApiDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> parameter is
        /// a null reference (Nothing in VB).</exception>
        public SimpleInjectorWebApiDependencyResolver(Container container) : this(container, beginScope: false)
        {
            Requires.IsNotNull(container, "container");
        }

        private SimpleInjectorWebApiDependencyResolver(Container container, bool beginScope)
        {
            this.container = container;

            if (beginScope)
            {
                this.scope = container.BeginExecutionContextScope();
            }
        }

        /// <summary>Starts a resolution scope.</summary>
        /// <returns>The dependency scope.</returns>
        IDependencyScope IDependencyResolver.BeginScope()
        {
            return new SimpleInjectorWebApiDependencyResolver(this.container, 
                beginScope: this.container.GetCurrentExecutionContextScope() == null);
        }

        /// <summary>Retrieves a service from the scope.</summary>
        /// <param name="serviceType">The service to be retrieved.</param>
        /// <returns>The retrieved service.</returns>
        object IDependencyScope.GetService(Type serviceType)
        {
            // By calling GetInstance instead of GetService when resolving a controller, we prevent the
            // container from returning null when the controller isn't registered explicitly and can't be
            // created because of an configuration error. GetInstance will throw a descriptive exception
            // instead. Not doing this will cause Web API to throw a non-descriptive "Make sure that the 
            // controller has a parameterless public constructor" exception.
            if (!serviceType.IsAbstract && typeof(IHttpController).IsAssignableFrom(serviceType))
            {
                return this.container.GetInstance(serviceType);
            }

            return ((IServiceProvider)this.container).GetService(serviceType);
        }

        /// <summary>Retrieves a collection of services from the scope.</summary>
        /// <param name="serviceType">The collection of services to be retrieved.</param>
        /// <returns>The retrieved collection of services.</returns>
        IEnumerable<object> IDependencyScope.GetServices(Type serviceType)
        {
            return this.container.GetAllInstances(serviceType);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged 
        /// resources.
        /// </summary>
        public void Dispose()
        {
            if (this.scope != null)
            {
                this.scope.Dispose();
            }
        }
    }
}