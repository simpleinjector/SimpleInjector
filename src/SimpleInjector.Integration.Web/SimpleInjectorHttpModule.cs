// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.Web
{
    using System.Web;

    /// <summary>
    /// Simple Injector web integration HTTP Module. This module is registered automatically by ASP.NET when
    /// the assembly of this class is included in the application's bin folder. The module will trigger the
    /// disposing of created instances that are flagged as needing to be disposed at the end of the web 
    /// request.
    /// </summary>
    public sealed class SimpleInjectorHttpModule : IHttpModule
    {
        /// <summary>Initializes a module and prepares it to handle requests.</summary>
        /// <param name="context">An <see cref="HttpApplication"/> that provides access to the methods, 
        /// properties, and events common to all application objects within an ASP.NET application.</param>
        void IHttpModule.Init(HttpApplication context)
        {
            context.EndRequest += (sender, e) =>
            {
                WebRequestLifestyle.CleanUpWebRequest(HttpContext.Current);
            };
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements 
        /// <see cref="IHttpModule"/>.
        /// </summary>
        void IHttpModule.Dispose()
        {
        }
    }
}