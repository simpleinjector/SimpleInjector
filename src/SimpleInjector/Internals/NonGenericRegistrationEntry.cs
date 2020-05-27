// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class NonGenericRegistrationEntry : IRegistrationEntry
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

            InstanceProducer? TryGetProducer(InjectionConsumerInfo consumer, bool handled);
        }

        public IEnumerable<InstanceProducer> CurrentProducers =>
            this.providers.SelectMany(p => p.CurrentProducers);

        private IEnumerable<InstanceProducer> ConditionalProducers =>
            this.CurrentProducers.Where(p => p.IsConditional);

        private IEnumerable<InstanceProducer> UnconditionalProducers =>
            this.CurrentProducers.Where(p => !p.IsConditional);

        public int GetNumberOfConditionalRegistrationsFor(Type serviceType) =>
            this.CurrentProducers.Count(p => p.IsConditional);

        public void Add(InstanceProducer producer)
        {
            this.container.ThrowWhenContainerIsLockedOrDisposed();
            this.ThrowWhenConditionalAndUnconditionalAreMixed(producer);
            this.ThrowWhenConditionalIsRegisteredInOverridingMode(producer);

            this.ThrowWhenTypeAlreadyRegistered(producer);
            this.ThrowWhenIdenticalImplementationIsAlreadyRegistered(producer);

            if (producer.IsUnconditional)
            {
                this.providers.Clear();
            }

            this.providers.Add(new SingleInstanceProducerProvider(producer));
        }

        public void Add(
            Type serviceType,
            Func<TypeFactoryContext, Type> implementationTypeFactory,
            Lifestyle lifestyle,
            Predicate<PredicateContext>? predicate)
        {
            Requires.IsNotNull(predicate, "only support conditional for now");

            this.container.ThrowWhenContainerIsLockedOrDisposed();

            if (this.UnconditionalProducers.Any())
            {
                throw new InvalidOperationException(
                    StringResources.NonGenericTypeAlreadyRegisteredAsUnconditionalRegistration(serviceType));
            }

            this.providers.Add(
                new ImplementationTypeFactoryInstanceProducerProvider(
                    serviceType,
                    implementationTypeFactory,
                    lifestyle,
                    predicate!,
                    this.container));
        }

        public InstanceProducer? TryGetInstanceProducer(Type serviceType, InjectionConsumerInfo consumer)
        {
            var instanceProducers = this.GetInstanceProducers(consumer).ToArray();

            if (instanceProducers.Length <= 1)
            {
                return instanceProducers.FirstOrDefault();
            }

            throw this.ThrowMultipleApplicableRegistrationsFound(instanceProducers);
        }

        public void AddGeneric(
            Type serviceType,
            Type implementationType,
            Lifestyle lifestyle,
            Predicate<PredicateContext>? predicate)
        {
            throw new NotSupportedException();
        }

        private IEnumerable<InstanceProducer> GetInstanceProducers(InjectionConsumerInfo consumer)
        {
            bool handled = false;

            foreach (var provider in this.providers)
            {
                InstanceProducer? producer = provider.TryGetProducer(consumer, handled);

                if (producer != null)
                {
                    yield return producer;
                    handled = true;
                }
            }
        }

        private void ThrowWhenTypeAlreadyRegistered(InstanceProducer producer)
        {
            if (producer.IsUnconditional
                && this.providers.Any()
                && !this.container.Options.AllowOverridingRegistrations)
            {
                throw new InvalidOperationException(
                    StringResources.TypeAlreadyRegistered(this.nonGenericServiceType));
            }
        }

        private void ThrowWhenIdenticalImplementationIsAlreadyRegistered(
            InstanceProducer producerToRegister)
        {
            // A provider overlaps the providerToRegister when it can be applied to ALL generic
            // types that the providerToRegister can be applied to as well.
            var overlappingProducers = this.GetOverlappingProducers(producerToRegister);

            bool isReplacement =
                producerToRegister.IsUnconditional && this.container.Options.AllowOverridingRegistrations;

            if (!isReplacement && overlappingProducers.Any())
            {
                var overlappingProducer = overlappingProducers.FirstOrDefault();

                throw new InvalidOperationException(
                    StringResources.AnOverlappingRegistrationExists(
                        producerToRegister.ServiceType,
                        overlappingProducer.FinalImplementationType,
                        overlappingProducer.IsConditional,
                        producerToRegister.FinalImplementationType,
                        producerToRegister.IsConditional));
            }
        }

        private IEnumerable<InstanceProducer> GetOverlappingProducers(InstanceProducer producerToRegister) =>
            from producer in this.CurrentProducers
            where producer.FinalImplementationType != null
            where !producer.Registration.WrapsInstanceCreationDelegate
            where !producerToRegister.Registration.WrapsInstanceCreationDelegate
            where producer.FinalImplementationType == producerToRegister.FinalImplementationType
            select producer;

        private ActivationException ThrowMultipleApplicableRegistrationsFound( InstanceProducer[] producers)
        {
            var producerInfos =
                from producer in producers
                select Tuple.Create(this.nonGenericServiceType, producer.Registration.ImplementationType, producer);

            return new ActivationException(
                StringResources.MultipleApplicableRegistrationsFound(
                    this.nonGenericServiceType, producerInfos.ToArray()));
        }

        private void ThrowWhenConditionalAndUnconditionalAreMixed(InstanceProducer producer)
        {
            this.ThrowWhenNonGenericTypeAlreadyRegisteredAsUnconditionalRegistration(producer);
            this.ThrowWhenNonGenericTypeAlreadyRegisteredAsConditionalRegistration(producer);
        }

        private void ThrowWhenConditionalIsRegisteredInOverridingMode(InstanceProducer producer)
        {
            if (producer.IsConditional && this.container.Options.AllowOverridingRegistrations)
            {
                throw new NotSupportedException(
                    StringResources.MakingConditionalRegistrationsInOverridingModeIsNotSupported());
            }
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

            public SingleInstanceProducerProvider(InstanceProducer producer) => this.producer = producer;

            public IEnumerable<InstanceProducer> CurrentProducers => Enumerable.Repeat(this.producer, 1);

            public InstanceProducer? TryGetProducer(InjectionConsumerInfo consumer, bool handled) =>
                this.producer.Predicate(new PredicateContext(this.producer, consumer, handled))
                    ? this.producer
                    : null;
        }

        private class ImplementationTypeFactoryInstanceProducerProvider : IProducerProvider
        {
            private readonly Dictionary<Type, InstanceProducer> cache =
                new Dictionary<Type, InstanceProducer>();

            private readonly Func<TypeFactoryContext, Type> implementationTypeFactory;
            private readonly Lifestyle lifestyle;
            private readonly Predicate<PredicateContext> predicate;
            private readonly Type serviceType;
            private readonly Container container;

            public ImplementationTypeFactoryInstanceProducerProvider(
                Type serviceType,
                Func<TypeFactoryContext, Type> implementationTypeFactory,
                Lifestyle lifestyle,
                Predicate<PredicateContext> predicate,
                Container container)
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

            public InstanceProducer? TryGetProducer(InjectionConsumerInfo consumer, bool handled)
            {
                Type GetImplementationType() => this.GetImplementationTypeThroughFactory(consumer);

                var context =
                    new PredicateContext(this.serviceType, GetImplementationType, consumer, handled);

                // NOTE: The producer should only get built after it matches the delegate, to prevent
                // unneeded producers from being created, because this might cause diagnostic warnings,
                // such as torn lifestyle warnings.
                return this.predicate(context) ? this.GetProducer(context) : null;
            }

            private Type GetImplementationTypeThroughFactory(InjectionConsumerInfo consumer)
            {
                var context = new TypeFactoryContext(this.serviceType, consumer);

                Type implementationType = this.implementationTypeFactory(context);

                if (implementationType is null)
                {
                    throw new InvalidOperationException(
                        StringResources.FactoryReturnedNull(this.serviceType));
                }

                if (implementationType.ContainsGenericParameters())
                {
                    throw new ActivationException(
                        StringResources.TheTypeReturnedFromTheFactoryShouldNotBeOpenGeneric(
                            this.serviceType, implementationType));
                }

                Requires.FactoryReturnsATypeThatIsAssignableFromServiceType(
                    this.serviceType, implementationType);

                return implementationType;
            }

            private InstanceProducer GetProducer(PredicateContext context)
            {
                InstanceProducer producer;

                // Never build a producer twice. This could cause components with a torn lifestyle.
                lock (this.cache)
                {
                    // ImplementationType will never be null at this point.
                    Type implementationType = context.ImplementationType!;

                    // We need to cache on implementation, because service type is always the same.
                    if (!this.cache.TryGetValue(implementationType, out producer))
                    {
                        this.cache[implementationType] =
                            producer = this.CreateNewProducerFor(implementationType);
                    }
                }

                return producer;
            }

            private InstanceProducer CreateNewProducerFor(Type concreteType) =>
                InstanceProducer.Create(
                    this.serviceType,
                    this.lifestyle.CreateRegistration(concreteType, this.container),
                    this.predicate);
        }
    }
}