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


namespace SimpleInjector
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using System;
    using SimpleInjector;
    using SimpleInjector.Integration.AspNetCoreMiddleware;
    using Microsoft.Extensions.DependencyInjection;

    public static class SimpleInjectorMiddlewareExtensions
    {
        public static IWebHostBuilder UseSimpleInjector(this IWebHostBuilder web, Action<IApplicationBuilder, Container> setup)
        {
            return web.ConfigureServices(new SimpleInjectorSetup(setup).Setup);
        }

        /// <remarks>
        /// setup should include new WebHostBuilder().UseSimpleInjector(...)
        /// </remarks>
        public static void RegisterWebHost(this Container container, Func<IWebHostBuilder> webhostFactory)
        {
            container.Register<IWebHostBuilder>(() => webhostFactory().Configure(app => { }));
            container.Register<IWebHost>(() => container.GetInstance<IWebHostBuilder>().Build());
        }
        public static void RegisterWebHost(this Container container, Action<IApplicationBuilder, Container> setup)
        {
            container.RegisterWebHost(() => new WebHostBuilder().UseSimpleInjector(setup));
        }
        public static void RegisterWebHost(this Container container, Action<IWebHostBuilder> webhostSetup, Action<IApplicationBuilder, Container> setup)
        {
            container.RegisterWebHost(() =>
            {
                var host = new WebHostBuilder().UseSimpleInjector(setup);
                webhostSetup(host);
                return host;
            });
        }
    }
}

