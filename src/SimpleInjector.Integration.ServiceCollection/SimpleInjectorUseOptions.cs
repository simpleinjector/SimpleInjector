// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector.Advanced;
    using SimpleInjector.Integration.ServiceCollection;

    /// <summary>
    /// Provides programmatic configuration for the Simple Injector on top of <see cref="IServiceProvider"/>.
    /// </summary>
    public class SimpleInjectorUseOptions : ApiObject
    {
        internal SimpleInjectorUseOptions(
            SimpleInjectorAddOptions builder, IServiceProvider applicationServices)
        {
            this.Builder = builder;
            this.ApplicationServices = applicationServices;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not Simple Injector should try to load framework
        /// components from the framework's configuration system or not. The default is <c>true</c>.
        /// </summary>
        /// <value>A boolean value.</value>
        public bool AutoCrossWireFrameworkComponents { get; set; } = true;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> that provides access to the framework's singleton
        /// services.
        /// </summary>
        /// <value>The <see cref="IServiceProvider"/> instance.</value>
        public IServiceProvider ApplicationServices { get; }

        /// <summary>
        /// Gets the application's Simple Injector <see cref="Container"/>.
        /// </summary>
        /// <value>The <see cref="Container"/> instance.</value>
        public Container Container => this.Builder.Container;

        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> that contains the collection of framework components.
        /// </summary>
        /// <value>The <see cref="IServiceCollection"/> instance.</value>
        public IServiceCollection Services => this.Builder.Services;

        internal SimpleInjectorAddOptions Builder { get; }
    }
}