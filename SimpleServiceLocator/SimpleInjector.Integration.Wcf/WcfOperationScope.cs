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

namespace SimpleInjector.Integration.Wcf
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;

    /// <summary>
    /// Thread and container specific cache for services that are registered with one of the 
    /// <see cref="SimpleInjectorWcfExtensions">RegisterPerWcfOperation</see> extension method overloads.
    /// This class is created implicitly and a current instance can be requested by calling
    /// <see cref="SimpleInjectorWcfExtensions.GetCurrentWcfOperationScope">GetCurrentWcfOperationScope</see>.
    /// </summary>
    public sealed class WcfOperationScope : IDisposable
    {
        private readonly Dictionary<Type, object> lifetimeScopedInstances = new Dictionary<Type, object>();
        private readonly int initialThreadId;
        private WcfOperationScopeManager manager;
        private List<IDisposable> disposables;

        internal WcfOperationScope(WcfOperationScopeManager manager)
        {
            this.manager = manager;
            this.initialThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Registers the supplied <paramref name="disposable"/> to be disposed when the WCF request ends.
        /// Calling this method is useful for instances that are registered with a lifecycle shorter than
        /// that of the scope (where possibly multiple instances are created per scope, such as transient
        /// services, that are registered with one of the <b>Register</b> overloads), but still need to be
        /// disposed explicitly.
        /// </summary>
        /// <example>
        /// The following example registers a <b>ServiceImpl</b> type as transient (a new instance will be
        /// returned every time) and registers an initializer for that type that will register that instance
        /// for disposal in the <see cref="WcfOperationScope"/> in which context it is created:
        /// <code lang="cs"><![CDATA[
        /// container.Register<IService, ServiceImpl>();
        /// container.RegisterInitializer<ServiceImpl>(instance =>
        /// {
        ///     var scope = container.GetCurrentLifetimeScope();
        ///     if (scope != null) scope.RegisterForDisposal(instance);
        /// });
        /// ]]></code>
        /// </example>
        /// <param name="disposable">The disposable.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="disposable"/> is a null reference.</exception>        
        public void RegisterForDisposal(IDisposable disposable)
        {
            if (disposable == null)
            {
                throw new ArgumentNullException("disposable");
            }

            if (this.disposables == null)
            {
                this.disposables = new List<IDisposable>();
            }

            this.disposables.Add(disposable);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged 
        /// resources.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <b>Dispose</b> was called on a different
        /// thread than where this instance was constructed.</exception>
        public void Dispose()
        {
            if (this.manager != null)
            {
                if (this.initialThreadId != Thread.CurrentThread.ManagedThreadId)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                        "It is not safe to use a LifetimeScope instance across threads. Make sure the " +
                        "complete operation that the lifetime scope surrounds gets executed within the " +
                        "same thread and make sure that the LifetimeScope instance gets disposed on the " +
                        "same thread as it gets created. Dispose was called on thread with ManagedThreadId " +
                        "{0}, but was created on thread with id {1}.", Thread.CurrentThread.ManagedThreadId,
                        this.initialThreadId));
                }

                this.manager.EndLifetimeScope(this);

                this.manager = null;

                if (this.disposables != null)
                {
                    foreach (var disposable in this.disposables)
                    {
                        disposable.Dispose();
                    }
                }

                this.disposables = null;
            }
        }

        internal TService GetInstance<TService>(Func<TService> instanceCreator,
            bool disposeWhenLifetimeScopeEnds)
            where TService : class
        {
            object instance;

            if (!this.lifetimeScopedInstances.TryGetValue(typeof(TService), out instance))
            {
                this.lifetimeScopedInstances[typeof(TService)] = instance = instanceCreator();

                if (disposeWhenLifetimeScopeEnds)
                {
                    var disposable = instance as IDisposable;

                    if (disposable != null)
                    {
                        this.RegisterForDisposal(disposable);
                    }
                }
            }

            return (TService)instance;
        }
    }
}