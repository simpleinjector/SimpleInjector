// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Integration.ServiceCollection;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Extensions to configure Simple Injector on top of <see cref="IServiceCollection"/>.
    /// </summary>
    public static class SimpleInjectorServiceCollectionExtensions
    {
        private static readonly object AddOptionsKey = new object();
        private static readonly object AddLoggingKey = new object();
        private static readonly object AddLocalizationKey = new object();

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

            HookAspNetCoreHostHostedServiceServiceProviderInitialization(options);

            setupAction?.Invoke(options);

            RegisterServiceScope(options);

            if (options.AutoCrossWireFrameworkComponents)
            {
                AddAutoCrossWiring(options);
            }

            return services;
        }

        /// <summary>
        /// Finalizes the configuration of Simple Injector on top of <see cref="IServiceCollection"/>. Will
        /// ensure framework components can be injected into Simple Injector-resolved components, unless
        /// <see cref="SimpleInjectorAddOptions.AutoCrossWireFrameworkComponents"/> is set to <c>false</c>.
        /// </summary>
        /// <param name="provider">The application's <see cref="IServiceProvider"/>.</param>
        /// <param name="container">The application's <see cref="Container"/> instance.</param>
        /// <returns>The supplied <paramref name="provider"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="provider"/> or
        /// <paramref name="container"/> are null references.</exception>
        public static IServiceProvider UseSimpleInjector(this IServiceProvider provider, Container container)
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

            addOptions.SetServiceProviderIfNull(provider);

            return provider;
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
        [Obsolete(
            "You are supplying a setup action, but due breaking changes in ASP.NET Core 3, the Simple " +
            "Injector contianer can get locked at an earlier stage, making it impossible to further setup " +
            "the container at this stage. Please call the UseSimpleInjector(IServiceProvider, Container) " +
            "overload instead. Take a look at the compiler warnings on the individual methods you are " +
            "calling inside your setupAction delegate to understand how to migrate them. " +
            " For more information, see: https://simpleinjector.org/aspnetcore. " +
            "Will be treated as an error from version 4.9. Will be removed in version 5.0.",
            error: false)]
        public static IServiceProvider UseSimpleInjector(
            this IServiceProvider provider,
            Container container,
            Action<SimpleInjectorUseOptions>? setupAction)
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

            addOptions.SetServiceProviderIfNull(provider);

            var useOptions = new SimpleInjectorUseOptions(addOptions, provider);

            setupAction?.Invoke(useOptions);

            return provider;
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
        public static SimpleInjectorAddOptions CrossWire<TService>(this SimpleInjectorAddOptions options)
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
        public static SimpleInjectorAddOptions CrossWire(
            this SimpleInjectorAddOptions options, Type serviceType)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (serviceType is null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            // At this point there is no IServiceProvider (ApplicationServices) yet, which is why we need to
            // postpone the registration of the cross-wired service. When the container gets locked, we will
            // (hopefully) have the IServiceProvider available.
            options.Container.Options.ContainerLocking += (s, e) =>
            {
                Registration registration = CreateCrossWireRegistration(
                    options,
                    options.ApplicationServices,
                    serviceType,
                    DetermineLifestyle(serviceType, options.Services));

                options.Container.AddRegistration(serviceType, registration);
            };

            return options;
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
        public static SimpleInjectorAddOptions AddLogging(this SimpleInjectorAddOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            EnsureMethodOnlyCalledOnce(options, nameof(AddLogging), AddLoggingKey);

            // Both RootLogger and Logger<T> depend on ILoggerFactory
            VerifyLoggerFactoryAvailable(options.Services);

            // Cross-wire ILoggerFactory explicitly, because auto cross-wiring might be disabled by the user.
            options.Container.RegisterSingleton(() => options.GetRequiredFrameworkService<ILoggerFactory>());

            options.Container.RegisterConditional(
                typeof(ILogger),
                c => c.Consumer is null
                    ? typeof(RootLogger)
                    : typeof(Integration.ServiceCollection.Logger<>)
                        .MakeGenericType(c.Consumer.ImplementationType),
                Lifestyle.Singleton,
                _ => true);

            return options;
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
        [Obsolete(
            "Please call services.AddSimpleInjector(options => { options.AddLogging(); } instead on " +
            "the IServiceCollection instance (typically from inside your Startup.ConfigureServices method)." +
            " For more information, see: https://simpleinjector.org/aspnetcore. " +
            "Will be treated as an error from version 4.9. Will be removed in version 5.0.",
            error: false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static SimpleInjectorUseOptions UseLogging(this SimpleInjectorUseOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Container.ContainerScope.GetItem(AddLoggingKey) != null)
            {
                throw new InvalidOperationException(
                    $"You already initialized logging by calling the {nameof(AddLogging)} extension " +
                    $"method. {nameof(UseLogging)} and {nameof(AddLogging)} are mutually exclusive—" +
                    $"they can't be used together. Prefer using {nameof(AddLogging)} as " +
                    "this method is obsolete.");
            }

            // Both RootLogger and Logger<T> depend on ILoggerFactory
            VerifyLoggerFactoryAvailable(options.Services);

            // Register logger factory explicitly. This allows the Logger<T> conditional registration to work
            // even when auto cross wiring is disabled.
            options.Container.RegisterInstance(options.Builder.GetRequiredFrameworkService<ILoggerFactory>());

            options.Container.RegisterConditional(
                typeof(ILogger),
                c => c.Consumer is null
                    ? typeof(RootLogger)
                    : typeof(Integration.ServiceCollection.Logger<>)
                        .MakeGenericType(c.Consumer.ImplementationType),
                Lifestyle.Singleton,
                _ => true);

            return options;
        }

        /// <summary>
        /// Allows components that are built by Simple Injector to depend on the (non-generic)
        /// <see cref="IStringLocalizer">Microsoft.Extensions.Localization.IStringLocalizer</see> abstraction.
        /// Components are injected with an contextual implementation. Using this method, application 
        /// components can simply depend on <b>IStringLocalizer</b> instead of its generic counter part,
        /// <b>IStringLocalizer&lt;T&gt;</b>, which simplifies development.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>The supplied <paramref name="options"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="options"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no <see cref="IStringLocalizerFactory"/>
        /// entry can be found in the framework's list of services defined by <see cref="IServiceCollection"/>.
        /// </exception>
        /// <exception cref="ActivationException">Thrown when an <see cref="IStringLocalizer"/> is directly 
        /// resolved from the container. Instead use <see cref="IStringLocalizer"/> within a constructor 
        /// dependency.</exception>
        public static SimpleInjectorAddOptions AddLocalization(this SimpleInjectorAddOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            EnsureMethodOnlyCalledOnce(options, nameof(AddLocalization), AddLocalizationKey);

            VerifyStringLocalizerFactoryAvailable(options.Services);

            // Cross-wire IStringLocalizerFactory explicitly, because auto cross-wiring might be disabled.
            options.Container.RegisterSingleton(
                () => options.GetRequiredFrameworkService<IStringLocalizerFactory>());

            options.Container.RegisterConditional(
                typeof(IStringLocalizer),
                c => c.Consumer is null
                    ? throw new ActivationException(
                        "IStringLocalizer is being resolved directly from the container, but this is not " +
                        "supported as string localizers need to be related to a consuming type. Instead, " +
                        "make IStringLocalizer a constructor dependency of the type it is used in.")
                    : typeof(Integration.ServiceCollection.StringLocalizer<>)
                    .MakeGenericType(c.Consumer.ImplementationType),
                Lifestyle.Singleton,
                _ => true);

            return options;
        }

        /// <summary>
        /// Allows components that are built by Simple Injector to depend on the (non-generic)
        /// <see cref="IStringLocalizer">Microsoft.Extensions.Localization.IStringLocalizer</see> abstraction.
        /// Components are injected with an contextual implementation. Using this method, application 
        /// components can simply depend on <b>IStringLocalizer</b> instead of its generic counter part,
        /// <b>IStringLocalizer&lt;T&gt;</b>, which simplifies development.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>The supplied <paramref name="options"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="options"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no <see cref="IStringLocalizerFactory"/>
        /// entry can be found in the framework's list of services defined by <see cref="IServiceCollection"/>.
        /// </exception>
        /// <exception cref="ActivationException">Thrown when an <see cref="IStringLocalizer"/> is directly 
        /// resolved from the container. Instead use <see cref="IStringLocalizer"/> within a constructor 
        /// dependency.</exception>
        [Obsolete(
            "Please call services.AddSimpleInjector(options => { options.AddLocalization(); } instead on " +
            "the IServiceCollection instance (typically from inside your Startup.ConfigureServices method)." +
            " For more information, see: https://simpleinjector.org/aspnetcore. " +
            "Will be treated as an error from version 4.9. Will be removed in version 5.0.",
            error: false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static SimpleInjectorUseOptions UseLocalization(this SimpleInjectorUseOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Container.ContainerScope.GetItem(AddLocalizationKey) != null)
            {
                throw new InvalidOperationException(
                    $"You already initialized logging by calling the {nameof(AddLocalization)} extension " +
                    $"method. {nameof(UseLocalization)} and {nameof(AddLocalization)} are mutually " +
                    $"exclusive—they can't be used together. Prefer using {nameof(AddLocalization)} as " +
                    "this method is obsolete.");
            }

            VerifyStringLocalizerFactoryAvailable(options.Services);

            // registration to work even when auto cross wiring is disabled.
            options.Container.RegisterInstance(
                options.Builder.GetRequiredFrameworkService<IStringLocalizerFactory>());

            options.Container.RegisterConditional(
                typeof(IStringLocalizer),
                c => c.Consumer is null
                    ? throw new ActivationException(
                        "IStringLocalizer is being resolved directly from the container, but this is not " +
                        "supported as string localizers need to be related to a consuming type. Instead, " +
                        "make IStringLocalizer a constructor dependency of the type it is used in.")
                    : typeof(Integration.ServiceCollection.StringLocalizer<>)
                    .MakeGenericType(c.Consumer.ImplementationType),
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
        [Obsolete(
            "Please call services.AddSimpleInjector(options => { options.CrossWire<TService>(); } instead " +
            "on the IServiceCollection instance (typically from inside your Startup.ConfigureServices " +
            "method). For more information, see: https://simpleinjector.org/servicecollection. " +
            "Will be treated as an error from version 4.9. Will be removed in version 5.0.",
            error: false)]
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
        [Obsolete(
            "Please call services.AddSimpleInjector(options => { options.CrossWire(Type); } instead " +
            "on the IServiceCollection instance (typically from inside your Startup.ConfigureServices " +
            "method). For more information, see: https://simpleinjector.org/servicecollection. " +
            "Will be treated as an error from version 4.9. Will be removed in version 5.0.",
            error: false)]
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

        private static void VerifyLoggerFactoryAvailable(IServiceCollection services)
        {
            var descriptor = FindServiceDescriptor(services, typeof(ILoggerFactory));

            if (descriptor is null)
            {
                throw new InvalidOperationException(
                    $"A registration for the {typeof(ILoggerFactory).FullName} is missing from the ASP.NET " +
                    "Core configuration system. This is most likely caused by a missing call to services" +
                    ".AddLogging() as part of the ConfigureServices(IServiceCollection) method of the " +
                    "Startup class. The .AddLogging() extension method is part of the LoggingService" +
                    "CollectionExtensions class of the Microsoft.Extensions.Logging assembly.");
            }
            else if (descriptor.Lifetime != ServiceLifetime.Singleton)
            {
                // By default, the LoggerFactory implementation is registered using auto-wiring (so not with
                // ImplementationInstance) which means we have to support that as well.
                throw new InvalidOperationException(
                    $"Although a registration for {typeof(ILoggerFactory).FullName} exists in the ASP.NET " +
                    $"Core configuration system, the registration is not added as Singleton. Instead the " +
                    $"registration exists as {descriptor.Lifetime}. This might be caused by a third-party " +
                    "library that replaced .NET Core's default ILoggerFactory. Make sure that you use one " +
                    "of the AddSingleton overloads to register ILoggerFactory. Simple Injector does not " +
                    "support ILoggerFactory to be registered with anything other than Singleton.");
            }
        }

        private static void VerifyStringLocalizerFactoryAvailable(IServiceCollection services)
        {
            var descriptor = FindServiceDescriptor(services, typeof(IStringLocalizerFactory));

            if (descriptor is null)
            {
                throw new InvalidOperationException(
                    $"A registration for the {typeof(IStringLocalizerFactory).FullName} is missing from " +
                    "the ASP.NET Core configuration system. This is most likely caused by a missing call " +
                    "to services.AddLocalization() as part of the ConfigureServices(IServiceCollection) " +
                    "method of the Startup class. The .AddLocalization() extension method is part of the " +
                    "LocalizationServiceCollectionExtensions class of the Microsoft.Extensions.Localization" +
                    " assembly.");
            }
            else if (descriptor.Lifetime != ServiceLifetime.Singleton)
            {
                // By default, the IStringLocalizerFactory implementation is registered using auto-wiring 
                // (so not with ImplementationInstance) which means we have to support that as well.
                throw new InvalidOperationException(
                    $"Although a registration for {typeof(IStringLocalizerFactory).FullName} exists in the " +
                    "ASP.NET Core configuration system, the registration is not added as Singleton. " +
                    $"Instead the registration exists as {descriptor.Lifetime}. This might be caused by a " +
                    "third-party library that replaced .NET Core's default IStringLocalizerFactory. Make " +
                    "sure that you use one of the AddSingleton overloads to register " +
                    "IStringLocalizerFactory. Simple Injector does not support IStringLocalizerFactory to " +
                    "be registered with anything other than Singleton.");
            }
        }

        private static void RegisterServiceScope(SimpleInjectorAddOptions options)
        {
            options.Container.Register(
                () => options.ServiceScopeFactory.CreateScope(),
                Lifestyle.Scoped);
        }

        private static SimpleInjectorAddOptions GetOptions(Container container)
        {
            var options =
                (SimpleInjectorAddOptions?)container.ContainerScope.GetItem(AddOptionsKey);

            if (options is null)
            {
                throw new InvalidOperationException(
                    "Please ensure the " +
                    $"{nameof(SimpleInjectorServiceCollectionExtensions.AddSimpleInjector)} extension " +
                    "method is called on the IServiceCollection instance before using this method.");
            }

            return options;
        }

        private static void AddAutoCrossWiring(SimpleInjectorAddOptions options)
        {
            // By using ContainerLocking, we ensure that this ResolveUnregisteredType registration is made 
            // after all possible ResolveUnregisteredType registrations the users did themselves.
            options.Container.Options.ContainerLocking += (_, __) =>
            {
                // If there's no IServiceProvider, the property will throw, which is something we want to do
                // at this point, not later on, when an unregistered type is resolved.
                IServiceProvider provider = options.ApplicationServices;

                options.Container.ResolveUnregisteredType += (_, e) =>
                {
                    if (!e.Handled)
                    {
                        Type serviceType = e.UnregisteredServiceType;

                        ServiceDescriptor? descriptor = FindServiceDescriptor(options.Services, serviceType);

                        if (descriptor != null)
                        {
                            Registration registration =
                                CreateCrossWireRegistration(
                                    options,
                                    provider,
                                    serviceType,
                                    ToLifestyle(descriptor.Lifetime));

                            e.Register(registration);
                        }
                    }
                };
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
            SimpleInjectorAddOptions options,
            IServiceProvider provider,
            Type serviceType,
            Lifestyle lifestyle)
        {
            var registration = lifestyle.CreateRegistration(
                serviceType,
                lifestyle == Lifestyle.Singleton
                    ? BuildSingletonInstanceCreator(serviceType, provider)
                    : BuildScopedInstanceCreator(serviceType, options.ServiceProviderAccessor),
                options.Container);

            // This registration is managed and disposed by IServiceProvider and should, therefore, not be
            // disposed (again) by Simple Injector.
            registration.SuppressDisposal = true;

            if (lifestyle == Lifestyle.Transient && typeof(IDisposable).IsAssignableFrom(serviceType))
            {
                registration.SuppressDiagnosticWarning(
                    DiagnosticType.DisposableTransientComponent,
                    justification: "This is a cross-wired service. It will be disposed by IServiceScope.");
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
            return () =>
            {
                IServiceProvider current;

                try
                {
                    current = accessor.Current;
                }
                catch (ActivationException ex)
                {
                    // The DefaultServiceProviderAccessor will throw an ActivationException in case the
                    // IServiceProvider (or in fact the underlying IServiceScope) is requested outside the
                    // context of an active scope. Here we enrich that exception message with information
                    // of the actual requested cross-wired service.
                    throw new ActivationException(
                        $"Error resolving the cross-wired {serviceType.ToFriendlyName()}. {ex.Message}", ex);
                }

                return current.GetRequiredService(serviceType);
            };
        }

        private static ServiceDescriptor? FindServiceDescriptor(IServiceCollection services, Type serviceType)
        {
            // In case there are multiple descriptors for a given type, .NET Core will use the last
            // descriptor when one instance is resolved. We will have to get this last one as well.
            ServiceDescriptor? descriptor = services.LastOrDefault(d => d.ServiceType == serviceType);

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
            var current = container.ContainerScope.GetItem(AddOptionsKey);

            if (current is null)
            {
                container.ContainerScope.SetItem(AddOptionsKey, builder);
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

        private static void HookAspNetCoreHostHostedServiceServiceProviderInitialization(
            SimpleInjectorAddOptions options)
        {
            // ASP.NET Core 3's new Host class resolves hosted services much earlier in the pipeline. This
            // registration ensures that the IServiceProvider is assigned before such resolve takes place,
            // to ensure that that hosted service can be injected with cross-wired dependencies.
            options.Services.AddSingleton<IHostedService>(provider =>
            {
                options.SetServiceProviderIfNull(provider);

                // We can't return null here, so we return an empty implementation.
                return new NullSimpleInjectorHostedService();
            });
        }

        private static void EnsureMethodOnlyCalledOnce(
            SimpleInjectorAddOptions options, string methodName, object key)
        {
            if (options.Container.ContainerScope.GetItem(key) != null)
            {
                throw new InvalidOperationException(
                    $"The {methodName} extension method can only be called once on a Container instance.");
            }
            else
            {
                options.Container.ContainerScope.SetItem(key, new object());
            }
        }

        private sealed class NullSimpleInjectorHostedService : IHostedService
        {
            public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }
    }
}