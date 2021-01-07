// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;

    /// <summary>
    /// This lifestyle can be used to implement ambient context-less scoping in Simple Injector. This lifestyle
    /// can be set as DefaultScopedLifestyle and later used via Lifestyle.Scoped to register scoped instances,
    /// while instances are resolved via Scope.GetInstance.
    /// </summary>
    internal sealed class FlowingScopedLifestyle : ScopedLifestyle
    {
        public FlowingScopedLifestyle() : base("Scoped")
        {
        }

        protected internal override Func<Scope?> CreateCurrentScopeProvider(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            // Notify the container that we're using the thread-resolve scope.
            container.UseCurrentThreadResolveScope();

            return () => container.GetVerificationOrResolveScopeForCurrentThread();
        }

        protected override Scope? GetCurrentScopeCore(Container container) =>
            container.GetVerificationOrResolveScopeForCurrentThread();

        protected override void SetCurrentScopeCore(Scope scope) =>
            scope.Container!.CurrentThreadResolveScope = scope;
    }
}