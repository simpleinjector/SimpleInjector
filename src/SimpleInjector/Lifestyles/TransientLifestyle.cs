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
            return new TransientRegistration(container, concreteType);
        }

        protected internal override Registration CreateRegistrationCore<TService>(
            Func<TService> instanceCreator, Container container)
        {
            return new TransientRegistration(container, typeof(TService), instanceCreator);
        }

        private sealed class TransientRegistration : Registration
        {
            public TransientRegistration(
                Container container, Type implementationType, Func<object>? creator = null)
                : base(Lifestyle.Transient, container, implementationType, creator)
            {
            }

            public override Expression BuildExpression() => this.BuildTransientExpression();
        }
    }
}