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
        /// <see cref="SimpleInjectorUseOptions.AutoCrossWireFrameworkComponents"/> is set to <c>false</c>.
        /// </summary>
        /// <param name="app">The application's <see cref="IApplicationBuilder"/>.</param>
        /// <param name="container">The application's <see cref="Container"/> instance.</param>
        /// <returns>The supplied <paramref name="app"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> or
        /// <paramref name="container"/> are null references.</exception>
        public static IApplicationBuilder UseSimpleInjector(this IApplicationBuilder app, Container container)
        {
            Requires.IsNotNull(app, nameof(app));
            Requires.IsNotNull(container, nameof(container));

            app.ApplicationServices.UseSimpleInjector(container);

            return app;
        }

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
        //// I wanted to add this obsolete message in 4.8, but it was confusing considering the obsolete
        //// messages for everything on top of SimpleInjectorUseOptions. When those obsolete messages are
        //// resolved by the user, there is no harm in calling this method any longer. So it will get
        //// obsoleted in a later release.
        ////[Obsolete(
        ////    "You are supplying a setup action, but due breaking changes in ASP.NET Core 3, the Simple " +
        ////    "Injector container can get locked at an earlier stage, making it impossible to further setup " +
        ////    "the container at this stage. Please call the UseSimpleInjector(IApplicationBuilder, Container) " +
        ////    "overload instead. Take a look at the compiler warnings on the individual methods you are " +
        ////    "calling inside your setupAction delegate to understand how to migrate them. " +
        ////    " For more information, see: https://simpleinjector.org/aspnetcore. " +
        ////    "Will be treated as an error from version 4.10. Will be removed in version 5.0.",
        ////    error: false)]
        public static IApplicationBuilder UseSimpleInjector(
            this IApplicationBuilder app,
            Container container,
            Action<SimpleInjectorUseOptions>? setupAction)
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
        [Obsolete("Please call IApplicationBuilder.UseMiddleware<TMiddleware>(Container) instead. This " +
            "method can cause your middleware to be applied at the wrong stage in the pipeline. This will, " +
            "for instance, cause your middleware to be executed before the static files middleware (i.e. " +
            "the .UseStaticFiles() call) or before authorization is applied (i.e. the .UseAuthorization() " +
            "call). Instead, take care that you call .UseMiddleware<TMiddleware>(Container) at the right " +
            "stage. This typically means after .UseStaticFiles() and .UseAuthorization(), but before " +
            ".UseEndpoints(...). See https://simpleinjector.org/aspnetcore for more information. " +
            "Will be removed in version 5.0.",
            error: true)]
        public static void UseMiddleware<TMiddleware>(
            this SimpleInjectorUseOptions options, IApplicationBuilder app)
            where TMiddleware : class, IMiddleware
        {
            Requires.IsNotNull(options, nameof(options));
            Requires.IsNotNull(app, nameof(app));

            options.UseMiddleware(typeof(TMiddleware), app);
        }

        /// <summary>
        /// Adds a middleware type to the application's request pipeline. The middleware will be resolved
        /// from Simple Injector. The middleware will be added to the container for verification.
        /// </summary>
        /// <param name="options">The <see cref="SimpleInjectorUseOptions"/>.</param>
        /// <param name="middlewareType">The middleware type.</param>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        [Obsolete("Please call IApplicationBuilder.UseMiddleware(Type, Container) instead. This " +
            "method can cause your middleware to be applied at the wrong stage in the pipeline. This will, " +
            "for instance, cause your middleware to be executed before the static files middleware (i.e. " +
            "the .UseStaticFiles() call) or before authorization is applied (i.e. the .UseAuthorization() " +
            "call). Instead, take care that you call .UseMiddleware<TMiddleware>(Container) at the right " +
            "stage. This typically means after .UseStaticFiles() and .UseAuthorization(), but before " +
            ".UseEndpoints(...). See https://simpleinjector.org/aspnetcore for more information. " +
            "Will be removed in version 5.0.",
            error: true)]
        public static void UseMiddleware(
            this SimpleInjectorUseOptions options, Type middlewareType, IApplicationBuilder app)
        {
            Requires.IsNotNull(options, nameof(options));
            Requires.IsNotNull(middlewareType, nameof(middlewareType));
            Requires.IsNotNull(app, nameof(app));

            Requires.ServiceIsAssignableFromImplementation(
                typeof(IMiddleware), middlewareType, nameof(middlewareType));
            Requires.IsNotOpenGenericType(middlewareType, nameof(middlewareType));

            var container = options.Container;

            var lifestyle = container.Options.LifestyleSelectionBehavior.SelectLifestyle(middlewareType);

            // By creating an InstanceProducer up front, it will be known to the container, and will be part
            // of the verification process of the container.
            InstanceProducer<IMiddleware> producer =
                lifestyle.CreateProducer<IMiddleware>(middlewareType, container);

            app.Use((c, next) =>
            {
                IMiddleware middleware = producer.GetInstance();
                return middleware.InvokeAsync(c, _ => next());
            });
        }
    }
}