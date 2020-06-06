// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Linq.Expressions;

    internal sealed class CustomLifestyle : Lifestyle
    {
        private readonly CreateLifestyleApplier lifestyleApplierFactory;

        public CustomLifestyle(string name, CreateLifestyleApplier lifestyleApplierFactory)
            : base(name)
        {
            this.lifestyleApplierFactory = lifestyleApplierFactory;
        }

        public override int Length =>
            throw new NotSupportedException("The length property is not supported for this lifestyle.");

        // Ensure that this lifestyle can only be safely used with singleton dependencies.
        internal override int ComponentLength(Container container) => Singleton.ComponentLength(container);

        // Ensure that this lifestyle can only be safely used with transient components/consumers.
        internal override int DependencyLength(Container container) => Transient.DependencyLength(container);

        protected internal override Registration CreateRegistrationCore(Type concreteType, Container container) =>
            new CustomRegistration(this.lifestyleApplierFactory, this, container, concreteType);

        protected internal override Registration CreateRegistrationCore<TService>(
            Func<TService> instanceCreator, Container container)
        {
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            return new CustomRegistration(
                this.lifestyleApplierFactory, this, container, typeof(TService), instanceCreator);
        }

        private sealed class CustomRegistration : Registration
        {
            private readonly CreateLifestyleApplier lifestyleApplierFactory;

            public CustomRegistration(
                CreateLifestyleApplier lifestyleApplierFactory,
                Lifestyle lifestyle,
                Container container,
                Type implementationType,
                Func<object>? instanceCreator = null)
                : base(lifestyle, container, implementationType, instanceCreator)
            {
                this.lifestyleApplierFactory = lifestyleApplierFactory;
            }

            public override Expression BuildExpression()
            {
                var creator = this.BuildTransientDelegate();

                var lifestyleAppliedCreator = this.lifestyleApplierFactory(creator);

                return
                    Expression.Convert(
                        Expression.Invoke(
                            Expression.Constant(lifestyleAppliedCreator)),
                        this.ImplementationType);
            }
        }
    }
}