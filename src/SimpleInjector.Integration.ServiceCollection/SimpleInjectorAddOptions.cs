﻿// Copyright (c) Simple Injector Contributors. All rights reserved.
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
        private IServiceScopeFactory? serviceScopeFactory;
        private IServiceProvider? applicationServices;

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

        /// <summary>
        /// Gets or sets a value indicating whether or not Simple Injector should try to load framework
        /// components from the framework's configuration system or not. The default is <c>true</c>.
        /// </summary>
        /// <value>A boolean value.</value>
        public bool AutoCrossWireFrameworkComponents { get; set; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance that will be used by Simple Injector to resolve
        /// cross-wired framework components. It's value will be set when
        /// <see cref="SimpleInjectorServiceCollectionExtensions.UseSimpleInjector(IServiceProvider, Container)">UseSimpleInjector</see>
        /// is called, or when ASP.NET Core resolves its hosted services (whatever comes first).
        /// </summary>
        /// <value>The <see cref="IServiceProvider"/> instance.</value>
        internal IServiceProvider ApplicationServices
        { 
            get
            {
                if (this.applicationServices is null)
                {
                    // TODO: Awesome exceptions in construction
                    throw new InvalidOperationException("Great exceptions are king.");
                }

                return this.applicationServices;
            }
        }

        internal IServiceScopeFactory ServiceScopeFactory
        {
            get
            {
                if (this.serviceScopeFactory is null)
                {
                    this.serviceScopeFactory = this.ApplicationServices.GetService<IServiceScopeFactory>();

                    if (this.serviceScopeFactory is null)
                    {
                        // TODO: Make good exception message
                        throw new InvalidOperationException(
                            "This is extremely weird; your IServiceProvider implementation is broken.");
                    }
                }

                return this.serviceScopeFactory;
            }
        }

        internal void SetServiceProviderIfNull(IServiceProvider provider)
        {
            if (this.applicationServices is null)
            {
                this.applicationServices = provider;
            }
        }
    }
}