﻿// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class GenericRegistrationEntry : IRegistrationEntry
    {
        private readonly List<IProducerProvider> providers = new();
        private readonly ContainerOptions options;

        // PERF: #985 These two collections exist solely for performance optimizations. Registering many
        // closed-generic types of the same generic abstractions got exponentially slower with the number
        // of registrations. These two collections help optimize this.
        private Dictionary<Type, ClosedToInstanceProducerProviderDictionaryEntry>? closedProviders;
        private List<OpenGenericToInstanceProducerProvider>? openProviders;

        internal GenericRegistrationEntry(Container container)
        {
            this.options = container.Options;
        }

        private Container Container => this.options.Container;
        private bool AllowOverridingRegistrations => this.options.AllowOverridingRegistrations;
        private bool IsEmpty => this.providers.Count == 0;

        private interface IProducerProvider
        {
            bool IsConditional { get; }

            // The first call to this method can be costly to perform.
            bool GetAppliesToAllClosedServiceTypes();

            Type ServiceType { get; }

            Type? ImplementationType { get; }

            IEnumerable<InstanceProducer> CurrentProducers { get; }

            bool OverlapsWith(InstanceProducer producerToCheck);

            InstanceProducer? TryGetProducer(
                Type serviceType, InjectionConsumerInfo consumer, bool handled = false);

            bool MatchesServiceType(Type serviceType);
        }

        public IEnumerable<InstanceProducer> CurrentProducers =>
            this.providers.SelectMany(p => p.CurrentProducers);

        public void Add(InstanceProducer producer)
        {
            this.Container.ThrowWhenContainerIsLockedOrDisposed();

            Type serviceType = producer.ServiceType;

            if (!this.AllowOverridingRegistrations)
            {
                this.ThrowWhenOverlappingRegistrationsExist(producer);
            }
            else
            {
                this.ThrowWhenConditionalIsRegisteredInOverridingMode(producer);

                if (this.closedProviders != null)
                {
                    if (this.closedProviders.ContainsKey(serviceType))
                    {
                        this.closedProviders.Remove(serviceType);
                        this.providers.RemoveAll(p => p.ServiceType == serviceType);
                    }
                }
            }

            var provider = new ClosedToInstanceProducerProvider(producer);
            
            this.AddClosedToInstanceProducerProvider(provider);
        }

        private void AddClosedToInstanceProducerProvider(ClosedToInstanceProducerProvider provider)
        {
            Type serviceType = provider.ServiceType;

            this.providers.Add(provider);

            if (this.closedProviders is null)
            {
                this.closedProviders = new Dictionary<Type, ClosedToInstanceProducerProviderDictionaryEntry>
                {
                    [serviceType] = new ClosedToInstanceProducerProviderDictionaryEntry(provider)
                };
            }
            else
            {
                if (this.closedProviders.TryGetValue(serviceType, out var entry))
                {
                    entry.Add(provider);
                }
                else
                {
                    this.closedProviders[serviceType] = new ClosedToInstanceProducerProviderDictionaryEntry(provider);
                }
            }
        }

        public void AddGeneric(
            Type serviceType,
            Type implementationType,
            Lifestyle lifestyle,
            Predicate<PredicateContext>? predicate)
        {
            this.Container.ThrowWhenContainerIsLockedOrDisposed();

            var provider = new OpenGenericToInstanceProducerProvider(
                serviceType, implementationType, lifestyle, predicate, this.Container);

            this.ThrowWhenConditionalIsRegisteredInOverridingMode(provider);

            this.ThrowWhenProviderToRegisterOverlapsWithExistingProvider(provider);

            if (this.AllowOverridingRegistrations)
            {
                if (provider.GetAppliesToAllClosedServiceTypes())
                {
                    this.providers.RemoveAll(p => p.GetAppliesToAllClosedServiceTypes());

                    this.openProviders?.RemoveAll(p => p.GetAppliesToAllClosedServiceTypes());
                }
            }

            this.AddOpenGenericToInstanceProducerProvider(provider);
        }

        public void Add(
            Type serviceType,
            Func<TypeFactoryContext, Type> implementationTypeFactory,
            Lifestyle lifestyle,
            Predicate<PredicateContext>? predicate)
        {
            this.Container.ThrowWhenContainerIsLockedOrDisposed();

            var provider = new OpenGenericToInstanceProducerProvider(
                serviceType, implementationTypeFactory, lifestyle, predicate, this.Container);

            this.ThrowWhenConditionalIsRegisteredInOverridingMode(provider);

            this.ThrowWhenProviderToRegisterOverlapsWithExistingProvider(provider);

            this.AddOpenGenericToInstanceProducerProvider(provider);
        }

        private void AddOpenGenericToInstanceProducerProvider(OpenGenericToInstanceProducerProvider provider)
        {
            this.providers.Add(provider);

            if (this.openProviders is null)
            {
                this.openProviders = new List<OpenGenericToInstanceProducerProvider>();
            }

            this.openProviders.Add(provider);
        }

        public InstanceProducer? TryGetInstanceProducer(
            Type serviceType, InjectionConsumerInfo consumer)
        {
            // Pre-condition: serviceType is always a closed-generic type
            List<FoundInstanceProducer>? producers = this.GetInstanceProducers(serviceType, consumer);

            if (producers is null)
            {
                return null;
            }
            else if (producers.Count == 1)
            {
                return producers[0].Producer;
            }
            else
            {
                throw new ActivationException(
                    StringResources.MultipleApplicableRegistrationsFound(serviceType, producers));
            }
        }

        // This method is only called when we're about to throw an exception.
        public int GetNumberOfConditionalRegistrationsFor(Type serviceType) =>
            this.GetConditionalProvidersThatMatchType(serviceType).Count();

        // This method is only called when we're about to throw an exception.
        private IEnumerable<IProducerProvider> GetConditionalProvidersThatMatchType(Type serviceType) =>
            from provider in this.providers
            where provider.IsConditional
            where provider.MatchesServiceType(serviceType)
            select provider;

        private void ThrowWhenOverlappingRegistrationsExist(InstanceProducer producerToRegister)
        {
            var overlappingProvider = this.GetFirstOverlappingProvider(producerToRegister);

            if (overlappingProvider != null)
            {
                if (overlappingProvider.ServiceType.IsGenericTypeDefinition())
                {
                    // An overlapping provider will always have an ImplementationType, because providers
                    // with a factory will never be overlapping.
                    Type implementationType = overlappingProvider.ImplementationType!;

                    throw new InvalidOperationException(
                        StringResources.RegistrationForClosedServiceTypeOverlapsWithOpenGenericRegistration(
                            producerToRegister.ServiceType,
                            implementationType));
                }

                bool eitherOneRegistrationIsConditional =
                    overlappingProvider.IsConditional != producerToRegister.IsConditional;

                throw eitherOneRegistrationIsConditional
                    ? GetAnOverlappingGenericRegistrationExistsException(
                        new ClosedToInstanceProducerProvider(producerToRegister),
                        overlappingProvider)
                    : new InvalidOperationException(
                        StringResources.TypeAlreadyRegistered(producerToRegister.ServiceType));
            }
        }

        private IProducerProvider? GetFirstOverlappingProvider(InstanceProducer producerToRegister)
        {
            ClosedToInstanceProducerProvider? firstOverlappingClosedProvider = null;
            OpenGenericToInstanceProducerProvider? firstOverlappingOpenProvider = null;

            if (this.closedProviders != null)
            {
                // PERF: Only closed providers exist. We can speed up the operation by going just through
                // the closed providers for the given service type.
                if (this.closedProviders.TryGetValue(producerToRegister.ServiceType, out var entry))
                {
                    firstOverlappingClosedProvider = entry.OverlapsWith(producerToRegister);
                }
            }

            if (this.openProviders != null)
            {
                foreach (var openProvider in this.openProviders)
                {
                    if (openProvider.OverlapsWith(producerToRegister))
                    {
                        firstOverlappingOpenProvider = openProvider;
                        break;
                    }
                }
            }

            // PERF: Is most cases we can prevent going through the list when there's only one of the two
            // overlapping.
            if (firstOverlappingClosedProvider is null && firstOverlappingOpenProvider is null) return null;
            if (firstOverlappingClosedProvider != null) return firstOverlappingClosedProvider;
            if (firstOverlappingOpenProvider != null) return firstOverlappingOpenProvider;

            // To bad, there is both an overlapping open and a closed provider. Since we must report the first
            // first overlapping provider, we have to go through the list (again).
            foreach (var provider in this.providers)
            {
                if (provider == firstOverlappingClosedProvider ||
                    provider == firstOverlappingOpenProvider)
                {
                    return provider;
                }
            }

            // Will never come here, but the C# compiler doesn't know.
            return null;
        }

        private void ThrowWhenConditionalIsRegisteredInOverridingMode(InstanceProducer producer)
        {
            if (!this.IsEmpty && producer.IsConditional && this.AllowOverridingRegistrations)
            {
                throw new NotSupportedException(
                    StringResources.MakingConditionalRegistrationsInOverridingModeIsNotSupported());
            }
        }

        private void ThrowWhenConditionalIsRegisteredInOverridingMode(
            OpenGenericToInstanceProducerProvider provider)
        {
            if (!this.IsEmpty
                && this.AllowOverridingRegistrations
                && !provider.GetAppliesToAllClosedServiceTypes())
            {
                if (provider.Predicate != null)
                {
                    throw new NotSupportedException(
                        StringResources.MakingConditionalRegistrationsInOverridingModeIsNotSupported());
                }
                else
                {
                    throw new NotSupportedException(
                        StringResources.MakingRegistrationsWithTypeConstraintsInOverridingModeIsNotSupported());
                }
            }
        }

        private void ThrowWhenProviderToRegisterOverlapsWithExistingProvider(
            OpenGenericToInstanceProducerProvider providerToRegister)
        {
            bool providerToRegisterIsSuperset =
                !this.IsEmpty && providerToRegister.GetAppliesToAllClosedServiceTypes();

            // A provider with AppliesToAllClosedServiceTypes true will always have an ImplementationType,
            // because the property will always be false for providers with a factory.
            Type providerImplementationType = providerToRegister.ImplementationType!;

            bool isReplacement = this.AllowOverridingRegistrations
                && providerToRegister.GetAppliesToAllClosedServiceTypes();

            // A provider is a superset of the providerToRegister when it can be applied to ALL generic
            // types that the providerToRegister can be applied to as well.
            var supersetProvider = this.GetFirstOrDefaultSupersetProvidersFor(providerImplementationType);

            bool overlaps = providerToRegisterIsSuperset || supersetProvider != null;

            if (!isReplacement && overlaps)
            {
                var overlappingProvider = supersetProvider ?? this.providers[0];

                throw GetAnOverlappingGenericRegistrationExistsException(
                    providerToRegister,
                    overlappingProvider);
            }
        }

        private IProducerProvider? GetFirstOrDefaultSupersetProvidersFor(Type implementationType)
        {
            if (implementationType is null || this.IsEmpty)
            {
                return null;
            }
            else
            {
                foreach (var provider in this.providers)
                {
                    if (provider.ImplementationType != null)
                    {
                        if (provider.ImplementationType == implementationType
                            || provider.GetAppliesToAllClosedServiceTypes())
                        {
                            return provider;
                        }
                    }
                }

                return null;
            }
        }

        private static InvalidOperationException GetAnOverlappingGenericRegistrationExistsException(
            IProducerProvider providerToRegister, IProducerProvider overlappingProvider) => new(
                StringResources.AnOverlappingRegistrationExists(
                    providerToRegister.ServiceType,
                    // ImplementationType will never be null, because providers can never be overlapping when they
                    // have a factory instead of an implementation type.
                    overlappingProvider.ImplementationType!,
                    overlappingProvider.IsConditional,
                    providerToRegister.ImplementationType!,
                    providerToRegister.IsConditional));

        private List<FoundInstanceProducer>? GetInstanceProducers(
            Type closedGenericServiceType, InjectionConsumerInfo consumer)
        {
            bool handled = false;

            List<FoundInstanceProducer>? list = null;

            foreach (var provider in this.providers)
            {
                var producer =
                    provider.TryGetProducer(closedGenericServiceType, consumer, handled: handled);

                if (producer != null)
                {
                    if (list is null)
                    {
                        list = new List<FoundInstanceProducer>(capacity: 1);
                    }

                    list.Add(new FoundInstanceProducer(
                        provider.ServiceType,
                        provider.ImplementationType ?? producer.FinalImplementationType,
                        producer));

                    handled = true;
                }
            }

            return list;
        }

        private sealed class ClosedToInstanceProducerProvider : IProducerProvider
        {
            private readonly InstanceProducer producer;

            public ClosedToInstanceProducerProvider(InstanceProducer producer)
            {
                this.producer = producer;
            }

            public bool IsConditional => this.producer.IsConditional;
            public bool GetAppliesToAllClosedServiceTypes() => false;
            public Type ServiceType => this.producer.ServiceType;
            public Type? ImplementationType => this.producer.Registration.ImplementationType;
            public IEnumerable<InstanceProducer> CurrentProducers => Enumerable.Repeat(this.producer, 1);
            public bool MatchesServiceType(Type serviceType) => serviceType == this.producer.ServiceType;

            public bool OverlapsWith(InstanceProducer producerToCheck) =>
                (this.producer.IsUnconditional || producerToCheck.IsUnconditional)
                && this.producer.ServiceType == producerToCheck.ServiceType;

            public InstanceProducer? TryGetProducer(
                Type serviceType, InjectionConsumerInfo consumer, bool handled) =>
                this.MatchesServiceType(serviceType) && this.MatchesPredicate(consumer, handled)
                    ? this.producer
                    : null;

            private bool MatchesPredicate(InjectionConsumerInfo consumer, bool handled) =>
                this.producer.Predicate(new PredicateContext(this.producer, consumer, handled));
        }

        private sealed class OpenGenericToInstanceProducerProvider : IProducerProvider
        {
            internal readonly Predicate<PredicateContext>? Predicate;

            private readonly Dictionary<object, InstanceProducer> cache = new();
            private readonly Dictionary<Type, Registration> registrationCache = new();

            private readonly Lifestyle lifestyle;
            private readonly Container container;

            private bool? appliesToAllClosedServiceTypes;

            internal OpenGenericToInstanceProducerProvider(
                Type serviceType,
                Type implementationType,
                Lifestyle lifestyle,
                Predicate<PredicateContext>? predicate,
                Container container)
            {
                this.ServiceType = serviceType;
                this.ImplementationType = implementationType;
                this.ImplementationTypeFactory = _ => implementationType;
                this.lifestyle = lifestyle;
                this.Predicate = predicate;
                this.container = container;
            }

            internal OpenGenericToInstanceProducerProvider(
                Type serviceType,
                Func<TypeFactoryContext, Type> implementationTypeFactory,
                Lifestyle lifestyle,
                Predicate<PredicateContext>? predicate,
                Container container)
            {
                this.ServiceType = serviceType;
                this.ImplementationTypeFactory = implementationTypeFactory;
                this.lifestyle = lifestyle;
                this.Predicate = predicate;
                this.container = container;
                this.appliesToAllClosedServiceTypes = false;
            }

            public bool IsConditional => this.Predicate != null;

            // I turned this former property into a method call to make it more obvious that this is can be
            // a very costly operation (which can also throw first-chance exceptions).
            public bool GetAppliesToAllClosedServiceTypes()
            {
                if (this.appliesToAllClosedServiceTypes is null)
                {
                    // We cache the result of this method. Not caching it can dramatically influence the
                    // performance of the registration process.
                    this.appliesToAllClosedServiceTypes =
                        this.RegistrationAppliesToAllClosedServiceTypes(this.ImplementationType!);
                }

                return this.appliesToAllClosedServiceTypes.Value;
            }

            public Type ServiceType { get; }
            public Type? ImplementationType { get; }
            public Func<TypeFactoryContext, Type> ImplementationTypeFactory { get; }

            public IEnumerable<InstanceProducer> CurrentProducers
            {
                get
                {
                    lock (this.cache)
                    {
                        return this.cache.Values.ToArray();
                    }
                }
            }

            public bool OverlapsWith(InstanceProducer producerToCheck) =>
                this.IsConditional || this.ImplementationType is null
                    ? false // Conditionals never overlap compile time.
                    : GenericTypeBuilder.IsImplementationApplicableToEveryGenericType(
                        producerToCheck.ServiceType,
                        this.ImplementationType);

            public InstanceProducer? TryGetProducer(
                Type serviceType, InjectionConsumerInfo consumer, bool handled)
            {
                Type? closedImplementation = this.ImplementationType != null
                    ? GenericTypeBuilder.MakeClosedImplementation(serviceType, this.ImplementationType)
                    : null;

                // This Func<Type> factory will return null when a closed implementation can't be built:
                // * from ImplementationType due to type constraints
                // * from the implementation returned from ImplementationTypeFactory due to type constraints,
                //   as it can return partly closed types.
                Type? GetImplementationType() => this.ImplementationType != null
                    ? closedImplementation
                    : this.GetImplementationTypeThroughFactory(serviceType, consumer);

                var context = new PredicateContext(serviceType, GetImplementationType, consumer, handled);

                // NOTE: The producer should only get built after it matches the delegate, to prevent
                // unneeded producers from being created, because this might cause diagnostic warnings,
                // such as torn lifestyle warnings.
                var shouldBuildProducer =
                    (this.ImplementationType is null || closedImplementation != null)
                    && this.MatchesPredicate(context)
                    && context.ImplementationType != null;

                return shouldBuildProducer ? this.GetProducer(context) : null;
            }

            // In case this is a type factory registration (meaning ImplementationType is null) we consider
            // the service to be matching, since we can't (and should not) invoke the factory.
            public bool MatchesServiceType(Type serviceType) =>
                this.ImplementationType is null
                || GenericTypeBuilder.MakeClosedImplementation(serviceType, this.ImplementationType) != null;

            private Type? GetImplementationTypeThroughFactory(Type serviceType, InjectionConsumerInfo consumer)
            {
                Type? implementationType =
                    this.ImplementationTypeFactory(new TypeFactoryContext(serviceType, consumer));

                if (implementationType is null)
                {
                    throw new InvalidOperationException(StringResources.FactoryReturnedNull(this.ServiceType));
                }

                if (implementationType.ContainsGenericParameters())
                {
                    Requires.TypeFactoryReturnedTypeThatDoesNotContainUnresolvableTypeArguments(
                        serviceType, implementationType);

                    // implementationType is null when type constraints don't match.
                    implementationType =
                        GenericTypeBuilder.MakeClosedImplementation(serviceType, implementationType);
                }
                else
                {
                    Requires.FactoryReturnsATypeThatIsAssignableFromServiceType(serviceType, implementationType);
                }

                return implementationType;
            }

            private InstanceProducer GetProducer(PredicateContext context)
            {
                InstanceProducer producer;

                // Never build a producer twice. This could cause components with a torn lifestyle.
                lock (this.cache)
                {
                    // Use both the service and implementation type as key. Using just the service type would
                    // case multiple consumers to accidentally get the same implementation type, which
                    // using only the implementation type, would break when one implementation type could be
                    // used for multiple services (implements multiple closed interfaces).
                    var key = new { context.ServiceType, context.ImplementationType };

                    if (!this.cache.TryGetValue(key, out producer))
                    {
                        this.cache[key] = producer = this.CreateNewProducerFor(context);
                    }
                }

                return producer;
            }

            private InstanceProducer CreateNewProducerFor(PredicateContext context) =>
                new(context.ServiceType, this.GetRegistration(context), this.Predicate);

            private Registration GetRegistration(PredicateContext context)
            {
                // At this stage, we know that ImplementationType is not null.
                Type key = context.ImplementationType!;

                // Never build a registration for a particular implementation type twice. This would break
                // the promise of returning singletons.
                if (!this.registrationCache.TryGetValue(key, out Registration registration))
                {
                    this.registrationCache[key] = registration = this.CreateNewRegistrationFor(key);
                }

                return registration;
            }

            private Registration CreateNewRegistrationFor(Type concreteType) =>
                this.lifestyle.CreateRegistration(concreteType, this.container);

            private bool MatchesPredicate(PredicateContext context) =>
                this.Predicate == null || this.Predicate(context);

            // This is nice, if we pass the open generic service type to the GenericTypeBuilder, it
            // can check for us whether the implementation adds extra type constraints that the service
            // type doesn't have. This works, because if it doesn't add any type constraints, it will be
            // able to construct a new open service type, based on the generic type arguments of the
            // implementation. If it can't, it means that the implementionType applies to a subset.
            private bool RegistrationAppliesToAllClosedServiceTypes(Type implementationType) =>
                this.Predicate is null
                && implementationType.IsGenericType()
                && !implementationType.IsPartiallyClosed()
                && this.IsImplementationApplicableToEveryGenericType(implementationType);

            private bool IsImplementationApplicableToEveryGenericType(Type implementationType) =>
                GenericTypeBuilder.IsImplementationApplicableToEveryGenericType(
                    this.ServiceType,
                    implementationType);
        }

        private sealed class ClosedToInstanceProducerProviderDictionaryEntry
        {
            private readonly ClosedToInstanceProducerProvider firstProvider;

            private List<ClosedToInstanceProducerProvider>? providers;

            public ClosedToInstanceProducerProviderDictionaryEntry(ClosedToInstanceProducerProvider firstProvider)
            {
                this.firstProvider = firstProvider;
            }

            public int Count => this.providers?.Count ?? 1;

            internal void Add(ClosedToInstanceProducerProvider provider)
            {
                if (this.providers is null)
                {
                    this.providers = new List<ClosedToInstanceProducerProvider>
                    {
                        this.firstProvider,
                        provider
                    };
                }
                else
                {
                    this.providers.Add(provider);
                }
            }

            internal ClosedToInstanceProducerProvider? OverlapsWith(InstanceProducer producerToRegister)
            {
                if (this.providers is null)
                {
                    return this.firstProvider.OverlapsWith(producerToRegister) ? this.firstProvider : null;
                }
                else
                {
                    foreach (var provider in this.providers)
                    {
                        if (provider.OverlapsWith(producerToRegister))
                        {
                            return provider;
                        }
                    }

                    return null;
                }
            }
        }
    }
}