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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using SimpleInjector.Advanced;
    using SimpleInjector.Extensions.Decorators;
    using SimpleInjector.Lifestyles;

#if !PUBLISH
    /// <summary>Methods for resolving instances.</summary>
#endif
    public partial class Container : IServiceProvider
    {
        /// <summary>Gets an instance of the given <typeparamref name="TService"/>.</summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <returns>The requested service instance.</returns>
        /// <exception cref="ActivationException">Thrown when there are errors resolving the service instance.</exception>
        public TService GetInstance<TService>() where TService : class
        {
            // Performance optimization: This if check is a duplicate to save a call to LockContainer.
            if (!this.locked)
            {
                this.LockContainer();
            }

            InstanceProducer instanceProducer;

            object instance;

            // Performance optimization: This if check is a duplicate to save a call to GetInstanceForType.
            if (!this.registrations.TryGetValue(typeof(TService), out instanceProducer))
            {
                instance = this.GetInstanceForType<TService>();
            }
            else
            {
                if (instanceProducer == null)
                {
                    this.ThrowMissingInstanceProducerException(typeof(TService));
                }

                instance = instanceProducer.GetInstance();
            }

            return (TService)instance;
        }

        /// <summary>Gets an instance of the given <paramref name="serviceType"/>.</summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <returns>The requested service instance.</returns>
        /// <exception cref="ActivationException">Thrown when there are errors resolving the service instance.</exception>
        public object GetInstance(Type serviceType)
        {
            if (!this.locked)
            {
                this.LockContainer();
            }

            InstanceProducer instanceProducer;

            if (!this.registrations.TryGetValue(serviceType, out instanceProducer))
            {
                return this.GetInstanceForType(serviceType);
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
            if (!this.locked)
            {
                this.LockContainer();
            }

            InstanceProducer instanceProducer;

            if (!this.registrations.TryGetValue(serviceType, out instanceProducer))
            {
                instanceProducer = (InstanceProducer)this.GetRegistration(serviceType);
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
            return this.GetRegistration(serviceType, throwOnFailure, autoCreateConcreteTypes: true);
        }

        /// <summary>
        /// Injects all public writable properties of the given <paramref name="instance"/> that have a type
        /// that can be resolved by this container instance.
        /// <b>NOTE:</b> This method will be removed in a future release. To use property injection,
        /// implement a custom the <see cref="IPropertySelectionBehavior"/> instead. For more information,
        /// read the 
        /// <a href="https://simpleinjector.org/xtppr">extendibility points</a> wiki.
        /// </summary>
        /// <param name="instance">The instance whose properties will be injected.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="instance"/> is null (Nothing in VB).</exception>
        /// <exception cref="ActivationException">Throw when injecting properties on the given instance
        /// failed due to security constraints of the sandbox. This can happen when injecting properties
        /// on an internal type in a Silverlight sandbox, or when running in partial trust.</exception>
        [Obsolete("Container.InjectProperties has been deprecated and will be removed in a future release. " +
            "See https://simpleinjector.org/depr1.", error: false)]
        public void InjectProperties(object instance)
        {
            Requires.IsNotNull(instance, "instance");

            PropertyInjector propertyInjector;

            if (!this.propertyInjectorCache.TryGetValue(instance.GetType(), out propertyInjector))
            {
                propertyInjector = new PropertyInjector(this, instance.GetType());

                this.RegisterPropertyInjector(propertyInjector);
            }

            propertyInjector.Inject(instance);
        }

        internal InstanceProducer GetRegistration(Type serviceType, bool throwOnFailure,
            bool autoCreateConcreteTypes)
        {
            if (!this.locked)
            {
                this.LockContainer();
            }

            var producer = this.GetRegistrationEvenIfInvalid(serviceType, autoCreateConcreteTypes);

            bool producerIsValid = producer != null && producer.IsValid;

            if (!producerIsValid && throwOnFailure)
            {
                this.ThrowInvalidRegistrationException(serviceType, producer);
            }

            // Prevent returning invalid producers
            return producerIsValid ? producer : null;
        }

        internal Action<TImplementation> GetInitializer<TImplementation>(InitializationContext context)
        {
            return this.GetInitializer<TImplementation>(typeof(TImplementation), context);
        }

        internal Action<object> GetInitializer(Type implementationType, InitializationContext context)
        {
            return this.GetInitializer<object>(implementationType, context);
        }

        internal InstanceProducer GetRegistrationEvenIfInvalid(Type serviceType, 
            bool autoCreateConcreteTypes = true)
        {
            // This Func<T> is a bit ugly, but does save us a lot of duplicate code.
            Func<InstanceProducer> buildProducer =
                () => this.BuildInstanceProducerForType(serviceType, autoCreateConcreteTypes);

            return this.GetInstanceProducerForType(serviceType, buildProducer);
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

        private void RegisterPropertyInjector(PropertyInjector injector)
        {
            var copy = this.propertyInjectorCache.MakeCopy();

            copy[injector.Type] = injector;

            // Prevent the compiler, JIT, and processor to reorder these statements to prevent the instance
            // producer from being added after the snapshot has been made accessible to other threads.
            Thread.MemoryBarrier();

            // Replace the original with the new version that includes the serviceType.
            this.propertyInjectorCache = copy;
        }

        private InstanceProducer GetInstanceProducerForType<TService>() where TService : class
        {
            Func<InstanceProducer> buildProducer = () => this.BuildInstanceProducerForType<TService>();
            return this.GetInstanceProducerForType(typeof(TService), buildProducer);
        }

        private InstanceProducer GetInstanceProducerForType(Type serviceType)
        {
            Func<InstanceProducer> buildProducer = () => this.BuildInstanceProducerForType(serviceType);
            return this.GetInstanceProducerForType(serviceType, buildProducer);
        }

        // Instead of using the this.registrations instance, this method takes a snapshot. This allows the
        // container to be thread-safe, without using locks.
        private InstanceProducer GetInstanceProducerForType(Type serviceType,
            Func<InstanceProducer> buildInstanceProducer)
        {
            InstanceProducer instanceProducer = null;

            if (!this.registrations.TryGetValue(serviceType, out instanceProducer))
            {
                var producer = buildInstanceProducer();

                // Always register the producer, even if it is null. This improves performance for the
                // GetService and GetRegistration methods.
                this.RegisterInstanceProducer(serviceType, producer);

                return producer;
            }

            return instanceProducer;
        }

        private object GetInstanceForType<TService>() where TService : class
        {
            InstanceProducer producer = this.GetInstanceProducerForType<TService>();
            return this.GetInstanceFromProducer(producer, typeof(TService));
        }

        private object GetInstanceForType(Type serviceType)
        {
            InstanceProducer producer = this.GetInstanceProducerForType(serviceType);
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

            throw new ActivationException(StringResources.NoRegistrationForTypeFound(serviceType));
        }

        private void ThrowNotConstructableException(Type concreteType)
        {
            string exceptionMessage;

            // Since we are at this point, we know the concreteType is NOT constructable.
            this.IsConstructableType(concreteType, concreteType, out exceptionMessage);

            throw new ActivationException(
                StringResources.ImplicitRegistrationCouldNotBeMadeForType(concreteType) + " " + 
                exceptionMessage);
        }

        private InstanceProducer BuildInstanceProducerForType<TService>() where TService : class
        {
            Func<InstanceProducer> buildInstanceProducerForConcreteType =
                () => this.TryBuildInstanceProducerForConcreteUnregisteredType<TService>();

            return this.BuildInstanceProducerForType(typeof(TService), buildInstanceProducerForConcreteType);
        }

        private InstanceProducer BuildInstanceProducerForType(Type serviceType, 
            bool autoCreateConcreteTypes = true)
        {
            Func<InstanceProducer> tryBuildInstanceProducerForConcreteType = () => null;

            if (autoCreateConcreteTypes)
            {
                tryBuildInstanceProducerForConcreteType =
                    () => this.TryBuildInstanceProducerForConcreteUnregisteredType(serviceType);
            }

            return this.BuildInstanceProducerForType(serviceType, tryBuildInstanceProducerForConcreteType);
        }

        private InstanceProducer BuildInstanceProducerForType(Type serviceType,
            Func<InstanceProducer> tryBuildInstanceProducerForConcreteType)
        {
            return
                this.TryBuildInstanceProducerThroughUnregisteredTypeResolution(serviceType) ??
                this.TryBuildInstanceProducerForCollection(serviceType) ??
                tryBuildInstanceProducerForConcreteType();
        }

        private InstanceProducer TryBuildInstanceProducerThroughUnregisteredTypeResolution(Type serviceType)
        {
            var e = new UnregisteredTypeEventArgs(serviceType);

            if (this.resolveUnregisteredType != null)
            {
                this.resolveUnregisteredType(this, e);
            }

            if (e.Handled)
            {
                var registration = e.Registration ?? new ExpressionRegistration(e.Expression, this);

                return new InstanceProducer(serviceType, registration);
            }
            else
            {
                return null;
            }
        }

        private InstanceProducer TryBuildInstanceProducerForCollection(Type serviceType)
        {
            if (!serviceType.IsGenericType)
            {
                return null;
            }

            Type[] arguments = serviceType.GetGenericArguments();

            // IEnumerable<T>, IList<T>, ICollection<T>, IReadOnlyCollection<T> and IReadOnlyList<T> are supported.
            if (serviceType.ContainsGenericParameters || arguments.Length != 1)
            {
                return null;
            }

            Type elementType = arguments.First();

            // We don't auto-register collections for ambiguous types.
            if (elementType.IsValueType || Helpers.IsAmbiguousType(elementType))
            {
                return null;
            }

            return this.TryBuildCollectionInstanceProducer(serviceType) 
                ?? this.TryBuildEmptyCollectionInstanceProducerForEnumerable(serviceType);
        }

        private InstanceProducer TryBuildEmptyCollectionInstanceProducerForEnumerable(Type serviceType)
        {
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

            var collection = DecoratorHelpers.CreateContainerControlledCollection(elementType, this, new Type[0]);

            var registration = new ExpressionRegistration(Expression.Constant(collection, enumerableType), this);

            return new InstanceProducer(enumerableType, registration);
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
                    var registration = SingletonLifestyle.CreateSingleRegistration(serviceType, collection, this);

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

        private InstanceProducer TryBuildInstanceProducerForConcreteUnregisteredType<TConcrete>()
            where TConcrete : class
        {
            if (this.IsConcreteConstructableType(typeof(TConcrete)))
            {
                var registration = this.SelectionBasedLifestyle.CreateRegistration<TConcrete, TConcrete>(this);

                return BuildInstanceProducerForConcreteUnregisteredType(typeof(TConcrete), registration);
            }

            return null;
        }

        private InstanceProducer TryBuildInstanceProducerForConcreteUnregisteredType(Type concreteType)
        {
            if (!concreteType.IsValueType && !concreteType.ContainsGenericParameters &&
                this.IsConcreteConstructableType(concreteType))
            {
                var registration = 
                    this.SelectionBasedLifestyle.CreateRegistration(concreteType, concreteType, this);

                return BuildInstanceProducerForConcreteUnregisteredType(concreteType, registration);
            }

            return null;
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

            return this.IsConstructableType(concreteType, concreteType, out errorMesssage);
        }

        // We're registering a service type after 'locking down' the container here and that means that the
        // type is added to a copy of the registrations dictionary and the original replaced with a new one.
        // This 'reference swapping' is thread-safe, but can result in types disappearing again from the 
        // registrations when multiple threads simultaneously add different types. This however, does not
        // result in a consistency problem, because the missing type will be again added later. This type of
        // swapping safes us from using locks.
        private void RegisterInstanceProducer(Type serviceType, InstanceProducer instanceProducer)
        {
            var snapshotCopy = this.registrations.MakeCopy();

            // This registration might already exist if it was added made by another thread. That's why we
            // need to use the indexer, instead of Add.
            snapshotCopy[serviceType] = instanceProducer;
            
            // Prevent the compiler, JIT, and processor to reorder these statements to prevent the instance
            // producer from being added after the snapshot has been made accessible to other threads.
            Thread.MemoryBarrier();

            // Replace the original with the new version that includes the serviceType (make snapshot public).
            this.registrations = snapshotCopy;

            if (instanceProducer != null)
            {
                this.RemoveExternalProducer(instanceProducer);
            }
        }
    }
}