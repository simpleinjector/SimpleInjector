#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2014 Simple Injector Contributors
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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using SimpleInjector.Advanced;
    using SimpleInjector.Decorators;
    using SimpleInjector.Internals;
    using SimpleInjector.Lifestyles;

#if !PUBLISH
    /// <summary>Methods for resolving instances.</summary>
#endif
    public partial class Container : IServiceProvider
    {
        // Cache for producers that are resolved as root type
        // PERF: The rootProducerCache uses a special equality comparer that does a quicker lookup of types.
        // PERF: This collection is updated by replacing the complete collection.
        private Dictionary<Type, InstanceProducer> rootProducerCache =
            new Dictionary<Type, InstanceProducer>(ReferenceEqualityComparer<Type>.Instance);

        private readonly Dictionary<Type, InstanceProducer> resolveUnregisteredTypeRegistrations =
            new Dictionary<Type, InstanceProducer>();

        private readonly Dictionary<Type, InstanceProducer> emptyAndRedirectedCollectionRegistrationCache =
            new Dictionary<Type, InstanceProducer>();

        /// <summary>Gets an instance of the given <typeparamref name="TService"/>.</summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <returns>The requested service instance.</returns>
        /// <exception cref="ActivationException">Thrown when there are errors resolving the service instance.</exception>
        public TService GetInstance<TService>() where TService : class
        {
            this.LockContainer();

            InstanceProducer instanceProducer;

            // Performance optimization: This if check is a duplicate to save a call to GetInstanceForType.
            if (!this.rootProducerCache.TryGetValue(typeof(TService), out instanceProducer))
            {
                return (TService)this.GetInstanceForRootType<TService>();
            }

            if (instanceProducer == null)
            {
                this.ThrowMissingInstanceProducerException(typeof(TService));
            }

            return (TService)instanceProducer.GetInstance();
        }

        /// <summary>Gets an instance of the given <paramref name="serviceType"/>.</summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <returns>The requested service instance.</returns>
        /// <exception cref="ActivationException">Thrown when there are errors resolving the service instance.</exception>
        public object GetInstance(Type serviceType)
        {
            this.LockContainer();

            InstanceProducer instanceProducer;

            if (!this.rootProducerCache.TryGetValue(serviceType, out instanceProducer))
            {
                return this.GetInstanceForRootType(serviceType);
            }

            if (instanceProducer == null)
            {
                this.ThrowMissingInstanceProducerException(serviceType);
            }

            return instanceProducer.GetInstance();
        }

        /// <summary>
        /// Gets all instances of the given <typeparamref name="TService"/> currently registered in the container.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <returns>A sequence of instances of the requested TService.</returns>
        /// <exception cref="ActivationException">Thrown when there are errors resolving the service instance.</exception>
        public IEnumerable<TService> GetAllInstances<TService>()
            where TService : class
        {
            return this.GetInstance<IEnumerable<TService>>();
        }

        /// <summary>
        /// Gets all instances of the given <paramref name="serviceType"/> currently registered in the container.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <returns>A sequence of instances of the requested serviceType.</returns>
        /// <exception cref="ActivationException">Thrown when there are errors resolving the service instance.</exception>
        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            Type collectionType = typeof(IEnumerable<>).MakeGenericType(serviceType);

            return (IEnumerable<object>)this.GetInstance(collectionType);
        }

        /// <summary>Gets the service object of the specified type.</summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type serviceType.  -or- null if there is no service object of type 
        /// <paramref name="serviceType"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
            Justification = "Users are not expected to inherit from this class and override this implementation.")]
        object IServiceProvider.GetService(Type serviceType)
        {
            this.LockContainer();

            InstanceProducer instanceProducer;

            if (!this.rootProducerCache.TryGetValue(serviceType, out instanceProducer))
            {
                instanceProducer = this.GetRegistration(serviceType);
            }

            if (instanceProducer != null && instanceProducer.IsValid)
            {
                return instanceProducer.GetInstance();
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="InstanceProducer"/> for the given <paramref name="serviceType"/>. When no
        /// registration exists, the container will try creating a new producer. A producer can be created
        /// when the type is a concrete reference type, there is an <see cref="ResolveUnregisteredType"/>
        /// event registered that acts on that type, or when the service type is an <see cref="IEnumerable{T}"/>.
        /// Otherwise <b>null</b> (Nothing in VB) is returned.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A call to this method locks the container. No new registrations can't be made after a call to this 
        /// method.
        /// </para>
        /// <para>
        /// <b>Note:</b> This method is <i>not</i> guaranteed to always return the same 
        /// <see cref="InstanceProducer"/> instance for a given <see cref="Type"/>. It will however either 
        /// always return <b>null</b> or always return a producer that is able to return the expected instance.
        /// </para>
        /// </remarks>
        /// <param name="serviceType">The <see cref="Type"/> that the returned instance producer should produce.</param>
        /// <returns>An <see cref="InstanceProducer"/> or <b>null</b> (Nothing in VB).</returns>
        public InstanceProducer GetRegistration(Type serviceType)
        {
            return this.GetRegistration(serviceType, throwOnFailure: false);
        }

        /// <summary>
        /// Gets the <see cref="InstanceProducer"/> for the given <paramref name="serviceType"/>. When no
        /// registration exists, the container will try creating a new producer. A producer can be created
        /// when the type is a concrete reference type, there is an <see cref="ResolveUnregisteredType"/>
        /// event registered that acts on that type, or when the service type is an <see cref="IEnumerable{T}"/>.
        /// Otherwise <b>null</b> (Nothing in VB) is returned, or an exception is throw when
        /// <paramref name="throwOnFailure"/> is set to <b>true</b>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A call to this method locks the container. No new registrations can't be made after a call to this 
        /// method.
        /// </para>
        /// <para>
        /// <b>Note:</b> This method is <i>not</i> guaranteed to always return the same 
        /// <see cref="InstanceProducer"/> instance for a given <see cref="Type"/>. It will however either 
        /// always return <b>null</b> or always return a producer that is able to return the expected instance.
        /// </para>
        /// </remarks>
        /// <param name="serviceType">The <see cref="Type"/> that the returned instance producer should produce.</param>
        /// <param name="throwOnFailure">The indication whether the method should return null or throw
        /// an exception when the type is not registered.</param>
        /// <returns>An <see cref="InstanceProducer"/> or <b>null</b> (Nothing in VB).</returns>
        //// Yippie, we broke a framework design guideline rule here :-).
        //// 7.1 DO NOT have public members that can either throw or not based on some option.
        public InstanceProducer GetRegistration(Type serviceType, bool throwOnFailure)
        {
            return this.GetRegistration(serviceType, InjectionConsumerInfo.Root, throwOnFailure);
        }

        internal InstanceProducer GetRegistration(Type serviceType, InjectionConsumerInfo consumer, bool throwOnFailure)
        {
            this.LockContainer();

            InstanceProducer producer;

            if (consumer == InjectionConsumerInfo.Root)
            {
                if (!this.rootProducerCache.TryGetValue(serviceType, out producer))
                {
                    producer =
                        this.GetRegistrationEvenIfInvalid(serviceType, consumer, autoCreateConcreteTypes: true);

                    this.AppendRootInstanceProducer(serviceType, producer);
                }
            }
            else
            {
                producer =
                    this.GetRegistrationEvenIfInvalid(serviceType, consumer, autoCreateConcreteTypes: true);
            }

            bool producerIsValid = producer != null && producer.IsValid;

            if (!producerIsValid && throwOnFailure)
            {
                this.ThrowInvalidRegistrationException(serviceType, producer);
            }

            // Prevent returning invalid producers
            return producerIsValid ? producer : null;
        }

        internal InstanceProducer GetRootRegistrationNoAutoCreateConcretesAndIgnoreFailures(Type serviceType)
        {
            // Don't cache this root producer here, because this causes us to invalidly flag the registration 
            // is invalid.
            return this.GetRegistration(serviceType, InjectionConsumerInfo.Root,
                throwOnFailure: false,
                autoCreateConcreteTypes: false);
        }

        internal Action<TImplementation> GetInitializer<TImplementation>(InitializationContext context)
        {
            return this.GetInitializer<TImplementation>(typeof(TImplementation), context);
        }

        internal Action<object> GetInitializer(Type implementationType, InitializationContext context)
        {
            return this.GetInitializer<object>(implementationType, context);
        }

        internal InstanceProducer GetRegistrationEvenIfInvalid(Type serviceType, InjectionConsumerInfo consumer,
            bool autoCreateConcreteTypes = true)
        {
            // This Func<T> is a bit ugly, but does save us a lot of duplicate code.
            Func<InjectionConsumerInfo, InstanceProducer> buildProducer =
                context => this.BuildInstanceProducerForType(serviceType, context, autoCreateConcreteTypes);

            return this.GetInstanceProducerForType(serviceType, consumer, buildProducer);
        }

        private InstanceProducer GetRegistration(Type serviceType, InjectionConsumerInfo context,
            bool throwOnFailure, bool autoCreateConcreteTypes)
        {
            this.LockContainer();

            var producer = this.GetRegistrationEvenIfInvalid(serviceType, context, autoCreateConcreteTypes);

            bool producerIsValid = producer != null && producer.IsValid;

            if (!producerIsValid && throwOnFailure)
            {
                this.ThrowInvalidRegistrationException(serviceType, producer);
            }

            // Prevent returning invalid producers
            return producerIsValid ? producer : null;
        }

        private Action<T> GetInitializer<T>(Type implementationType, InitializationContext context)
        {
            Action<T>[] initializersForType = this.GetInstanceInitializersFor<T>(implementationType, context);

            if (initializersForType.Length <= 1)
            {
                return initializersForType.FirstOrDefault();
            }
            else
            {
                return obj =>
                {
                    for (int index = 0; index < initializersForType.Length; index++)
                    {
                        initializersForType[index](obj);
                    }
                };
            }
        }

        private InstanceProducer GetInstanceProducerForType<TService>(InjectionConsumerInfo context)
            where TService : class
        {
            // This generic overload allows retrieving types that are internal inside a sandbox.
            return this.GetInstanceProducerForType(typeof(TService), context,
                this.BuildInstanceProducerForType<TService>);
        }

        private InstanceProducer GetInstanceProducerForType(Type serviceType, InjectionConsumerInfo context)
        {
            return this.GetInstanceProducerForType(serviceType, context,
                c => this.BuildInstanceProducerForType(serviceType, c));
        }

        private object GetInstanceForRootType<TService>() where TService : class
        {
            InstanceProducer producer = this.GetInstanceProducerForType<TService>(InjectionConsumerInfo.Root);
            this.AppendRootInstanceProducer(typeof(TService), producer);
            return this.GetInstanceFromProducer(producer, typeof(TService));
        }

        private object GetInstanceForRootType(Type serviceType)
        {
            InstanceProducer producer = this.GetInstanceProducerForType(serviceType, InjectionConsumerInfo.Root);
            this.AppendRootInstanceProducer(serviceType, producer);
            return this.GetInstanceFromProducer(producer, serviceType);
        }

        private object GetInstanceFromProducer(InstanceProducer instanceProducer, Type serviceType)
        {
            if (instanceProducer == null)
            {
                this.ThrowMissingInstanceProducerException(serviceType);
            }

            // We create the instance AFTER registering the instance producer. Registering the producer after
            // creating an instance, could make us loose all registrations that are done by GetInstance. This
            // will not have any functional effects, but can result in a performance penalty.
            return instanceProducer.GetInstance();
        }

        private InstanceProducer BuildInstanceProducerForType<TService>(InjectionConsumerInfo context)
            where TService : class
        {
            return this.BuildInstanceProducerForType(typeof(TService), context,
                this.TryBuildInstanceProducerForConcreteUnregisteredType<TService>);
        }

        private InstanceProducer BuildInstanceProducerForType(Type serviceType, InjectionConsumerInfo context,
            bool autoCreateConcreteTypes = true)
        {
            Func<InstanceProducer> tryBuildInstanceProducerForConcrete = autoCreateConcreteTypes
                ? () => this.TryBuildInstanceProducerForConcreteUnregisteredType(serviceType)
                : (Func<InstanceProducer>)(() => null);

            return this.BuildInstanceProducerForType(serviceType, context, tryBuildInstanceProducerForConcrete);
        }

        private InstanceProducer BuildInstanceProducerForType(Type serviceType, InjectionConsumerInfo context,
            Func<InstanceProducer> tryBuildInstanceProducerForConcreteType)
        {
            return
                this.TryBuildInstanceProducerThroughUnregisteredTypeResolution(serviceType, context) ??
                this.TryBuildArrayInstanceProducer(serviceType) ??
                this.TryBuildInstanceProducerForCollection(serviceType) ??
                tryBuildInstanceProducerForConcreteType();
        }

        private InstanceProducer TryBuildInstanceProducerThroughUnregisteredTypeResolution(Type serviceType,
            InjectionConsumerInfo context)
        {
            // Instead of wrapping the complete method in a lock, we lock inside the individual methods. We 
            // don't want to hold a lock while calling back into user code, because who knows what the user 
            // is doing there. We don't want a dead lock.
            return this.TryGetInstanceProducerForUnregisteredTypeResolutionFromCache(serviceType)
                ?? this.TryGetInstanceProducerThroughResolveUnregisteredTypeEvent(serviceType);
        }

        private InstanceProducer TryGetInstanceProducerForUnregisteredTypeResolutionFromCache(Type serviceType)
        {
            lock (this.resolveUnregisteredTypeRegistrations)
            {
                return this.resolveUnregisteredTypeRegistrations.ContainsKey(serviceType)
                    ? this.resolveUnregisteredTypeRegistrations[serviceType]
                    : null;
            }
        }

        private InstanceProducer TryGetInstanceProducerThroughResolveUnregisteredTypeEvent(Type serviceType)
        {
            UnregisteredTypeEventArgs e = null;

            if (this.resolveUnregisteredType != null)
            {
                e = new UnregisteredTypeEventArgs(serviceType);

                this.resolveUnregisteredType(this, e);
            }

            return e != null && e.Handled
                ? TryGetProducerFromUnregisteredTypeResolutionCacheOrAdd(e)
                : null;
        }

        private InstanceProducer TryGetProducerFromUnregisteredTypeResolutionCacheOrAdd(
            UnregisteredTypeEventArgs e)
        {
            Type serviceType = e.UnregisteredServiceType;

            var registration = e.Registration ?? new ExpressionRegistration(e.Expression, this);

            lock (this.resolveUnregisteredTypeRegistrations)
            {
                if (this.resolveUnregisteredTypeRegistrations.ContainsKey(serviceType))
                {
                    return this.resolveUnregisteredTypeRegistrations[serviceType];
                }

                // By creating the InstanceProducer after checking the dictionary, we prevent the producer
                // from being created twice when multiple threads are running. Having the same duplicate
                // producer can cause a torn lifestyle warning in the container.
                var producer = new InstanceProducer(serviceType, registration);

                this.resolveUnregisteredTypeRegistrations[serviceType] = producer;

                return producer;
            }
        }

        private InstanceProducer TryBuildArrayInstanceProducer(Type serviceType)
        {
            if (serviceType.IsArray)
            {
                Type elementType = serviceType.GetElementType();

                // We don't auto-register collections for ambiguous types.
                if (elementType.IsValueType || Helpers.IsAmbiguousType(elementType))
                {
                    return null;
                }

                bool isContainerControlledCollection =
                    this.GetAllInstances(elementType) is IContainerControlledCollection;

                if (isContainerControlledCollection)
                {
                    return this.BuildArrayProducerFromControlledCollection(serviceType, elementType);
                }
                else
                {
                    return this.BuildArrayProducerFromUncontrolledCollection(serviceType, elementType);
                }
            }

            return null;
        }

        private InstanceProducer BuildArrayProducerFromControlledCollection(Type serviceType, Type elementType)
        {
            var arrayMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(elementType);

            IEnumerable<object> singletonCollection = this.GetAllInstances(elementType);

            var collectionExpression = Expression.Constant(
                singletonCollection,
                typeof(IEnumerable<>).MakeGenericType(elementType));

            // Enumerable.ToArray(collection)
            var arrayExpression = Expression.Call(arrayMethod, collectionExpression);

            Registration registration =
                new ExpressionRegistration(arrayExpression, serviceType, Lifestyle.Transient, this);

            var producer = new InstanceProducer(serviceType, registration);

            if (!singletonCollection.Any())
            {
                producer.IsContainerAutoRegistered = true;
            }

            return producer;
        }

        private InstanceProducer BuildArrayProducerFromUncontrolledCollection(Type serviceType, Type elementType)
        {
            var arrayMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(elementType);

            var enumerableProducer = this.GetRegistration(typeof(IEnumerable<>).MakeGenericType(elementType));
            var enumerableExpression = enumerableProducer.BuildExpression();

            var arrayExpression = Expression.Call(arrayMethod, enumerableExpression);

            Registration registration =
                new ExpressionRegistration(arrayExpression, serviceType, Lifestyle.Transient, this);

            var producer = new InstanceProducer(serviceType, registration);

            producer.IsContainerAutoRegistered = true;

            return producer;
        }

        private InstanceProducer TryBuildInstanceProducerForCollection(Type serviceType)
        {
            if (!IsGenericCollectionType(serviceType))
            {
                return null;
            }

            lock (this.emptyAndRedirectedCollectionRegistrationCache)
            {
                InstanceProducer producer;

                // We need to cache these generated producers, to prevent getting duplicate producers; which
                // will cause (incorrect) diagnostic warnings.
                if (!this.emptyAndRedirectedCollectionRegistrationCache.TryGetValue(serviceType, out producer))
                {
                    producer = this.TryBuildCollectionInstanceProducer(serviceType)
                        ?? this.TryBuildEmptyCollectionInstanceProducerForEnumerable(serviceType);

                    this.emptyAndRedirectedCollectionRegistrationCache[serviceType] = producer;
                }

                return producer;
            }
        }

        private static bool IsGenericCollectionType(Type serviceType)
        {
            if (!serviceType.IsGenericType)
            {
                return false;
            }

            Type[] arguments = serviceType.GetGenericArguments();

            // IEnumerable<T>, IList<T>, ICollection<T>, IReadOnlyCollection<T> and IReadOnlyList<T> are supported.
            if (serviceType.ContainsGenericParameters || arguments.Length != 1)
            {
                return false;
            }

            Type elementType = arguments.First();

            // We don't auto-register collections for ambiguous types.
            if (elementType.IsValueType || Helpers.IsAmbiguousType(elementType))
            {
                return false;
            }

            return true;
        }

        private InstanceProducer TryBuildCollectionInstanceProducer(Type serviceType)
        {
            Type serviceTypeDefinition = serviceType.GetGenericTypeDefinition();

            if (
#if NET45
                serviceTypeDefinition == typeof(IReadOnlyList<>) ||
                serviceTypeDefinition == typeof(IReadOnlyCollection<>) ||
#endif
                serviceTypeDefinition == typeof(IList<>) ||
                serviceTypeDefinition == typeof(ICollection<>))
            {
                Type elementType = serviceType.GetGenericArguments()[0];

                var collection = this.GetAllInstances(elementType) as IContainerControlledCollection;

                if (collection != null)
                {
                    var registration = SingletonLifestyle.CreateSingleInstanceRegistration(serviceType, collection, this);

                    var producer = new InstanceProducer(serviceType, registration);

                    if (!((IEnumerable<object>)collection).Any())
                    {
                        producer.IsContainerAutoRegistered = true;
                    }

                    return producer;
                }
            }

            return null;
        }

        private InstanceProducer TryBuildEmptyCollectionInstanceProducerForEnumerable(Type serviceType)
        {
            if (!this.Options.ResolveUnregisteredCollections)
            {
                return null;
            }

            if (serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                // During the time that this method is called we are after the registration phase and there is
                // no registration for this IEnumerable<T> type (and unregistered type resolution didn't pick
                // it up). This means that we will must always return an empty set and we will do this by
                // registering a SingletonInstanceProducer with an empty array of that type.
                var producer = this.BuildEmptyCollectionInstanceProducerForEnumerable(serviceType);

                producer.IsContainerAutoRegistered = true;

                return producer;
            }

            return null;
        }

        private InstanceProducer BuildEmptyCollectionInstanceProducerForEnumerable(Type enumerableType)
        {
            Type elementType = enumerableType.GetGenericArguments()[0];

            var collection = DecoratorHelpers.CreateContainerControlledCollection(elementType, this);

            var registration = new ExpressionRegistration(Expression.Constant(collection, enumerableType), this);

            // Producers for ExpressionRegistration are normally ignored as external producer, but in this
            // case the empty collection producer should pop up in the list of GetCurrentRegistrations().
            return new InstanceProducer(enumerableType, registration, registerExternalProducer: true);
        }

        private InstanceProducer TryBuildInstanceProducerForConcreteUnregisteredType<TConcrete>()
            where TConcrete : class
        {
            if (this.IsConcreteConstructableType(typeof(TConcrete)))
            {
                return this.GetOrBuildInstanceProducerForConcreteUnregisteredType(typeof(TConcrete), () =>
                {
                    var registration =
                        this.SelectionBasedLifestyle.CreateRegistration<TConcrete, TConcrete>(this);

                    return BuildInstanceProducerForConcreteUnregisteredType(typeof(TConcrete), registration);
                });
            }

            return null;
        }

        private InstanceProducer TryBuildInstanceProducerForConcreteUnregisteredType(Type concreteType)
        {
            if (!concreteType.IsValueType && !concreteType.ContainsGenericParameters &&
                this.IsConcreteConstructableType(concreteType))
            {
                return this.GetOrBuildInstanceProducerForConcreteUnregisteredType(concreteType, () =>
                {
                    var registration =
                        this.SelectionBasedLifestyle.CreateRegistration(concreteType, concreteType, this);

                    return BuildInstanceProducerForConcreteUnregisteredType(concreteType, registration);
                });
            }

            return null;
        }

        private InstanceProducer GetOrBuildInstanceProducerForConcreteUnregisteredType(Type concreteType,
            Func<InstanceProducer> instanceProducerBuilder)
        {
            // We need to take a lock here to make sure that we never create multiple InstanceProducer
            // instances for the same concrete type, which is a problem when the LifestyleSelectionBehavior
            // has been overridden. For instance in case the overridden behavior returns a Singleton lifestyle,
            // but the concrete type is requested concurrently over multiple threads, not taking the lock
            // could cause two InstanceProducers to be created, which might cause two instances from being
            // created.
            lock (this.unregisteredConcreteTypeInstanceProducers)
            {
                InstanceProducer producer;

                if (!this.unregisteredConcreteTypeInstanceProducers.TryGetValue(concreteType, out producer))
                {
                    producer = instanceProducerBuilder.Invoke();

                    this.unregisteredConcreteTypeInstanceProducers[concreteType] = producer;
                }

                return producer;
            }
        }

        private static InstanceProducer BuildInstanceProducerForConcreteUnregisteredType(Type concreteType,
            Registration registration)
        {
            var producer = new InstanceProducer(concreteType, registration);

            producer.EnsureTypeWillBeExplicitlyVerified();

            // Flag that this producer is resolved by the container or using unregistered type resolution.
            producer.IsContainerAutoRegistered = true;

            return producer;
        }

        private bool IsConcreteConstructableType(Type concreteType)
        {
            string errorMesssage;

            return this.Options.IsConstructableType(concreteType, concreteType, out errorMesssage);
        }

        // We're registering a service type after 'locking down' the container here and that means that the
        // type is added to a copy of the registrations dictionary and the original replaced with a new one.
        // This 'reference swapping' is thread-safe, but can result in types disappearing again from the 
        // registrations when multiple threads simultaneously add different types. This however, does not
        // result in a consistency problem, because the missing type will be again added later. This type of
        // swapping safes us from using locks.
        private void AppendRootInstanceProducer(Type serviceType, InstanceProducer rootProducer)
        {
            var snapshotCopy = this.rootProducerCache.MakeCopy();

            // This registration might already exist if it was added made by another thread. That's why we
            // need to use the indexer, instead of Add.
            snapshotCopy[serviceType] = rootProducer;

            // Prevent the compiler, JIT, and processor to reorder these statements to prevent the instance
            // producer from being added after the snapshot has been made accessible to other threads.
            // This is important on architectures with a weak memory model (such as ARM).
            Thread.MemoryBarrier();

            // Replace the original with the new version that includes the serviceType (make snapshot public).
            this.rootProducerCache = snapshotCopy;

            if (rootProducer != null)
            {
                this.RemoveExternalProducer(rootProducer);
            }
        }

        private void ThrowInvalidRegistrationException(Type serviceType, InstanceProducer producer)
        {
            if (producer != null)
            {
                throw producer.Exception;
            }
            else
            {
                this.ThrowMissingInstanceProducerException(serviceType);
            }
        }

        private void ThrowMissingInstanceProducerException(Type serviceType)
        {
            if (Helpers.IsConcreteConstructableType(serviceType))
            {
                this.ThrowNotConstructableException(serviceType);
            }

            if (serviceType.ContainsGenericParameters)
            {
                throw new ActivationException(StringResources.OpenGenericTypesCanNotBeResolved(serviceType));
            }

            throw new ActivationException(StringResources.NoRegistrationForTypeFound(
                serviceType, 
                this.HasRegistrations,
                this.ContainsOneToOneRegistrationForCollectionType(serviceType),
                this.ContainsCollectionRegistrationFor(serviceType)));
        }

        private bool ContainsOneToOneRegistrationForCollectionType(Type collectionServiceType) =>
            IsGenericCollectionType(collectionServiceType) && 
                this.ContainsExplicitRegistrationFor(collectionServiceType.GetGenericArguments()[0]);

        // NOTE: MakeGenericType will fail for IEnumerable<T> when T is a pointer.
        private bool ContainsCollectionRegistrationFor(Type serviceType) =>
            !IsGenericCollectionType(serviceType) && !serviceType.IsPointer &&
                this.ContainsExplicitRegistrationFor(typeof(IEnumerable<>).MakeGenericType(serviceType));

        private bool ContainsExplicitRegistrationFor(Type serviceType) =>
            this.GetRegistrationEvenIfInvalid(serviceType, InjectionConsumerInfo.Root, false) != null;

        private void ThrowNotConstructableException(Type concreteType)
        {
            string exceptionMessage;

            // Since we are at this point, we know the concreteType is NOT constructable.
            this.Options.IsConstructableType(concreteType, concreteType, out exceptionMessage);

            throw new ActivationException(
                StringResources.ImplicitRegistrationCouldNotBeMadeForType(concreteType, this.HasRegistrations)
                + " " + exceptionMessage);
        }
    }
}