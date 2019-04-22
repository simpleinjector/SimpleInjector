#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2019 Simple Injector Contributors
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
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using SimpleInjector.Integration.AspNetCore;
    using SimpleInjector.Integration.ServiceCollection;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Extensions for configuring Simple Injector with ASP.NET Core using 
    /// <see cref="SimpleInjectorAddOptions"/>.
    /// </summary>
    public static class SimpleInjectorAddOptionsAspNetCoreExtensions
    {
        /// <summary>
        /// Adds basic Simple Injector integration for ASP.NET Core and returns a builder object that allow
        /// additional integration options to be applied. These basic integrations includes wrapping each web
        /// request in an <see cref="AsyncScopedLifestyle"/> scope and making the nessesary changes that make
        /// it possible for enabling the injection of framework components in Simple Injector-constructed
        /// components when <see cref="SimpleInjectorServiceCollectionExtensions.UseSimpleInjector"/> is called.
        /// </summary>
        /// <param name="options">The options to which the integration should be applied.</param>
        /// <returns>A new <see cref="SimpleInjectorAspNetCoreBuilder"/> instance that allows additional
        /// configurations to be made.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static SimpleInjectorAspNetCoreBuilder AddAspNetCore(this SimpleInjectorAddOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            IServiceCollection services = options.Services;

            var container = options.Container;

            // Add the IHttpContextAccessor to allow Simple Injector cross wiring to work in ASP.NET Core.
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Replace the default IServiceProviderAccessor with on that can use IHttpContextAccessor to
            // resolve instances that are scoped inside the current request.
            options.ServiceProviderAccessor = new AspNetCoreServiceProviderAccessor(
                new HttpContextAccessor(),
                options.ServiceProviderAccessor);

            services.UseSimpleInjectorAspNetRequestScoping(container);

            return new SimpleInjectorAspNetCoreBuilder(options);
        }
    }
}