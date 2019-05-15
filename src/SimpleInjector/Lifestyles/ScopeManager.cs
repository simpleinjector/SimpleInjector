// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;

    internal sealed class ScopeManager
    {
        private readonly Container container;
        private readonly Func<Scope> scopeRetriever;
        private readonly Action<Scope> scopeReplacer;

        internal ScopeManager(Container container, Func<Scope> scopeRetriever, Action<Scope> scopeReplacer)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(scopeRetriever, nameof(scopeRetriever));
            Requires.IsNotNull(scopeReplacer, nameof(scopeReplacer));

            this.container = container;
            this.scopeRetriever = scopeRetriever;
            this.scopeReplacer = scopeReplacer;
        }

        internal Scope CurrentScope => this.GetCurrentScopeWithAutoCleanup();

        private Scope CurrentScopeInternal
        {
            get { return this.scopeRetriever(); }
            set { this.scopeReplacer(value); }
        }

        internal Scope BeginScope() =>
            this.CurrentScopeInternal = new Scope(this.container, this, this.GetCurrentScopeWithAutoCleanup());

        internal void RemoveScope(Scope scope)
        {
            // If the scope is not the current scope or one of its ancestors, this means that either one of
            // the scope's parents have already been disposed, or the scope is disposed on a completely
            // unrelated thread. In both cases we shouldn't change the CurrentScope, since doing this,
            // since would cause an invalid scope to be registered as the current scope (this scope will
            // either be disposed or does not belong to the current execution context).
            if (this.IsScopeInLocalChain(scope))
            {
                this.CurrentScopeInternal = scope.ParentScope;
            }
        }

        // Determines whether this instance is the currently registered lifetime scope or an ancestor of it.
        private bool IsScopeInLocalChain(Scope scope)
        {
            Scope localScope = this.CurrentScopeInternal;

            while (localScope != null)
            {
                if (object.ReferenceEquals(scope, localScope))
                {
                    return true;
                }

                localScope = localScope.ParentScope;
            }

            return false;
        }

        private Scope GetCurrentScopeWithAutoCleanup()
        {
            Scope scope = this.CurrentScopeInternal;

            // When the current scope is disposed, make the parent scope the current.
            while (scope?.Disposed == true)
            {
                this.CurrentScopeInternal = scope = scope.ParentScope;
            }

            return scope;
        }
    }
}