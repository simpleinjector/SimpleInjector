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
    using Microsoft.AspNetCore.Http;

    public static class SimpleInjectorMiddlewareExtensions
    {

        public static IWebHostBuilder ConfigureApplicationContainer(this IWebHostBuilder web, SetupCallDelegate setup)
        {
            return web.ConfigureServices(services => services.AddSingleton<IApplicationContainerSetup>(p => new DelegateAppContainerSetup(setup)));
        }

        public static IWebHostBuilder ConfigureApplicationSetup(this IWebHostBuilder web, Action<IApplicationBuilder> appSetup)
        {
            return web.ConfigureServices(services => services.AddSingleton<IApplicationSetup>(p => new DelegateApplicationSetup(appSetup)));
        }

        public static void RegisterWebHost(
            this Container hostingContainer,
            Func<IWebHostBuilder, IWebHostBuilder> webhostSetup,
            SetupCallDelegate setup,
            Action<IApplicationBuilder> appSetup)
        {
            hostingContainer.Register<IWebHost>(() =>
                webhostSetup(
                    ((IServiceProvider)hostingContainer)
                    .GetService<IWebHostBuilder>() ?? new WebHostBuilder()
                    .ConfigureServices(services => services.AddSingleton<IApplicationContainerSetup, DefaultOptionsAppContainerSetup>())
                    .ConfigureServices(services => services.AddSingleton<IApplicationContainerSetup, AddServiceRegistrationsAppContainerSetup>())
                    .ConfigureServices(services => services.AddSingleton<IApplicationContainerSetup, AddScopeFactoryAppContainerSetup>())
                    .ConfigureServices(services => services.AddSingleton<IApplicationContainerSetup, ContainerAsServiceProviderAppContainerSetup>())
                    .ConfigureApplicationContainer(setup)
                    .ConfigureApplicationSetup(appSetup)
                    //.ConfigureServices(services => services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>())
                    .UseStartup<SimpleInjectorStartup>()
                ).Build(), Lifestyle.Scoped);
        }
    }
}

