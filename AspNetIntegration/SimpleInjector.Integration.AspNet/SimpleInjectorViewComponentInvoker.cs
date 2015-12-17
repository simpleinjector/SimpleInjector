namespace SimpleInjector.Integration.AspNet
{
    using System.Diagnostics;
    using Microsoft.AspNet.Mvc.ViewComponents;
    using Microsoft.Extensions.Logging;

    internal class SimpleInjectorViewComponentInvoker : _DefaultViewComponentInvoker
    {
        private readonly IViewComponentActivator viewComponentActivator;
        private readonly Container container;

        public SimpleInjectorViewComponentInvoker(DiagnosticSource source, ILogger logger,
            IViewComponentActivator viewComponentActivator, Container container)
            : base(source, logger)
        {
            this.viewComponentActivator = viewComponentActivator;
            this.container = container;
        }

        protected override object CreateComponent(ViewComponentContext context)
        {
            var component = this.container.GetInstance(context.ViewComponentDescriptor.Type);
            this.viewComponentActivator.Activate(component, context);
            return component;
        }
    }
}