// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using SimpleInjector.Integration.ServiceCollection;

    /// <summary>
    /// Extension methods for integrating Simple Injector with Generic Hosts.
    /// </summary>
    public static class SimpleInjectorGenericHostExtensions
    {
        /// <summary>
        /// Registers the given <typeparamref name="THostedService"/> in the Container as Singleton and
        /// adds it to the host's pipeline of hosted services.
        /// </summary>
        /// <typeparam name="THostedService">An <see cref="IHostedService"/> to register.</typeparam>
        /// <param name="options">The options.</param>
        /// <returns>The <paramref name="options"/>.</returns>
        public static SimpleInjectorAddOptions AddHostedService<THostedService>(
            this SimpleInjectorAddOptions options)
            where THostedService : class, IHostedService
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var registration = Lifestyle.Singleton.CreateRegistration<THostedService>(options.Container);

            // Let the built-in configuration system dispose this instance.
            registration.SuppressDisposal = true;

            options.Container.AddRegistration<THostedService>(registration);

            options.Services.AddSingleton<IHostedService>(
                _ => options.Container.GetInstance<THostedService>());

            return options;
        }

        /// <summary>
        /// Finalizes the configuration of Simple Injector on top of <see cref="IHost"/>.
        /// Ensures framework components can be injected into Simple Injector-resolved components, unless
        /// <see cref="SimpleInjectorUseOptions.AutoCrossWireFrameworkComponents"/> is set to <c>false</c>
        /// using the <paramref name="setupAction"/>.
        /// </summary>
        /// <param name="host">The application's <see cref="IHost"/>.</param>
        /// <param name="container">The application's <see cref="Container"/> instance.</param>
        /// <param name="setupAction">An optional setup action.</param>
        /// <returns>The supplied <paramref name="host"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> or
        /// <paramref name="container"/> are null references.</exception>
        public static IHost UseSimpleInjector(
            this IHost host,
            Container container,
            Action<SimpleInjectorUseOptions> setupAction = null)
        {
            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            host.Services.UseSimpleInjector(container, setupAction);

            return host;
        }
    }
}