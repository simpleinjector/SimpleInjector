// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Linq.Expressions;

    internal sealed class ScopedRegistration<TImplementation> : Registration
        where TImplementation : class
    {
        private readonly Func<TImplementation>? userSuppliedInstanceCreator;

        private Func<Scope?>? scopeFactory;

        internal ScopedRegistration(
            ScopedLifestyle lifestyle, Container container, Func<TImplementation> instanceCreator)
            : this(lifestyle, container)
        {
            this.userSuppliedInstanceCreator = instanceCreator;
        }

        internal ScopedRegistration(ScopedLifestyle lifestyle, Container container)
            : base(lifestyle, container)
        {
        }

        public override Type ImplementationType => typeof(TImplementation);
        public new ScopedLifestyle Lifestyle => (ScopedLifestyle)base.Lifestyle;

        // Initialized when BuildExpression is called
        internal Func<TImplementation>? InstanceCreator { get; private set; }

        public override Expression BuildExpression()
        {
            if (this.InstanceCreator == null)
            {
                this.scopeFactory = this.Lifestyle.CreateCurrentScopeProvider(this.Container);

                this.InstanceCreator = this.BuildInstanceCreator();
            }

            return Expression.Call(
                instance: Expression.Constant(this),
                method: this.GetType().GetMethod(nameof(this.GetInstance)));
        }

        // This method needs to be public, because the BuildExpression methods build a
        // MethodCallExpression using this method, and this would fail in partial trust when the
        // method is not public.
        // Simple Injector does some aggressive optimizations for scoped lifestyles and this method will
        // is most cases not be called. It will however be called when the expression that is built by
        // this instance will get compiled by someone else than the core library. That's why this method
        // is still important.
        public TImplementation GetInstance() => Scope.GetInstance(this, this.scopeFactory!());

        private Func<TImplementation> BuildInstanceCreator() =>
            this.userSuppliedInstanceCreator != null
                ? this.BuildTransientDelegate(this.userSuppliedInstanceCreator)
                : (Func<TImplementation>)this.BuildTransientDelegate();
    }
}