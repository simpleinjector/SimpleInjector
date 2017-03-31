#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015-2016 Simple Injector Contributors
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
    [Obsolete("This lifestyle is obsolete. Please use SimpleInjector.Lifestyles.AsyncScopedLifestyle instead.", error: false)]
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
        [Obsolete("This constructor overload has been deprecated. " +
            "Please use ExecutionContextScopeLifestyle() instead.",
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