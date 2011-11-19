#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2010 S. van Deursen
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

namespace SimpleInjector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using SimpleInjector.InstanceProducers;

#if DEBUG
    /// <summary>
    /// Methods for resolving instances.
    /// </summary>
#endif
    public partial class Container : IServiceProvider
    {
        /// <summary>Gets an instance of the given <typeparamref name="TService"/>.</summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <returns>The requested service instance.</returns>
        /// <exception cref="ActivationException">Thrown when there are errors resolving the service instance.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "This class already contains an overload that takes a Type.")]
        public TService GetInstance<TService>() where TService : class
        {
            // Performance optimization: This if check is a duplicate to save a call to LockContainer.
            if (!this.locked)
            {
                this.LockContainer();
            }

            IInstanceProducer instanceProducer;

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
                    ThrowMissingInstanceProducerException(typeof(TService));
                }

                instance = instanceProducer.GetInstance();
            }

            try
            {
                return (TService)instance;
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    StringResources.DelegateRegisteredUsingResolveUnregisteredTypeReturnedAnUnassignableFrom(typeof(TService),
                    instance.GetType()), ex);
            }
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

            IInstanceProducer instanceProducer;

            if (!this.registrations.TryGetValue(serviceType, out instanceProducer))
            {
                return this.GetInstanceForType(serviceType);
            }

            if (instanceProducer == null)
            {
                ThrowMissingInstanceProducerException(serviceType);
            }

            return instanceProducer.GetInstance();
        }

        /// <summary>
        /// Gets all instances of the given <typeparamref name="TService"/> currently registered in the container.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <returns>A sequence of instances of the requested TService.</returns>
        /// <exception cref="ActivationException">Thrown when there are errors resolving the service instance.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "This class already contains an overload that takes a Type.")]
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

            var collection = (IEnumerable)this.GetInstance(collectionType);

            return collection.Cast<object>();
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

            IInstanceProducer instanceProducer;

            if (!this.registrations.TryGetValue(serviceType, out instanceProducer))
            {
                instanceProducer = (InstanceProducer)this.GetRegistration(serviceType);
            }

            if (instanceProducer != null)
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
        /// A call to this method locks the container. No new registrations can be made after a call to this 
        /// method.
        /// </para>
        /// <para>
        /// <b>Note:</b> This method is <i>not</i> guaranteed to always return the same <b>IInstanceProducer</b>
        /// instance for a given <see cref="Type"/>. It will however either always return <b>null</b> or
        /// always return a producer that is able to return the expected instance.
        /// </para>
        /// </remarks>
        /// <param name="serviceType">The <see cref="Type"/> that the returned instance producer should produce.</param>
        /// <returns>An <see cref="InstanceProducer"/> or <b>null</b> (Nothing in VB).</returns>
        public IInstanceProducer GetRegistration(Type serviceType)
        {
            // Performance optimization: This if check is a duplicate to save a call to LockContainer.
            if (!this.locked)
            {
                // We must lock, because not locking could lead to race conditions.
                this.LockContainer();
            }

            // This Func<T> is a bit ugly, but does save us a lot of duplicate code.
            Func<InstanceProducer> buildProducer = () => this.BuildInstanceProducerForType(serviceType);

            return this.GetInstanceProducerForType(serviceType, buildProducer);
        }

        /// <summary>
        /// Injects all public writable properties of the given <paramref name="instance"/> that have a type
        /// that can be resolved by this container instance.
        /// </summary>
        /// <param name="instance">The instance whos properties will be injected.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="instance"/> is null (Nothing in VB).</exception>
        /// <exception cref="ActivationException">Throw when injecting properties on the given instance
        /// failed due to security constraints of the sandbox. This can happen when injecting properties
        /// on an internal type in a Silverlight sandbox, or when running in partial trust.</exception>
        public void InjectProperties(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            
            var snapshot = this.propertyInjectorCache;

            PropertyInjector propertyInjector;

            if (!snapshot.TryGetValue(instance.GetType(), out propertyInjector))
            {
                propertyInjector = new PropertyInjector(this, instance.GetType());

                this.RegisterPropertyInjector(instance.GetType(), propertyInjector, snapshot);
            }

            propertyInjector.Inject(instance);
        }

        private void RegisterPropertyInjector(Type serviceType, PropertyInjector injector,
            Dictionary<Type, PropertyInjector> snapshot)
        {
            var snapshotCopy = Helpers.MakeCopyOf(snapshot);

            snapshotCopy.Add(serviceType, injector);

            // Replace the original with the new version that includes the serviceType.
            this.propertyInjectorCache = snapshotCopy;
        }

        private IInstanceProducer GetInstanceProducerForType<TService>() where TService : class
        {
            Func<InstanceProducer> buildProducer = () => this.BuildInstanceProducerForType<TService>();
            return this.GetInstanceProducerForType(typeof(TService), buildProducer);
        }

        // Instead of using the this.registrations instance, this method takes a snapshot. This allows the
        // container to be thread-safe, without using locks.
        private IInstanceProducer GetInstanceProducerForType(Type serviceType,
            Func<InstanceProducer> buildInstanceProducer)
        {
            IInstanceProducer instanceProducer = null;

            var snapshot = this.registrations;

            if (!snapshot.TryGetValue(serviceType, out instanceProducer))
            {
                var producer = buildInstanceProducer();

                // Always register the producer, even if it is null. This improves performance for the
                // GetService and GetRegistration methods.
                this.RegisterInstanceProducer(serviceType, producer, snapshot);

                return producer;
            }

            return instanceProducer;
        }

        private object GetInstanceForType<TService>() where TService : class
        {
            IInstanceProducer producer = this.GetInstanceProducerForType<TService>();
            return GetInstanceFromProducer(producer, typeof(TService));
        }

        private object GetInstanceForType(Type serviceType)
        {
            IInstanceProducer producer = this.GetRegistration(serviceType);
            return GetInstanceFromProducer(producer, serviceType);
        }

        private static object GetInstanceFromProducer(IInstanceProducer instanceProducer, Type serviceType)
        {
            if (instanceProducer == null)
            {
                ThrowMissingInstanceProducerException(serviceType);
            }

            // We create the instance AFTER registering the instance producer. Registering the producer after
            // creating an instance, could make us loose all registrations that are done by GetInstance. This
            // will not have any functional effects, but can result in a performance penalty.
            return instanceProducer.GetInstance();
        }

        private static void ThrowMissingInstanceProducerException(Type serviceType)
        {
            if (Helpers.IsConcreteType(serviceType))
            {
                Helpers.ThrowActivationExceptionWhenTypeIsNotConstructable(serviceType);
            }

            throw new ActivationException(StringResources.NoRegistrationForTypeFound(serviceType));
        }

        private InstanceProducer BuildInstanceProducerForType<TService>() where TService : class
        {
            Func<InstanceProducer> buildInstanceProducerForConcreteType =
                () => BuildInstanceProducerForConcreteType<TService>();

            return this.BuildInstanceProducerForType(typeof(TService), buildInstanceProducerForConcreteType);
        }

        private InstanceProducer BuildInstanceProducerForType(Type serviceType)
        {
            Func<InstanceProducer> buildInstanceProducerForConcreteType =
                () => this.BuildInstanceProducerForConcreteType(serviceType);

            return this.BuildInstanceProducerForType(serviceType, buildInstanceProducerForConcreteType);
        }

        private InstanceProducer BuildInstanceProducerForType(Type serviceType,
            Func<InstanceProducer> buildInstanceProducerForConcreteType)
        {
            InstanceProducer instanceProducer =
                this.BuildInstanceProducerThroughUnregisteredTypeResolution(serviceType);

            if (instanceProducer == null)
            {
                instanceProducer = BuildInstanceProducerForCollection(serviceType);
            }

            if (instanceProducer == null)
            {
                instanceProducer = buildInstanceProducerForConcreteType();
            }

            return instanceProducer;
        }

        private InstanceProducer BuildInstanceProducerThroughUnregisteredTypeResolution(Type serviceType)
        {
            var e = new UnregisteredTypeEventArgs(serviceType);

            this.resolveUnregisteredType(this, e);

            if (e.Handled)
            {
                Type instanceProducerType;

                // Either the client registered a Func<object> or a Func<{ServiceType}>.
                if (e.InstanceCreator is Func<object>)
                {
                    instanceProducerType = typeof(FuncResolutionInstanceProducer<>);
                }
                else
                {
                    instanceProducerType = typeof(ExpressionResolutionInstanceProducer<>);
                }

                return (InstanceProducer)Activator.CreateInstance(
                    instanceProducerType.MakeGenericType(serviceType), e.InstanceCreator);
            }
            else
            {
                return null;
            }
        }

        private static InstanceProducer BuildInstanceProducerForCollection(Type serviceType)
        {
            bool typeIsGenericEnumerable =
                serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>);

            if (typeIsGenericEnumerable)
            {
                // During the time that this method is called we are after the registration phase and there is
                // no registration for this IEnumerable<T> type (and unregistered type resolution didn't pick
                // it up). This means that we will must always return an empty set and we will do this by
                // registering a SingletonInstanceProducer with an empty array of that type.
                return BuildEmptyCollectionInstanceProducer(serviceType);
            }
            else
            {
                return null;
            }
        }

        private static InstanceProducer BuildEmptyCollectionInstanceProducer(Type enumerableType)
        {
            Type elementType = enumerableType.GetGenericArguments()[0];

            var emptyArray = Array.CreateInstance(elementType, 0);

            var instanceProducerType = typeof(SingletonInstanceProducer<>).MakeGenericType(enumerableType);

            return (InstanceProducer)Activator.CreateInstance(instanceProducerType, emptyArray);
        }

        private static InstanceProducer BuildInstanceProducerForConcreteType<TConcrete>() 
            where TConcrete : class
        {
            // NOTE: We don't check if the type is actually constructable. The TransientInstanceProducer will
            // do that by the time GetInstance is called for the first time on it.
            if (Helpers.IsConcreteType(typeof(TConcrete)))
            {
                return new ConcreteTransientInstanceProducer<TConcrete>();
            }
            else
            {
                return null;
            }
        }

        private InstanceProducer BuildInstanceProducerForConcreteType(Type serviceType)
        {
            if (Helpers.IsConcreteConstructableType(serviceType) && !serviceType.IsValueType)
            {
                return Helpers.CreateTransientInstanceProducerFor(serviceType, this);
            }
            else
            {
                return null;
            }
        }

        // We're registering a service type after 'locking down' the container here and that means that the
        // type is added to a copy of the registrations dictionary and the original replaced with a new one.
        // This 'reference swapping' is thread-safe, but can result in types disappearing again from the 
        // registrations when multiple threads simultaneously add different types. This however, does not
        // result in a consistency problem, because the missing type will be again added later. This type of
        // swapping safes us from using locks.
        private void RegisterInstanceProducer(Type serviceType, InstanceProducer instanceProducer,
            Dictionary<Type, IInstanceProducer> snapshot)
        {
            if (instanceProducer != null)
            {
                // Set the container (must be done before the snapshot gets public).
                instanceProducer.Container = this;
            }

            var snapshotCopy = Helpers.MakeCopyOf(snapshot);

            snapshotCopy.Add(serviceType, instanceProducer);

            // Replace the original with the new version that includes the serviceType (make snapshot public).
            this.registrations = snapshotCopy;
        }

        /// <summary>Prevents any new registrations to be made to the container.</summary>
        private void LockContainer()
        {
            // By using a lock, we have the certainty that all threads will see the new value for 'locked'
            // immediately.
            if (!this.locked)
            {
                lock (this.locker)
                {
                    this.locked = true;
                }
            }
        }
    }
}