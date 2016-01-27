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

namespace SimpleInjector.Extensions.LifetimeScoping
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading;

    /// <summary>
    /// Thread and container specific cache for services that are registered with one of the 
    /// <see cref="SimpleInjectorLifetimeScopeExtensions">RegisterLifetimeScope</see> extension method overloads.
    /// </summary>
    internal sealed class LifetimeScope : Scope
    {
        private readonly LifetimeScopeManager manager;
#if !DNXCORE50
        private readonly int initialThreadId = Thread.CurrentThread.ManagedThreadId;
#endif

        internal LifetimeScope(LifetimeScopeManager manager, LifetimeScope parentScope)
        {
            this.manager = manager;
            this.ParentScope = parentScope;
        }

        internal LifetimeScope ParentScope { get; }

        internal bool Disposed { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged 
        /// resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release 
        /// only unmanaged resources.</param>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations",
            Justification = "This is the only reliable place where we can see that the scope has been " +
                            "used over multiple threads is here.")]
        protected override void Dispose(bool disposing)
        {
            this.VerifyThatScopeIsDisposedOnOriginatingThread();

            try
            {
                base.Dispose(disposing);
            }
            finally
            {
                this.Disposed = true;
                this.manager.RemoveLifetimeScope(this);
            }
        }

        private void VerifyThatScopeIsDisposedOnOriginatingThread()
        {
#if !DNXCORE50
            if (!this.Disposed)
            {
                // EndLifetimeScope should not be called from a different thread than where it was started.
                // Calling this method from another thread could remove the wrong scope.
                if (this.initialThreadId != Thread.CurrentThread.ManagedThreadId)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                        "It is not safe to use a LifetimeScope instance across threads. The LifetimeScope " +
                        "instance was disposed on thread id {0} but was created on thread id {1}. " +
                        "Make sure the complete operation that the lifetime scope surrounds gets executed " +
                        "within the same thread and make sure that the LifetimeScope instance gets " +
                        "disposed on the same thread as it gets created. In case your intention is to " +
                        "create a scope that flows with the logical flow of control of asynchronous " +
                        "methods, please switch to the ExecutionContextScopeLifestyle. Please goto " +
                        "https://simpleinjector.org/await" + " for more information on this lifestyle.",
                        Thread.CurrentThread.ManagedThreadId, this.initialThreadId));
                }
            }
#endif
        }
    }
}