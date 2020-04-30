// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

#if NET461 || NETSTANDARD2_0 || NETSTANDARD2_1
namespace SimpleInjector
{
    using System;
    using System.Threading.Tasks;
    using SimpleInjector.Advanced;

    public partial class ContainerScope : IAsyncDisposable
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public ValueTask DisposeAsync() => this.scope.DisposeAsync();
    }
}
#endif