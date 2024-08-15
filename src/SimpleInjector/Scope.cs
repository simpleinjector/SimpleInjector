// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleInjector.Advanced;
    using SimpleInjector.Internals;
    using SimpleInjector.Lifestyles;

#if NETSTANDARD2_1
    using DisposeAsyncTask = System.Threading.Tasks.ValueTask;

    public partial class Scope : IAsyncDisposable
    {
        private static readonly ValueTask CompletedTask = default(ValueTask);
        
        /// <summary>
        /// Releases all instances that are cached by the <see cref="Scope"/> object asynchronously.
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public async Task DisposeScopeAsync() => await this.DisposeAsync().ConfigureAwait(false);

        /// <summary>
        /// Releases all instances that are cached by the <see cref="Scope"/> object asynchronously.
        /// </summary>
        public virtual ValueTask DisposeAsync() => this.DisposeInternalAsync();
    }
#else
    using DisposeAsyncTask = System.Threading.Tasks.Task;

    public partial class Scope
    {
        private static readonly Task CompletedTask = Task.FromResult(string.Empty);

        /// <summary>
        /// Releases all instances that are cached by the <see cref="Scope"/> object asynchronously.
        /// </summary>
        public async Task DisposeScopeAsync() => await this.DisposeInternalAsync().ConfigureAwait(false);
    }
#endif

    /// <summary>Implements a cache for <see cref="ScopedLifestyle"/> implementations.</summary>
    /// <remarks>
    /// <see cref="Scope"/> is thread safe can be used over multiple threads concurrently, but methods that
    /// are related to the disposal of the Scope (e.g. <see cref="GetDisposables"/>, <see cref="Dispose()"/>,
    /// and <b>DisposeAsync()</b> are <b>not</b> thread safe and can't be used in combination with the
    /// thread-safe methods. This means that once the Scope is ready for disposal, only a single thread should
    /// access it. Also note that although the scope is thread safe, cached instances might not be.
    /// </remarks>
    [DebuggerDisplay("{DebugString,nq}")]
    public partial class Scope : ApiObject, IDisposable, IServiceProvider
    {
        private const int MaximumDisposeRecursion = 100;

        private static long counter = 0;

        private readonly object syncRoot = new();
        private readonly ScopeManager? manager;
        private readonly long scopeId = Interlocked.Increment(ref counter);

        private IDictionary? items;
        private Dictionary<Registration, object>? cachedInstances;
        private List<Action>? scopeEndActions;
        private List<object>? disposables;
        private DisposeState state;
        private int recursionDuringDisposalCounter;

        /// <summary>Initializes a new instance of the <see cref="Scope"/> class.</summary>
        [Obsolete("Use the overloaded Scope(Container) constructor instead. " +
            "Will be removed in version 6.0.",
            error: true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Scope()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Scope"/> class.</summary>
        /// <param name="container">The container instance that the scope belongs to.</param>
        public Scope(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            this.Container = container;
        }

        internal Scope(Container container, ScopeManager manager, Scope? parentScope) : this(container)
        {
            Requires.IsNotNull(manager, nameof(manager));

            this.ParentScope = parentScope;
            this.manager = manager;
        }

        private enum DisposeState
        {
            Alive,
            Disposing,
            Disposed
        }

        /// <summary>Gets the container instance that this scope belongs to.</summary>
        /// <value>The <see cref="Container"/> instance.</value>
        public Container? Container { get; }

        internal bool Disposed => this.state == DisposeState.Disposed;

        internal Scope? ParentScope { get; }

        internal bool IsContainerScope { get; set; }

        private string DebugString =>
            "Scope #" + this.scopeId
            + (this.ParentScope is not null ? (" for Parent #" + this.ParentScope.scopeId) : "")
            + (this.Container is not null ? (" for Container #" + this.Container!.ContainerId) : "");

        /// <summary>Gets an instance of the given <typeparamref name="TService"/> for the current scope.</summary>
        /// <typeparam name="TService">The type of the service to resolve.</typeparam>
        /// <remarks><b>Thread safety:</b> Calls to this method are thread safe.</remarks>
        /// <returns>An instance of the given service type.</returns>
        public TService GetInstance<TService>()
            where TService : class
        {
            return (TService)this.GetInstance(typeof(TService));
        }

        /// <summary>Gets an instance of the given <paramref name="serviceType" /> for the current scope.</summary>
        /// <remarks><b>Thread safety:</b> Calls to this method are thread safe.</remarks>
        /// <param name="serviceType">The type of the service to resolve.</param>
        /// <returns>An instance of the given service type.</returns>
        public object GetInstance(Type serviceType)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));

            if (this.Container is null)
            {
                throw new InvalidOperationException(
                    "This method can only be called on Scope instances that are related to a Container. " +
                    "Please use the overloaded constructor of Scope create an instance with a Container.");
            }

            Scope? originalScope = this.Container.CurrentThreadResolveScope;

            try
            {
                this.Container.CurrentThreadResolveScope = this;
                return this.Container.GetInstance(serviceType);
            }
            finally
            {
                this.Container.CurrentThreadResolveScope = originalScope;
            }
        }

        /// <summary>
        /// Allows registering an <paramref name="action"/> delegate that will be called when the scope ends,
        /// but before the scope disposes any instances.
        /// </summary>
        /// <remarks>
        /// <para>
        /// During the call to <see cref="Scope.Dispose()"/> all registered <see cref="Action"/> delegates are
        /// processed in the order of registration. Do note that registered actions <b>are not guaranteed
        /// to run</b>. In case an exception is thrown during the call to <see cref="Dispose()"/>, the
        /// <see cref="Scope"/> will stop running any actions that might not have been invoked at that point.
        /// Instances that are registered for disposal using <see cref="RegisterForDisposal(IDisposable)"/>
        /// on the other hand, are guaranteed to be disposed. Note that registered actions won't be invoked
        /// during a call to <see cref="Container.Verify()" />.
        /// </para>
        /// <para>
        /// <b>Thread safety:</b> Calls to this method are thread safe.
        /// </para>
        /// </remarks>
        /// <param name="action">The delegate to run when the scope ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Thrown when the scope has been disposed.</exception>
        public virtual void WhenScopeEnds(Action action)
        {
            Requires.IsNotNull(action, nameof(action));

            lock (this.syncRoot)
            {
                this.RequiresInstanceNotDisposed();
                this.PreventCyclicDependenciesDuringDisposal();

                (this.scopeEndActions ??= new List<Action>()).Add(action);
            }
        }

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
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Thrown when the scope has been disposed.</exception>
        public void RegisterForDisposal(IDisposable disposable)
        {
            Requires.IsNotNull(disposable, nameof(disposable));

            this.RegisterForDisposal((object)disposable);
        }

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
        /// <exception cref="ArgumentException">Thrown when the argument neither implements IDisposable nor
        /// IAsyncDisposable.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the scope has been disposed.</exception>
        public void RegisterForDisposal(object disposable)
        {
            Requires.IsNotNull(disposable, nameof(disposable));

            lock (this.syncRoot)
            {
                this.RequiresInstanceNotDisposed();
                this.PreventCyclicDependenciesDuringDisposal();

#if NETSTANDARD2_1
                if (disposable is IDisposable || disposable is IAsyncDisposable)
                {
                    this.RegisterForDisposalInternal(disposable);
                }
#else
                // A value of null means that it's possible the registration returns (sometimes) disposable
                // instances (due to the dynamic nature of the registration), which is why we have to fallback
                // to this (slower) type checking call.
                if (AsyncDisposableTypeCache.IsAsyncDisposable(disposable))
                {
                    this.RegisterForDisposalInternal(new AsyncDisposableWrapper(disposable));
                }
                else if (disposable is IDisposable)
                {
                    this.RegisterForDisposalInternal(disposable);
                }
#endif
                else
                {
                    throw new ArgumentException("Instance should be disposable.", nameof(disposable));
                }
            }
        }

        /// <summary>
        /// Retrieves an item from the scope stored by the given <paramref name="key"/> or null when no
        /// item is stored by that key.
        /// </summary>
        /// <remarks>
        /// <b>Thread safety:</b> Calls to this method are thread safe, but users should take proper
        /// percussions when they call both <b>GetItem</b> and <see cref="SetItem"/>.
        /// </remarks>
        /// <param name="key">The key of the item to retrieve.</param>
        /// <returns>The stored item or null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference.</exception>
        public object? GetItem(object key)
        {
            Requires.IsNotNull(key, nameof(key));

            lock (this.syncRoot)
            {
                return this.items?[key];
            }
        }

        /// <summary>Stores an item by the given <paramref name="key"/> in the scope.</summary>
        /// <remarks>
        /// <b>Thread safety:</b> Calls to this method are thread safe, but users should take proper
        /// percussions when they call both <see cref="GetItem"/> and <b>SetItem</b>.
        /// </remarks>
        /// <param name="key">The key of the item to insert or override.</param>
        /// <param name="item">The actual item. May be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when paramref name="key"/> is a null reference.
        /// </exception>
        public void SetItem(object key, object? item)
        {
            Requires.IsNotNull(key, nameof(key));

            lock (this.syncRoot)
            {
                if (this.items is null)
                {
                    this.items = new Dictionary<object, object?>(capacity: 1);
                }

                if (item is null)
                {
                    this.items.Remove(key);
                }
                else
                {
                    this.items[key] = item;
                }
            }
        }

        /// <summary>
        /// Returns a copy of the list of <see cref="IDisposable"/> instances that will be disposed of when this
        /// <see cref="Scope"/> instance is being disposed. The list contains scoped instances that are cached
        /// in this <see cref="Scope"/> instance, and instances explicitly registered for disposal using
        /// <see cref="RegisterForDisposal(IDisposable)"/>. The instances are returned in order of creation.
        /// When <see cref="Dispose()">Scope.Dispose</see> is called, the scope will ensure
        /// <see cref="IDisposable.Dispose"/> is called on each instance in this list. The instance will be
        /// disposed in opposite order as they appear in the list.
        /// </summary>
        /// <remarks>
        /// <b>Thread safety:</b> This method is <b>not</b> thread safe and should not be used in combination
        /// with any of the thread-safe methods.
        /// </remarks>
        /// <returns>The list of <see cref="IDisposable"/> instances that will be disposed of when this
        /// <see cref="Scope"/> instance is being disposed.</returns>
        public IDisposable[] GetDisposables()
        {
            this.RequiresInstanceNotDisposed();

            if (this.disposables is null)
            {
                return Helpers.Array<IDisposable>.Empty;
            }

            var list = new List<IDisposable>(this.disposables.Count);

            foreach (var instance in this.disposables)
            {
                if (AsyncDisposableTypeCache.Unwrap(instance) is IDisposable disposable)
                {
                    list.Add(disposable);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Returns a copy of the list of IDisposable and IAsyncDisposable instances that will be disposed of
        /// when this <see cref="Scope"/> instance is being disposed. The list contains scoped instances that
        /// are cached in this <see cref="Scope"/> instance, and instances explicitly registered for disposal
        /// using <see cref="RegisterForDisposal(object)"/>. The instances are returned in order of creation.
        /// When <see cref="Dispose()">Scope.Dispose</see> is called, the scope will ensure
        /// <see cref="IDisposable.Dispose"/> is called on each instance in this list. The instance will be
        /// disposed in opposite order as they appear in the list.
        /// </summary>
        /// <remarks>
        /// <b>Thread safety:</b> This method is <b>not</b> thread safe and should not be used in combination
        /// with any of the thread-safe methods.
        /// </remarks>
        /// <returns>The list of <see cref="IDisposable"/> instances that will be disposed of when this
        /// <see cref="Scope"/> instance is being disposed.</returns>
        public object[] GetAllDisposables()
        {
            this.RequiresInstanceNotDisposed();

            if (this.disposables is null)
            {
                return Helpers.Array<object>.Empty;
            }
            else
            {
                var list = this.disposables.ToArray();

                for (int index = 0; index < list.Length; index++)
                {
                    list[index] = AsyncDisposableTypeCache.Unwrap(list[index]);
                }

                return list;
            }
        }

        /// <summary>Releases all instances that are cached by the <see cref="Scope"/> object.</summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Gets the service object of the specified type.</summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type serviceType -or- null if there is no service object of type
        /// <paramref name="serviceType"/>.</returns>
        object? IServiceProvider.GetService(Type serviceType)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));

            IServiceProvider? provider = this.Container;

            if (provider is null)
            {
                throw new InvalidOperationException(
                    "This method can only be called on Scope instances that are related to a Container. " +
                    "Please use the overloaded constructor of Scope create an instance with a Container.");
            }

            Scope? originalScope = this.Container!.CurrentThreadResolveScope;

            try
            {
                this.Container.CurrentThreadResolveScope = this;
                return provider.GetService(serviceType);
            }
            finally
            {
                this.Container.CurrentThreadResolveScope = originalScope;
            }
        }

        internal static TImplementation GetInstance<TImplementation>(
            ScopedRegistration registration, Scope? scope)
            where TImplementation : class
        {
            if (scope is null)
            {
                return (TImplementation)Scope.GetScopelessInstance(registration);
            }

            return (TImplementation)scope.GetInstanceInternal(registration);
        }

        internal T GetOrSetItem<T>(object key, Func<Container, object, T> valueFactory)
        {
            lock (this.syncRoot)
            {
                if (this.items is null)
                {
                    this.items = new Dictionary<object, object>(capacity: 1);
                }

                object? item = this.items[key];

                if (item is null)
                {
                    this.items[key] = item = valueFactory(this.Container!, key);
                }

                return (T)item!;
            }
        }

        /// <summary>
        /// Releases all instances that are cached by the <see cref="Scope"/> object.
        /// </summary>
        /// <param name="disposing">False when only unmanaged resources should be released.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.state == DisposeState.Alive)
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

                    this.manager?.RemoveScope(this);

                    // Remove all references, so we won't hold on to created instances even if the
                    // scope accidentally keeps referenced. This prevents leaking memory.
                    this.ClearState();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearState()
        {
            this.cachedInstances = null;
            this.scopeEndActions = null;
            this.disposables = null;
            this.items = null;
        }

        private void DisposeRecursively(bool operatingInException = false)
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
                if (this.recursionDuringDisposalCounter <= MaximumDisposeRecursion)
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

                foreach (var action in actions)
                {
                    action.Invoke();
                }
            }
        }

        private static object GetScopelessInstance(ScopedRegistration registration)
        {
            if (registration.Container.IsVerifying)
            {
                return registration.Container.VerificationScope!.GetInstanceInternal(registration);
            }

            throw new ActivationException(
                StringResources.TheServiceIsRequestedOutsideTheContextOfAScopedLifestyle(
                    registration.ImplementationType,
                    registration.Lifestyle));
        }

        private object GetInstanceInternal(ScopedRegistration registration)
        {
            lock (this.syncRoot)
            {
                this.RequiresInstanceNotDisposed();

                bool cacheIsEmpty = this.cachedInstances is null;

                this.cachedInstances ??=
                        new Dictionary<Registration, object>(ReferenceEqualityComparer<Registration>.Instance);

                return !cacheIsEmpty && this.cachedInstances.TryGetValue(registration, out object? instance)
                    ? instance
                    : this.CreateAndCacheInstanceInternal(registration, this.cachedInstances);
            }
        }

        private object CreateAndCacheInstanceInternal(
            ScopedRegistration registration, Dictionary<Registration, object> cache)
        {
            // registration.BuildExpression has been called, and InstanceCreate thus been initialized.
            Func<object> instanceCreator = registration.InstanceCreator!;

            object instance = instanceCreator.Invoke();

            cache[registration] = instance;

            this.TryRegisterForDisposalInternal(registration, instance);

            return instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryRegisterForDisposalInternal(ScopedRegistration registration, object instance)
        {
            if (registration.SuppressDisposal)
            {
                return;
            }

            var disposability = registration.Disposability;

            if (disposability.Sync == Disposability.Never && disposability.Async == Disposability.Never)
            {
                // Break out. Based on static type information we know for sure that the instance never be
                // disposable.
                return;
            }
            else if (disposability.Async == Disposability.Always)
            {
                // Based on static type information we know for sure that the instance will be IAsyncDisposable.
#if NETSTANDARD2_1
                this.RegisterForDisposalInternal(instance);
#else
                // IAsyncDisposable implementations need to be wrapped when not running .NET Standard 2.1.
                // This way we can do a simple type check for AsyncDisposableWrapper during disposal,
                // which is much faster than having to call (the slow) IsAsyncDisposable again. Unfortunately,
                // this does cause the creation of an extra object (this AsyncDisposableWrapper).
                this.RegisterForDisposalInternal(new AsyncDisposableWrapper(instance));
#endif
            }
            else if (disposability.Sync == Disposability.Always)
            {
                // Based on static type information we know for sure that the instance will be IDisposable.
                this.RegisterForDisposalInternal(instance);
            }
            else
            {
#if NETSTANDARD2_1
                if (instance is IDisposable || instance is IAsyncDisposable)
                {
                    this.RegisterForDisposalInternal(instance);
                }
#else
                // Due to the dynamic nature of the registration (either because of the registration was done
                // using a factory delegate, or because the expression got intercepted), we need to check to
                // see if this specific instance is disposable. Unfortunately, this call is much slower
                // compared to simply doing 'instance is IAsyncDisposable'.
                if (AsyncDisposableTypeCache.IsAsyncDisposable(instance))
                {
                    this.RegisterForDisposalInternal(new AsyncDisposableWrapper(instance));
                }
                else if (instance is IDisposable)
                {
                    this.RegisterForDisposalInternal(instance);
                }
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RegisterForDisposalInternal(object disposable)
        {
            this.disposables ??= new List<object>(capacity: 4);

            this.disposables.Add(disposable);
        }

        private void DisposeAllRegisteredDisposables()
        {
            if (this.disposables != null)
            {
                var instances = this.disposables;

                this.disposables = null;

                this.DisposeInstancesInReverseOrder(instances);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PreventCyclicDependenciesDuringDisposal()
        {
            if (this.recursionDuringDisposalCounter > MaximumDisposeRecursion)
            {
                ThrowRecursionException();
            }
        }

        private static void ThrowRecursionException() =>
            throw new InvalidOperationException(StringResources.RecursiveInstanceRegistrationDetected());

        // This method simulates the behavior of a set of nested 'using' statements: It ensures that dispose
        // is called on each element, even if a previous instance threw an exception.
        private void DisposeInstancesInReverseOrder(
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
                    object instance = AsyncDisposableTypeCache.Unwrap(disposables[startingAsIndex]);

                    if (instance is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    else
                    {
                        this.ThrowTypeOnlyImplementsIAsyncDisposable(instance);
                    }

                    startingAsIndex--;
                }
            }
            finally
            {
                if (startingAsIndex >= 0)
                {
                    this.DisposeInstancesInReverseOrder(disposables, startingAsIndex - 1);
                }
            }
        }

        private void ThrowTypeOnlyImplementsIAsyncDisposable(object instance)
        {
            // Must first check for disposal because IsVerifying throws when the container is disposed of.
            if (this.Container?.IsDisposed != true)
            {
                // In case there is no active (ambient) scope, Verify creates and disposes of its own scope.
                // Verify(), however, is completely synchronous and adding a Container.VerifyAsync() method
                // makes little sense because:
                //  1. in many cases the user will call Verify() in a context where there is no option
                //     to await (ASP.NET Core startup, ASP.NET MVC startup, etc).
                //  2. Verify is called during a call to GetInstance when auto verification is enabled.
                //     Adding VerifyAsync() would, therefore, mean adding an GetInstanceAsync(), but
                //     would be a bad to add such method (see: https://stackoverflow.com/a/43240576/).
                // At this point, we can either:
                //  1. ignore the instance with the risk of causing a memory leak
                //  2. call DisposeAsync().GetAwaiter().GetResult() with the possible risk of deadlock.
                // Because 2 a really, really bad place to be in, we pick 1 and choose to ignore those async
                // disposables during verification. If this is a problem the user can two things to prevent
                // those disposables from _not_ being disposed:
                // 1. Wrap the call to Verify() in a Scope
                // 2. Skip the call to Verify() and rely on auto verification, which will typically be
                //    executed within the context of an active scope.
                if (this.Container?.IsVerifying != true)
                {
                    throw new InvalidOperationException(
                        StringResources.TypeOnlyImplementsIAsyncDisposable(instance, this.IsContainerScope));
                }
            }
        }

        private async DisposeAsyncTask DisposeInternalAsync()
        {
            if (this.state == DisposeState.Alive)
            {
                this.state = DisposeState.Disposing;

                try
                {
                    await this.DisposeRecursivelyAsync().ConfigureAwait(false);
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

        private async DisposeAsyncTask DisposeRecursivelyAsync(bool operatingInException = false)
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

                await this.DisposeAllRegisteredDisposablesAsync().ConfigureAwait(false);
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
                    await this.DisposeRecursivelyAsync(operatingInException).ConfigureAwait(false);
                }
            }
        }

        private DisposeAsyncTask DisposeAllRegisteredDisposablesAsync()
        {
            if (this.disposables != null)
            {
                var instances = this.disposables;

                this.disposables = null;

                return DisposeInstancesInReverseOrderAsync(instances);
            }
            else
            {
                return CompletedTask;
            }
        }

        // This method simulates the behavior of a set of nested 'using' statements: It ensures that dispose
        // is called on each element, even if a previous instance threw an exception.
        private static async DisposeAsyncTask DisposeInstancesInReverseOrderAsync(
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

                    await DisposeInstanceAsync(instance).ConfigureAwait(false);

                    startingAsIndex--;
                }
            }
            finally
            {
                if (startingAsIndex >= 0)
                {
                    await DisposeInstancesInReverseOrderAsync(disposables, startingAsIndex - 1).ConfigureAwait(false);
                }
            }
        }

        private static DisposeAsyncTask DisposeInstanceAsync(object instance)
        {
            // Always try disposing asynchronously first. When DisposeAsync() is called, Dispose()
            // doesn't have to be called any longer.
#if NETSTANDARD2_1
            if (instance is IAsyncDisposable asyncDisposable)
            {
                return asyncDisposable.DisposeAsync();
            }
#else
            if (instance is AsyncDisposableWrapper wrapper)
            {
                return wrapper.DisposeAsync();
            }
#endif
            else
            {
                // Dispose synchronously.
                ((IDisposable)instance).Dispose();
                return CompletedTask;
            }
        }

#if !NETSTANDARD2_1
        private sealed class AsyncDisposableWrapper(object instance)
        {
            public readonly object Instance = instance;

            public Task DisposeAsync() => DisposableHelpers.DisposeAsync(this.Instance);
        }
#endif

        private static class AsyncDisposableTypeCache
        {
#if NETSTANDARD2_1
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static object Unwrap(object instance) => instance;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsAsyncDisposable(object instance) => instance is IAsyncDisposable;
#else
            // This instance is never updated, only completely replaced.
            // Although IsAsyncDisposable called from within the context of a lock(), that lock is specific
            // to a Scope instance. This still allows parallel access to this dictionary.
            private static Dictionary<Type, bool> Cache = new();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static object Unwrap(object instance) =>
                instance is AsyncDisposableWrapper w ? w.Instance : instance;

            public static bool IsAsyncDisposable(object instance)
            {
                Type type = instance.GetType();
                if (Cache.TryGetValue(type, out bool isAsyncDisposable))
                {
                    return isAsyncDisposable;
                }
                else
                {
                    isAsyncDisposable = DisposableHelpers.IsAsyncDisposableType(type);

                    Helpers.InterlockedAddAndReplace(ref Cache, type, isAsyncDisposable);

                    return isAsyncDisposable;
                }
            }
#endif
        }
    }
}