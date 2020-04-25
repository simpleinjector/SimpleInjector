// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using Microsoft.AspNetCore.Builder;
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
    }
}