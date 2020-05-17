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
        /// <summary>
        /// Adds the <paramref name="disposable"/> to the list of items that will get disposed when the
        /// scope ends.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Instances that are registered for disposal, will be disposed in opposite order of registration and
        /// they are guaranteed to be disposed when <see cref="Scope.Dispose()"/> is called (even when
        /// exceptions are thrown). This mimics the behavior of the C# and VB <code>using</code> statements,
        /// where the <see cref="IDisposable.Dispose"/> method is called inside the <code>finally</code> block.
        /// </para>
        /// <para>
        /// <b>Thread safety:</b> Calls to this method are thread safe.
        /// </para>
        /// </remarks>
        /// <param name="disposable">The instance that should be disposed when the scope ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the scope has been disposed.</exception>
        public void RegisterForDisposal(IAsyncDisposable disposable)
        {
            Requires.IsNotNull(disposable, nameof(disposable));

            lock (this.syncRoot)
            {
                this.RequiresInstanceNotDisposed();
                this.PreventCyclicDependenciesDuringDisposal();

                this.RegisterForDisposalInternal(disposable);
            }
        }

        /// <inheritdoc />
        public virtual async ValueTask DisposeAsync()
        {
            if (this.state == DisposeState.Alive)
            {
                this.state = DisposeState.Disposing;

                try
                {
                    await this.DisposeRecursivelyAsync();
                }
                finally
                {
                    this.state = DisposeState.Disposed;

                    this.manager?.RemoveScope(this);

                    // Remove all references, so we won't hold on to created instances even if the scope
                    // accidentally keeps referenced. This prevents leaking memory.
                    this.ClearState();
                }
            }
        }

        private async ValueTask DisposeRecursivelyAsync(bool operatingInException = false)
        {
            if (this.disposables is null && (operatingInException || this.scopeEndActions is null))
            {
                return;
            }

            this.recursionDuringDisposalCounter++;

            try
            {
                if (!operatingInException)
                {
                    while (this.scopeEndActions != null)
                    {
                        this.ExecuteAllRegisteredEndScopeActions();
                        this.recursionDuringDisposalCounter++;
                    }
                }

                await this.DisposeAllRegisteredDisposablesAsync();
            }
            catch
            {
                // When an exception is thrown during disposing, we immediately stop executing all
                // registered actions, but continue disposing all cached instances. This simulates the
                // behavior of a using statement, where the actions are part of the try-block.
                bool firstException = !operatingInException;

                if (firstException)
                {
                    // We must reset the counter here, because even if a recursion was detected in one of the
                    // actions, we still want to try disposing all instances.
                    this.recursionDuringDisposalCounter = 0;
                    operatingInException = true;
                }

                throw;
            }
            finally
            {
                // We must break out of the recursion when we reach MaxRecursion, because not doing so
                // could cause a stack overflow.
                if (this.recursionDuringDisposalCounter <= MaximumDisposeRecursion)
                {
                    await this.DisposeRecursivelyAsync(operatingInException);
                }
            }
        }

        private ValueTask DisposeAllRegisteredDisposablesAsync()
        {
            if (this.disposables != null)
            {
                var instances = this.disposables;

                this.disposables = null;

                return DisposeInstancesInReverseOrderAsync(instances);
            }
            else
            {
                return default;
            }
        }

        // This method simulates the behavior of a set of nested 'using' statements: It ensures that dispose
        // is called on each element, even if a previous instance threw an exception.
        private static async ValueTask DisposeInstancesInReverseOrderAsync(
            List<object> disposables, int startingAsIndex = int.MinValue)
        {
            if (startingAsIndex == int.MinValue)
            {
                startingAsIndex = disposables.Count - 1;
            }

            try
            {
                while (startingAsIndex >= 0)
                {
                    object instance = disposables[startingAsIndex];

                    // Always try disposing asynchronously first. When DisposeAsync() is called, Dispose()
                    // doesn't have to be called any longer.
                    if (instance is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync();
                    }
                    else
                    {
                        ((IDisposable)instance).Dispose();
                    }

                    startingAsIndex--;
                }
            }
            finally
            {
                if (startingAsIndex >= 0)
                {
                    await DisposeInstancesInReverseOrderAsync(disposables, startingAsIndex - 1);
                }
            }
        }
    }
}
#endif