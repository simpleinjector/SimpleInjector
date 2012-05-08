#region Copyright (c) 2012 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2012 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
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

namespace SimpleInjector.Extensions.LifetimeScoping
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Thread and container specific cache for services that are registered with one of the 
    /// <see cref="SimpleInjectorLifetimeScopeExtensions">RegisterLifetimeScope</see> extension method overloads.
    /// </summary>
    public sealed class LifetimeScope : IDisposable
    {
        // Design Note: LifetimeScope needs to be public, because it is returned from the BeginLifetimeScope 
        // extension method. By returning LifetimeScope instead of IDisposable, we can extend the API in a 
        // future release by adding public methods to LifetimeScope, without breaking the existing API. 
        private readonly Dictionary<Type, object> lifetimeScopedInstances = new Dictionary<Type, object>();
        private LifetimeScopeManager manager;
        private List<IDisposable> disposables;

        internal LifetimeScope(LifetimeScopeManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.manager != null)
            {
                this.manager.EndLifetimeScope(this);

                this.manager = null;

                if (this.disposables != null)
                {
                    this.disposables.ForEach(d => d.Dispose());
                }

                this.disposables = null;
            }
        }

        internal void RegisterForDisposal(IDisposable disposable)
        {
            if (this.disposables == null)
            {
                this.disposables = new List<IDisposable>();
            }

            this.disposables.Add(disposable);
        }

        internal TService GetInstance<TService>(Func<TService> instanceCreator)
            where TService : class
        {
            object instance;

            if (!this.lifetimeScopedInstances.TryGetValue(typeof(TService), out instance))
            {
                this.lifetimeScopedInstances[typeof(TService)] = instance = instanceCreator();
            }

            return (TService)instance;
        }
    }
}