#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015 Simple Injector Contributors
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

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class GenericRegistrationEntry : IRegistrationEntry
    {
        private readonly List<IProducerProvider> providers = new List<IProducerProvider>();
        private readonly Container container;

        internal GenericRegistrationEntry(Type serviceType, Container container)
        {
            Requires.IsTrue(serviceType.IsGenericTypeDefinition, nameof(serviceType));
            this.container = container;
        }

        private interface IProducerProvider
        {
            bool IsConditional { get; }

            bool AppliesToAllClosedServiceTypes { get; }

            Type ServiceType { get; }

            Type ImplementationType { get; }

            IEnumerable<InstanceProducer> CurrentProducers { get; }

            bool OverlapsWith(Type closedServiceType);

            InstanceProducer TryGetProducer(Type serviceType, InjectionConsumerInfo consumer,
                bool handled = false);

            bool MatchesServiceType(Type serviceType);
        }

        public IEnumerable<InstanceProducer> CurrentProducers => this.providers.SelectMany(p => p.CurrentProducers);

        public void Add(InstanceProducer producer)
        {
            this.container.ThrowWhenContainerIsLocked();

            this.ThrowWhenOverlappingRegistrationsExist(producer);

            this.providers.RemoveAll(p => p.ServiceType == producer.ServiceType);

            this.providers.Add(new ClosedToInstanceProducerProvider(producer));
        }

        public void AddGeneric(Type serviceType, Type implementationType,
            Lifestyle lifestyle, Predicate<PredicateContext> predicate)
        {
            this.container.ThrowWhenContainerIsLocked();
            this.ThrowWhenConditionalIsRegisteredInOverridingMode(predicate);

            var provider = new OpenGenericToInstanceProducerProvider(
                serviceType, implementationType, lifestyle, predicate, this.container);

            this.ThrowWhenProviderToRegisterOverlapsWithExistingProvider(provider);

            this.providers.Add(provider);
        }

        public void Add(Type serviceType, Func<TypeFactoryContext, Type> implementationTypeFactory,
            Lifestyle lifestyle, Predicate<PredicateContext> predicate)
        {
            this.container.ThrowWhenContainerIsLocked();
            this.ThrowWhenConditionalIsRegisteredInOverridingMode(predicate);

            var provider = new OpenGenericToInstanceProducerProvider(
                serviceType, implementationTypeFactory, lifestyle, predicate, this.container);

            this.ThrowWhenProviderToRegisterOverlapsWithExistingProvider(provider);

            this.providers.Add(provider);
        }

        public InstanceProducer TryGetInstanceProducer(Type closedGenericServiceType,
            InjectionConsumerInfo context)
        {
            var producers = this.GetInstanceProducers(closedGenericServiceType, context).ToArray();

            if (producers.Length <= 1)
            {
                return producers.Select(p => p.Item3).FirstOrDefault();
            }

            throw new ActivationException(
                StringResources.MultipleApplicableRegistrationsFound(closedGenericServiceType, producers));
        }

        public int GetNumberOfConditionalRegistrationsFor(Type serviceType) =>
            this.GetConditionalProvidersThatMatchType(serviceType).Count();

        private IEnumerable<IProducerProvider> GetConditionalProvidersThatMatchType(Type serviceType) =>
            from provider in this.providers
            where provider.IsConditional
            where provider.MatchesServiceType(serviceType)
            select provider;

        private void ThrowWhenOverlappingRegistrationsExist(InstanceProducer producer)
        {
            if (!this.container.Options.AllowOverridingRegistrations)
            {
                var overlappingProviders =
                    from provider in this.providers
                    where provider.OverlapsWith(producer.ServiceType)
                    select provider;

                if (overlappingProviders.Any())
                {
                    var overlappingProvider = overlappingProviders.First();

                    if (overlappingProvider.ServiceType.IsGenericTypeDefinition)
                    {
                        throw new InvalidOperationException(
                            StringResources.RegistrationForClosedServiceTypeOverlapsWithOpenGenericRegistration(
                                producer.ServiceType,
                                overlappingProvider.ImplementationType));
                    }

                    throw new InvalidOperationException(StringResources.TypeAlreadyRegistered(producer.ServiceType));
                }
            }
        }

        private void ThrowWhenConditionalIsRegisteredInOverridingMode(Predicate<PredicateContext> predicate)
        {
            if (predicate != null && this.container.Options.AllowOverridingRegistrations)
            {
                throw new NotSupportedException(
                    StringResources.MakingConditionalRegistrationsInOverridingModeIsNotSupported());
            }
        }

        private void ThrowWhenProviderToRegisterOverlapsWithExistingProvider(
            OpenGenericToInstanceProducerProvider providerToRegister)
        {
            bool providerToRegisterIsSuperset =
                providerToRegister.AppliesToAllClosedServiceTypes && this.providers.Any();

            // A provider is a superset of the providerToRegister when it can be applied to ALL generic
            // types that the providerToRegister can be applied to as well.
            var supersetProviders = this.GetSupersetProvidersFor(providerToRegister.ImplementationType);

            if (providerToRegisterIsSuperset || supersetProviders.Any())
            {
                var overlappingProvider = supersetProviders.FirstOrDefault() ?? this.providers.First();

                throw this.GetAnOverlappingGenericRegistrationExistsException(
                    providerToRegister,
                    overlappingProvider);
            }
        }

        private IEnumerable<IProducerProvider> GetSupersetProvidersFor(Type implementationType) =>
            from provider in this.providers
            where implementationType != null
            where provider.ImplementationType != null
            where provider.AppliesToAllClosedServiceTypes
                || provider.ImplementationType == implementationType
            select provider;

        private InvalidOperationException GetAnOverlappingGenericRegistrationExistsException(
            IProducerProvider providerToRegister, IProducerProvider overlappingProvider)
        {
            return new InvalidOperationException(
                StringResources.AnOverlappingGenericRegistrationExists(
                    providerToRegister.ServiceType,
                    overlappingProvider.ImplementationType,
                    overlappingProvider.IsConditional,
                    providerToRegister.ImplementationType,
                    providerToRegister.IsConditional));
        }

        private IEnumerable<Tuple<Type, Type, InstanceProducer>> GetInstanceProducers(
            Type closedGenericServiceType, InjectionConsumerInfo consumer)
        {
            bool handled = false;

            foreach (var provider in this.providers)
            {
                var producer =
                    provider.TryGetProducer(closedGenericServiceType, consumer, handled: handled);

                if (producer != null)
                {
                    yield return Tuple.Create(
                        item1: provider.ServiceType,
                        item2: provider.ImplementationType ?? producer.ImplementationType,
                        item3: producer);

                    handled = true;
                }
            }
        }

        private sealed class ClosedToInstanceProducerProvider : IProducerProvider
        {
            private readonly InstanceProducer producer;

            public ClosedToInstanceProducerProvider(InstanceProducer producer)
            {
                this.producer = producer;
            }

            public bool IsConditional => this.producer.IsConditional;
            public bool AppliesToAllClosedServiceTypes => false;
            public Type ServiceType => this.producer.ServiceType;
            public Type ImplementationType => this.producer.Registration.ImplementationType;
            public IEnumerable<InstanceProducer> CurrentProducers => Enumerable.Repeat(this.producer, 1);
            public bool MatchesServiceType(Type serviceType) => serviceType == this.producer.ServiceType;

            public bool OverlapsWith(Type closedServiceType) =>
                !this.producer.IsConditional && this.producer.ServiceType == closedServiceType;

            public InstanceProducer TryGetProducer(Type serviceType, InjectionConsumerInfo consumer,
                bool handled) =>
                this.MatchesServiceType(serviceType) && this.MatchesPredicate(consumer, handled)
                    ? this.producer
                    : null;

            private bool MatchesPredicate(InjectionConsumerInfo consumer, bool handled) =>
                this.producer.Predicate(new PredicateContext(this.producer, consumer, handled));
        }

        private sealed class OpenGenericToInstanceProducerProvider : IProducerProvider
        {
            internal readonly Predicate<PredicateContext> Predicate;

            private readonly Lifestyle lifestyle;
            private readonly Container container;

            private readonly Dictionary<object, InstanceProducer> cache = new Dictionary<object, InstanceProducer>();
            private readonly Dictionary<Type, Registration> registrationCache = new Dictionary<Type, Registration>();

            internal OpenGenericToInstanceProducerProvider(Type serviceType, Type implementationType,
                Lifestyle lifestyle, Predicate<PredicateContext> predicate, Container container)
            {
                this.ServiceType = serviceType;
                this.ImplementationType = implementationType;
                this.ImplementationTypeFactory = c => this.ImplementationType;
                this.lifestyle = lifestyle;
                this.Predicate = predicate;
                this.container = container;

                // We cache the result of this method, because this is a really heavy operation.
                // Not caching it can dramatically influence the performance of the registration process.
                this.AppliesToAllClosedServiceTypes = this.RegistrationAppliesToAllClosedServiceTypes();
            }

            public OpenGenericToInstanceProducerProvider(Type serviceType,
                Func<TypeFactoryContext, Type> implementationTypeFactory, Lifestyle lifestyle,
                Predicate<PredicateContext> predicate, Container container)
            {
                this.ServiceType = serviceType;
                this.ImplementationTypeFactory = implementationTypeFactory;
                this.lifestyle = lifestyle;
                this.Predicate = predicate;
                this.container = container;
            }

            public bool IsConditional => this.Predicate != null;
            public bool AppliesToAllClosedServiceTypes { get; }
            public Type ServiceType { get; }
            public Type ImplementationType { get; }
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

            public bool OverlapsWith(Type serviceType) =>
                this.Predicate != null || this.ImplementationType == null
                    ? false // Conditionals never overlap compile time.
                    : GenericTypeBuilder.IsImplementationApplicableToEveryGenericType(serviceType,
                        this.ImplementationType);

            public InstanceProducer TryGetProducer(Type serviceType, InjectionConsumerInfo consumer,
                bool handled)
            {
                Type closedImplementation = this.ImplementationType != null
                    ? GenericTypeBuilder.MakeClosedImplementation(serviceType, this.ImplementationType)
                    : null;

                Func<Type> implementationTypeProvider = () => this.ImplementationType != null
                    ? closedImplementation
                    : this.GetImplementationTypeThroughFactory(serviceType, consumer);

                var context = new PredicateContext(serviceType, implementationTypeProvider, consumer, handled);

                // NOTE: The producer should only get built after it matches the delegate, to prevent
                // unneeded producers from being created, because this might cause diagnostic warnings, 
                // such as torn lifestyle warnings.
                var shouldBuildProducer =
                    (this.ImplementationType == null || closedImplementation != null)
                    && this.MatchesPredicate(context)
                    && context.ImplementationType != null;

                return shouldBuildProducer ? this.GetProducer(context) : null;
            }

            private Type GetImplementationTypeThroughFactory(Type serviceType, InjectionConsumerInfo consumer)
            {
                Type implementationType =
                    this.ImplementationTypeFactory(new TypeFactoryContext(serviceType, consumer));

                if (implementationType == null)
                {
                    throw new InvalidOperationException(StringResources.FactoryReturnedNull(this.ServiceType));
                }

                if (implementationType.ContainsGenericParameters)
                {
                    Requires.TypeFactoryReturnedTypeThatDoesNotContainUnresolvableTypeArguments(
                        serviceType, implementationType);

                    // implementationType == null when type constraints don't match.
                    implementationType =
                        GenericTypeBuilder.MakeClosedImplementation(serviceType, implementationType);
                }
                else
                {
                    Requires.FactoryReturnsATypeThatIsAssignableFromServiceType(serviceType, implementationType);
                }

                return implementationType;
            }

            public bool MatchesServiceType(Type serviceType) =>
                GenericTypeBuilder.MakeClosedImplementation(serviceType, this.ImplementationType) != null;

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
                new InstanceProducer(context.ServiceType, this.GetRegistration(context), this.Predicate);

            private Registration GetRegistration(PredicateContext context)
            {
                Type key = context.ImplementationType;

                Registration registration;

                // Never build a registration for a particular implementation type twice. This would break
                // the promise of returning singletons.
                if (!this.registrationCache.TryGetValue(key, out registration))
                {
                    this.registrationCache[key] = registration = this.CreateNewRegistrationFor(context);
                }

                return registration;
            }

            private Registration CreateNewRegistrationFor(PredicateContext context) =>
                this.lifestyle.CreateRegistration(context.ImplementationType, context.ImplementationType,
                    this.container);

            private bool MatchesPredicate(PredicateContext context) =>
                this.Predicate != null ? this.Predicate(context) : true;

            private bool RegistrationAppliesToAllClosedServiceTypes()
            {
                // This is nice, if we pass the open generic service type to the GenericTypeBuilder, it
                // can check for us whether the implementation adds extra type constraints that the service
                // type doesn't have. This works, because if it doesn't add any type constraints, it will be
                // able to construct a new open service type, based on the generic type arguments of the
                // implementation. If it can't, it means that the implementionType applies to a subset.
                return this.Predicate == null
                    && this.ImplementationType.IsGenericType
                    && !this.ImplementationType.IsPartiallyClosed()
                    && this.IsImplementationApplicableToEveryGenericType();
            }

            private bool IsImplementationApplicableToEveryGenericType() =>
                GenericTypeBuilder.IsImplementationApplicableToEveryGenericType(
                    this.ServiceType,
                    this.ImplementationType);
        }
    }
}