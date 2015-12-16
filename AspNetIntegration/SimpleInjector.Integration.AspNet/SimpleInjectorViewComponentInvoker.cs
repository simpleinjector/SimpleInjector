namespace SimpleInjector.Integration.AspNet
{
    using System.Diagnostics;
    using Microsoft.AspNet.Mvc.ViewComponents;
    using Microsoft.Extensions.Logging;

    internal class SimpleInjectorViewComponentInvoker : _DefaultViewComponentInvoker
    {
        private readonly Container container;

        public SimpleInjectorViewComponentInvoker(DiagnosticSource source, ILogger logger, Container container)
            : base(source, logger)
        {
            this.container = container;
        }

        protected override object CreateComponent(ViewComponentContext context) =>
            this.container.GetInstance(context.ViewComponentDescriptor.Type);
    }
}