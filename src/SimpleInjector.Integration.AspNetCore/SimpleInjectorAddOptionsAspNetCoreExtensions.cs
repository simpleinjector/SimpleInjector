// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

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