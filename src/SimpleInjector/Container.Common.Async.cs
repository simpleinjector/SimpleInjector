// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

#if NET461 || NETSTANDARD2_0 || NETSTANDARD2_1
namespace SimpleInjector
{
    using System;
    using System.Threading.Tasks;

    public partial class Container : IAsyncDisposable
    {
        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (!this.disposed)
            {
                this.stackTraceThatDisposedTheContainer = GetStackTraceOrNull();

                try
                {
                    await this.ContainerScope.DisposeAsync();
                }
                finally
                {
                    this.disposed = true;
                    this.isVerifying.Dispose();
                }
            }
        }
    }
}
#endif