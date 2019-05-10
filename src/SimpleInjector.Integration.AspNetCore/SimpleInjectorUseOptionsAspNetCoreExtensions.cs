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
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension methods to finalize Simple Injector integration on top of ASP.NET Core.
    /// </summary>
    public static class SimpleInjectorUseOptionsAspNetCoreExtensions
    {
        /// <summary>
        /// Finalizes the configuration of Simple Injector on top of <see cref="IServiceCollection"/>. Will
        /// ensure framework components can be injected into Simple Injector-resolved components, unless
        /// <see cref="SimpleInjectorUseOptions.AutoCrossWireFrameworkComponents"/> is set to <c>false</c>
        /// using the <paramref name="setupAction"/>.
        /// </summary>
        /// <param name="app">The application's <see cref="IApplicationBuilder"/>.</param>
        /// <param name="container">The application's <see cref="Container"/> instance.</param>
        /// <param name="setupAction">An optional setup action.</param>
        /// <returns>The supplied <paramref name="app"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> or
        /// <paramref name="container"/> are null references.</exception>
        public static IApplicationBuilder UseSimpleInjector(
            this IApplicationBuilder app,
            Container container,
            Action<SimpleInjectorUseOptions> setupAction = null)
        {
            Requires.IsNotNull(app, nameof(app));
            Requires.IsNotNull(container, nameof(container));

            app.ApplicationServices.UseSimpleInjector(container, setupAction);

            return app;
        }

        /// <summary>
        /// Adds a middleware type to the application's request pipeline. The middleware will be resolved
        /// from Simple Injector. The middleware will be added to the container for verification.
        /// </summary>
        /// <typeparam name="TMiddleware">The middleware type.</typeparam>
        /// <param name="options">The <see cref="SimpleInjectorUseOptions"/>.</param>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        public static void UseMiddleware<TMiddleware>(
            this SimpleInjectorUseOptions options, IApplicationBuilder app)
            where TMiddleware : class, IMiddleware
        {
            Requires.IsNotNull(options, nameof(options));
            Requires.IsNotNull(app, nameof(app));

            var container = options.Container;

            var lifestyle = container.Options.LifestyleSelectionBehavior.SelectLifestyle(typeof(TMiddleware));

            // By creating an InstanceProducer up front, it will be known to the container, and will be part
            // of the verification process of the container.
            InstanceProducer<IMiddleware> producer =
                lifestyle.CreateProducer<IMiddleware, TMiddleware>(container);

            app.Use((c, next) =>
            {
                IMiddleware middleware = producer.GetInstance();
                return middleware.InvokeAsync(c, _ => next());
            });
        }
    }
}