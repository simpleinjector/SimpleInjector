#region Copyright (c) 2012 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2012 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

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
                SimpleInjectorWebExtensions.CleanUpWebRequest();
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