namespace SimpleInjector.Integration.AspNet
{
    using System;
    using System.Diagnostics;
    using Microsoft.AspNet.Mvc.ViewComponents;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public sealed class SimpleInjectorViewComponentInvokerFactory : IViewComponentInvokerFactory
    {
        private readonly Container container;

        private IViewComponentInvoker invoker;

        public SimpleInjectorViewComponentInvokerFactory(Container container)
        {
            this.container = container;
        }

        public IViewComponentInvoker CreateInstance(ViewComponentContext context)
        {
            Requires.IsNotNull(context, nameof(context));

            if (this.invoker == null)
            {
                IServiceProvider provider = context.ViewContext.HttpContext.RequestServices;

                this.invoker = new SimpleInjectorViewComponentInvoker(
                    provider.GetRequiredService<DiagnosticSource>(),
                    provider.GetRequiredService<ILoggerFactory>().CreateLogger<DefaultViewComponentInvoker>(),
                    this.container);
            }

            return this.invoker;
        }
    }
}