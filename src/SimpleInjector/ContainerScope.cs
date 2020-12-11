// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using SimpleInjector.Advanced;

    /// <summary>
    /// The scope that manages the lifetime of singletons and other container-controlled instances.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification =
            "This type has an internal Dispose method, which is called by Container.Dispose. " +
            "Users should not be able to dispose this class directly bu do so by calling Container.Dispose.")]
    public partial class ContainerScope : ApiObject
    {
        private readonly Scope scope;

        internal ContainerScope(Container container) =>
            this.scope = new Scope(container) { IsContainerScope = true };

        /// <summary>
        /// Allows registering an <paramref name="action"/> delegate that will be called when the container
        /// gets disposed, but before the container disposes any instances.
        /// </summary>
        /// <remarks>
        /// During the call to <see cref="Container.Dispose()"/> all registered <see cref="Action"/> delegates are
        /// processed in the order of registration. Do note that registered actions <b>are not guaranteed
        /// to run</b>. In case an exception is thrown during the call to
        /// <see cref="Container.Dispose()">Dispose</see>, the
        /// <see cref="ContainerScope"/> will stop running any actions that might not have been invoked at that point.
        /// Instances that are registered for disposal using <see cref="RegisterForDisposal(IDisposable)"/>
        /// on the other hand, are guaranteed to be disposed. Note that registered actions won't be invoked
        /// during a call to <see cref="Container.Verify()" />.
        /// </remarks>
        /// <param name="action">The delegate to run when the scope ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Thrown when the container has been disposed.</exception>
        public virtual void WhenScopeEnds(Action action) => this.scope.WhenScopeEnds(action);

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
        public void RegisterForDisposal(IDisposable disposable) => this.scope.RegisterForDisposal(disposable);

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
        /// <exception cref="ArgumentException">Thrown when one of the arguments is not a disposable type.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Thrown when the container has been disposed.</exception>
        public void RegisterForDisposal(object disposable) => this.scope.RegisterForDisposal(disposable);

        /// <summary>
        /// Retrieves an item from the scope stored by the given <paramref name="key"/> or null when no
        /// item is stored by that key.
        /// </summary>
        /// <remarks>
        /// <b>Thread-safety:</b> Calls to this method are thread-safe, but users should take proper
        /// percussions when they call both <b>GetItem</b> and <see cref="SetItem"/>.
        /// </remarks>
        /// <param name="key">The key of the item to retrieve.</param>
        /// <returns>The stored item or null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference.</exception>
        public object? GetItem(object key) => this.scope.GetItem(key);

        /// <summary>Stores an item by the given <paramref name="key"/> in the scope.</summary>
        /// <remarks>
        /// <b>Thread-safety:</b> Calls to this method are thread-safe, but users should take proper
        /// percussions when they call both <see cref="GetItem"/> and <b>SetItem</b>. Instead,
        /// <see cref="GetOrSetItem"/> provides an atomic operation for getting and setting an item.
        /// </remarks>
        /// <param name="key">The key of the item to insert or override.</param>
        /// <param name="item">The actual item. May be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when paramref name="key"/> is a null reference.
        /// </exception>
        public void SetItem(object key, object item) => this.scope.SetItem(key, item);

        /// <summary>
        /// Adds an item by the given <paramref name="key"/> in the container by using the specified function,
        /// if the key does not already exist. This operation is atomic.
        /// </summary>
        /// <typeparam name="T">The Type of the item to create.</typeparam>
        /// <param name="key">The key of the item to insert or override.</param>
        /// <param name="valueFactory">The function used to generate a value for the given key. The supplied
        /// value of <paramref name="key"/> will be supplied to the function when called.</param>
        /// <returns>The stored item or the item from the <paramref name="valueFactory"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either of the arguments is a null reference.
        /// </exception>
        public T GetOrSetItem<T>(object key, Func<Container, object, T> valueFactory)
        {
            Requires.IsNotNull(key, nameof(key));
            Requires.IsNotNull(valueFactory, nameof(valueFactory));

            return this.scope.GetOrSetItem(key, valueFactory);
        }

        /// <summary>
        /// Returns the list of <see cref="IDisposable"/> instances that will be disposed of when this
        /// instance is being disposed. The list contains scoped instances that are cached in this instance,
        /// and instances explicitly registered for disposal using
        /// <see cref="RegisterForDisposal(IDisposable)"/>. The instances are returned in order of creation.
        /// When <see cref="Container.Dispose()">Container.Dispose</see> is called, the scope will ensure
        /// <see cref="IDisposable.Dispose"/> is called on each instance in this list. The instance will be
        /// disposed in opposite order as they appear in the list.
        /// </summary>
        /// <returns>The list of <see cref="IDisposable"/> instances that will be disposed of when thi
        /// <see cref="Scope"/>
        /// instance is being disposed.</returns>
        public IDisposable[] GetDisposables() => this.scope.GetDisposables();

        /// <summary>
        /// Returns a copy of the list of IDisposable and IAsyncDisposable instances that will be disposed of
        /// when this <see cref="Scope"/> instance is being disposed. The list contains scoped instances that
        /// are cached in this <see cref="Container"/> instance, and instances explicitly registered for
        /// disposal using <see cref="RegisterForDisposal(object)"/>. The instances are returned in order of
        /// creation.
        /// </summary>
        /// <remarks>
        /// <b>Thread safety:</b> This method is <b>not</b> thread safe and should not be used in combination
        /// with any of the thread-safe methods.
        /// </remarks>
        /// <returns>The list of <see cref="IDisposable"/> instances that will be disposed of when this
        /// <see cref="Scope"/> instance is being disposed.</returns>
        public object[] GetAllDisposables() => this.scope.GetAllDisposables();

        // This method is internal, because we don't want to expose Dispose through its public API. That would
        // allow the container scope to be disposed, while the container isn't. Disposing should happen using the
        // Container API.
        internal void Dispose() => this.scope.Dispose();
    }
}