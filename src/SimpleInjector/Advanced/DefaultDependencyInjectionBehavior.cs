// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System.Diagnostics;

    [DebuggerDisplay(nameof(DefaultDependencyInjectionBehavior))]
    internal sealed class DefaultDependencyInjectionBehavior : IDependencyInjectionBehavior
    {
        private readonly Container container;

        internal DefaultDependencyInjectionBehavior(Container container)
        {
            this.container = container;
        }

        public void Verify(InjectionConsumerInfo consumer)
        {
            Requires.IsNotNull(consumer, nameof(consumer));

            InjectionTargetInfo target = consumer.Target;

            if (target.TargetType.IsValueType() || target.TargetType == typeof(string))
            {
                throw new ActivationException(StringResources.TypeMustNotContainInvalidInjectionTarget(target));
            }
        }

        public InstanceProducer GetInstanceProducer(InjectionConsumerInfo consumer, bool throwOnFailure)
        {
            Requires.IsNotNull(consumer, nameof(consumer));

            InjectionTargetInfo target = consumer.Target;

            InstanceProducer producer =
                this.container.GetRegistrationEvenIfInvalid(target.TargetType, consumer);

            if (producer == null && throwOnFailure)
            {
                // By redirecting to Verify() we let the verify throw an expressive exception. If it doesn't
                // we throw the exception ourselves.
                this.container.Options.DependencyInjectionBehavior.Verify(consumer);

                this.container.ThrowParameterTypeMustBeRegistered(target);
            }

            return producer;
        }
    }
}