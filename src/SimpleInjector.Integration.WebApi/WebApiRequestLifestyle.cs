// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.WebApi
{
    using System;
    using Lifestyles;

    /// <summary>
    /// Defines a lifestyle that caches instances during the execution of a single ASP.NET Web API Request.
    /// Unless explicitly stated otherwise, instances created by this lifestyle will be disposed at the end
    /// of the Web API request. Do note that this lifestyle requires the
    /// <see cref="SimpleInjectorWebApiDependencyResolver"/> to be registered in the Web API configuration.
    /// </summary>
    /// <example>
    /// The following example shows the usage of the <b>WebApiRequestLifestyle</b> class:
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    /// container.Options.DefaultScopedLifestyle = new WebApiRequestLifestyle();
    /// container.Register<IUnitOfWork, EntityFrameworkUnitOfWork>(Lifestyle.Scoped);
    /// ]]></code>
    /// </example>
    [Obsolete("Please use SimpleInjector.Lifestyles.AsyncScopedLifestyle instead. " +
        "Will be removed in version 5.0.",
        error: true)]
    public sealed class WebApiRequestLifestyle : AsyncScopedLifestyle
    {
        /// <summary>Initializes a new instance of the <see cref="WebApiRequestLifestyle"/> class.
        /// The created and cached instance will be disposed when the Web API request ends, and when the
        /// created object implements <see cref="IDisposable"/>.
        /// </summary>
        public WebApiRequestLifestyle()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="WebApiRequestLifestyle"/> class.</summary>
        /// <param name="disposeInstanceWhenScopeEnds">
        /// Specifies whether the created and cached instance will be disposed when the Web API request ends,
        /// and when the created object implements <see cref="IDisposable"/>.
        /// </param>
        [Obsolete("Please use WebApiRequestLifestyle() instead. " +
            "Will be removed in version 5.0.",
            error: true)]
        public WebApiRequestLifestyle(bool disposeInstanceWhenScopeEnds) : this()
        {
            throw new NotSupportedException(
                "This constructor overload has been deprecated. " +
                "Please use WebApiRequestLifestyle() instead.");
        }
    }
}