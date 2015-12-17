#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015 Simple Injector Contributors
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