// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.AspNetCore.Mvc
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    /// <summary>
    /// Provides methods to create a Razor Page model using Simple Injector.
    /// </summary>
    public class SimpleInjectorPageModelActivatorProvider : IPageModelActivatorProvider
    {
        private readonly ConcurrentDictionary<Type, InstanceProducer> pageModelProducers =
            new ConcurrentDictionary<Type, InstanceProducer>();

        private readonly Container container;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorPageModelActivatorProvider"/> class.
        /// </summary>
        /// <param name="container">The container instance that will be used to create page model instances.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/> is null.</exception>
        public SimpleInjectorPageModelActivatorProvider(Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            this.container = container;
        }

        /// <summary>
        /// Creates a Razor Page model activator.
        /// </summary>
        /// <param name="descriptor">The <see cref="CompiledPageActionDescriptor"/>.</param>
        /// <returns>The delegate used to activate the page model.</returns>
        public Func<PageContext, object> CreateActivator(CompiledPageActionDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (descriptor.ModelTypeInfo == null)
            {
                throw new ArgumentNullException(
                    nameof(descriptor.ModelTypeInfo) + " property cannot be null.",
                    nameof(descriptor));
            }

            Type pageModelType = descriptor.ModelTypeInfo.AsType();

            var producer = this.pageModelProducers.GetOrAdd(pageModelType, this.GetPageModelProducer);

            if (producer == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "For the {0} to function properly, it requires all page models to be registered explicitly " +
                        "in Simple Injector, but a registration for {1} is missing. To ensure all page models are " +
                        "registered properly, call the RegisterPageModels extension method on the Container " +
                        "from within your Startup.Configure method while supplying the IApplicationBuilder " +
                        "instance, e.g. \"this.container.RegisterPageModels(app);\".{2}" +
                        "Full page model name: {3}.",
                        typeof(SimpleInjectorPageModelActivatorProvider).Name,
                        pageModelType.ToFriendlyName(),
                        Environment.NewLine,
                        pageModelType.FullName));
            }
            
            return _ => producer.GetInstance();
        }

        /// <summary>
        /// Releases a Razor Page model.
        /// </summary>
        /// <param name="descriptor">The <see cref="CompiledPageActionDescriptor"/>.</param>
        /// <returns>The delegate used to dispose the activated Razor Page model or null.</returns>
        public Action<PageContext, object> CreateReleaser(CompiledPageActionDescriptor descriptor) => null;

        // By searching through the current registrations, we ensure that the page model is not auto-registered,
        // because that might cause it to be resolved from ASP.NET Core, in case auto cross-wiring is enabled.
        private InstanceProducer GetPageModelProducer(Type pageModelType) =>
            this.container.GetCurrentRegistrations().SingleOrDefault(r => r.ServiceType == pageModelType);
    }
}