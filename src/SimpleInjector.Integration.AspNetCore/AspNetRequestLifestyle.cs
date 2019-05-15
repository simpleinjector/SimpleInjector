// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.AspNetCore
{
    using System;
    using Lifestyles;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Defines a lifestyle that caches instances during the execution of a single ASP.NET Request.
    /// Unless explicitly stated otherwise, instances created by this lifestyle will be disposed at the end
    /// of the request. Do note that this lifestyle requires the
    /// <see cref="SimpleInjectorAspNetCoreIntegrationExtensions.UseSimpleInjectorAspNetRequestScoping(IServiceCollection, Container)">UseSimpleInjectorAspNetRequestScoping.</see>
    /// to be registered in the Web API configuration.
    /// </summary>
    /// <example>
    /// The following example shows the usage of the <b>AspNetRequestLifestyle</b> class:
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    /// container.Options.DefaultScopedLifestyle = new AspNetRequestLifestyle();
    ///
    /// container.Register<IUnitOfWork, EntityFrameworkUnitOfWork>(Lifestyle.Scoped);
    /// ]]></code>
    /// </example>
    [Obsolete("Please use SimpleInjector.Lifestyles.AsyncScopedLifestyle instead. " +
        "Will be removed in version 5.0.",
        error: true)]
    public sealed class AspNetRequestLifestyle : AsyncScopedLifestyle
    {
        /// <summary>Initializes a new instance of the <see cref="AspNetRequestLifestyle"/> class.
        /// The created and cached instance will be disposed when the Web API request ends, and when the
        /// created object implements <see cref="IDisposable"/>.
        /// </summary>
        public AspNetRequestLifestyle()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AspNetRequestLifestyle"/> class.</summary>
        /// <param name="disposeInstanceWhenScopeEnds">
        /// Specifies whether the created and cached instance will be disposed when the Web API request ends,
        /// and when the created object implements <see cref="IDisposable"/>.
        /// </param>
        [Obsolete("Please use SimpleInjector.Lifestyles.AsyncScopedLifestyle instead. " +
            "Will be removed in version 5.0.",
            error: true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public AspNetRequestLifestyle(bool disposeInstanceWhenScopeEnds)
        {
            throw new NotSupportedException(
                "This constructor overload has been deprecated. " +
                "Please use AspNetRequestLifestyle() instead.");
        }
    }
}