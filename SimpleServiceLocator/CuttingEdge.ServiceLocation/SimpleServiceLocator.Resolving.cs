using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.Practices.ServiceLocation;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Methods for resolving instances.
    /// </summary>
    public partial class SimpleServiceLocator : ServiceLocatorImplBase
    {
        /// <summary>Get all instances of the given TService currently registered in the container.</summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <returns>A sequence of instances of the requested TService.</returns>
        /// <exception cref="ActivationException">
        /// Thrown when there is an error resolving the service instances.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A design without a generic TService is not possible, because this method is " +
            "overridden from the base class.")]
        public override IEnumerable<TService> GetAllInstances<TService>()
        {
            // This method is overridden to improve performance. The base method resolves collections in a
            // non-generic way, which caused us to use reflection to retrieve the correct type. Because this
            // overload is also used during automatic constructor injection (through the DelegateBuilder) the
            // performance improvements can be quite significant.
            this.LockContainer();

            try
            {
                return DoGetAllInstancesWithoutLocking<TService>();
            }
            catch (Exception ex)
            {
                throw new ActivationException(this.FormatActivateAllExceptionMessage(ex, typeof(TService)), ex);
            }
        }

        internal bool ContainsUnregisteredTypeResolutionFor(Type serviceType)
        {
            var e = new UnregisteredTypeEventArgs(serviceType);

            this.OnResolveUnregisteredType(e);

            return e.Handled;
        }

        internal IInstanceProducer GetInstanceProducerForType(Type serviceType,
            Dictionary<Type, IInstanceProducer> snapshot)
        {
            IInstanceProducer instanceProducer;

            if (!this.registrations.TryGetValue(serviceType, out instanceProducer))
            {
                instanceProducer = this.BuildInstanceProducerForType(serviceType);

                if (instanceProducer != null)
                {
                    this.RegisterInstanceProducer(instanceProducer, serviceType, snapshot);
                }
            }

            return instanceProducer;
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving
        /// the requested service instance.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param>
        /// <param name="key">Name of registered service you want. May be null.</param>
        /// <returns>The requested service instance.</returns>
        protected override object DoGetInstance(Type serviceType, string key)
        {
            this.LockContainer();

            return this.DoGetInstanceWithoutLocking(serviceType, key);
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of
        /// resolving all the requested service instances.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <returns>Sequence of service instance objects.</returns>
        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            this.LockContainer();

            return this.DoGetAllInstancesWithoutLocking(serviceType);
        }

        /// <summary>
        /// Format the exception message for use in an <see cref="ActivationException"/>
        /// that occurs while resolving multiple service instances.
        /// </summary>
        /// <param name="actualException">The actual exception thrown by the implementation.</param>
        /// <param name="serviceType">Type of service requested.</param>
        /// <returns>The formatted exception message string.</returns>
        protected override string FormatActivateAllExceptionMessage(Exception actualException,
            Type serviceType)
        {
            string message = base.FormatActivateAllExceptionMessage(actualException, serviceType);

            return ReformatActivateAllExceptionMessage(message, actualException);
        }

        /// <summary>
        /// Format the exception message for use in an <see cref="ActivationException"/>
        /// that occurs while resolving a single service.
        /// </summary>
        /// <param name="actualException">The actual exception thrown by the implementation.</param>
        /// <param name="serviceType">Type of service requested.</param>
        /// <param name="key">Name requested.</param>
        /// <returns>The formatted exception message string.</returns>
        protected override string FormatActivationExceptionMessage(Exception actualException,
            Type serviceType, string key)
        {
            string message = base.FormatActivationExceptionMessage(actualException, serviceType, key);

            return ReformatActivateAllExceptionMessage(message, actualException);
        }

        private object DoGetInstanceWithoutLocking(Type serviceType, string key)
        {
            if (key == null)
            {
                return this.GetInstanceForType(serviceType);
            }
            else
            {
                return this.GetKeyedInstanceForType(serviceType, key);
            }
        }

        private IEnumerable<TService> DoGetAllInstancesWithoutLocking<TService>()
        {
            IInstanceProducer instanceProducer;

            if (!this.registrations.TryGetValue(typeof(IEnumerable<TService>), out instanceProducer))
            {
                return Enumerable.Empty<TService>();
            }

            return (IEnumerable<TService>)instanceProducer.GetInstance();
        }

        private IEnumerable<object> DoGetAllInstancesWithoutLocking(Type serviceType)
        {
            IInstanceProducer instanceProducer;

            Type collectionType = typeof(IEnumerable<>).MakeGenericType(serviceType);

            // Create a copy, because the reference may change during this method call.
            var snapshot = this.registrations;

            if (!snapshot.TryGetValue(collectionType, out instanceProducer))
            {
                return Enumerable.Empty<object>();
            }

            IEnumerable registeredCollection = (IEnumerable)instanceProducer.GetInstance();

            return registeredCollection.Cast<object>();
        }

        private static string ReformatActivateAllExceptionMessage(string message, Exception actualException)
        {
            // HACK: The ServiceLocatorImplBase contains a spelling error. We fix it here.
            message = message.Replace("occured", "occurred");

            if (actualException != null && !String.IsNullOrEmpty(actualException.Message))
            {
                return message + ". " + actualException.Message;
            }

            // HACK: We add a period after the exception message, because the ServiceLocatorImplBase does
            // not produce an exception message ending with a period (which is a bug).
            return message + ".";
        }

        private object GetInstanceForType(Type serviceType)
        {
            // Create a copy, because the reference may change during this method call.
            var snapshot = this.registrations;

            IInstanceProducer instanceProducer = this.GetInstanceProducerForType(serviceType, snapshot);

            if (instanceProducer != null)
            {
                // We create the instance AFTER registering the instance producer. Calling registration
                // after creating it, could make us loose all registrations that are done by GetInstance.
                return instanceProducer.GetInstance();
            }

            throw new ActivationException(StringResources.NoRegistrationForTypeFound(serviceType));
        }

        private IInstanceProducer BuildInstanceProducerForType(Type serviceType)
        {
            var instanceProducer =
                this.BuildInstanceProducerThroughUnregisteredTypeResolution(serviceType);

            if (instanceProducer == null)
            {
                instanceProducer = this.BuildInstanceProducerForConcreteInstance(serviceType);
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

                return (IInstanceProducer)
                    Activator.CreateInstance(instanceProducerType, serviceType, e.InstanceCreator);
            }
            else
            {
                return null;
            }
        }

        private IInstanceProducer BuildInstanceProducerForConcreteInstance(Type serviceType)
        {
            // NOTE: We don't check if the type is actually constructable. The TransientInstanceProducer will
            // do that by the time GetInstance is called for the first time on it.
            if (Helpers.IsConcreteType(serviceType))
            {
                return (IInstanceProducer)Activator.CreateInstance(
                    typeof(TransientInstanceProducer<>).MakeGenericType(serviceType), this);
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

        private object GetKeyedInstanceForType(Type serviceType, string key)
        {
            IKeyedInstanceProducer keyedInstanceProducer;

            // the keyedRegistrations will never change after the container is locked down; there is no need
            // for making a local copy of its reference.
            if (this.keyedInstanceProducers.TryGetValue(serviceType, out keyedInstanceProducer))
            {
                return keyedInstanceProducer.GetInstance(key);
            }

            throw new ActivationException(StringResources.NoRegistrationFoundForKeyedType(serviceType));
        }

        /// <summary>Prevents any new registrations to be made to the container.</summary>
        private void LockContainer()
        {
            if (!this.locked)
            {
                // By using a lock, we have the certainty that all threads will see the new value for 'locked'
                // immediately.
                lock (this.locker)
                {
                    this.locked = true;
                }
            }
        }

        private KeyedInstanceProducer GetRegistrationDictionaryFor<T>(string methodName)
        {
            IKeyedInstanceProducer producer;

            this.keyedInstanceProducers.TryGetValue(typeof(T), out producer);

            if (producer == null)
            {
                producer = new KeyedInstanceProducer(typeof(T), methodName);

                this.keyedInstanceProducers[typeof(T)] = producer;
            }

            return (KeyedInstanceProducer)producer;
        }
    }
}