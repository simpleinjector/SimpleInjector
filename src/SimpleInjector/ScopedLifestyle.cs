#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2016 Simple Injector Contributors
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
    using System.Diagnostics;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Base class for scoped lifestyles. A scoped lifestyle caches instances for the duration of an implicitly
    /// or explicitly defined scope. Such scope can be an (implicitly defined) web request or an explicitly
    /// defined Lifetime Scope. The lifetime of instances registered with a scoped lifestyle is always equal
    /// or bigger than one-instance-per-object-graph. In other words, a call to GetInstance() will never create
    /// more than one instance of such registered type.
    /// </summary>
    public abstract class ScopedLifestyle : Lifestyle
    {
        /// <summary>Initializes a new instance of the <see cref="ScopedLifestyle"/> class.</summary>
        /// <param name="name">The user friendly name of this lifestyle.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null (Nothing in VB) 
        /// or an empty string.</exception>
        protected ScopedLifestyle(string name) : base(name)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ScopedLifestyle"/> class.</summary>
        /// <param name="name">The user friendly name of this lifestyle.</param>
        /// <param name="disposeInstances">Signals the lifestyle whether instances should be
        /// disposed or not.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null (Nothing in VB) 
        /// or an empty string.</exception>
        [Obsolete(
            "This constructor overload is deprecated. The disposal of instances can't be suppressed anymore", 
            error: true)]
        protected ScopedLifestyle(string name, bool disposeInstances) : base(name)
        {
        }

        /// <summary>Gets the length of the lifestyle.</summary>
        /// <value>The <see cref="int"/> representing the length of this lifestyle.</value>
        public override int Length => 500;

        /// <summary>
        /// Allows registering an <paramref name="action"/> delegate that will be called when the scope ends,
        /// but before the scope disposes any instances.
        /// </summary>
        /// <remarks>
        /// During the call to <see cref="Scope.Dispose()"/> all registered <see cref="Action"/> delegates are
        /// processed in the order of registration. Do note that registered actions <b>are not guaranteed
        /// to run</b>. In case an exception is thrown during the call to <see cref="Scope.Dispose()"/>, the 
        /// <see cref="Scope"/> will stop running any actions that might not have been invoked at that point. 
        /// Instances that are registered for disposal using <see cref="RegisterForDisposal"/> on the other
        /// hand, are guaranteed to be disposed. Note that registered actions won't be invoked during a call
        /// to <see cref="Container.Verify()" />.
        /// </remarks>
        /// <param name="container">The <see cref="Container"/> instance.</param>
        /// <param name="action">The delegate to run when the scope ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="InvalidOperationException">Will be thrown when there is currently no active
        /// scope for the supplied <paramref name="container"/>.</exception>
        public void WhenScopeEnds(Container container, Action action)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(action, nameof(action));

            this.GetCurrentScopeOrThrow(container).WhenScopeEnds(action);
        }

        /// <summary>
        /// Adds the <paramref name="disposable"/> to the list of items that will get disposed when the
        /// scope ends.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> instance.</param>
        /// <param name="disposable">The instance that should be disposed when the scope ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="InvalidOperationException">Will be thrown when there is currently no active
        /// scope for the supplied <paramref name="container"/>.</exception>
        public void RegisterForDisposal(Container container, IDisposable disposable)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(disposable, nameof(disposable));

            this.GetCurrentScopeOrThrow(container).RegisterForDisposal(disposable);
        }

        /// <summary>
        /// Returns the current <see cref="Scope"/> for this lifestyle and the given 
        /// <paramref name="container"/>, or null when this method is executed outside the context of a scope.
        /// </summary>
        /// <param name="container">The container instance that is related to the scope to return.</param>
        /// <returns>A <see cref="Scope"/> instance or null when there is no scope active in this context.</returns>
        public Scope GetCurrentScope(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            return this.GetCurrentScopeInternal(container);
        }

        /// <summary>
        /// Creates a delegate that upon invocation return the current <see cref="Scope"/> for this
        /// lifestyle and the given <paramref name="container"/>, or null when the delegate is executed outside
        /// the context of such scope.
        /// </summary>
        /// <param name="container">The container for which the delegate gets created.</param>
        /// <returns>A <see cref="Func{T}"/> delegate. This method should never return null.</returns>
        protected internal abstract Func<Scope> CreateCurrentScopeProvider(Container container);

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of the
        /// specified <typeparamref name="TService"/> using the supplied <paramref name="instanceCreator"/> 
        /// with the caching as specified by this lifestyle.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <param name="instanceCreator">A delegate that will create a new instance of 
        /// <typeparamref name="TService"/> every time it is called.</param>
        /// <param name="container">The <see cref="Container"/> instance for which a 
        /// <see cref="Registration"/> must be created.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        protected internal override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container)
        {
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));
            Requires.IsNotNull(container, nameof(container));

            return new ScopedRegistration<TService>(this, container, instanceCreator);
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of the
        /// specified <typeparamref name="TConcrete"/> with the caching as specified by this lifestyle.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <param name="container">The <see cref="Container"/> instance for which a 
        /// <see cref="Registration"/> must be created.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        protected internal override Registration CreateRegistrationCore<TConcrete>(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            return new ScopedRegistration<TConcrete>(this, container);
        }

        /// <summary>
        /// Returns the current <see cref="Scope"/> for this lifestyle and the given 
        /// <paramref name="container"/>, or null when this method is executed outside the context of a scope.
        /// </summary>
        /// <remarks>
        /// By default, this method calls the <see cref="CreateCurrentScopeProvider"/> method and invokes the
        /// returned delegate. This method can be overridden to provide an optimized way for getting the
        /// current scope.
        /// </remarks>
        /// <param name="container">The container instance that is related to the scope to return.</param>
        /// <returns>A <see cref="Scope"/> instance or null when there is no scope active in this context.</returns>
        protected virtual Scope GetCurrentScopeCore(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            Func<Scope> currentScopeProvider = this.CreateCurrentScopeProvider(container);

            return currentScopeProvider.Invoke();
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private Scope GetCurrentScopeOrThrow(Container container)
        {
            Scope scope = this.GetCurrentScopeInternal(container);

            if (scope == null)
            {
                this.ThrowThisMethodCanOnlyBeCalledWithinTheContextOfAnActiveScope();
            }

            return scope;
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private Scope GetCurrentScopeInternal(Container container)
        {
            // If we are running verification in the current thread, we prefer returning a verification scope
            // over a real active scope (issue #95).
            return container.GetVerificationScopeForCurrentThread()
                ?? this.GetCurrentScopeCore(container);
        }

        private void ThrowThisMethodCanOnlyBeCalledWithinTheContextOfAnActiveScope()
        {
            throw new InvalidOperationException(
                StringResources.ThisMethodCanOnlyBeCalledWithinTheContextOfAnActiveScope(this));
        }
    }
}