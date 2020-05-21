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

        /// <summary>
        /// Adds the <paramref name="disposable"/> to the list of items that will get disposed when the
        /// container gets disposed.
        /// </summary>
        /// <remarks>
        /// Instances that are registered for disposal, will be disposed in opposite order of registration and
        /// they are guaranteed to be disposed when <see cref="Container.Dispose()"/> is called (even when
        /// exceptions are thrown). This mimics the behavior of the C# and VB <c>using</c> statements,
        /// where the <see cref="IDisposable.Dispose"/> method is called inside the <c>finally</c> block.
        /// </remarks>
        /// <param name="disposable">The instance that should be disposed when the scope ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Thrown when the container has been disposed.</exception>
        public void RegisterForDisposal(IAsyncDisposable disposable) =>
            this.scope.RegisterForDisposal(disposable);
    }
}
#endif