namespace SimpleInjector.Integration.AspNet
{
    using System;
    using System.Diagnostics;
    using Microsoft.AspNet.Mvc.ViewComponents;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// View component invoker factory for Simple Injector.
    /// </summary>
    public sealed class SimpleInjectorViewComponentInvokerFactory : IViewComponentInvokerFactory
    {
        private readonly Container container;

        private IViewComponentInvoker invoker;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorViewComponentInvokerFactory"/> class.
        /// </summary>
        /// <param name="container">The container instance.</param>
        public SimpleInjectorViewComponentInvokerFactory(Container container)
        {
            Requires.IsNotNull(container, nameof(container));
            this.container = container;
        }

        /// <summary>Creates a <see cref="IViewComponentInvoker"/>.</summary>
        /// <param name="context">The context</param>
        /// <returns>A  <see cref="IViewComponentInvoker"/>.</returns>
        public IViewComponentInvoker CreateInstance(ViewComponentContext context)
        {
            Requires.IsNotNull(context, nameof(context));

            if (this.invoker == null)
            {
                this.invoker = this.CreateViewComponentInvoker(context);
            }

            return this.invoker;
        }

        private SimpleInjectorViewComponentInvoker CreateViewComponentInvoker(ViewComponentContext context)
        {
            IServiceProvider provider = context.ViewContext.HttpContext.RequestServices;

            return new SimpleInjectorViewComponentInvoker(
                provider.GetRequiredService<DiagnosticSource>(),
                provider.GetRequiredService<ILoggerFactory>().CreateLogger<SimpleInjectorViewComponentInvoker>(),
                provider.GetRequiredService<IViewComponentActivator>(),
                this.container);
        }
    }
}