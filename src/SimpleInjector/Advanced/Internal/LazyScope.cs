// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced.Internal
{
    using System;

    /// <summary>
    /// This is an internal type. Only depend on this type when you want to be absolutely sure a future
    /// version of the framework will break your code.
    /// </summary>
    /// <remarks>Initializes a new instance of the <see cref="LazyScope"/> struct.</remarks>
    /// <param name="scopeFactory">The scope factory.</param>
    /// <param name="container">The container.</param>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public struct LazyScope(Func<Scope> scopeFactory, Container container)
    {
        private Func<Scope>? scopeFactory = scopeFactory;
        private Scope? value = null;

        /// <summary>Gets the lazily initialized Scope of the current LazyScope instance.</summary>
        /// <value>The current Scope or null.</value>
        public Scope Value
        {
            get
            {
                if (this.scopeFactory is not null)
                {
                    this.value = container.GetVerificationOrResolveScopeForCurrentThread()
                        ?? this.scopeFactory.Invoke();
                    this.scopeFactory = null;
                }

                return this.value!;
            }
        }
    }
}