// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;

    internal interface IRegistrationEntry
    {
        IEnumerable<InstanceProducer> CurrentProducers { get; }

        void Add(InstanceProducer producer);

        void AddGeneric(
            Type serviceType,
            Type implementationType,
            Lifestyle lifestyle,
            Predicate<PredicateContext> predicate = null);

        void Add(
            Type serviceType,
            Func<TypeFactoryContext, Type> implementationTypeFactory,
            Lifestyle lifestyle,
            Predicate<PredicateContext> predicate = null);

        InstanceProducer TryGetInstanceProducer(Type serviceType, InjectionConsumerInfo consumer);

        int GetNumberOfConditionalRegistrationsFor(Type serviceType);
    }
}