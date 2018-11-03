#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2016-2018 Simple Injector Contributors
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
    using Microsoft.AspNetCore.Mvc.ViewComponents;

    /// <summary>
    /// View component activator for Simple Injector.
    /// </summary>
    public sealed class SimpleInjectorViewComponentActivator : IViewComponentActivator
    {
        private readonly ConcurrentDictionary<Type, InstanceProducer> viewComponentProducers =
            new ConcurrentDictionary<Type, InstanceProducer>();

        private readonly Container container;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorViewComponentActivator"/> class.
        /// </summary>
        /// <param name="container">The container instance.</param>
        public SimpleInjectorViewComponentActivator(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            this.container = container;
        }

        /// <summary>Creates a view component.</summary>
        /// <param name="context">The <see cref="ViewComponentContext"/> for the executing <see cref="ViewComponent"/>.</param>
        /// <returns>A view component instance.</returns>
        public object Create(ViewComponentContext context)
        {
            Type viewComponentType = context.ViewComponentDescriptor.TypeInfo.AsType();

            var producer = this.viewComponentProducers.GetOrAdd(viewComponentType, this.GetViewComponentProducer);

            if (producer == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "For the {0} to function properly, it requires all view components to be registered explicitly " +
                        "in Simple Injector, but a registration for {1} is missing. To ensure all view components are " +
                        "registered properly, call the RegisterMvcViewComponents extension method on the Container " +
                        "from within your Startup.Configure method while supplying the IApplicationBuilder " +
                        "instance, e.g. \"this.container.RegisterMvcViewComponents(app);\".{2}" +
                        "Full view component name: {3}.",
                        typeof(SimpleInjectorViewComponentActivator).Name,
                        viewComponentType.ToFriendlyName(),
                        Environment.NewLine,
                        viewComponentType.FullName));
            }

            return producer.GetInstance();
        }

        /// <summary>Releases the view component.</summary>
        /// <param name="context">The <see cref="ViewComponentContext"/> associated with the viewComponent.</param>
        /// <param name="viewComponent">The view component to release.</param>
        public void Release(ViewComponentContext context, object viewComponent)
        {
            // No-op.
        }

        // By searching through the current registrations, we ensure that the component is not auto-registered, because
        // that might cause it to be resolved from ASP.NET Core, in case auto cross-wiring is enabled.
        private InstanceProducer GetViewComponentProducer(Type viewComponentType) =>
            this.container.GetCurrentRegistrations().SingleOrDefault(r => r.ServiceType == viewComponentType);
    }
}