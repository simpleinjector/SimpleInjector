// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.ServiceCollection
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Provides programmatic configuration for the Simple Injector on top of <see cref="IServiceCollection"/>.
    /// </summary>
    public sealed class SimpleInjectorAddOptions : ApiObject
    {
        private IServiceProviderAccessor serviceProviderAccessor;

        internal SimpleInjectorAddOptions(
            IServiceCollection services, Container container, IServiceProviderAccessor accessor)
        {
            this.Services = services;
            this.Container = container;
            this.serviceProviderAccessor = accessor;
        }

        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> that contains the collection of framework components.
        /// </summary>
        /// <value>The <see cref="IServiceCollection"/> instance.</value>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Gets the <see cref="Container"/> instance used by the application.
        /// </summary>
        /// <value>The <see cref="Container"/> instance.</value>
        public Container Container { get; }

        /// <summary>
        /// Gets or sets an <see cref="IServiceProviderAccessor"/> instance that will be used by Simple
        /// Injector to resolve cross-wired framework components.
        /// </summary>
        /// <value>The <see cref="IServiceProviderAccessor"/> instance.</value>
        /// <exception cref="ArgumentNullException">Thrown when a null value is provided.</exception>
        public IServiceProviderAccessor ServiceProviderAccessor
        {
            get
            {
                return this.serviceProviderAccessor;
            }

            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this.serviceProviderAccessor = value;
            }
        }
    }
}