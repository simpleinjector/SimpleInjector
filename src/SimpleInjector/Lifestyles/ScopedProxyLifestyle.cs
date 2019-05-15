// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;

    internal sealed class ScopedProxyLifestyle : ScopedLifestyle
    {
        public ScopedProxyLifestyle() : base("Scoped")
        {
        }

        internal override int ComponentLength(Container container) =>
            GetDefaultScopedLifestyle(container).ComponentLength(container);

        internal override int DependencyLength(Container container) =>
            GetDefaultScopedLifestyle(container).DependencyLength(container);

        protected internal override Func<Scope> CreateCurrentScopeProvider(Container container) =>
            GetDefaultScopedLifestyle(container).CreateCurrentScopeProvider(container);

        protected internal override Registration CreateRegistrationCore<TConcrete>(Container container) =>
            GetDefaultScopedLifestyle(container).CreateRegistrationCore<TConcrete>(container);

        protected internal override Registration CreateRegistrationCore<TService>(
            Func<TService> instanceCreator, Container container) =>
            GetDefaultScopedLifestyle(container).CreateRegistrationCore(instanceCreator, container);

        protected override Scope GetCurrentScopeCore(Container container) =>
            GetDefaultScopedLifestyle(container).GetCurrentScope(container);

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static ScopedLifestyle GetDefaultScopedLifestyle(Container container) =>
            container.Options.DefaultScopedLifestyle ?? ThrowDefaultScopeLifestyleIsNotSet();

        private static ScopedLifestyle ThrowDefaultScopeLifestyleIsNotSet() =>
            throw new InvalidOperationException(
                StringResources.ScopePropertyCanOnlyBeUsedWhenDefaultScopedLifestyleIsConfigured());
    }
}