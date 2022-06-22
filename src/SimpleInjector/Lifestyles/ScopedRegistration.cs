// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal sealed class ScopedRegistration : Registration
    {
        private static readonly Func<string> EmptyStringProvider = () => string.Empty;

        private Func<Scope?>? scopeFactory;
        private DisposabilityTypeInfo? disposability;

        internal ScopedRegistration(
            ScopedLifestyle lifestyle, Container container, Type implementationType, Func<object>? creator)
            : base(lifestyle, container, implementationType, creator)
        {
        }

        public new ScopedLifestyle Lifestyle => (ScopedLifestyle)base.Lifestyle;

        // Initialized when BuildExpression is called
        internal Func<object>? InstanceCreator { get; private set; }

        // MEM: We store a delegate here instead of the final string, because the formatted string will take
        // up a lot of memory, especially because there could be many scoped registrations with this info set.
        internal Func<string> AdditionalInformationForLifestyleMismatchDiagnosticsProvider { get; set; } =
            EmptyStringProvider;

        internal DisposabilityTypeInfo Disposability
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (this.disposability is null)
                {
                    this.disposability = new DisposabilityTypeInfo(this.ImplementationType, this);
                }

                return this.disposability.Value;
            }
        }

        public override Expression BuildExpression()
        {
            if (this.InstanceCreator is null)
            {
                this.scopeFactory = this.Lifestyle.CreateCurrentScopeProvider(this.Container);

                this.InstanceCreator = this.BuildTransientDelegate();
            }

            return Expression.Call(
                instance: Expression.Constant(this),
                method: this.GetType().GetMethod(nameof(this.GetInstance))
                    .MakeGenericMethod(this.ImplementationType));
        }

        internal static ScopedRegistration? GetScopedRegistration(MethodCallExpression node) =>
            node.Object is ConstantExpression instance
                ? instance.Value as ScopedRegistration
                : null;

        // This method needs to be public, because the BuildExpression methods build a
        // MethodCallExpression using this method, and this would fail in partial trust when the
        // method is not public.
        // Simple Injector does some aggressive optimizations for scoped lifestyles and this method will
        // is most cases not be called. It will however be called when the expression that is built by
        // this instance will get compiled by someone else than the core library. That's why this method
        // is still important.
        public TImplementation GetInstance<TImplementation>()
            where TImplementation : class =>
            Scope.GetInstance<TImplementation>(this, this.scopeFactory!());
    }
}