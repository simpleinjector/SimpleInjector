#region Copyright Simple Injector Contributors
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
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using SimpleInjector.Advanced;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Diagnostics.Debugger;
    using SimpleInjector.Internals;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// The container. Create an instance of this type for registration of dependencies.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Thread-safety:</b>
    /// Resolving instances can be done safely from multiple threads concurrently, but registration needs to
    /// be done from one single thread.
    /// </para>
    /// <para> 
    /// It is therefore safe to call <see cref="GetInstance"/>, <see cref="GetAllInstances"/>, 
    /// <see cref="IServiceProvider.GetService">GetService</see>, <see cref="GetRegistration(System.Type)"/> and
    /// <see cref="GetCurrentRegistrations()"/> and anything related to resolving instances from multiple thread 
    /// concurrently. It is however <b>unsafe</b> to call
    /// <see cref="Register{TService, TImplementation}(Lifestyle)">RegisterXXX</see>,
    /// <see cref="ExpressionBuilding"/>, <see cref="ExpressionBuilt"/>, <see cref="ResolveUnregisteredType"/>,
    /// <see cref="AddRegistration"/> or anything related to registering from multiple threads concurrently.
    /// </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",
        Justification = "Not much we can do about this. Container is the facade where users work with.")]
    [DebuggerTypeProxy(typeof(ContainerDebugView))]
    public partial class Container : IDisposable
    {
        internal readonly Dictionary<object, Dictionary<Type, WeakReference>> LifestyleRegistrationCache =
            new Dictionary<object, Dictionary<Type, WeakReference>>();

        private static long counter;

        private readonly object locker = new object();
        private readonly List<IInstanceInitializer> instanceInitializers = new List<IInstanceInitializer>();
        private readonly List<ContextualResolveInterceptor> resolveInterceptors = new List<ContextualResolveInterceptor>();
        private readonly IDictionary items = new Dictionary<object, object>();
        private readonly long containerId;
        private readonly Scope disposableSingletonsScope;

        // Collection of (both conditional and unconditional) instance producers that are explicitly 
        // registered by the user and implicitly registered through unregistered type resolution.
        private readonly Dictionary<Type, IRegistrationEntry> explicitRegistrations =
            new Dictionary<Type, IRegistrationEntry>(64);

        private readonly Dictionary<Type, CollectionResolver> collectionResolvers =
            new Dictionary<Type, CollectionResolver>();

        // This list contains all instance producers that not yet have been explicitly registered in the container.
        private readonly ConditionalHashSet<InstanceProducer> externalProducers = 
            new ConditionalHashSet<InstanceProducer>();

        private readonly Dictionary<Type, InstanceProducer> unregisteredConcreteTypeInstanceProducers =
            new Dictionary<Type, InstanceProducer>();

        // Flag to signal that the container can't be altered by using any of the Register methods.
        private bool locked;
        private string stackTraceThatLockedTheContainer;
        private bool disposed;
        private string stackTraceThatDisposedTheContainer;

        private EventHandler<UnregisteredTypeEventArgs> resolveUnregisteredType;
        private EventHandler<ExpressionBuildingEventArgs> expressionBuilding;

        private EventHandler<ExpressionBuiltEventArgs> expressionBuilt;

        /// <summary>Initializes a new instance of the <see cref="Container"/> class.</summary>
        public Container()
        {
            this.containerId = Interlocked.Increment(ref counter);

            this.disposableSingletonsScope = new Scope(this);

            this.Options = new ContainerOptions(this);

            this.SelectionBasedLifestyle = new LifestyleSelectionBehaviorProxyLifestyle(this.Options);

            this.AddContainerRegistrations();
        }

        // Wrapper for instance initializer delegates
        private interface IInstanceInitializer
        {
            bool AppliesTo(Type implementationType, InitializerContext context);

            Action<T> CreateAction<T>(InitializerContext context);
        }

        /// <summary>Gets the container options.</summary>
        /// <value>The <see cref="ContainerOptions"/> instance for this container.</value>
        public ContainerOptions Options { get; }

        /// <summary>
        /// Gets a value indicating whether the container is currently being verified on the current thread.
        /// </summary>
        /// <value>True in case the container is currently being verified on the current thread; otherwise
        /// false.</value>
        public bool IsVerifying
        {
            get { return this.isVerifying.Value; }
            private set { this.isVerifying.Value = value; }
        }

        /// <summary>
        /// Gets the intermediate lifestyle that forwards CreateRegistration calls to the lifestyle that is 
        /// returned from the registered container.Options.LifestyleSelectionBehavior.
        /// </summary>
        internal LifestyleSelectionBehaviorProxyLifestyle SelectionBasedLifestyle { get; }

        internal bool IsLocked
        {
            get
            {
                if (this.locked)
                {
                    return true;
                }

                // By using a lock, we have the certainty that all threads will see the new value for 'locked'
                // immediately.
                lock (this.locker)
                {
                    return this.locked;
                }
            }
        }

        internal bool HasRegistrations => this.explicitRegistrations.Any() || this.collectionResolvers.Any();

        internal bool HasResolveInterceptors => this.resolveInterceptors.Count > 0;

        /// <summary>
        /// Returns an array with the current registrations. This list contains all explicitly registered
        /// types, and all implicitly registered instances. Implicit registrations are  all concrete 
        /// unregistered types that have been requested, all types that have been resolved using
        /// unregistered type resolution (using the <see cref="ResolveUnregisteredType"/> event), and
        /// requested unregistered collections. Note that the result of this method may change over time, 
        /// because of these implicit registrations.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method has a performance characteristic of O(n). Prevent from calling this in a performance
        /// critical path of the application.
        /// </para>
        /// <para>
        /// <b>Note:</b> This method is <i>not</i> guaranteed to always return the same 
        /// <see cref="InstanceProducer"/> instance for a given registration. It will however either 
        /// always return a producer that is able to return the expected instance. Because of this, do not 
        /// compare sets of instances returned by different calls to <see cref="GetCurrentRegistrations()"/> 
        /// by reference. The way of comparing lists is by the actual type. The type of each instance is 
        /// guaranteed to be unique in the returned list.
        /// </para>
        /// </remarks>
        /// <returns>An array of <see cref="InstanceProducer"/> instances.</returns>
        public InstanceProducer[] GetCurrentRegistrations()
        {
            return this.GetCurrentRegistrations(includeInvalidContainerRegisteredTypes: false);
        }

        /// <summary>
        /// Returns an array with the current registrations for root objects. Root objects are registrations
        /// that are in the root of the object graph, meaning that no other registration is depending on it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method has a performance characteristic of O(n). Prevent from calling this in a performance
        /// critical path of the application.
        /// </para>
        /// <para>
        /// This list contains the root objects of all explicitly registered types, and all implicitly 
        /// registered instances. Implicit registrations are all concrete unregistered types that have been 
        /// requested, all types that have been resolved using unregistered type resolution (using the 
        /// <see cref="ResolveUnregisteredType"/> event), and requested unregistered collections. Note that 
        /// the result of this method may change over time, because of these implicit registrations.
        /// </para>
        /// <para>
        /// <b>Note:</b> This method is <i>not</i> guaranteed to always return the same 
        /// <see cref="InstanceProducer"/> instance for a given registration. It will however either 
        /// always return a producer that is able to return the expected instance. Because of this, do not 
        /// compare sets of instances returned by different calls to <see cref="GetCurrentRegistrations()"/> 
        /// by reference. The way of comparing lists is by the actual type. The type of each instance is 
        /// guaranteed to be unique in the returned list.
        /// </para>
        /// </remarks>
        /// <returns>An array of <see cref="InstanceProducer"/> instances.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this method is called before
        /// <see cref="Verify()"/> has been successfully called.</exception>
        public InstanceProducer[] GetRootRegistrations()
        {
            if (!this.SuccesfullyVerified)
            {
                throw new InvalidOperationException(
                    StringResources.GetRootRegistrationsCanNotBeCalledBeforeVerify());
            }

            return this.GetRootRegistrations(includeInvalidContainerRegisteredTypes: false);
        }

        /// <summary>Determines whether the specified System.Object is equal to the current System.Object.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current System.Object.</param>
        /// <returns>
        /// True if the specified System.Object is equal to the current System.Object; otherwise, false.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>Returns the hash code of the current instance.</summary>
        /// <returns>The hash code of the current instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the <see cref="Container"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the <see cref="Container"/>.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>Gets the <see cref="System.Type"/> of the current instance.</summary>
        /// <returns>The <see cref="System.Type"/> instance that represents the exact runtime 
        /// type of the current instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = @"
            This FxCop warning is valid, but this method is used to be able to attach an 
            EditorBrowsableAttribute to the GetType method, which will hide the method when the user browses 
            the methods of the Container class with IntelliSense. The GetType method has no value for the user
            who will only use this class for registration.")]
        public new Type GetType() => base.GetType();

        /// <summary>Releases all instances that are cached by the <see cref="Container"/> object.</summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);

            if (!this.disposed)
            {
                this.disposed = true;

                GetStackTrace(ref this.stackTraceThatDisposedTheContainer);
            }
        }

        internal InstanceProducer[] GetRootRegistrations(bool includeInvalidContainerRegisteredTypes)
        {
            var currentRegistrations = this.GetCurrentRegistrations(
                includeInvalidContainerRegisteredTypes: includeInvalidContainerRegisteredTypes);

            var nonRootProducers =
                from registration in currentRegistrations
                from relationship in registration.GetRelationships()
                select relationship.Dependency;

            return currentRegistrations.Except(nonRootProducers, InstanceProducer.EqualityComparer).ToArray();
        }

        internal InstanceProducer[] GetCurrentRegistrations(bool includeInvalidContainerRegisteredTypes,
            bool includeExternalProducers = true)
        {
            var producers =
                from entry in this.explicitRegistrations.Values
                from producer in entry.CurrentProducers
                select producer;

            producers = producers.Concat(this.rootProducerCache.Values);

            if (includeExternalProducers)
            {
                producers = producers.Concat(this.externalProducers.GetLivingItems());
            }

            // Filter out the invalid registrations (see the IsValid property for more information).
            producers =
                from producer in producers.Distinct(InstanceProducer.EqualityComparer)
                where producer != null
                where includeInvalidContainerRegisteredTypes || producer.IsValid
                select producer;

            return producers.ToArray();
        }

        internal object GetItem(object key)
        {
            lock (this.items)
            {
                return this.items[key];
            }
        }

        internal void SetItem(object key, object item)
        {
            lock (this.items)
            {
                if (object.ReferenceEquals(item, null))
                {
                    this.items.Remove(key);
                }
                else
                {
                    this.items[key] = item;
                }
            }
        }

        internal T GetOrSetItem<T>(object key, Func<Container, object, T> valueFactory)
        {
            lock (this.items)
            {
                object item = this.items[key];

                if (item == null)
                {
                    this.items[key] = item = valueFactory(this, key);
                }

                return (T)item;
            }
        }

        internal Expression OnExpressionBuilding(Registration registration, Type implementationType, 
            Expression instanceCreatorExpression)
        {
            if (this.expressionBuilding != null)
            {
                var e = new ExpressionBuildingEventArgs(implementationType,
                    instanceCreatorExpression, registration.Lifestyle);

                var relationships = new KnownRelationshipCollection(registration.GetRelationships().ToList());

                e.KnownRelationships = relationships;

                this.expressionBuilding(this, e);

                // Optimization.
                if (relationships.HasChanged)
                {
                    registration.ReplaceRelationships(e.KnownRelationships);
                }

                return e.Expression;
            }

            return instanceCreatorExpression;
        }

        internal void OnExpressionBuilt(ExpressionBuiltEventArgs e, InstanceProducer instanceProducer)
        {
            if (this.expressionBuilt != null)
            {
                var relationships =
                    new KnownRelationshipCollection(instanceProducer.GetRelationships().ToList());

                e.KnownRelationships = relationships;

                this.expressionBuilt(this, e);

                if (relationships.HasChanged)
                {
                    instanceProducer.ReplaceRelationships(e.KnownRelationships);
                }
            }
        }

        /// <summary>Prevents any new registrations to be made to the container.</summary>
#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal void LockContainer()
        {
            if (!this.locked)
            {
                // Performance optimization: The real locking is moved to another method to allow this method
                // to be in-lined.
                this.FlagContainerAsLocked();
            }
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal void ThrowWhenDisposed()
        {
            if (this.disposed)
            {
                // Performance optimization: Throwing moved to another method to allow this method to be in-lined.
                this.ThrowContainerDisposedException();
            }
        }

        internal Func<object> WrapWithResolveInterceptor(InstanceProducer instanceProducer, Func<object> producer)
        {
            if (this.HasResolveInterceptors)
            {
                var context = new InitializationContext(instanceProducer, instanceProducer.Registration);

                ResolveInterceptor[] interceptors = this.GetResolveInterceptorsFor(context);

                foreach (ResolveInterceptor interceptor in interceptors)
                {
                    producer = ApplyResolveInterceptor(interceptor, context, producer);
                }
            }

            return producer;
        }

        internal void RegisterForDisposal(IDisposable disposable)
        {
            this.disposableSingletonsScope.RegisterForDisposal(disposable);
        }

        internal void ThrowWhenContainerIsLockedOrDisposed()
        {
            this.ThrowWhenDisposed();

            // By using a lock, we have the certainty that all threads will see the new value for 'locked'
            // immediately.
            lock (this.locker)
            {
                if (this.locked)
                {
                    throw new InvalidOperationException(StringResources.ContainerCanNotBeChangedAfterUse(
                        this.stackTraceThatLockedTheContainer));
                }
            }
        }

        internal void ThrowParameterTypeMustBeRegistered(InjectionTargetInfo target)
        {
            throw new ActivationException(
                StringResources.ParameterTypeMustBeRegistered(
                    target,
                    this.GetNumberOfConditionalRegistrationsFor(target.TargetType),
                    this.ContainsOneToOneRegistrationForCollectionType(target.TargetType),
                    this.ContainsCollectionRegistrationFor(target.TargetType),
                    this.GetNonGenericDecoratorsThatWereSkippedDuringBatchRegistration(target.TargetType),
                    this.GetLookalikesForMissingType(target.TargetType)));
        }

        /// <summary>Releases all instances that are cached by the <see cref="Container"/> object.</summary>
        /// <param name="disposing">True for a normal dispose operation; false to finalize the handle.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    this.disposableSingletonsScope.Dispose();
                }
                finally
                {
                    this.isVerifying.Dispose();

                    // HACK: Due to a bug (https://stackoverflow.com/questions/33156432) in ThreadLocal<T>,
                    // the container gets rooted when decorators are applied. See issue #135.
                    this.items.Clear();
                }
            }
        }

        static partial void GetStackTrace(ref string stackTrace);

        private static Func<object> ApplyResolveInterceptor(ResolveInterceptor interceptor,
            InitializationContext context, Func<object> wrappedProducer)
        {
            return () => ThrowWhenResolveInterceptorReturnsNull(interceptor(context, wrappedProducer));
        }

        private void FlagContainerAsLocked()
        {
            // By using a lock, we have the certainty that all threads will see the new value for 'locked'
            // immediately, since ThrowWhenContainerIsLocked also locks on 'locker'.
            lock (this.locker)
            {
                GetStackTrace(ref this.stackTraceThatLockedTheContainer);

                this.locked = true;
            }
        }

        private void ThrowContainerDisposedException()
        {
            throw new ObjectDisposedException(
                objectName: null,
                message: StringResources.ContainerCanNotBeUsedAfterDisposal(this.GetType(),
                    this.stackTraceThatDisposedTheContainer));
        }

        private static object ThrowWhenResolveInterceptorReturnsNull(object instance)
        {
            if (instance == null)
            {
                throw new ActivationException(StringResources.ResolveInterceptorDelegateReturnedNull());
            }

            return instance;
        }

        private ResolveInterceptor[] GetResolveInterceptorsFor(InitializationContext context)
        {
            return (
                from resolveInterceptor in this.resolveInterceptors
                where resolveInterceptor.Predicate(context)
                select resolveInterceptor.Interceptor)
                .ToArray();
        }

        private Action<T>[] GetInstanceInitializersFor<T>(Type type, Registration registration)
        {
            if (this.instanceInitializers.Count == 0)
            {
                return Helpers.Array<Action<T>>.Empty;
            }

            var context = new InitializerContext(registration);

            return (
                from instanceInitializer in this.instanceInitializers
                where instanceInitializer.AppliesTo(type, context)
                select instanceInitializer.CreateAction<T>(context))
                .ToArray();
        }
        
        private void RegisterOpenGeneric(Type serviceType, Type implementationType, 
            Lifestyle lifestyle, Predicate<PredicateContext> predicate = null)
        {
            Requires.IsGenericType(serviceType, nameof(serviceType));
            Requires.IsNotPartiallyClosed(serviceType, nameof(serviceType));
            Requires.ServiceOrItsGenericTypeDefinitionIsAssignableFromImplementation(serviceType,
                implementationType, nameof(serviceType));
            Requires.OpenGenericTypeDoesNotContainUnresolvableTypeArguments(serviceType,
                implementationType, nameof(implementationType));

            this.GetOrCreateRegistrationalEntry(serviceType)
                .AddGeneric(serviceType, implementationType, lifestyle, predicate);
        }
       
        private void AddInstanceProducer(InstanceProducer producer)
        {
            if (typeof(IEnumerable<>).IsGenericTypeDefinitionOf(producer.ServiceType))
            {
                this.AddCollectionInstanceProducer(producer);
            }
            else
            {
                var entry = this.GetOrCreateRegistrationalEntry(producer.ServiceType);

                entry.Add(producer);

                this.RemoveExternalProducer(producer);
            }
        }

        private void AddCollectionInstanceProducer(InstanceProducer producer)
        {
            Type itemType = producer.ServiceType.GetGenericArguments()[0];

            var resolver = this.GetContainerUncontrolledResolver(itemType);

            resolver.RegisterUncontrolledCollection(itemType, producer);
        }

        private int GetNumberOfConditionalRegistrationsFor(Type serviceType)
        {
            var entry = this.GetRegistrationalEntryOrNull(serviceType);

            return entry == null 
                ? 0 
                : entry.GetNumberOfConditionalRegistrationsFor(serviceType);
        }

        // Instead of using the this.registrations instance, this method takes a snapshot. This allows the
        // container to be thread-safe, without using locks.
        private InstanceProducer GetInstanceProducerForType(Type serviceType, InjectionConsumerInfo consumer,
            Func<InstanceProducer> buildInstanceProducer)
        {
            return 
                this.GetExplicitlyRegisteredInstanceProducer(serviceType, consumer)
                ?? this.TryGetInstanceProducerForRegisteredCollection(serviceType)
                ?? buildInstanceProducer();
        }

        private InstanceProducer GetExplicitlyRegisteredInstanceProducer(Type serviceType, InjectionConsumerInfo consumer)
        {
            var entry = this.GetRegistrationalEntryOrNull(serviceType);

            return entry != null
                ? entry.TryGetInstanceProducer(serviceType, consumer)
                : null;
        }

        private InstanceProducer TryGetInstanceProducerForRegisteredCollection(Type enumerableServiceType) =>
            typeof(IEnumerable<>).IsGenericTypeDefinitionOf(enumerableServiceType)
                ? this.GetInstanceProducerForRegisteredCollection(
                    enumerableServiceType.GetGenericArguments()[0])
                : null;

        private InstanceProducer GetInstanceProducerForRegisteredCollection(Type serviceType)
        {
            Type key = GetRegistrationKey(serviceType);
            var resolver = this.collectionResolvers.GetValueOrDefault(key);

            return resolver != null
                ? resolver.TryGetInstanceProducer(serviceType)
                : null;
        }

        private IRegistrationEntry GetOrCreateRegistrationalEntry(Type serviceType)
        {
            Type key = GetRegistrationKey(serviceType);

            var entry = this.explicitRegistrations.GetValueOrDefault(key);

            if (entry == null)
            {
                this.explicitRegistrations[key] = entry = RegistrationEntry.Create(serviceType, this);
            }

            return entry;
        }

        private IRegistrationEntry GetRegistrationalEntryOrNull(Type serviceType)
        {
            return this.explicitRegistrations.GetValueOrDefault(GetRegistrationKey(serviceType));
        }

        private static Type GetRegistrationKey(Type serviceType)
        {
            return serviceType.IsGenericType()
                ? serviceType.GetGenericTypeDefinition()
                : serviceType;
        }

        private void AddContainerRegistrations()
        {
            // Add the default registrations. This adds them as registration, but only in case some component
            // starts depending on them.
            var scopeLifestyle = new ScopedScopeLifestyle();

            this.resolveUnregisteredTypeRegistrations[typeof(Scope)] = new Lazy<InstanceProducer>(
                () => scopeLifestyle.CreateProducer(() => scopeLifestyle.GetCurrentScope(this), this));

            this.resolveUnregisteredTypeRegistrations[typeof(Container)] = new Lazy<InstanceProducer>(
                () => Lifestyle.Singleton.CreateProducer(() => this, this));
        }

        private Type[] GetLookalikesForMissingType(Type missingServiceType) =>
            this.GetLookalikesForMissingNonGenericType(missingServiceType.ToFriendlyName())
                .Concat(missingServiceType.IsGenericType()
                    ? this.GetLookalikesForMissingGeneritTypeDefinitions(
                        missingServiceType.GetGenericTypeDefinition().ToFriendlyName())
                    : Enumerable.Empty<Type>())
                .ToArray();

        // A lookalike type is a registered type that shares the same type name (where casing is ignored
        // and the parent type name of a nested type is included) as the missing type. Nested types are
        // mostly excluded from this list, because it would be quite common for developers to have lots
        // of nested types with the same name.
        private IEnumerable<Type> GetLookalikesForMissingNonGenericType(string missingServiceTypeName) => 
            from registration in this.GetCurrentRegistrations(false, includeExternalProducers: false)
            let typeName = registration.ServiceType.ToFriendlyName()
            where StringComparer.OrdinalIgnoreCase.Equals(typeName, missingServiceTypeName)
            select registration.ServiceType;

        private IEnumerable<Type> GetLookalikesForMissingGeneritTypeDefinitions(string missingTypeDefName) =>
            from type in this.explicitRegistrations.Keys
            where type.IsGenericTypeDefinition()
            let friendlyName = type.GetGenericTypeDefinition().ToFriendlyName()
            where StringComparer.OrdinalIgnoreCase.Equals(friendlyName, missingTypeDefName)
            select type;

        private sealed class ContextualResolveInterceptor
        {
            public readonly ResolveInterceptor Interceptor;
            public readonly Predicate<InitializationContext> Predicate;

            public ContextualResolveInterceptor(ResolveInterceptor interceptor,
                Predicate<InitializationContext> predicate)
            {
                this.Interceptor = interceptor;
                this.Predicate = predicate;
            }
        }

        private sealed class TypedInstanceInitializer : IInstanceInitializer
        {
            private Type serviceType;
            private object instanceInitializer;

            public bool AppliesTo(Type implementationType, InitializerContext context)
            {
                var typeHierarchy = Types.GetTypeHierarchyFor(implementationType);

                return typeHierarchy.Contains(this.serviceType);
            }

            public Action<T> CreateAction<T>(InitializerContext context)
            {
                return Helpers.CreateAction<T>(this.instanceInitializer);
            }

            internal static IInstanceInitializer Create<TImplementation>(
                Action<TImplementation> instanceInitializer)
            {
                return new TypedInstanceInitializer
                {
                    serviceType = typeof(TImplementation),
                    instanceInitializer = instanceInitializer
                };
            }
        }

        private sealed class ContextualInstanceInitializer : IInstanceInitializer
        {
            private Predicate<InitializerContext> predicate;
            private Action<InstanceInitializationData> instanceInitializer;

            public bool AppliesTo(Type implementationType, InitializerContext context) => this.predicate(context);

            public Action<T> CreateAction<T>(InitializerContext context)
            {
                return instance => this.instanceInitializer(new InstanceInitializationData(context, instance));
            }

            internal static IInstanceInitializer Create(
                Action<InstanceInitializationData> instanceInitializer,
                Predicate<InitializerContext> predicate)
            {
                return new ContextualInstanceInitializer
                {
                    instanceInitializer = instanceInitializer,
                    predicate = predicate,
                };
            }
        }
    }
}