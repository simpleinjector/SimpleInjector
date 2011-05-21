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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SimpleInjector
{
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

            // Performance optimization: This if check is a duplicate to save a call to GetInstanceForType.
            if (!this.registrations.TryGetValue(typeof(TService), out instanceProducer))
            {
                return (TService)this.GetInstanceForType<TService>();
            }

            return (TService)instanceProducer.GetInstance();
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
            if (!this.locked)
            {
                this.LockContainer();
            }

            return GetAllInstancesInternal<TService>();
        }

        /// <summary>
        /// Gets all instances of the given <paramref name="serviceType"/> currently registered in the container.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <returns>A sequence of instances of the requested serviceType.</returns>
        /// <exception cref="ActivationException">Thrown when there are errors resolving the service instance.</exception>
        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            if (!this.locked)
            {
                this.LockContainer();
            }

            return this.GetAllInstancesInternal(serviceType);
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

            var snapshot = this.unavailableTypes;

            if (snapshot.ContainsKey(serviceType))
            {
                return null;
            }

            IInstanceProducer instanceProducer;

            if (this.registrations.TryGetValue(serviceType, out instanceProducer))
            {
                return instanceProducer.GetInstance();
            }

            if (!serviceType.IsValueType)
            {
                instanceProducer = this.GetRegistration(serviceType);
            }

            if (instanceProducer != null)
            {
                // We create the instance AFTER registering the instance producer. Calling registration
                // after creating it, could make us loose all registrations that are done by GetInstance.
                try
                {
                    return instanceProducer.GetInstance();
                }
                catch (ActivationException)
                {
                    if (instanceProducer.GetType().Name == "TransientInstanceProducer`1")
                    {
                        // When we get here, the user requested an (unregistered) concrete and onconstructable
                        // type (i.e. System.String). In that case this method should not fail, but return
                        // null.
                    }
                    else
                    {
                        // When we get here, the user requested an (unregistered) type and it has been
                        // resolved by unregistered type resolution, but the registered creation delegate
                        // failed. In that case we should not suppress the error, and let it bubble up.
                        throw;
                    }
                }
            }

            // Register the current serviceType as unavailable type to prevent a major performance issue.
            // The GetInstanceProducerForType method would get called on every request for this serviceType.
            this.AddToUnavailableTypes(serviceType, snapshot);

            // The contract of the IServiceProvider dictates to return null when no registration exists.
            return null;
        }

        /// <summary>
        /// Gets the <see cref="IInstanceProducer"/> for the given <paramref name="serviceType"/>. When no
        /// registration exists, the container will try creating a new producer. A producer can be created
        /// when the type is a concrete reference type, there is an <see cref="ResolveUnregisteredType"/>
        /// event registered that acts on that type, or when the service type is an <see cref="IEnumerable{T}"/>.
        /// Otherwise <b>null</b> (Nothing in VB) is returned.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> that the returned instance producer should produce.</param>
        /// <returns>An <see cref="IInstanceProducer"/> or <b>null</b> (Nothing in VB).</returns>
        public IInstanceProducer GetRegistration(Type serviceType)
        {
            Func<IInstanceProducer> buildProducer = () => this.BuildInstanceProducerForType(serviceType);
            return this.GetInstanceProducerForType(serviceType, buildProducer);
        }

        private IInstanceProducer GetInstanceProducerForType<TService>() where TService : class
        {
            Func<IInstanceProducer> buildProducer = () => this.BuildInstanceProducerForType<TService>();
            return this.GetInstanceProducerForType(typeof(TService), buildProducer);
        }

        // Instead of using the this.registrations instance, this method takes a snapshot. This allows the
        // container to be thread-safe, without using locks.
        private IInstanceProducer GetInstanceProducerForType(Type serviceType,
            Func<IInstanceProducer> buildInstanceProducer)
        {
            IInstanceProducer instanceProducer;

            var snapshot = this.Registrations;

            if (!snapshot.TryGetValue(serviceType, out instanceProducer))
            {
                instanceProducer = buildInstanceProducer();

                if (instanceProducer != null)
                {
                    this.RegisterInstanceProducer(instanceProducer, serviceType, snapshot);
                }
            }

            return instanceProducer;
        }

        private IEnumerable<TService> GetAllInstancesInternal<TService>()
        {
            IInstanceProducer instanceProducer;

            if (!this.registrations.TryGetValue(typeof(IEnumerable<TService>), out instanceProducer))
            {
                return Enumerable.Empty<TService>();
            }

            return (IEnumerable<TService>)instanceProducer.GetInstance();
        }

        private IEnumerable<object> GetAllInstancesInternal(Type serviceType)
        {
            IInstanceProducer instanceProducer;

            Type collectionType = typeof(IEnumerable<>).MakeGenericType(serviceType);

            if (!this.registrations.TryGetValue(collectionType, out instanceProducer))
            {
                return Enumerable.Empty<object>();
            }

            IEnumerable registeredCollection = (IEnumerable)instanceProducer.GetInstance();

            return registeredCollection.Cast<object>();
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
                throw new ActivationException(StringResources.NoRegistrationForTypeFound(serviceType));
            }

            // We create the instance AFTER registering the instance producer. Registering the producer after
            // creating an instance, could make us loose all registrations that are done by GetInstance. This
            // will not have any functional effects, but can result in a performance penalty.
            return instanceProducer.GetInstance();
        }

        private IInstanceProducer BuildInstanceProducerForType<TService>() where TService : class
        {
            Func<IInstanceProducer> buildInstanceProducerForConcreteType =
                () => this.BuildInstanceProducerForConcreteType<TService>();

            return this.BuildInstanceProducerForType(typeof(TService), buildInstanceProducerForConcreteType);
        }

        private IInstanceProducer BuildInstanceProducerForType(Type serviceType)
        {
            Func<IInstanceProducer> buildInstanceProducerForConcreteType =
                () => this.BuildInstanceProducerForConcreteType(serviceType);

            return this.BuildInstanceProducerForType(serviceType, buildInstanceProducerForConcreteType);
        }

        private IInstanceProducer BuildInstanceProducerForType(Type serviceType,
            Func<IInstanceProducer> buildInstanceProducerForConcreteType)
        {
            IInstanceProducer instanceProducer =
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

        private IInstanceProducer BuildInstanceProducerThroughUnregisteredTypeResolution(Type serviceType)
        {
            var e = new UnregisteredTypeEventArgs(serviceType);

            this.OnResolveUnregisteredType(e);

            if (e.Handled)
            {
                var instanceProducerType = typeof(ResolutionInstanceProducer<>).MakeGenericType(serviceType);

                return (IInstanceProducer)Activator.CreateInstance(instanceProducerType, e.InstanceCreator);
            }
            else
            {
                return null;
            }
        }

        private static IInstanceProducer BuildInstanceProducerForCollection(Type serviceType)
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

        private static IInstanceProducer BuildEmptyCollectionInstanceProducer(Type enumerableType)
        {
            Type elementType = enumerableType.GetGenericArguments()[0];

            var emptyArray = Array.CreateInstance(elementType, 0);

            var instanceProducerType = typeof(SingletonInstanceProducer<>).MakeGenericType(enumerableType);

            return (IInstanceProducer)Activator.CreateInstance(instanceProducerType, emptyArray);
        }

        private IInstanceProducer BuildInstanceProducerForConcreteType<TService>() where TService : class
        {
            // NOTE: We don't check if the type is actually constructable. The TransientInstanceProducer will
            // do that by the time GetInstance is called for the first time on it.
            if (Helpers.IsConcreteType(typeof(TService)))
            {
                return TransientInstanceProducer<TService>.Create(this, typeof(TService));
            }
            else
            {
                return null;
            }
        }

        private IInstanceProducer BuildInstanceProducerForConcreteType(Type serviceType)
        {
            // NOTE: We don't check if the type is actually constructable. The TransientInstanceProducer will
            // do that by the time GetInstance is called for the first time on it.
            if (Helpers.IsConcreteType(serviceType) && !serviceType.IsValueType)
            {
                return Helpers.CreateTransientInstanceProducerFor(serviceType, this);
            }
            else
            {
                return null;
            }
        }

        private void OnResolveUnregisteredType(UnregisteredTypeEventArgs e)
        {
            var handler = this.resolveUnregisteredType;

            if (handler == null)
            {
                return;
            }

            handler(this, e);
        }

        // We're registering a service type after 'locking down' the container here and that means that the
        // type is added to a copy of the registrations dictionary and the original replaced with a new one.
        // This 'reference swapping' is thread-safe, but can result in types disappearing again from the 
        // registrations when multiple threads simultaneously add different types. This however, does not
        // result in a consistency problem, because the missing type will be again added later. This type of
        // swapping safes us from using locks.
        private void RegisterInstanceProducer(IInstanceProducer instanceProducer, Type serviceType,
            Dictionary<Type, IInstanceProducer> snapshot)
        {
            var snapshotCopy = Helpers.MakeCopyOf(snapshot);

            snapshotCopy.Add(serviceType, instanceProducer);

            // Replace the original with the new version that includes the serviceType.
            this.registrations = snapshotCopy;
        }

        // Here we do our reference swapping trick as well.
        private void AddToUnavailableTypes(Type serviceType, Dictionary<Type, bool> snapshot)
        {
            var snapshotCopy = new Dictionary<Type, bool>(snapshot);

            snapshotCopy.Add(serviceType, false);

            // Replace the original with the new version that includes the serviceType.
            this.unavailableTypes = snapshotCopy;
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