// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;

    internal struct FoundInstanceProducer
    {
        public readonly Type ServiceType;
        public readonly Type ImplementationType;
        public readonly InstanceProducer Producer;

        public FoundInstanceProducer(Type serviceType, Type implementationType, InstanceProducer producer)
        {
            this.ServiceType = serviceType;
            this.ImplementationType = implementationType;
            this.Producer = producer;
        }
    }
}