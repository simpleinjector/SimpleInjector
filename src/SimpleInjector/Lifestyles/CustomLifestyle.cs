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

        protected internal override Registration CreateRegistrationCore<TConcrete>(Container container) =>
            new CustomRegistration<TConcrete>(this.lifestyleApplierFactory, this, container);

        protected internal override Registration CreateRegistrationCore<TService>(
            Func<TService> instanceCreator, Container container)
        {
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            return new CustomRegistration<TService>(
                this.lifestyleApplierFactory, this, container, instanceCreator);
        }

        private sealed class CustomRegistration<TImplementation> : Registration where TImplementation : class
        {
            private readonly CreateLifestyleApplier lifestyleApplierFactory;
            private readonly Func<TImplementation>? instanceCreator;

            public CustomRegistration(
                CreateLifestyleApplier lifestyleApplierFactory,
                Lifestyle lifestyle,
                Container container,
                Func<TImplementation>? instanceCreator = null)
                : base(lifestyle, container)
            {
                this.lifestyleApplierFactory = lifestyleApplierFactory;
                this.instanceCreator = instanceCreator;
            }

            public override Type ImplementationType => typeof(TImplementation);

            public override Expression BuildExpression() =>
                Expression.Convert(
                    Expression.Invoke(
                        Expression.Constant(this.CreateInstanceCreator())),
                    typeof(TImplementation));

            private Func<object> CreateInstanceCreator()
            {
                Func<TImplementation> transientInstanceCreator =
                    this.instanceCreator == null
                        ? (Func<TImplementation>)this.BuildTransientDelegate()
                        : this.BuildTransientDelegate(this.instanceCreator);

                return this.lifestyleApplierFactory(() => transientInstanceCreator());
            }
        }
    }
}