// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Integration.ServiceCollection;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Extensions to configure Simple Injector on top of <see cref="IServiceCollection"/>.
    /// </summary>
    public static class SimpleInjectorServiceCollectionExtensions
    {
        private static readonly object SimpleInjectorAddOptionsKey = new object();

        /// <summary>
        /// Sets up the basic configuration that allows Simple Injector to be used in frameworks that require
        /// the use of <see cref="IServiceCollection"/> for registration of framework components.
        /// In case of the absense of a
        /// <see cref="ContainerOptions.DefaultScopedLifestyle">DefaultScopedLifestyle</see>, this method
        /// will configure <see cref="AsyncScopedLifestyle"/> as the default scoped lifestyle.
        /// In case a <paramref name="setupAction"/> is supplied, that delegate will be called that allow
        /// further configuring the container.
        /// </summary>
        /// <param name="services">The framework's <see cref="IServiceCollection"/> instance.</param>
        /// <param name="container">The application's <see cref="Container"/> instance.</param>
        /// <param name="setupAction">An optional setup action.</param>
        /// <returns>The supplied <paramref name="services"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or
        /// <paramref name="container"/> are null references.</exception>
        public static IServiceCollection AddSimpleInjector(
            this IServiceCollection services,
            Container container,
            Action<SimpleInjectorAddOptions>? setupAction = null)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            var options = new SimpleInjectorAddOptions(
                services,
                container,
                new DefaultServiceProviderAccessor(container));

            // This stores the options, which includes the IServiceCollection. IServiceCollection is required
            // when calling UseSimpleInjector to enable auto cross wiring.
            AddSimpleInjectorOptions(container, options);

            // Set lifestyle before calling setupAction. Code in the delegate might depend on that.
            TrySetDefaultScopedLifestyle(container);

            setupAction?.Invoke(options);

            return services;
        }

        /// <summary>
        /// Finalizes the configuration of Simple Injector on top of <see cref="IServiceCollection"/>. Will
        /// ensure framework components can be injected into Simple Injector-resolved components, unless
        /// <see cref="SimpleInjectorUseOptions.AutoCrossWireFrameworkComponents"/> is set to <c>false</c>
        /// using the <paramref name="setupAction"/>.
        /// </summary>
        /// <param name="provider">The application's <see cref="IServiceProvider"/>.</param>
        /// <param name="container">The application's <see cref="Container"/> instance.</param>
        /// <param name="setupAction">An optional setup action.</param>
        /// <returns>The supplied <paramref name="provider"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="provider"/> or
        /// <paramref name="container"/> are null references.</exception>
        public static IServiceProvider UseSimpleInjector(
            this IServiceProvider provider,
            Container container,
            Action<SimpleInjectorUseOptions>? setupAction = null)
        {
            if (provider is null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            SimpleInjectorAddOptions addOptions = GetOptions(container);

            RegisterServiceScope(provider, container);

            var useOptions = new SimpleInjectorUseOptions(addOptions, provider);

            setupAction?.Invoke(useOptions);

            if (useOptions.AutoCrossWireFrameworkComponents)
            {
                AddAutoCrossWiring(container, provider, addOptions);
            }

            return provider;
        }

        /// <summary>
        /// Allows components that are built by Simple Injector to depend on the (non-generic)
        /// <see cref="ILogger">Microsoft.Extensions.Logging.ILogger</see> abstraction. Components are
        /// injected with an contextual implementation. Using this method, application components can simply
        /// depend on <b>ILogger</b> instead of its generic counter part, <b>ILogger&lt;T&gt;</b>, which
        /// simplifies development.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>The supplied <paramref name="options"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="options"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no <see cref="ILoggerFactory"/> entry
        /// can be found in the framework's list of services defined by <see cref="IServiceCollection"/>.
        /// </exception>
        public static SimpleInjectorUseOptions UseLogging(this SimpleInjectorUseOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Both RootLogger and Logger<T> depend on ILoggerFactory
            var loggerFactory = options.ApplicationServices.GetService<ILoggerFactory>();

            if (loggerFactory is null)
            {
                throw new InvalidOperationException(
                    $"The IServiceCollection is missing an entry for {typeof(ILoggerFactory).FullName}. " +
                    "This is most likely caused by a missing call to .AddLogging(). Make sure that the " +
                    "AddLogging() extension method is called on the IServiceCollection. This method is " +
                    "part of the LoggingServiceCollectionExtensions class of the Microsoft.Extensions" +
                    ".Logging assembly.");
            }

            // Register logger factory explicitly. This allows the Logger<T> conditional registration to work
            // even when auto cross wiring is disabled.
            options.Container.RegisterInstance(loggerFactory);

            options.Container.RegisterConditional(
                typeof(ILogger),
                c => c.Consumer is null
                    ? typeof(RootLogger)
                    : typeof(Logger<>).MakeGenericType(c.Consumer.ImplementationType),
                Lifestyle.Singleton,
                _ => true);

            return options;
        }

        /// <summary>
        /// Cross wires an ASP.NET Core or third-party service to the container, to allow the service to be
        /// injected into components that are built by Simple Injector.
        /// </summary>
        /// <typeparam name="TService">The type of service object to cross-wire.</typeparam>
        /// <param name="options">The options.</param>
        /// <returns>The supplied <paramref name="options"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the parameter is a null reference.
        /// </exception>
        public static SimpleInjectorUseOptions CrossWire<TService>(this SimpleInjectorUseOptions options)
            where TService : class
        {
            return CrossWire(options, typeof(TService));
        }

        /// <summary>
        /// Cross wires an ASP.NET Core or third-party service to the container, to allow the service to be
        /// injected into components that are built by Simple Injector.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="serviceType">The type of service object to ross-wire.</param>
        /// <returns>The supplied <paramref name="options"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the parameters is a null reference.
        /// </exception>
        public static SimpleInjectorUseOptions CrossWire(
            this SimpleInjectorUseOptions options, Type serviceType)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (serviceType is null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            Registration registration = CreateCrossWireRegistration(
                options.Builder,
                options.ApplicationServices,
                serviceType,
                DetermineLifestyle(serviceType, options.Services));

            options.Container.AddRegistration(serviceType, registration);

            return options;
        }

        private static void RegisterServiceScope(IServiceProvider provider, Container container)
        {
            if (container.Options.DefaultScopedLifestyle is null)
            {
                throw new InvalidOperationException(
                    "Please ensure that the container is configured with a default scoped lifestyle by " +
                    "setting the Container.Options.DefaultScopedLifestyle property with the required " +
                    "scoped lifestyle for your type of application. In ASP.NET Core, the typical " +
                    $"lifestyle to use is the {nameof(AsyncScopedLifestyle)}. " +
                    "See: https://simpleinjector.org/lifestyles#scoped");
            }

            container.Register<IServiceScope>(
                provider.GetRequiredService<IServiceScopeFactory>().CreateScope,
                Lifestyle.Scoped);
        }

        private static SimpleInjectorAddOptions GetOptions(Container container)
        {
            var options =
                (SimpleInjectorAddOptions?)container.ContainerScope.GetItem(SimpleInjectorAddOptionsKey);

            if (options is null)
            {
                throw new InvalidOperationException(
                    "Please ensure the " +
                    $"{nameof(SimpleInjectorServiceCollectionExtensions.AddSimpleInjector)} extension " +
                    "method is called on the IServiceCollection instance before using this method.");
            }

            return options;
        }

        private static void AddAutoCrossWiring(
            Container container, IServiceProvider provider, SimpleInjectorAddOptions builder)
        {
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            var services = builder.Services;

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
                    Registration registration =
                        CreateCrossWireRegistration(
                            builder,
                            provider,
                            serviceType,
                            ToLifestyle(descriptor.Lifetime));

                    e.Register(registration);
                }
            };
        }

        private static Lifestyle DetermineLifestyle(Type serviceType, IServiceCollection services)
        {
            var descriptor = FindServiceDescriptor(services, serviceType);

            // In case the service type is an IEnumerable, a registration can't be found, but collections are
            // in Core always registered as Transient, so it's safe to fall back to the transient lifestyle.
            return ToLifestyle(descriptor?.Lifetime ?? ServiceLifetime.Transient);
        }

        private static Registration CreateCrossWireRegistration(
            SimpleInjectorAddOptions builder,
            IServiceProvider provider,
            Type serviceType,
            Lifestyle lifestyle)
        {
            var registration = lifestyle.CreateRegistration(
                serviceType,
                lifestyle == Lifestyle.Singleton
                    ? BuildSingletonInstanceCreator(serviceType, provider)
                    : BuildScopedInstanceCreator(serviceType, builder.ServiceProviderAccessor),
                builder.Container);

            // This registration is managed and disposed by IServiceProvider and should, therefore, not be
            // disposed (again) by Simple Injector.
            registration.SuppressDisposal = true;

            if (lifestyle == Lifestyle.Transient && typeof(IDisposable).IsAssignableFrom(serviceType))
            {
                registration.SuppressDiagnosticWarning(
                    DiagnosticType.DisposableTransientComponent,
                    justification: "This is a cross-wired service. It will  be disposed by IServiceScope.");
            }

            return registration;
        }

        private static Func<object> BuildSingletonInstanceCreator(
            Type serviceType, IServiceProvider rootProvider)
        {
            return () => rootProvider.GetRequiredService(serviceType);
        }

        private static Func<object> BuildScopedInstanceCreator(
            Type serviceType, IServiceProviderAccessor accessor)
        {
            // The ServiceProviderAccessor allows access to a request-specific IServiceProvider. This
            // allows Scoped and Transient instances to be resolved from a scope instead of the root
            // container—resolving them from the root container will cause memory leaks. Specific
            // framework integration (such as Simple Injector's ASP.NET Core integration) can override
            // this accessor with one that allows retrieving the IServiceProvider from a web request.
            return () => accessor.Current.GetRequiredService(serviceType);
        }

        private static ServiceDescriptor FindServiceDescriptor(IServiceCollection services, Type serviceType)
        {
            // In case there are multiple descriptors for a given type, .NET Core will use the last
            // descriptor when one instance is resolved. We will have to get this last one as well.
            ServiceDescriptor descriptor = services.LastOrDefault(d => d.ServiceType == serviceType);

            if (descriptor == null && serviceType.GetTypeInfo().IsGenericType)
            {
                // In case the registration is made as open-generic type, the previous query will return
                // null, and we need to go find the last open generic registration for the service type.
                var serviceTypeDefinition = serviceType.GetTypeInfo().GetGenericTypeDefinition();
                descriptor = services.LastOrDefault(d => d.ServiceType == serviceTypeDefinition);
            }

            return descriptor;
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

        private static void AddSimpleInjectorOptions(Container container, SimpleInjectorAddOptions builder)
        {
            var current = container.ContainerScope.GetItem(SimpleInjectorAddOptionsKey);

            if (current is null)
            {
                container.ContainerScope.SetItem(SimpleInjectorAddOptionsKey, builder);
            }
            else
            {
                throw new InvalidOperationException(
                    $"The {nameof(AddSimpleInjector)} extension method can only be called once.");
            }
        }

        private static void TrySetDefaultScopedLifestyle(Container container)
        {
            if (container.Options.DefaultScopedLifestyle is null)
            {
                container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            }
        }

        private sealed class RootLogger : ILogger
        {
            private readonly ILogger logger;

            // This constructor needs to be public for Simple Injector to create this type.
            public RootLogger(ILoggerFactory factory) => this.logger = factory.CreateLogger(string.Empty);

            public IDisposable BeginScope<TState>(TState state) => this.logger.BeginScope(state);

            public bool IsEnabled(LogLevel logLevel) => this.logger.IsEnabled(logLevel);

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter) =>
                this.logger.Log(logLevel, eventId, state, exception, formatter);
        }

        // This class wouldn't strictly be required, but since Microsoft could decide to add an extra ctor
        // to the Microsoft.Extensions.Logging.Logger<T> class, this sub type prevents this integration
        // package to break when this happens.
        private sealed class Logger<T> : Microsoft.Extensions.Logging.Logger<T>
        {
            // This constructor needs to be public for Simple Injector to create this type.
            public Logger(ILoggerFactory factory) : base(factory)
            {
            }
        }
    }
}