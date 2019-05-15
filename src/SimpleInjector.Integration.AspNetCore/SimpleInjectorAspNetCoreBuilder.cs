// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.AspNetCore
{
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector.Integration.ServiceCollection;

    /// <summary>
    /// Builder object returned by <see cref="SimpleInjectorAddOptionsAspNetCoreExtensions.AddAspNetCore"/>
    /// that allows additional integration options to be applied.
    /// </summary>
    public sealed class SimpleInjectorAspNetCoreBuilder
    {
        private readonly SimpleInjectorAddOptions options;

        internal SimpleInjectorAspNetCoreBuilder(SimpleInjectorAddOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> that contains the collection of framework components.
        /// </summary>
        /// <value>The <see cref="IServiceCollection"/> instance.</value>
        public IServiceCollection Services => this.options.Services;

        /// <summary>
        /// Gets the <see cref="Container"/> instance used by the application.
        /// </summary>
        /// <value>The <see cref="Container"/> instance.</value>
        public Container Container => this.options.Container;
    }
}