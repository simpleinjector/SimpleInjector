// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;

    // Lifestyle explicitly to allow resolve and inject a SimpleInjector.Scope itself.
    internal sealed class ScopedScopeLifestyle : ScopedLifestyle
    {
        internal static readonly ScopedScopeLifestyle Instance = new ScopedScopeLifestyle();

        internal ScopedScopeLifestyle() : base("Scoped")
        {
        }

        internal new Scope GetCurrentScope(Container container)
        {
            // ScopedScopeLifestyle.GetCurrentScopeCore will never return null.
            return base.GetCurrentScope(container)!;
        }

        protected internal override Func<Scope?> CreateCurrentScopeProvider(Container container) =>
            () => this.GetScopeFromDefaultScopedLifestyle(container);

        protected override Scope? GetCurrentScopeCore(Container container) =>
            this.GetScopeFromDefaultScopedLifestyle(container);

        private Scope GetScopeFromDefaultScopedLifestyle(Container container)
        {
            ScopedLifestyle? lifestyle = container.Options.DefaultScopedLifestyle;

            if (lifestyle != null)
            {
                return lifestyle.GetCurrentScope(container)
                    ?? ThrowThereIsNoActiveScopeException();
            }

            return container.GetVerificationOrResolveScopeForCurrentThread()
                ?? ThrowResolveFromScopeOrRegisterDefaultScopedLifestyleException();
        }

        private static Scope ThrowResolveFromScopeOrRegisterDefaultScopedLifestyleException() =>
            throw new InvalidOperationException(
                "To be able to resolve and inject Scope instances, you need to either resolve " +
                "instances directly from the Scope using a Scope.GetInstance overload, or you will " +
                "have to set the Container.Options.DefaultScopedLifestyle property with the required " +
                "scoped lifestyle for your type of application.");

        private static Scope ThrowThereIsNoActiveScopeException() =>
            throw new InvalidOperationException("There is no active scope.");
    }
}