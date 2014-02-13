#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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
    using System.Linq;
    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Implements a cache <see cref="ScopedLifestyle"/> implemenations.
    /// </summary>
    /// <remarks>
    /// A scope is not thread-safe and should not be used over multiple threads concurrently.
    /// </remarks>
    public class Scope : IDisposable
    {
        private readonly Dictionary<Registration, object> cachedInstances =
            new Dictionary<Registration, object>(ReferenceEqualityComparer<Registration>.Instance);

        private List<Action> scopeEndActions;
        private List<IDisposable> disposables;
        private bool disposed;

        /// <summary>
        /// Allows registering an <paramref name="action"/> delegate that will be called when the scope ends,
        /// but before the scope disposes any instances.
        /// </summary>
        /// <param name="action">The delegate to run when the scope ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        public void WhenScopeEnds(Action action)
        {
            Requires.IsNotNull(action, "action");
            Requires.InstanceNotDisposed(this.disposed, "Scope");
            
            if (this.scopeEndActions == null)
            {
                this.scopeEndActions = new List<Action>();
            }

            this.scopeEndActions.Add(action);
        }

        /// <summary>
        /// Adds the <paramref name="disposable"/> to the list of items that will get disposed when the
        /// scope ends.
        /// </summary>
        /// <remarks>
        /// Note to implementers: Instances registered for disposal will have to be disposed in the opposite
        /// order of registration, since disposable components might still need to call disposable dependencies
        /// in their Dispose() method.
        /// </remarks>
        /// <param name="disposable">The instance that should be disposed when the scope ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        public void RegisterForDisposal(IDisposable disposable)
        {
            Requires.IsNotNull(disposable, "disposable");
            Requires.InstanceNotDisposed(this.disposed, "Scope");

            if (this.disposables == null)
            {
                this.disposables = new List<IDisposable>();
            }

            this.disposables.Add(disposable);
        }

        /// <summary>Releases all instances that are cached by the <see cref="Scope"/> object.</summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal IDisposable[] GetDisposables()
        {
            return (this.disposables ?? Enumerable.Empty<IDisposable>()).ToArray();
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

            object instance;

            if (!scope.cachedInstances.TryGetValue(registration, out instance))
            {
                scope.cachedInstances[registration] = instance = registration.InstanceCreator();

                if (registration.RegisterForDisposal)
                {
                    var disposable = instance as IDisposable;

                    if (disposable != null)
                    {
                        scope.RegisterForDisposal(disposable);
                    }
                }
            }

            return (TService)instance;
        }

        /// <summary>
        /// Releases all instances that are cached by the <see cref="Scope"/> object.
        /// </summary>
        /// <param name="disposing">False when only unmanaged resources should be released.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    this.ExecuteAllRegisteredEndScopeActions();
                }
                finally
                {
                    this.disposed = true;
                    this.DisposeAllRegisteredDisposables();
                }
            }
        }

        private void ExecuteAllRegisteredEndScopeActions()
        {
            if (this.scopeEndActions != null)
            {
                try
                {
                    int index = 0;

                    // We can't use a foreach here, since a registered action could cause the registration of
                    // a new actionn.
                    while (index < this.scopeEndActions.Count)
                    {
                        this.scopeEndActions[index].Invoke();
                        index++;
                    }
                }
                finally
                {
                    this.scopeEndActions = null;
                }
            }
        }

        private void DisposeAllRegisteredDisposables()
        {
            if (this.disposables != null)
            {
                try
                {
                    int lastIndex = this.disposables.Count - 1;
                    DisposeInstancesInReverseOrder(this.disposables, startingAsIndex: lastIndex);
                }
                finally
                {
                    this.disposables = null;
                }
            }
        }

        // This method simulates the behavior of a set of nested 'using' statements: It ensures that dispose
        // is called on each element, even if a previous instance threw an exception. 
        internal static void DisposeInstancesInReverseOrder(List<IDisposable> disposables, int startingAsIndex)
        {
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

        private static TService GetScopelessInstance<TService, TImplementation>(
            ScopedRegistration<TService, TImplementation> registration)
            where TImplementation : class, TService
            where TService : class
        {
            if (registration.Container.IsVerifying())
            {
                // Return a transient instance when this method is called during verification
                return registration.InstanceCreator.Invoke();
            }

            throw new ActivationException(
                StringResources.TheServiceIsRequestedOutsideTheContextOfAScopedLifestyle(
                    typeof(TService),
                    registration.Lifestyle));
        }
    }
}