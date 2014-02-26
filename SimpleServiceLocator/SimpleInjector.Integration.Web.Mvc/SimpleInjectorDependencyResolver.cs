#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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

namespace SimpleInjector.Integration.Web.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    /// <summary>MVC <see cref="IDependencyResolver"/> for Simple Injector.</summary>
    /// <example>
    /// The following example shows the usage of the <b>SimpleInjectorDependencyResolver</b> in an
    /// MVC application:
    /// <code lang="cs"><![CDATA[
    /// public class MvcApplication : System.Web.HttpApplication
    /// {
    ///     protected void Application_Start()
    ///     {
    ///         var container = new Container();
    /// 
    ///         // Make the container registrations, example:
    ///         // container.Register<IUserRepository, SqlUserRepository>();
    /// 
    ///         container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
    ///         container.RegisterMvcIntegratedFilterProvider();
    ///
    ///         // Create a new SimpleInjectorDependencyResolver that wraps the,
    ///         // container, and register that resolver in MVC.
    ///         System.Web.Mvc.DependencyResolver.SetResolver(
    ///             new SimpleInjectorDependencyResolver(container));
    /// 
    ///         // Normal MVC stuff here
    ///         AreaRegistration.RegisterAllAreas();
    /// 
    ///         RegisterGlobalFilters(GlobalFilters.Filters);
    ///         RegisterRoutes(RouteTable.Routes);
    ///     }
    /// }
    /// ]]></code>
    /// The previous example show the use of the 
    /// <see cref="SimpleInjectorMvcExtensions.RegisterMvcControllers">RegisterMvcControllers</see> and
    /// <see cref="SimpleInjectorMvcExtensions.RegisterMvcIntegratedFilterProvider">RegisterMvcIntegratedFilterProvider</see>
    /// extension methods and how the <b>SimpleInjectorDependencyResolver</b> can be used to set the created
    /// <see cref="Container"/> instance as default dependency resolver in MVC.
    /// </example>
    public class SimpleInjectorDependencyResolver : IDependencyResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/> is a null
        /// reference.</exception>
        public SimpleInjectorDependencyResolver(Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            this.Container = container;
        }

        /// <summary>Gets the container.</summary>
        /// <value>The <see cref="Container"/>.</value>
        public Container Container { get; private set; }

        /// <summary>Resolves singly registered services that support arbitrary object creation.</summary>
        /// <param name="serviceType">The type of the requested service or object.</param>
        /// <returns>The requested service or object.</returns>
        public object GetService(Type serviceType)
        {
            // By calling GetInstance instead of GetService when resolving a controller, we prevent the
            // container from returning null when the controller isn't registered explicitly and can't be
            // created because of an configuration error. GetInstance will throw a descriptive exception
            // instead. Not doing this will cause MVC to throw a non-descriptive "Make sure that the 
            // controller has a parameterless public constructor" exception.
            if (!serviceType.IsAbstract && typeof(IController).IsAssignableFrom(serviceType))
            {
                return this.Container.GetInstance(serviceType);
            }

            return ((IServiceProvider)this.Container).GetService(serviceType);
        }

        /// <summary>Resolves multiply registered services.</summary>
        /// <param name="serviceType">The type of the requested services.</param>
        /// <returns>The requested services.</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return this.Container.GetAllInstances(serviceType);
        }
    }
}