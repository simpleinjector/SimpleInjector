﻿#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2015 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using SimpleInjector.Advanced;
    using SimpleInjector.Internals;
    using SimpleInjector.Lifestyles;

    /// <summary>Implements a cache for <see cref="ScopedLifestyle"/> implementations.</summary>
    /// <remarks>
    /// <see cref="Scope"/> is thread-safe can be used over multiple threads concurrently, but note that the
    /// cached instances might not be thread-safe.
    /// </remarks>
    public class Scope : IDisposable
    {
        private const int MaxRecursion = 100;

        private readonly object syncRoot = new object();

        private Dictionary<Registration, object> cachedInstances;
        private List<Action> scopeEndActions;
        private List<IDisposable> disposables;
        private DisposeState state = DisposeState.Alive;
        private int recursionDuringDisposalCounter;

        private enum DisposeState
        {
            Alive,
            Disposing,
            Disposed
        }

        /// <summary>
        /// Allows registering an <paramref name="action"/> delegate that will be called when the scope ends,
        /// but before the scope disposes any instances.
        /// </summary>
        /// <remarks>
        /// During the call to <see cref="Scope.Dispose()"/> all registered <see cref="Action"/> delegates are
        /// processed in the order of registration. Do note that registered actions <b>are not guaranteed
        /// to run</b>. In case an exception is thrown during the call to <see cref="Dispose()"/>, the 
        /// <see cref="Scope"/> will stop running any actions that might not have been invoked at that point. 
        /// Instances that are registered for disposal using <see cref="RegisterForDisposal"/> on the other
        /// hand, are guaranteed to be disposed. Note that registered actions won't be invoked during a call
        /// to <see cref="Container.Verify()" />.
        /// </remarks>
        /// <param name="action">The delegate to run when the scope ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the scope has been disposed.</exception>
        public virtual void WhenScopeEnds(Action action)
        {
            Requires.IsNotNull(action, nameof(action));

            lock (this.syncRoot)
            {
                this.RequiresInstanceNotDisposed();
                this.PreventCyclicDependenciesDuringDisposal();

                if (this.scopeEndActions == null)
                {
                    this.scopeEndActions = new List<Action>();
                }

                this.scopeEndActions.Add(action);
            }
        }

        /// <summary>
        /// Adds the <paramref name="disposable"/> to the list of items that will get disposed when the
        /// scope ends.
        /// </summary>
        /// <remarks>
        /// Instances that are registered for disposal, will be disposed in opposite order of registration and
        /// they are guaranteed to be disposed when <see cref="Scope.Dispose()"/> is called (even when 
        /// exceptions are thrown). This mimics the behavior of the C# and VB <code>using</code> statements,
        /// where the <see cref="IDisposable.Dispose"/> method is called inside the <code>finally</code> block.
        /// </remarks>
        /// <param name="disposable">The instance that should be disposed when the scope ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the scope has been disposed.</exception>
        public void RegisterForDisposal(IDisposable disposable)
        {
            Requires.IsNotNull(disposable, nameof(disposable));

            lock (this.syncRoot)
            {
                this.RequiresInstanceNotDisposed();
                this.PreventCyclicDependenciesDuringDisposal();

                this.RegisterForDisposalInternal(disposable);
            }
        }

        /// <summary>Releases all instances that are cached by the <see cref="Scope"/> object.</summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal static TService GetInstance<TService, TImplementation>(
            ScopedRegistration<TService, TImplementation> registration, Scope scope)
            where TImplementation : class, TService
            where TService : class
        {
            if (scope == null)
            {
                return GetScopelessInstance(registration);
            }

            lock (scope.syncRoot)
            {
                return scope.GetInstance(registration);
            }
        }

        // This method is called from within the test suite.
        internal IDisposable[] GetDisposables()
        {
            if (this.disposables == null)
            {
                return Helpers.Array<IDisposable>.Empty;
            }

            return this.disposables.ToArray();
        }

        /// <summary>
        /// Releases all instances that are cached by the <see cref="Scope"/> object.
        /// </summary>
        /// <param name="disposing">False when only unmanaged resources should be released.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // We completely block the Dispose method from running in parallel, because there's all kinds
                // of state that needs to be read/written, such as this.state, this.disposables, and 
                // this.scopeEndActions. Making this thread-safe with smaller granular locks will be much 
                // harder and simply not necessarily, since Dispose should normally only be called from one thread.
                lock (this.syncRoot)
                {
                    if (this.state != DisposeState.Alive)
                    {
                        // Either this instance is already disposed, or a different thread is currently
                        // disposing it. We can break out immediately.
                        return;
                    }

                    this.state = DisposeState.Disposing;

                    try
                    {
                        this.DisposeRecursively();
                    }
                    finally
                    {
                        this.state = DisposeState.Disposed;

                        // Remove all references, so we won't hold on to created instances even if the
                        // scope accidentally keeps referenced. This prevents leaking memory.
                        this.cachedInstances = null;
                        this.scopeEndActions = null;
                        this.disposables = null;
                    }
                }
            }
        }

        private void DisposeRecursively(bool operatingInException = false)
        {
            if (this.disposables == null && (operatingInException || this.scopeEndActions == null))
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

                this.DisposeAllRegisteredDisposables();
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
                if (this.recursionDuringDisposalCounter <= MaxRecursion)
                {
                    this.DisposeRecursively(operatingInException);
                }
            }
        }

        private void ExecuteAllRegisteredEndScopeActions()
        {
            if (this.scopeEndActions != null)
            {
                var actions = this.scopeEndActions;

                this.scopeEndActions = null;

                actions.ForEach(action => action.Invoke());
            }
        }

        private static TService GetScopelessInstance<TService, TImplementation>(
            ScopedRegistration<TService, TImplementation> registration)
            where TImplementation : class, TService
            where TService : class
        {
            if (registration.Container.IsVerifying())
            {
                return registration.Container.VerificationScope
                    .GetInstance<TService, TImplementation>(registration);
            }

            throw new ActivationException(
                StringResources.TheServiceIsRequestedOutsideTheContextOfAScopedLifestyle(
                    typeof(TService),
                    registration.Lifestyle));
        }

        private TService GetInstance<TService, TImplementation>(
            ScopedRegistration<TService, TImplementation> registration)
            where TService : class
            where TImplementation : class, TService
        {
            this.RequiresInstanceNotDisposed();

            bool cacheIsEmpty = this.cachedInstances == null;

            if (this.cachedInstances == null)
            {
                this.cachedInstances =
                    new Dictionary<Registration, object>(ReferenceEqualityComparer<Registration>.Instance);
            }

            object instance;

            return !cacheIsEmpty && this.cachedInstances.TryGetValue(registration, out instance)
                ? (TService)instance
                : this.CreateAndCacheInstance(registration);
        }

        private TService CreateAndCacheInstance<TService, TImplementation>(
            ScopedRegistration<TService, TImplementation> registration)
            where TService : class
            where TImplementation : class, TService
        {
            TService service = registration.InstanceCreator.Invoke();

            this.cachedInstances[registration] = service;

            if (registration.RegisterForDisposal)
            {
                var disposable = service as IDisposable;

                if (disposable != null)
                {
                    this.RegisterForDisposalInternal(disposable);
                }
            }

            return service;
        }

#if NET45
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private void RegisterForDisposalInternal(IDisposable disposable)
        {
            if (this.disposables == null)
            {
                this.disposables = new List<IDisposable>(capacity: 8);
            }

            this.disposables.Add(disposable);
        }

        private void DisposeAllRegisteredDisposables()
        {
            if (this.disposables != null)
            {
                var instances = this.disposables;

                this.disposables = null;

                DisposeInstancesInReverseOrder(instances);
            }
        }

#if NET45
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private void RequiresInstanceNotDisposed()
        {
            if (this.state == DisposeState.Disposed)
            {
                this.ThrowObjectDisposedException();
            }
        }

        private void ThrowObjectDisposedException()
        {
            throw new ObjectDisposedException(this.GetType().FullName);
        }

#if NET45
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private void PreventCyclicDependenciesDuringDisposal()
        {
            if (this.recursionDuringDisposalCounter > MaxRecursion)
            {
                ThrowRecursionException();
            }
        }

        private static void ThrowRecursionException()
        {
            throw new InvalidOperationException(StringResources.RecursiveInstanceRegistrationDetected());
        }

        // This method simulates the behavior of a set of nested 'using' statements: It ensures that dispose
        // is called on each element, even if a previous instance threw an exception. 
        internal static void DisposeInstancesInReverseOrder(List<IDisposable> disposables,
            int startingAsIndex = int.MinValue)
        {
            if (startingAsIndex == int.MinValue)
            {
                startingAsIndex = disposables.Count - 1;
            }

            try
            {
                while (startingAsIndex >= 0)
                {
                    disposables[startingAsIndex].Dispose();

                    startingAsIndex--;
                }
            }
            finally
            {
                if (startingAsIndex >= 0)
                {
                    DisposeInstancesInReverseOrder(disposables, startingAsIndex - 1);
                }
            }
        }
    }
}