// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay(nameof(DefaultDependencyInjectionBehavior))]
    internal sealed class DefaultDependencyInjectionBehavior : IDependencyInjectionBehavior
    {
        private readonly Container container;

        internal DefaultDependencyInjectionBehavior(Container container) => this.container = container;

        public bool VerifyDependency(InjectionConsumerInfo dependency, out string? errorMessage)
        {
            Requires.IsNotNull(dependency, nameof(dependency));

            var valid = !HasValueTypeSemantics(dependency.Target.TargetType);

            errorMessage =
                valid ? null : StringResources.TypeMustNotContainInvalidInjectionTarget(dependency.Target);

            return valid;
        }

        public InstanceProducer? GetInstanceProducer(InjectionConsumerInfo dependency, bool throwOnFailure)
        {
            Requires.IsNotNull(dependency, nameof(dependency));

            InjectionTargetInfo target = dependency.Target;

            InstanceProducer? producer =
                this.container.GetRegistrationEvenIfInvalid(target.TargetType, dependency);

            if (producer is null && throwOnFailure)
            {
                // By redirecting to Verify() we let the verify throw an expressive exception. If it doesn't
                // we throw the exception ourselves.
                this.container.Options.DependencyInjectionBehavior.Verify(dependency);

                this.container.ThrowParameterTypeMustBeRegistered(target);
            }

            return producer;
        }

        private static bool HasValueTypeSemantics(Type targetType) =>
            targetType.IsValueType() || targetType == typeof(string);
    }
}