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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector.Advanced;


    public interface IApplicationContainerSetup
    {
        Container Setup(Container applicationContainer, IServiceCollection services);
    }

    public  delegate Container SetupCallDelegate(Container applicationContainer, IServiceCollection services);

    public class DelegateAppContainerSetup : IApplicationContainerSetup
    {
        private readonly SetupCallDelegate _call;

        public DelegateAppContainerSetup(SetupCallDelegate call)
        {
            _call = call;
        }
        public Container Setup(Container applicationContainer, IServiceCollection services)
        {
            return _call(applicationContainer, services);            
        }
    }

    public class DefaultOptionsAppContainerSetup : IApplicationContainerSetup
    {
        public Container Setup(Container container, IServiceCollection services)
        {
            container.Options.SuppressLifestyleMismatchVerification = true;
            container.Options.AllowOverridingRegistrations = true;
            container.Options.ConstructorResolutionBehavior = LargestConstructorFinder.Instance;
            container.Options.DefaultScopedLifestyle = new SimpleInjector.Lifestyles.AsyncScopedLifestyle();
            return container;
        }
    }

    public class AddScopeFactoryAppContainerSetup : IApplicationContainerSetup
    {
        public Container Setup(Container container, IServiceCollection services)
        {
            container.Register<IServiceScopeFactory, SimpleInjectorServiceScopeFactory>();
            return container;
        }
    }
    public class ContainerAsServiceProviderAppContainerSetup : IApplicationContainerSetup
    {
        public Container Setup(Container container, IServiceCollection services)
        {
            container.RegisterSingleton<IServiceProvider>(container);
            return container;
        }
    }

    public class AddServiceRegistrationsAppContainerSetup : IApplicationContainerSetup
    {
        public Container Setup(Container container, IServiceCollection services)
        {
            foreach (var svc in services)
                Add(container, svc);

            return container;
        }
        
        public static void Add(Container container, ServiceDescriptor svc)
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

                IServiceProvider provider = container;
#if(DEBUG)
                provider = new LoggingServiceProvider(provider);
#endif
                container.Register(svc.ServiceType, () => factory(provider), lifetime);
            }
            else if (type != null)
            {
                container.Register(svc.ServiceType, type, lifetime);
            }
            else
            {
                container.RegisterSingleton(svc.ServiceType, instance);
            }

            var generic = svc.ServiceType.GetTypeInfo().IsGenericTypeDefinition;
            var entype = typeof(IEnumerable<>).MakeGenericType(svc.ServiceType);
            var doMulti = !generic && type != null;

            if (doMulti)
            {
                container.AppendToCollection(svc.ServiceType, type);
            }
        }
    }
}