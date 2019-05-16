// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

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

        /// <summary>
        /// Adds a middleware type to the application's request pipeline. The middleware will be resolved
        /// from Simple Injector. The middleware will be added to the container for verification.
        /// </summary>
        /// <param name="options">The <see cref="SimpleInjectorUseOptions"/>.</param>
        /// <param name="middleWareType">The middleware type.</param>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        public static void UseMiddleware(
            this SimpleInjectorUseOptions options, Type middleWareType, IApplicationBuilder app)
        {
            Requires.IsNotNull(options, nameof(options));
            Requires.IsNotNull(app, nameof(app));
            Requires.IsOfType<IMiddleware>(middleWareType, nameof(app));

            var container = options.Container;

            var lifestyle = container.Options.LifestyleSelectionBehavior.SelectLifestyle(middleWareType);

            // By creating an InstanceProducer up front, it will be known to the container, and will be part
            // of the verification process of the container.
            InstanceProducer<IMiddleware> producer =
                lifestyle.CreateProducer<IMiddleware>(middleWareType,container);

            app.Use((c, next) =>
            {
                IMiddleware middleware = producer.GetInstance();
                return middleware.InvokeAsync(c, _ => next());
            });
        }
    }
}