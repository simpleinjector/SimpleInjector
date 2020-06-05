// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Linq.Expressions;

    internal sealed class TransientLifestyle : Lifestyle
    {
        internal TransientLifestyle()
            : base("Transient")
        {
        }

        public override int Length => 1;

        protected internal override Registration CreateRegistrationCore(Type concreteType, Container container)
        {
            return new TransientLifestyleRegistration(this, container, concreteType);
        }

        protected internal override Registration CreateRegistrationCore<TService>(
            Func<TService> instanceCreator, Container container)
        {
            return new TransientLifestyleRegistration<TService>(this, container, instanceCreator);
        }

        private sealed class TransientLifestyleRegistration<TImplementation> : Registration
            where TImplementation : class
        {
            private readonly Func<TImplementation>? instanceCreator;

            public TransientLifestyleRegistration(
                Lifestyle lifestyle, Container container, Func<TImplementation>? instanceCreator = null)
                : base(lifestyle, container)
            {
                this.instanceCreator = instanceCreator;
            }

            public override Type ImplementationType => typeof(TImplementation);

            public override Expression BuildExpression() =>
                this.instanceCreator is null
                    ? this.BuildTransientExpression()
                    : this.BuildTransientExpression(this.instanceCreator);
        }

        private sealed class TransientLifestyleRegistration : Registration
        {
            public TransientLifestyleRegistration(
                Lifestyle lifestyle, Container container, Type implementationType)
                : base(lifestyle, container)
            {
                this.ImplementationType = implementationType;
            }

            public override Type ImplementationType { get; }

            public override Expression BuildExpression() => this.BuildTransientExpression();
        }
    }
}