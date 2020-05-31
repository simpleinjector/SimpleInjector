// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals.Builders
{
    using System;
    using System.Collections.Generic;
    using SimpleInjector.Lifestyles;

    internal sealed class UnregisteredTypeResolutionInstanceProducerBuilder
    {
        private readonly Dictionary<Type, LazyEx<InstanceProducer>> resolveUnregisteredTypeRegistrations =
            new Dictionary<Type, LazyEx<InstanceProducer>>();

        private readonly Container container;
        private readonly Func<bool> shouldResolveUnregisteredTypes;
        private readonly Action<UnregisteredTypeEventArgs> resolveUnregisteredType;

        internal UnregisteredTypeResolutionInstanceProducerBuilder(
            Container container,
            Func<bool> shouldResolveUnregisteredTypes,
            Action<UnregisteredTypeEventArgs> resolveUnregisteredType)
        {
            this.container = container;
            this.shouldResolveUnregisteredTypes = shouldResolveUnregisteredTypes;
            this.resolveUnregisteredType = resolveUnregisteredType;

            // Add the default registrations. This adds them as registration, but only in case some component
            // starts depending on them.
            var scopeLifestyle = new ScopedScopeLifestyle();

            this.resolveUnregisteredTypeRegistrations[typeof(Scope)] = new LazyEx<InstanceProducer>(
                () => scopeLifestyle.CreateProducer(() => scopeLifestyle.GetCurrentScope(container), container));

            this.resolveUnregisteredTypeRegistrations[typeof(Container)] = new LazyEx<InstanceProducer>(
                () => Lifestyle.Singleton.CreateProducer(() => container, container));
        }

        // Instead of wrapping the complete method in a lock, we lock inside the individual methods. We
        // don't want to hold a lock while calling back into user code, because who knows what the user
        // is doing there. We don't want a dead lock.
        public InstanceProducer? TryBuild(Type serviceType) =>
            this.TryGetInstanceProducerForUnregisteredTypeResolutionFromCache(serviceType) ??
            this.TryGetInstanceProducerThroughResolveUnregisteredTypeEvent(serviceType);

        private InstanceProducer? TryGetInstanceProducerForUnregisteredTypeResolutionFromCache(Type serviceType)
        {
            lock (this.resolveUnregisteredTypeRegistrations)
            {
                return this.resolveUnregisteredTypeRegistrations.ContainsKey(serviceType)
                    ? this.resolveUnregisteredTypeRegistrations[serviceType].Value
                    : null;
            }
        }

        private InstanceProducer? TryGetInstanceProducerThroughResolveUnregisteredTypeEvent(Type serviceType)
        {
            if (!this.shouldResolveUnregisteredTypes())
            {
                return null;
            }

            var e = new UnregisteredTypeEventArgs(serviceType);

            this.resolveUnregisteredType(e);

            return e.Handled
                ? this.TryGetProducerFromUnregisteredTypeResolutionCacheOrAdd(e)
                : null;
        }

        private InstanceProducer? TryGetProducerFromUnregisteredTypeResolutionCacheOrAdd(
            UnregisteredTypeEventArgs e)
        {
            lock (this.resolveUnregisteredTypeRegistrations)
            {
                if (this.resolveUnregisteredTypeRegistrations.ContainsKey(e.UnregisteredServiceType))
                {
                    // This line will only get hit, in case a different thread came here first.
                    return this.resolveUnregisteredTypeRegistrations[e.UnregisteredServiceType].Value;
                }

                var registration = e.Registration ?? new ExpressionRegistration(e.Expression!, this.container);

                // By creating the InstanceProducer after checking the dictionary, we prevent the producer
                // from being created twice when multiple threads are running. Having the same duplicate
                // producer can cause a torn lifestyle warning in the container.
                var producer = InstanceProducer.Create(e.UnregisteredServiceType, registration);

                this.resolveUnregisteredTypeRegistrations[e.UnregisteredServiceType] =
                    Helpers.ToLazy(producer);

                return producer;
            }
        }
    }
}