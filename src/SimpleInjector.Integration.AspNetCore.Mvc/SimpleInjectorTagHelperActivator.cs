#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2016 Simple Injector Contributors
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
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Razor.TagHelpers;

    /// <summary>Tag Helper Activator for Simple Injector.</summary>
    public class SimpleInjectorTagHelperActivator : ITagHelperActivator
    {
        private readonly Container container;
        private readonly Predicate<Type> tagHelperSelector;
        private readonly ITagHelperActivator frameworkTagHelperActivator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorTagHelperActivator"/> class.
        /// </summary>
        /// <param name="container">The container instance.</param>
        [Obsolete("This constructor is deprecated. Please use the other constructor overload or use the " +
            "SimpleInjectorAspNetCoreMvcIntegrationExtensions.AddSimpleInjectorTagHelperActivation " +
            "extension method instead.", 
            error: false)]
        public SimpleInjectorTagHelperActivator(Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            this.container = container;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorTagHelperActivator"/> class.
        /// </summary>
        /// <param name="container">The container instance.</param>
        /// <param name="tagHelperSelector">The predicate that determines which tag helpers should be created
        /// by the supplied <paramref name="container"/> (when the predicate returns true) or using the 
        /// supplied <paramref name="frameworkTagHelperActivator"/> (when the predicate returns false).</param>
        /// <param name="frameworkTagHelperActivator">The framework's tag helper activator.</param>
        public SimpleInjectorTagHelperActivator(Container container, Predicate<Type> tagHelperSelector,
            ITagHelperActivator frameworkTagHelperActivator)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (tagHelperSelector == null)
            {
                throw new ArgumentNullException(nameof(tagHelperSelector));
            }

            if (frameworkTagHelperActivator == null)
            {
                throw new ArgumentNullException(nameof(frameworkTagHelperActivator));
            }

            this.container = container;
            this.tagHelperSelector = tagHelperSelector;
            this.frameworkTagHelperActivator = frameworkTagHelperActivator;
        }

        /// <summary>Creates an <see cref="ITagHelper"/>.</summary>
        /// <typeparam name="TTagHelper">The <see cref="ITagHelper"/> type.</typeparam>
        /// <param name="context">The <see cref="ViewContext"/> for the executing view.</param>
        /// <returns>The tag helper.</returns>
        public TTagHelper Create<TTagHelper>(ViewContext context) where TTagHelper : ITagHelper =>
            this.tagHelperSelector(typeof(TTagHelper))
                ? (TTagHelper)this.container.GetInstance(typeof(TTagHelper))
                : this.frameworkTagHelperActivator.Create<TTagHelper>(context);
    }
}