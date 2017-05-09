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


namespace SimpleInjector.Integration.AspNetCoreMiddleware
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using System;
    using SimpleInjector;
    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;
    using System.Collections.Generic;
    using SimpleInjector.Advanced;

    class SimpleInjectorStartup : IStartup
    {
        private readonly IEnumerable<IApplicationContainerSetup> _controllerSetups;
        private readonly IEnumerable<IApplicationSetup> _appSetups;

        public SimpleInjectorStartup(IEnumerable<IApplicationContainerSetup> controllerSetups, IEnumerable<IApplicationSetup> appSetups)
        {
            _controllerSetups = controllerSetups;
            _appSetups = appSetups;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var container =  new Container();

            foreach(var setup in _controllerSetups)
            {
                container = setup.Setup(container, services);
            }

            // Make sure IServiceProvider is registered, it can be the Container directly.
            var provider = container.GetRequiredService<IServiceProvider>();
#if (DEBUG)
            provider = new LoggingServiceProvider(provider);
#endif
            return provider;
        }
        
        public void Configure(IApplicationBuilder app)
        {
            foreach(var setup in _appSetups)
            {
                setup.Configure(app);
            }
        }
    }
}

