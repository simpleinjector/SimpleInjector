// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Threading.Tasks;

namespace SimpleInjector
{
#if NETSTANDARD2_1
    using System;
    using System.Threading.Tasks;

    public partial class Container : IAsyncDisposable
    {
        /// <summary>
        /// Releases all instances that are cached by the <see cref="Container"/> object asynchronously.
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public async Task DisposeContainerAsync() => await this.DisposeAsync().ConfigureAwait(false);

        /// <summary>
        /// Releases all instances that are cached by the <see cref="Container"/> object asynchronously.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (!this.disposed)
            {
                this.stackTraceThatDisposedTheContainer = GetStackTraceOrNull();

                try
                {
                    await this.ContainerScope.DisposeAsync().ConfigureAwait(false);
                }
                finally
                {
                    this.disposed = true;
                    this.isVerifying.Dispose();
                }
            }
        }
    }
#else
    public partial class Container
    {
        /// <summary>
        /// Releases all instances that are cached by the <see cref="Container"/> object asynchronously.
        /// </summary>
        public async Task DisposeContainerAsync()
        {
            if (!this.disposed)
            {
                this.stackTraceThatDisposedTheContainer = GetStackTraceOrNull();

                try
                {
                    await this.ContainerScope.DisposeScopeAsync().ConfigureAwait(false);
                }
                finally
                {
                    this.disposed = true;
                    this.isVerifying.Dispose();
                }
            }
        }
    }
#endif
}