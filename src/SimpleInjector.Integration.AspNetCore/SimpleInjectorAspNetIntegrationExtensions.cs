#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015-2017 Simple Injector Contributors
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
    using System;
    using System.Linq;
    using System.Reflection;
    using Integration.AspNetCore;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector.Advanced;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Extension methods for integrating Simple Injector with ASP.NET applications.
    /// </summary>
    public static class SimpleInjectorAspNetCoreIntegrationExtensions
    {
        private static readonly object CrossWireContextKey = new object();
        private static readonly object ServiceScopeKey = new object();

        /// <summary>Wraps ASP.NET requests in an <see cref="AsyncScopedLifestyle"/>.</summary>
        /// <param name="applicationBuilder">The ASP.NET application builder instance that references all
        /// framework components.</param>
        /// <param name="container">The container.</param>
        [Obsolete(nameof(UseSimpleInjectorAspNetRequestScoping) + "(IApplicationBuilder, Container) " +
            "is deprecated. Please use " + 
            nameof(UseSimpleInjectorAspNetRequestScoping) + "(IServiceCollection, Container) " +
            "instead. This new overload can be called from within the ConfigureServices method of the " +
            "Startup class. See https://simpleinjector.org/aspnetcore for more information.", error: false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void UseSimpleInjectorAspNetRequestScoping(this IApplicationBuilder applicationBuilder,
            Container container)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            applicationBuilder.Use(async (context, next) =>
            {
                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    await next();
                }
            });
        }

        /// <summary>Wraps ASP.NET requests in an <see cref="AsyncScopedLifestyle"/>.</summary>
        /// <param name="services">The ASP.NET application builder instance that references all
        /// framework components.</param>
        /// <param name="container">The container.</param>
        public static void UseSimpleInjectorAspNetRequestScoping(this IServiceCollection services,
            Container container)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            services.AddSingleton<IStartupFilter>(new RequestScopingStartupFilter(container));
        }

        /// <summary>
        /// Get service of type <typeparamref name="T"/> from the list of request-specific services of the
        /// application builder. This preserves the lifestyle of the registered component.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <param name="builder">The IApplicationBuilder to retrieve the service object from.</param>
        /// <returns>A service object of type T or null if there is no such service.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the method is called outside the 
        /// context of a web request.</exception>
        public static T GetRequestService<T>(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return GetRequestServiceProvider(builder, typeof(T)).GetService<T>();
        }

        /// <summary>
        /// Get service of type <typeparamref name="T"/> from the list of request-specific services of the
        /// application builder. This preserves the lifestyle of the registered component.
        /// </summary>
        /// <typeparam name="T"> The type of service object to get.</typeparam>
        /// <param name="builder">The IApplicationBuilder to retrieve the service object from.</param>
        /// <returns>A service object of type T.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the method is called outside the 
        /// context of a web request, or when there is no service of type <typeparamref name="T"/>.</exception>
        public static T GetRequiredRequestService<T>(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return GetRequestServiceProvider(builder, typeof(T)).GetRequiredService<T>();
        }
        
        /// <summary>
        /// Enables ASP.NET Core services to be cross-wired in the Container. This method should be called 
        /// in the <b>ConfigureServices</b> method of the application's <b>Startup</b> class. When cross-wiring
        /// is enabled, individual cross-wire registrations can be made by calling 
        /// <see cref="CrossWire{TService}(Container, IApplicationBuilder)"/>.
        /// </summary>
        /// <param name="services">The ASP.NET application builder instance that references all
        /// framework components.</param>
        /// <param name="container">The container.</param>
        public static void EnableSimpleInjectorCrossWiring(this IServiceCollection services, Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (container.GetItem(CrossWireContextKey) == null)
            {
                container.SetItem(CrossWireContextKey, services);
            }            
        }

        /// <summary>
        /// Cross-wires an ASP.NET Core or third party service to the container, to allow the service to be
        /// injected into components that are built by the container.
        /// </summary>
        /// <typeparam name="TService">The type of service object to cross-wire.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="builder">The IApplicationBuilder to retrieve the service object from.</param>
        public static void CrossWire<TService>(this Container container, IApplicationBuilder builder)
            where TService : class
        {
            CrossWire(container, typeof(TService), builder);
        }

        /// <summary>
        /// Cross-wires an ASP.NET Core or third party service to the container, to allow the service to be
        /// injected into components that are built by the container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="serviceType">The type of service object to ross-wire.</param>
        /// <param name="builder">The IApplicationBuilder to retrieve the service object from.</param>
        public static void CrossWire(this Container container, Type serviceType, IApplicationBuilder builder)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            CrossWireServiceScope(container, builder);

            Registration registration = CreateRegistration(container, serviceType, builder);

            container.AddRegistration(serviceType, registration);
        }

        private static void CrossWireServiceScope(Container container, IApplicationBuilder builder)
        {
            if (container.Options.DefaultScopedLifestyle == null)
            {
                throw new InvalidOperationException(
                    "To be able to cross-wire a service with a transient or scoped lifestyle, " +
                    "please ensure that the container is configured with a default scoped lifestyle by " +
                    "setting the Container.Options.DefaultScopedLifestyle property with the required " +
                    "scoped lifestyle for your type of application. In ASP.NET Core, the typical " +
                    $"lifestyle to use is the {nameof(AsyncScopedLifestyle)}. " +
                    "See: https://simpleinjector.org/lifestyles#scoped");
            }

            if (container.GetItem(ServiceScopeKey) == null)
            {
                var scopeFactory = builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>();

                // We use unregistered type resolution, to allow the user to register IServiceScope manually
                // if he needs.
                container.ResolveUnregisteredType += (s, e) =>
                {
                    if (e.UnregisteredServiceType == typeof(IServiceScope) && !e.Handled)
                    {
                        e.Register(Lifestyle.Scoped.CreateRegistration(scopeFactory.CreateScope, container));
                    }
                };

                container.SetItem(ServiceScopeKey, new object());
            }
        }

        private static Registration CreateRegistration(
            Container container, Type serviceType, IApplicationBuilder builder)
        {
            IServiceCollection services = GetServiceCollection(container);

            Lifestyle lifestyle = DetermineLifestyle(serviceType, services);

            Registration registration;

            if (lifestyle == Lifestyle.Singleton)
            {
                registration = lifestyle.CreateRegistration(
                    serviceType,
                    () => builder.ApplicationServices.GetRequiredService(serviceType),
                    container);
            }
            else
            {
                IHttpContextAccessor accessor = GetHttpContextAccessor(builder);

                registration = lifestyle.CreateRegistration(
                    serviceType,
                    () => GetServiceProvider(accessor, container).GetRequiredService(serviceType),
                    container);
            }

            if (lifestyle == Lifestyle.Transient && typeof(IDisposable).IsAssignableFrom(serviceType))
            {
                registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent,
                    justification: "This is a cross-wired service. ASP.NET Core will ensure it gets disposed.");
            }

            return registration;
        }

        private static IServiceCollection GetServiceCollection(Container container)
        {
            var context = (IServiceCollection)container.GetItem(CrossWireContextKey);

            if (context == null)
            {
                throw new InvalidOperationException(
                    "Cross-wiring has to be enabled first. Please make sure the " +
                    $"{nameof(EnableSimpleInjectorCrossWiring)} extension method is called first by " +
                    "adding it to the ConfigureServices method as follows: " + Environment.NewLine +
                    $"services.{nameof(EnableSimpleInjectorCrossWiring)}(container);" + Environment.NewLine +
                    "See: https://simpleinjector.org/aspnetcore");
            }

            return context;
        }

        private static IServiceProvider GetServiceProvider(IHttpContextAccessor accessor, Container container)
        {
            // Pull the IServiceProvider from the current request. If there is no request, pull it from an 
            // IServiceScope that that will be managed by Simple Injector as scoped registration
            // (see EnsureServiceScopeIsRegistered).
            return accessor.HttpContext?.RequestServices ?? container.GetInstance<IServiceScope>().ServiceProvider;
        }

        private static Lifestyle DetermineLifestyle(Type serviceType, IServiceCollection services)
        {
            var descriptor = FindDescriptor(serviceType, services);

            // In case the service type is an IEnumerable, a registration can't be found, but collections are
            // in Core always registered as Transient, so it's safe to fall back to the transient lifestyle.
            return ToLifestyle(descriptor?.Lifetime ?? ServiceLifetime.Transient);
        }

        private static ServiceDescriptor FindDescriptor(Type serviceType, IServiceCollection services)
        {
            var descriptor = services.LastOrDefault(d => d.ServiceType == serviceType);

            if (descriptor == null && serviceType.GetTypeInfo().IsGenericType)
            {
                var serviceTypeDefinition = serviceType.GetTypeInfo().GetGenericTypeDefinition();

                // In case the type is an IEnumerable<T>, no registration can be found and null is returned.
                return services.LastOrDefault(d => d.ServiceType == serviceTypeDefinition);
            }
            else
            {
                return descriptor;
            }
        }
        
        private static Lifestyle ToLifestyle(ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton: return Lifestyle.Singleton;
                case ServiceLifetime.Scoped: return Lifestyle.Scoped;
                default: return Lifestyle.Transient;
            }
        }

        private static IServiceProvider GetRequestServiceProvider(IApplicationBuilder builder, Type serviceType)
        {
            IHttpContextAccessor accessor = GetHttpContextAccessor(builder);

            var context = accessor.HttpContext;

            if (context == null)
            {
                throw new InvalidOperationException(
                    $"Unable to request service '{serviceType.ToFriendlyName()} from ASP.NET Core request services." +
                    "Please make sure this method is called within the context of an active HTTP request.");
            }

            return context.RequestServices;
        }

        private static IHttpContextAccessor GetHttpContextAccessor(IApplicationBuilder builder)
        {
            var accessor = builder.ApplicationServices.GetService<IHttpContextAccessor>();

            if (accessor == null)
            {
                throw new InvalidOperationException(
                    "Type 'Microsoft.AspNetCore.Http.IHttpContextAccessor' is not available in the " +
                    "IApplicationBuilder.ApplicationServices collection. Please make sure it is " +
                    "registered by adding it to the ConfigureServices method as follows: " + Environment.NewLine +
                    "services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();");
            }

            return accessor;
        }
    }
}