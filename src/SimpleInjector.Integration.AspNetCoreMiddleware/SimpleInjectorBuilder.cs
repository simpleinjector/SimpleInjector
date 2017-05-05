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

namespace Big.Paw.AgentService.SimpleInjectorMiddleware
{
    using Microsoft.AspNetCore.Builder;
    using System;
    using SimpleInjector;
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Generic;
    using System.Reflection;
    using SimpleInjector.Advanced;

    class SimpleInjectorBuilder
    {
        private Action<IApplicationBuilder> _next;
        private Action<IApplicationBuilder, Container> _setup;

        public SimpleInjectorBuilder(Action<IApplicationBuilder> next, Action<IApplicationBuilder, Container> setup)
        {
            _next = next;
            _setup = setup;
        }

        public void Build(IApplicationBuilder app)
        {
            var services = app
            .ApplicationServices
            .GetRequiredService<ServicesAccessor>()
            .GetServices();

            var container = new Container();

            container.Options.AllowOverridingRegistrations = true;
            container.Options.ConstructorResolutionBehavior = new LargestConstructorFinder();
            container.Options.DefaultScopedLifestyle = new SimpleInjector.Lifestyles.AsyncScopedLifestyle();
            container.Register<IServiceScopeFactory, SimpleInjectorServiceScopeFactory>();

            foreach (var svc in services)
                Add(container, svc);

            _setup(app, container);

                IServiceProvider provider = container;
#if(DEBUG)
                provider = new LoggingServiceProvider(provider)
#endif
            app.ApplicationServices = provider;

            app.Use(new SimpleInjectorAsyncScopeSetup(container).Setup);

            _next(app);
        }

        public void Add(Container _container, ServiceDescriptor svc)
        {
            var lifetime
                = svc.Lifetime == ServiceLifetime.Scoped
                ? Lifestyle.Scoped
                : svc.Lifetime == ServiceLifetime.Singleton
                ? Lifestyle.Singleton
                : Lifestyle.Transient;


            var type = svc.ImplementationType;
            var factory = svc.ImplementationFactory; // ?? DotNetFix(type);
            var instance = svc.ImplementationInstance;

            if (factory != null)
            {
                
                IServiceProvider provider = _container;
#if(DEBUG)
                provider = new LoggingServiceProvider(provider)
#endif
                _container.Register(svc.ServiceType, () => factory(provider), lifetime);
            }
            else if (type != null)
            {
                _container.Register(svc.ServiceType, type, lifetime);
            }
            else
            {
                _container.RegisterSingleton(svc.ServiceType, instance);
            }

            var generic = svc.ServiceType.GetTypeInfo().IsGenericTypeDefinition;
            var entype = typeof(IEnumerable<>).MakeGenericType(svc.ServiceType);
            var doMulti = !generic && type != null;

            if (doMulti)
            {
                _container.AppendToCollection(svc.ServiceType, type);
            }
        }
    }
}

