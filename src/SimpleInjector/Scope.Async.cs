// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

#if NET461 || NETSTANDARD2_0 || NETSTANDARD2_1
namespace SimpleInjector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using SimpleInjector.Advanced;
    using SimpleInjector.Internals;
    using SimpleInjector.Lifestyles;

    public partial class Scope : IAsyncDisposable
    {
        private List<IAsyncDisposable>? asyncDisposables;

        /// <inheritdoc />
        public virtual async ValueTask DisposeAsync()
        {
            if (this.asyncDisposables != null)
            {
                var copy = this.asyncDisposables.ToList();
                copy.Reverse();
                this.asyncDisposables = null;

                foreach (var disposable in copy)
                {
                    await disposable.DisposeAsync();
                }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        partial void TryRegisterForAsyncDisposalInternal(object instance)
        {
            if (instance is IAsyncDisposable disposable)
            {
                if (this.asyncDisposables is null)
                {
                    this.asyncDisposables = new List<IAsyncDisposable>(capacity: 4);
                }

                this.asyncDisposables.Add(disposable);
            }
        }
    }
}
#endif