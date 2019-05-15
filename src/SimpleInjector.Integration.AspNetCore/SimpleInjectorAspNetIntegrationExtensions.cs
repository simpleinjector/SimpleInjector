// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

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
        [Obsolete("Please use " +
            nameof(UseSimpleInjectorAspNetRequestScoping) + "(IServiceCollection, Container) " +
            "instead. This new overload can be called from within the ConfigureServices method of the " +
            "Startup class. See https://simpleinjector.org/aspnetcore for more information. " +
            "Will be removed in version 5.0.",
            error: true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void UseSimpleInjectorAspNetRequestScoping(
            this IApplicationBuilder applicationBuilder, Container container)
        {
            Requires.IsNotNull(applicationBuilder, nameof(applicationBuilder));
            Requires.IsNotNull(container, nameof(container));

            applicationBuilder.Use(async (_, next) =>
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
        public static void UseSimpleInjectorAspNetRequestScoping(
            this IServiceCollection services, Container container)
        {
            Requires.IsNotNull(services, nameof(services));
            Requires.IsNotNull(container, nameof(container));

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
            Requires.IsNotNull(builder, nameof(builder));

            return GetRequestServiceProvider(builder.GetApplicationServices(), typeof(T)).GetService<T>();
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
            Requires.IsNotNull(builder, nameof(builder));

            return GetRequestServiceProvider(
                builder.GetApplicationServices(), typeof(T)).GetRequiredService<T>();
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
        public static void EnableSimpleInjectorCrossWiring(
            this IServiceCollection services, Container container)
        {
            Requires.IsNotNull(services, nameof(services));
            Requires.IsNotNull(container, nameof(container));

            if (container.ContainerScope.GetItem(CrossWireContextKey) is null)
            {
                container.ContainerScope.SetItem(CrossWireContextKey, services);
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
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(builder, nameof(builder));

            CrossWireServiceScope(container, builder.GetApplicationServices());

            Registration registration =
                CreateCrossWireRegistration(container, serviceType, builder.GetApplicationServices());

            container.AddRegistration(serviceType, registration);
        }

        // WARNING: Although most of the extension methods in this class can become obsolete, because of the
        // new fluent API, this method should not be obsoleted, as there is no alternative for the following
        // call: app.Map("/api/queries", builder => builder.UseMiddleware<QueryHandlerMiddleware>(container));
        /// <summary>
        /// Adds a middleware type to the application's request pipeline. The middleware will be resolved from
        /// the supplied the Simple Injector <paramref name="container"/>. The middleware will be added to the
        /// container for verification.
        /// </summary>
        /// <typeparam name="TMiddleware">The middleware type.</typeparam>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        /// <param name="container">The container to resolve <typeparamref name="TMiddleware"/> from.</param>
        /// <returns>The supplied <see cref="IApplicationBuilder"/> instance.</returns>
        public static IApplicationBuilder UseMiddleware<TMiddleware>(
            this IApplicationBuilder app, Container container)
            where TMiddleware : class, IMiddleware
        {
            Requires.IsNotNull(app, nameof(app));
            Requires.IsNotNull(container, nameof(container));

            var lifestyle = container.Options.LifestyleSelectionBehavior.SelectLifestyle(typeof(TMiddleware));

            // By creating an InstanceProducer up front, it will be known to the container, and will be part
            // of the verification process of the container.
            InstanceProducer<IMiddleware> producer =
                lifestyle.CreateProducer<IMiddleware, TMiddleware>(container);

            app.Use((c, next) =>
            {
                IMiddleware middleware = producer.GetInstance();
                return middleware.InvokeAsync(c, _ => next());
            });

            return app;
        }

        /// <summary>
        /// Allows registrations made using the <see cref="IServiceCollection"/> API to be resolved by Simple
        /// Injector.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        public static void AutoCrossWireAspNetComponents(this Container container, IApplicationBuilder app)
        {
            Requires.IsNotNull(app, nameof(app));
            Requires.IsNotNull(container, nameof(container));

            container.AutoCrossWireAspNetComponents(app.GetApplicationServices());
        }

        /// <summary>
        /// Allows registrations made using the <see cref="IServiceCollection"/> API to be resolved by Simple
        /// Injector.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="appServices">The <see cref="IServiceProvider"/> instance that provides the set of
        /// singleton services.</param>
        public static void AutoCrossWireAspNetComponents(
            this Container container, IServiceProvider appServices)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(appServices, nameof(appServices));

            var services = (IServiceCollection?)container.ContainerScope.GetItem(CrossWireContextKey);

            if (services is null)
            {
                throw new InvalidOperationException(
                    "To use this method, please make sure cross-wiring is enabled, by invoking " +
                    $"the {nameof(EnableSimpleInjectorCrossWiring)} extension method as part of the " +
                    "ConfigureServices method of the Startup class. " + Environment.NewLine +
                    "See https://simpleinjector.org/aspnetcore for more information.");
            }

            if (container.Options.DefaultScopedLifestyle is null)
            {
                throw new InvalidOperationException(
                    "To be able to allow auto cross-wiring, please ensure that the container is configured " +
                    "with a default scoped lifestyle by setting the Container.Options" +
                    ".DefaultScopedLifestyle property with the required scoped lifestyle for your type of " +
                    "application. In ASP.NET Core, the typical lifestyle to use is the " +
                    $"{nameof(AsyncScopedLifestyle)}. " + Environment.NewLine +
                    "See: https://simpleinjector.org/lifestyles#scoped");
            }

            CrossWireServiceScope(container, appServices);

            container.ResolveUnregisteredType += (s, e) =>
            {
                if (e.Handled)
                {
                    return;
                }

                Type serviceType = e.UnregisteredServiceType;

                ServiceDescriptor descriptor = FindServiceDescriptor(services, serviceType);

                if (descriptor != null)
                {
                    Lifestyle lifestyle = ToLifestyle(descriptor.Lifetime);

                    Registration registration = lifestyle == Lifestyle.Singleton
                        ? CreateSingletonRegistration(container, serviceType, appServices)
                        : CreateNonSingletonRegistration(container, serviceType, appServices, lifestyle);

                    e.Register(registration);
                }
            };
        }

        private static IServiceProvider GetApplicationServices(this IApplicationBuilder builder)
        {
            var appServices = builder.ApplicationServices;

            if (appServices == null)
            {
                throw new ArgumentNullException(nameof(builder) + ".ApplicationServices");
            }

            return appServices;
        }

        private static ServiceDescriptor FindServiceDescriptor(IServiceCollection services, Type serviceType)
        {
            // In case there are multiple descriptors for a given type, .NET Core will use the last descriptor
            // when one instance is resolved. We will have to get this last one as well.
            ServiceDescriptor descriptor = services.LastOrDefault(d => d.ServiceType == serviceType);

            if (descriptor == null && serviceType.GetTypeInfo().IsGenericType)
            {
                // In case the registration is made as open-generic type, the previous query will return null,
                // and we need to go find the last open generic registration for the service type.
                var serviceTypeDefinition = serviceType.GetTypeInfo().GetGenericTypeDefinition();
                descriptor = services.LastOrDefault(d => d.ServiceType == serviceTypeDefinition);
            }

            return descriptor;
        }

        private static void CrossWireServiceScope(Container container, IServiceProvider appServices)
        {
            if (container.Options.DefaultScopedLifestyle == null)
            {
                throw new InvalidOperationException(
                    "To be able to cross-wire a service with a transient or scoped lifestyle, " +
                    "please ensure that the container is configured with a default scoped lifestyle by " +
                    "setting the Container.Options.DefaultScopedLifestyle property with the required " +
                    "scoped lifestyle for your type of application. In ASP.NET Core, the typical " +
                    $"lifestyle to use is the {nameof(AsyncScopedLifestyle)}. " + Environment.NewLine +
                    "See: https://simpleinjector.org/lifestyles#scoped");
            }

            if (container.ContainerScope.GetItem(ServiceScopeKey) is null)
            {
                var scopeFactory = appServices.GetRequiredService<IServiceScopeFactory>();

                // We use unregistered type resolution, to allow the user to register IServiceScope manually
                // if he needs.
                container.ResolveUnregisteredType += (s, e) =>
                {
                    if (e.UnregisteredServiceType == typeof(IServiceScope) && !e.Handled)
                    {
                        e.Register(Lifestyle.Scoped.CreateRegistration(scopeFactory.CreateScope, container));
                    }
                };

                container.ContainerScope.SetItem(ServiceScopeKey, new object());
            }
        }

        private static Registration CreateCrossWireRegistration(
            Container container, Type serviceType, IServiceProvider appServices)
        {
            IServiceCollection services = GetServiceCollection(container);

            Lifestyle lifestyle = DetermineLifestyle(serviceType, services);

            return lifestyle == Lifestyle.Singleton
                ? CreateSingletonRegistration(container, serviceType, appServices)
                : CreateNonSingletonRegistration(container, serviceType, appServices, lifestyle);
        }

        private static Registration CreateSingletonRegistration(
            Container container, Type serviceType, IServiceProvider appServices)
        {
            var registration = Lifestyle.Singleton.CreateRegistration(
                serviceType,
                () => appServices.GetRequiredService(serviceType),
                container);

            // This registration is managed and disposed by IServiceProvider and should, therefore, not be
            // disposed (again) by Simple Injector.
            registration.SuppressDisposal = true;

            return registration;
        }

        private static Registration CreateNonSingletonRegistration(
            Container container, Type serviceType, IServiceProvider appServices, Lifestyle lifestyle)
        {
            IHttpContextAccessor accessor = GetHttpContextAccessor(appServices);

            Registration registration = lifestyle.CreateRegistration(
                serviceType,
                () => GetServiceProvider(accessor, container, lifestyle).GetRequiredService(serviceType),
                container);

            // This registration is managed and disposed by IServiceProvider and should, therefore, not be
            // disposed (again) by Simple Injector.
            registration.SuppressDisposal = true;

            if (lifestyle == Lifestyle.Transient && typeof(IDisposable).IsAssignableFrom(serviceType))
            {
                registration.SuppressDiagnosticWarning(
                    DiagnosticType.DisposableTransientComponent,
                    justification:
                        "This is a cross-wired service. ASP.NET Core will ensure it gets disposed.");
            }

            return registration;
        }

        private static IServiceCollection GetServiceCollection(Container container)
        {
            var context = (IServiceCollection?)container.ContainerScope.GetItem(CrossWireContextKey);

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

        private static IServiceProvider GetServiceProvider(
            IHttpContextAccessor accessor, Container container, Lifestyle lifestyle)
        {
            // Pull the IServiceProvider from the current request. If there is no request, pull it from an
            // IServiceScope that that will be managed by Simple Injector as scoped registration
            // (see CrossWireServiceScope).
            return accessor.HttpContext?.RequestServices
                ?? GetServiceProviderForBackgroundThread(container, lifestyle);
        }

        private static IServiceProvider GetServiceProviderForBackgroundThread(
            Container container, Lifestyle lifestyle) =>
            Lifestyle.Scoped.GetCurrentScope(container) != null
                ? container.GetInstance<IServiceScope>().ServiceProvider
                : throw new ActivationException(
                    $"You are trying to resolve a {lifestyle.Name} cross-wired service, but are doing so " +
                    "outside the context of a web request. To be able to resolve this service, the " +
                    "operation must run in the context of an active " +
                    $"({container.Options.DefaultScopedLifestyle!.Name}) scope.");

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

        private static IServiceProvider GetRequestServiceProvider(
            IServiceProvider appServices, Type serviceType)
        {
            IHttpContextAccessor accessor = GetHttpContextAccessor(appServices);

            var context = accessor.HttpContext;

            if (context == null)
            {
                throw new InvalidOperationException(
                    $"Unable to request service '{serviceType.ToFriendlyName()} from ASP.NET Core request " +
                    "services. Please make sure this method is called within the context of an active HTTP " +
                    "request.");
            }

            return context.RequestServices;
        }

        private static IHttpContextAccessor GetHttpContextAccessor(IServiceProvider appServices)
        {
            var accessor = appServices.GetService<IHttpContextAccessor>();

            if (accessor == null)
            {
                throw new InvalidOperationException(
                    "Type 'Microsoft.AspNetCore.Http.IHttpContextAccessor' is not available in the " +
                    "IApplicationBuilder.ApplicationServices collection. Please make sure it is registered " +
                    "by adding it to the ConfigureServices method as follows: " + Environment.NewLine +
                    "services.AddHttpContextAccessor();");
            }

            return accessor;
        }
    }
}