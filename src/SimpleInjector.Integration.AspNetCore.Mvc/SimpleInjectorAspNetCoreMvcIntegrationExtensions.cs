#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015-2016 Simple Injector Contributors
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

namespace SimpleInjector
{
    using System;
    using System.Reflection;
    using Integration.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Mvc.Razor.Internal;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension methods for integrating Simple Injector with ASP.NET Core MVC applications.
    /// </summary>
    public static class SimpleInjectorAspNetCoreMvcIntegrationExtensions
    {
         /// <summary>
        /// Registers a custom <see cref="SimpleInjectorTagHelperActivator"/> that allows the resolval of
        /// tag helpers using the <paramref name="container"/>. In case no <paramref name="applicationTypeSelector"/>
        /// is supplied, the custom tag helper activator will forward the creation of tag helpers that are not
        /// located in a "Microsoft*" namespace to Simple Injector.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> the custom tag helper activator should
        /// be registered in.</param>
        /// <param name="container">The container tag helpers should be resolved from.</param>
        /// <param name="applicationTypeSelector">An optional predicate that allows specifying which types
        /// should be resolved by Simple Injector (true) and which should be resolved by the framework (false).
        /// When not specified, all tag helpers whose namespace does not start with "Microsoft" will be forwarded
        /// to the Simple Injector container.</param>
        public static void AddSimpleInjectorTagHelperActivation(this IServiceCollection services, Container container,
            Predicate<Type> applicationTypeSelector = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            // There are tag helpers OOTB in MVC. Letting the application container try to create them will fail
            // because of the dependencies these tag helpers have. This means that OOTB tag helpers need to remain
            // created by the framework's DefaultTagHelperActivator, hence the selector predicate.
            applicationTypeSelector =
                applicationTypeSelector ?? (type => !type.GetTypeInfo().Namespace.StartsWith("Microsoft"));

            services.AddSingleton<ITagHelperActivator>(p => new SimpleInjectorTagHelperActivator(
                container,
                applicationTypeSelector,
                new DefaultTagHelperActivator(p.GetRequiredService<ITypeActivatorCache>())));
        }
    }
}