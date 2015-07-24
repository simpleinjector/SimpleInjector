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

    internal interface IRegistrationEntry
    {
        IEnumerable<InstanceProducer> CurrentProducers { get; }

        void Add(InstanceProducer producer);

        void AddGeneric(Type serviceType, Type implementationType, Lifestyle lifestyle,
            Predicate<PredicateContext> predicate = null);

        void Add(Type serviceType, Func<TypeFactoryContext, Type> implementationTypeFactory, 
            Lifestyle lifestyle, Predicate<PredicateContext> predicate = null);

        InstanceProducer TryGetInstanceProducer(Type serviceType, InjectionConsumerInfo consumer);

        int GetNumberOfConditionalRegistrationsFor(Type serviceType);
    }

    internal static class RegistrationEntry
    {
        internal static IRegistrationEntry Create(Type serviceType, Container container)
        {
            return serviceType.IsGenericType
                ? new GenericRegistrationEntry(serviceType.GetGenericTypeDefinition(), container)
                : (IRegistrationEntry)new NonGenericRegistrationEntry(serviceType, container);
        }

        private sealed class NonGenericRegistrationEntry : IRegistrationEntry
        {
            private readonly List<IProducerProvider> providers = new List<IProducerProvider>(1);
            private readonly Type nonGenericServiceType;
            private readonly Container container;

            public NonGenericRegistrationEntry(Type nonGenericServiceType, Container container)
            {
                this.nonGenericServiceType = nonGenericServiceType;
                this.container = container;
            }

            private interface IProducerProvider
            {
                IEnumerable<InstanceProducer> CurrentProducers { get; }

                InstanceProducer TryGetProducer(InjectionConsumerInfo consumer, bool handled);
            }

            public IEnumerable<InstanceProducer> CurrentProducers => this.providers.SelectMany(p => p.CurrentProducers);
            private IEnumerable<InstanceProducer> ConditionalProducers => this.CurrentProducers.Where(p => p.IsConditional);
            private IEnumerable<InstanceProducer> UnconditionalProducers => this.CurrentProducers.Where(p => !p.IsConditional);
            public int GetNumberOfConditionalRegistrationsFor(Type serviceType) => this.CurrentProducers.Count(p => p.IsConditional);

            public void Add(InstanceProducer producer)
            {
                this.container.ThrowWhenContainerIsLocked();
                this.ThrowWhenConditionalAndUnconditionalAreMixed(producer);

                this.ThrowWhenTypeAlreadyRegistered(producer);
                this.ThrowWhenProviderToRegisterOverlapsWithExistingProvider(producer);

                if (producer.IsUnconditional)
                {
                    this.providers.Clear();
                }

                this.providers.Add(new SingleInstanceProducerProvider(producer));
            }
            
            private void ThrowWhenProviderToRegisterOverlapsWithExistingProvider(
                InstanceProducer producerToRegister)
            {
                // A provider is a superset of the providerToRegister when it can be applied to ALL generic
                // types that the providerToRegister can be applied to as well.
                var overlappingProducers =
                    from producer in this.CurrentProducers
                    where producer.ImplementationType != null
                    where !producer.Registration.WrapsInstanceCreationDelegate
                    where !producerToRegister.Registration.WrapsInstanceCreationDelegate
                    where producer.ImplementationType == producerToRegister.ImplementationType
                    select producer;

                if (overlappingProducers.Any())
                {
                    var overlappingProducer = overlappingProducers.FirstOrDefault();

                    throw new InvalidOperationException(
                        StringResources.AnOverlappingGenericRegistrationExists(
                            producerToRegister.ServiceType,
                            overlappingProducer.ImplementationType,
                            overlappingProducer.IsConditional,
                            producerToRegister.ImplementationType,
                            producerToRegister.Predicate != null));
                }
            }

            public void Add(Type serviceType, Func<TypeFactoryContext, Type> implementationTypeFactory,
                Lifestyle lifestyle, Predicate<PredicateContext> predicate)
            {
                Requires.IsNotNull(predicate, "only support conditional for now");

                this.container.ThrowWhenContainerIsLocked();

                if (this.UnconditionalProducers.Any())
                {
                    throw new InvalidOperationException(
                        StringResources.NonGenericTypeAlreadyRegisteredAsUnconditionalRegistration(serviceType));
                }

                this.providers.Add(new ImplementationTypeFactoryInstanceProducerProvider(serviceType, 
                    implementationTypeFactory, lifestyle, predicate, this.container));
            }

            public InstanceProducer TryGetInstanceProducer(Type serviceType, InjectionConsumerInfo context)
            {
                var instanceProducers = this.GetInstanceProducers(context).ToArray();

                if (instanceProducers.Length <= 1)
                {
                    return instanceProducers.FirstOrDefault();
                }

                throw this.ThrowMultipleApplicableRegistrationsFound(instanceProducers);
            }

            public void AddGeneric(Type serviceType, Type implementationType,
                Lifestyle lifestyle, Predicate<PredicateContext> predicate)
            {
                throw new NotSupportedException();
            }

            private IEnumerable<InstanceProducer> GetInstanceProducers(InjectionConsumerInfo consumer)
            {
                bool handled = false;

                foreach (var provider in this.providers)
                {
                    InstanceProducer producer = provider.TryGetProducer(consumer, handled);

                    if (producer != null)
                    {
                        yield return producer;
                        handled = true;
                    }
                }
            }

            private void ThrowWhenTypeAlreadyRegistered(InstanceProducer producer)
            {
                if (producer.IsUnconditional && this.providers.Any() &&
                    !this.container.Options.AllowOverridingRegistrations)
                {
                    throw new InvalidOperationException(StringResources.TypeAlreadyRegistered(this.nonGenericServiceType));
                }
            }

            private ActivationException ThrowMultipleApplicableRegistrationsFound(
                InstanceProducer[] instanceProducers)
            {
                var producersInfo =
                    from producer in instanceProducers
                    select Tuple.Create(this.nonGenericServiceType, producer.Registration.ImplementationType, producer);

                return new ActivationException(
                    StringResources.MultipleApplicableRegistrationsFound(
                        this.nonGenericServiceType, producersInfo.ToArray()));
            }

            private void ThrowWhenConditionalAndUnconditionalAreMixed(InstanceProducer producer)
            {
                this.ThrowWhenNonGenericTypeAlreadyRegisteredAsUnconditionalRegistration(producer);
                this.ThrowWhenNonGenericTypeAlreadyRegisteredAsConditionalRegistration(producer);
            }

            private void ThrowWhenNonGenericTypeAlreadyRegisteredAsUnconditionalRegistration(
                InstanceProducer producer)
            {
                if (producer.IsConditional && this.UnconditionalProducers.Any())
                {
                    throw new InvalidOperationException(
                        StringResources.NonGenericTypeAlreadyRegisteredAsUnconditionalRegistration(
                            producer.ServiceType));
                }
            }

            private void ThrowWhenNonGenericTypeAlreadyRegisteredAsConditionalRegistration(
                InstanceProducer producer)
            {
                if (producer.IsUnconditional && this.ConditionalProducers.Any())
                {
                    throw new InvalidOperationException(
                        StringResources.NonGenericTypeAlreadyRegisteredAsConditionalRegistration(
                            producer.ServiceType));
                }
            }

            private sealed class SingleInstanceProducerProvider : IProducerProvider
            {
                private readonly InstanceProducer producer;

                public SingleInstanceProducerProvider(InstanceProducer producer)
                {
                    this.producer = producer;
                }

                public IEnumerable<InstanceProducer> CurrentProducers => Enumerable.Repeat(this.producer, 1);

                public InstanceProducer TryGetProducer(InjectionConsumerInfo consumer, bool handled)
                {
                    return producer.IsUnconditional || producer.Predicate(new PredicateContext(producer, consumer, handled))
                        ? this.producer
                        : null;
                }
            }

            internal class ImplementationTypeFactoryInstanceProducerProvider : IProducerProvider
            {
                private readonly Dictionary<Type, InstanceProducer> cache = new Dictionary<Type, InstanceProducer>();

                private readonly Func<TypeFactoryContext, Type> implementationTypeFactory;
                private readonly Lifestyle lifestyle;
                private readonly Predicate<PredicateContext> predicate;
                private readonly Type serviceType;
                private readonly Container container;

                public ImplementationTypeFactoryInstanceProducerProvider(Type serviceType, 
                    Func<TypeFactoryContext, Type> implementationTypeFactory, Lifestyle lifestyle, 
                    Predicate<PredicateContext> predicate, Container container)
                {
                    this.serviceType = serviceType;
                    this.implementationTypeFactory = implementationTypeFactory;
                    this.lifestyle = lifestyle;
                    this.predicate = predicate;
                    this.container = container;
                }

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

                public InstanceProducer TryGetProducer(InjectionConsumerInfo consumer, bool handled)
                {
                    Func<Type> implementationTypeProvider = () => 
                        this.GetImplementationTypeThroughFactory(serviceType, consumer);

                    var context = new PredicateContext(serviceType, implementationTypeProvider, consumer, handled);

                    // NOTE: The producer should only get built after it matches the delegate, to prevent
                    // unneeded producers from being created, because this might cause diagnostic warnings, 
                    // such as torn lifestyle warnings.
                    return this.predicate(context)
                        ? this.GetProducer(serviceType, context.ImplementationType)
                        : null;
                }
                
                private Type GetImplementationTypeThroughFactory(Type serviceType, InjectionConsumerInfo consumer)
                {
                    Type implementationType =
                        this.implementationTypeFactory(new TypeFactoryContext(serviceType, consumer));

                    if (implementationType == null)
                    {
                        throw new InvalidOperationException(StringResources.FactoryReturnedNull(this.serviceType));
                    }

                    if (implementationType.ContainsGenericParameters)
                    {
                        throw new ActivationException(
                            StringResources.TheTypeReturnedFromTheFactoryShouldNotBeOpenGeneric(
                                serviceType, implementationType));
                    }

                    Requires.FactoryReturnsATypeThatIsAssignableFromServiceType(serviceType, implementationType);

                    return implementationType;
                }

                private InstanceProducer GetProducer(Type serviceType, Type closedImplementation)
                {
                    InstanceProducer producer;

                    // Never build a producer twice. This could cause components with a torn lifestyle.
                    lock (this.cache)
                    {
                        if (!this.cache.TryGetValue(serviceType, out producer))
                        {
                            this.cache[serviceType] =
                                producer = this.CreateNewProducerFor(serviceType, closedImplementation);
                        }
                    }

                    return producer;
                }

                private InstanceProducer CreateNewProducerFor(Type serviceType, Type closedImplementation)
                {
                    return new InstanceProducer(
                        serviceType,
                        this.lifestyle.CreateRegistration(serviceType, closedImplementation, this.container),
                        this.predicate);
                }
            }
        }

        private sealed class GenericRegistrationEntry : IRegistrationEntry
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

                IEnumerable<InstanceProducer> GetCurrentProducers();

                bool OverlapsWith(Type closedServiceType);

                InstanceProducer TryGetProducer(Type serviceType, InjectionConsumerInfo consumer, 
                    bool handled = false);

                bool MatchesServiceType(Type serviceType);
            }

            public IEnumerable<InstanceProducer> CurrentProducers
            {
                get { return this.providers.SelectMany(p => p.GetCurrentProducers()); }
            }

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

            public int GetNumberOfConditionalRegistrationsFor(Type serviceType)
            {
                var conditionalProvidersForServiceType =
                    from provider in this.providers
                    where provider.IsConditional
                    where provider.MatchesServiceType(serviceType)
                    select provider;

                return conditionalProvidersForServiceType.Count();
            }

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
                // A provider is a superset of the providerToRegister when it can be applied to ALL generic
                // types that the providerToRegister can be applied to as well.
                var supersetProviders =
                    from provider in this.providers
                    where provider.ImplementationType != null
                    where providerToRegister.ImplementationType != null
                    where provider.AppliesToAllClosedServiceTypes
                        || provider.ImplementationType == providerToRegister.ImplementationType
                    select provider;

                bool providerToRegisterIsSuperset =
                    providerToRegister.AppliesToAllClosedServiceTypes && this.providers.Any();

                if (providerToRegisterIsSuperset || supersetProviders.Any())
                {
                    var overlappingProvider = supersetProviders.FirstOrDefault() ?? this.providers.First();

                    throw new InvalidOperationException(
                        StringResources.AnOverlappingGenericRegistrationExists(
                            providerToRegister.ServiceType,
                            overlappingProvider.ImplementationType,
                            overlappingProvider.IsConditional,
                            providerToRegister.ImplementationType,
                            providerToRegister.Predicate != null));
                }
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

                public bool IsConditional
                {
                    get { return this.producer.IsConditional; }
                }

                public bool AppliesToAllClosedServiceTypes
                {
                    get { return false; }
                }

                public Type ServiceType
                {
                    get { return this.producer.ServiceType; }
                }

                public Type ImplementationType
                {
                    get { return this.producer.Registration.ImplementationType; }
                }

                public IEnumerable<InstanceProducer> GetCurrentProducers()
                {
                    return Enumerable.Repeat(this.producer, 1);
                }

                public bool OverlapsWith(Type closedServiceType)
                {
                    return !this.producer.IsConditional && this.producer.ServiceType == closedServiceType;
                }

                public InstanceProducer TryGetProducer(Type serviceType, InjectionConsumerInfo consumer, bool handled)
                {
                    return this.MatchesServiceType(serviceType) && this.MatchesPredicate(consumer, handled)
                        ? this.producer
                        : null;
                }

                public bool MatchesServiceType(Type serviceType)
                {
                    return serviceType == this.producer.ServiceType;
                }

                private bool MatchesPredicate(InjectionConsumerInfo consumer, bool handled)
                {
                    if (this.producer.IsConditional)
                    {
                        var context = new PredicateContext(this.producer, consumer, handled);

                        return this.producer.Predicate(context);
                    }

                    return true;
                }
            }

            private sealed class OpenGenericToInstanceProducerProvider : IProducerProvider
            {
                internal readonly Predicate<PredicateContext> Predicate;

                private readonly Lifestyle lifestyle;
                private readonly Container container;

                private readonly Dictionary<Type, InstanceProducer> cache = new Dictionary<Type, InstanceProducer>();

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

                public bool IsConditional
                {
                    get { return this.Predicate != null; }
                }

                public bool AppliesToAllClosedServiceTypes { get; }

                public Type ServiceType { get; }

                public Type ImplementationType { get; }

                public Func<TypeFactoryContext, Type> ImplementationTypeFactory { get; }

                public IEnumerable<InstanceProducer> GetCurrentProducers() => this.cache.Values;

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

                    return shouldBuildProducer
                        ? this.GetProducer(serviceType, context.ImplementationType)
                        : null;
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
                        //Requires.ServiceOrItsGenericTypeDefinitionIsAssignableFromImplementation(serviceType, 
                        //    implementationType, nameof(serviceType));

                        //if (!serviceType.IsGenericType)
                        //{
                        //    throw new ActivationException(
                        //        StringResources.TheTypeReturnedFromTheFactoryShouldNotBeOpenGeneric(
                        //            serviceType, implementationType));
                        //}

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

                public bool MatchesServiceType(Type serviceType)
                {
                    return GenericTypeBuilder.MakeClosedImplementation(serviceType, this.ImplementationType) != null;
                }

                private InstanceProducer GetProducer(Type serviceType, Type closedImplementation)
                {
                    InstanceProducer producer;

                    // Never build a producer twice. This could cause components with a torn lifestyle.
                    lock (this.cache)
                    {
                        if (!this.cache.TryGetValue(serviceType, out producer))
                        {
                            this.cache[serviceType] = 
                                producer = this.CreateNewProducerFor(serviceType, closedImplementation);
                        }
                    }

                    return producer;
                }

                private InstanceProducer CreateNewProducerFor(Type serviceType, Type closedImplementation)
                {
                    return new InstanceProducer(
                        serviceType,
                        this.lifestyle.CreateRegistration(serviceType, closedImplementation, this.container),
                        this.Predicate);
                }

                private bool MatchesPredicate(PredicateContext context)
                {
                    return this.Predicate != null ? this.Predicate(context) : true;
                }

                private bool RegistrationAppliesToAllClosedServiceTypes()
                {
                    // This is nice, if we pass the open generic service type to the GenericTypeBuilder, it
                    // can check for us whether the implementation adds extra type constraints that the service
                    // type doesn't have. This works, because if it doesn't add any type constraints, it will be
                    // able to construct a new open service type, based on the generic type arguments of the
                    // implementation. If it can't, it means that the implementionType applies to a subset.
                    return
                        this.Predicate == null
                        && this.ImplementationType.IsGenericType
                        && !this.ImplementationType.IsPartiallyClosed()
                        && this.IsImplementationApplicableToEveryGenericType();
                }

                private bool IsImplementationApplicableToEveryGenericType()
                {
                    return GenericTypeBuilder.IsImplementationApplicableToEveryGenericType(
                        this.ServiceType,
                        this.ImplementationType);
                }
            }
        }
    }
}