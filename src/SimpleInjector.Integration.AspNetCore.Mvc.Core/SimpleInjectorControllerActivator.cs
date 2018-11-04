#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015-2018 Simple Injector Contributors
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

namespace SimpleInjector.Integration.AspNetCore.Mvc
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;

    /// <summary>Controller activator for Simple Injector.</summary>
    public sealed class SimpleInjectorControllerActivator : IControllerActivator
    {
        private readonly ConcurrentDictionary<Type, InstanceProducer> controllerProducers =
            new ConcurrentDictionary<Type, InstanceProducer>();

        private readonly Container container;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorControllerActivator"/> class.
        /// </summary>
        /// <param name="container">The container instance.</param>
        public SimpleInjectorControllerActivator(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            this.container = container;
        }

        /// <summary>Creates a controller.</summary>
        /// <param name="context">The Microsoft.AspNet.Mvc.ActionContext for the executing action.</param>
        /// <returns>A new controller instance.</returns>
        public object Create(ControllerContext context)
        {
            Type controllerType = context.ActionDescriptor.ControllerTypeInfo.AsType();

            var producer = this.controllerProducers.GetOrAdd(controllerType, this.GetControllerProducer);

            if (producer == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "For the {0} to function properly, it requires all controllers to be registered explicitly " +
                        "in Simple Injector, but a registration for {1} is missing. To ensure all controllers are " +
                        "registered properly, call the RegisterMvcControllers extension method on the Container " +
                        "from within your Startup.Configure method while supplying the IApplicationBuilder " +
                        "instance, e.g. \"this.container.RegisterMvcControllers(app);\".{2}" +
                        "Full controller name: {3}.",
                        typeof(SimpleInjectorControllerActivator).Name,
                        controllerType.ToFriendlyName(),
                        Environment.NewLine,
                        controllerType.FullName));
            }

            return producer.GetInstance();
        }

        /// <summary>Releases the controller.</summary>
        /// <param name="context">The Microsoft.AspNet.Mvc.ActionContext for the executing action.</param>
        /// <param name="controller">The controller instance.</param>
        public void Release(ControllerContext context, object controller)
        {
        }

        // By searching through the current registrations, we ensure that the controller is not auto-registered, because
        // that might cause it to be resolved from ASP.NET Core, in case auto cross-wiring is enabled.
        private InstanceProducer GetControllerProducer(Type controllerType) =>
            this.container.GetCurrentRegistrations().SingleOrDefault(r => r.ServiceType == controllerType);
    }
}