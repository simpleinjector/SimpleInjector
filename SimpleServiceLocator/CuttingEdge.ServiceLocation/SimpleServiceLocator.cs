#region Copyright (c) 2010 S. van Deursen
/* The SimpleServiceLocator library is a simple but complete implementation of the CommonServiceLocator 
 * interface.
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

using Microsoft.Practices.ServiceLocation;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// The Simple Service Locator container. Create an instance of this type for registration of dependencies.
    /// </summary>
    public class SimpleServiceLocator : ServiceLocatorImplBase
    {
        internal static readonly StringComparer StringComparer = StringComparer.Ordinal;

        private readonly Dictionary<Type, IKeyedInstanceProducer> keyedInstanceProducers =
            new Dictionary<Type, IKeyedInstanceProducer>();

        private readonly object locker = new object();

        private Dictionary<Type, IInstanceProducer> registrations = new Dictionary<Type, IInstanceProducer>();

        // This dictionary is only used for validation. After validation is gets erased.
        private Dictionary<Type, IEnumerable> collectionsToValidate = new Dictionary<Type, IEnumerable>();

        private bool locked;

        private EventHandler<UnregisteredTypeEventArgs> resolveUnregisteredType;
        
        /// <summary>Initializes a new instance of the <see cref="SimpleServiceLocator"/> class.</summary>
        public SimpleServiceLocator()
        {
        }

        /// <summary>
        /// Occurs when an instance of a type is requested that has not been registered, allowing resolution
        /// of unregistered types.
        /// </summary>
        public event EventHandler<UnregisteredTypeEventArgs> ResolveUnregisteredType
        {
            add
            {
                this.ThrowWhenContainerIsLocked();

                this.resolveUnregisteredType += value;
            }

            remove
            {
                this.ThrowWhenContainerIsLocked();

                this.resolveUnregisteredType -= value;
            }
        }

        internal Dictionary<Type, IInstanceProducer> Registrations
        {
            get { return this.registrations; }
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, or when a 
        /// <paramref name="instanceCreator"/> for <typeparamref name="T"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instanceCreator"/> is a null reference.</exception>
        public void Register<T>(Func<T> instanceCreator) where T : class
        {
            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            this.ThrowWhenContainerIsLocked();

            this.ThrowWhenUnkeyedTypeAlreadyRegistered(typeof(T));

            this.registrations[typeof(T)] = new FuncInstanceProducer<T>(instanceCreator);
        }

        /// <summary>
        /// Registers a concrete instance that will be constructed using constructor injection, and that will
        /// be initialized using the <paramref name="instanceInitializer"/> delegate.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <param name="instanceInitializer">The delegate that will be called after the instance has been
        /// constructed and before it is returned.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="instanceInitializer"/> is a null reference.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, or when an <paramref name="instance"/>
        /// for <typeparamref name="TConcrete"/> has already been registered, or when the given 
        /// <typeparamref name="TConcrete"/> is not a concrete type.
        /// </exception>
        public void Register<TConcrete>(Action<TConcrete> instanceInitializer) where TConcrete : class
        {
            Helpers.ThrowArgumentExceptionWhenTypeIsNotConstructable(typeof(TConcrete), "TConcrete");

            if (instanceInitializer == null)
            {
                throw new ArgumentNullException("instanceInitializer");
            }

            this.ThrowWhenContainerIsLocked();

            this.ThrowWhenUnkeyedTypeAlreadyRegistered(typeof(TConcrete));

            var instanceProducer = new TransientInstanceProducer<TConcrete>(this, instanceInitializer);

            this.registrations[typeof(TConcrete)] = instanceProducer;
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="T"/> by
        /// a string key.
        /// </summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="keyedInstanceCreator">The keyed instance creator.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, or when a 
        /// <paramref name="keyedInstanceCreator"/> for <typeparamref name="T"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyedInstanceCreator"/> is a
        /// null reference.</exception>
        public void RegisterByKey<T>(Func<string, T> keyedInstanceCreator) where T : class
        {
            if (keyedInstanceCreator == null)
            {
                throw new ArgumentNullException("keyedInstanceCreator");
            }

            this.ThrowWhenContainerIsLocked();
            this.ThrowWhenKeyedFuncIsAlreadyRegisteredFor<T>();

            IKeyedInstanceProducer locator = new KeyedFuncInstanceProducer<T>(keyedInstanceCreator);
            this.keyedInstanceProducers[typeof(T)] = locator;
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="T"/> for
        /// the specified (ordinal) case-sensitive <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="key">The (ordinal) case-sensitive key that can be used to retrieve the instance.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, or when a 
        /// <paramref name="key"/> for <typeparamref name="T"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or the
        /// <paramref name="instanceCreator"/> is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> contains an empty string.</exception>
        public void RegisterByKey<T>(string key, Func<T> instanceCreator) where T : class
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (key.Length == 0)
            {
                throw new ArgumentException(StringResources.KeyCanNotBeAnEmptyString, "key");
            }

            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            this.ThrowWhenContainerIsLocked();
            this.ThrowWhenKeyIsAlreadyRegisteredFor<T>(key);

            KeyedInstanceProducer dictionary = 
                this.GetRegistrationDictionaryFor<T>("RegisterByKey<T>(string, Func<T>)");

            dictionary.Add(key, new FuncInstanceProducer<T>(instanceCreator));
        }

        /// <summary>
        /// Registers a single concrete instance that will be constructed using constructor injection. 
        /// This <typeparamref name="TConcrete"/> must be thread-safe.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, or when an <paramref name="instance"/>
        /// for <typeparamref name="TConcrete"/> has already been registered, or when the given 
        /// <typeparamref name="TConcrete"/> is not a concrete type.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A design without a generic T would be unpractical, because the other " +
            "overloads also take a generic T.")]
        public void RegisterSingle<TConcrete>() where TConcrete : class
        {
            Helpers.ThrowArgumentExceptionWhenTypeIsNotConstructable(typeof(TConcrete), "TConcrete");

            this.ThrowWhenContainerIsLocked();

            this.ThrowWhenUnkeyedTypeAlreadyRegistered(typeof(TConcrete));

            var instanceProducer = new TransientInstanceProducer<TConcrete>(this);

            var singletonProducer = new FuncSingletonInstanceProducer<TConcrete>(instanceProducer.GetInstance);

            this.registrations[typeof(TConcrete)] = singletonProducer;
        }

        /// <summary>Registers a single instance. This <paramref name="instance"/> must be thread-safe.</summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve the instance.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, or when an <paramref name="instance"/>
        /// for <typeparamref name="T"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is a null reference.</exception>
        public void RegisterSingle<T>(T instance) where T : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            this.ThrowWhenContainerIsLocked();

            this.ThrowWhenUnkeyedTypeAlreadyRegistered(typeof(T));

            this.registrations[typeof(T)] = new SingletonInstanceProducer<T>(instance);
        }

        /// <summary>
        /// Registers the specified delegate that allows constructing a single instance of 
        /// <typeparamref name="T"/>. This delegate will be called at most once during the lifetime of the
        /// application.
        /// </summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="instanceCreator">The delegate that allows building or creating this single
        /// instance.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, or when a 
        /// <paramref name="instanceCreator"/> for <typeparamref name="T"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="singleInstanceCreator"/> is a 
        /// null reference.</exception>
        public void RegisterSingle<T>(Func<T> instanceCreator) where T : class
        {
            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            this.ThrowWhenContainerIsLocked();

            this.ThrowWhenUnkeyedTypeAlreadyRegistered(typeof(T));

            this.registrations[typeof(T)] = new FuncSingletonInstanceProducer<T>(instanceCreator);
        }

        /// <summary>
        /// Registers a single concrete instance that will be constructed using constructor injection, and 
        /// that will be initialized using the <paramref name="instanceInitializer"/> delegate. 
        /// This <typeparamref name="TConcrete"/> must be thread-safe.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <param name="instanceInitializer">The delegate that will be called once after the instance has been
        /// constructed and before it is returned.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="instanceInitializer"/> is a null reference.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, or when an <paramref name="instance"/>
        /// for <typeparamref name="TConcrete"/> has already been registered, or when the given 
        /// <typeparamref name="TConcrete"/> is not a concrete type.
        /// </exception>
        public void RegisterSingle<TConcrete>(Action<TConcrete> instanceInitializer) where TConcrete : class
        {
            Helpers.ThrowArgumentExceptionWhenTypeIsNotConstructable(typeof(TConcrete), "TConcrete");

            if (instanceInitializer == null)
            {
                throw new ArgumentNullException("instanceInitializer");
            }

            this.ThrowWhenContainerIsLocked();

            this.ThrowWhenUnkeyedTypeAlreadyRegistered(typeof(TConcrete));

            var instanceProducer = new TransientInstanceProducer<TConcrete>(this, instanceInitializer);

            this.RegisterSingle<TConcrete>(instanceProducer.GetInstance);
        }

        /// <summary>Registers a single instance by a given (ordinal) case-sensitive <paramref name="key"/>. 
        /// This <paramref name="instance"/> must be thread-safe.</summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="key">The (ordinal) case-sensitive key that can be used to retrieve the instance.</param>
        /// <param name="instance">The instance to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, or when an 
        /// <paramref name="instance"/> with <paramref name="key"/> for <typeparamref name="T"/> has already
        /// been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or 
        /// <paramref name="instance"/> are null references.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is an empty string.</exception>
        public void RegisterSingleByKey<T>(string key, T instance) where T : class
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (key.Length == 0)
            {
                throw new ArgumentException(StringResources.KeyCanNotBeAnEmptyString, "key");
            }

            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            this.ThrowWhenContainerIsLocked();
            this.ThrowWhenKeyIsAlreadyRegisteredFor<T>(key);

            KeyedInstanceProducer dictionary =
                this.GetRegistrationDictionaryFor<T>("RegisterSingleByKey<T>(string, T)");

            dictionary.Add(key, new SingletonInstanceProducer<T>(instance));
        }

        /// <summary>
        /// Registers a single instance by a given (ordinal) case-sensitive <paramref name="key"/> by 
        /// specifying a delegate.
        /// </summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="key">The (ordinal) case-sensitive key that can be used to retrieve the instance.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, when a 
        /// <paramref name="key"/> for <typeparamref name="T"/> has already been registered or when 
        /// <typeparamref name="T"/> has already been registered using one of the overloads that take an
        /// <b>Func&lt;string, T&gt;</b>.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or the
        /// <paramref name="instanceCreator"/> is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> contains an empty string.
        /// </exception>
        public void RegisterSingleByKey<T>(string key, Func<T> instanceCreator) where T : class
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (key.Length == 0)
            {
                throw new ArgumentException(StringResources.KeyCanNotBeAnEmptyString, "key");
            }

            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            this.ThrowWhenContainerIsLocked();
            this.ThrowWhenKeyIsAlreadyRegisteredFor<T>(key);

            KeyedInstanceProducer dictionary =
                this.GetRegistrationDictionaryFor<T>("RegisterSingleByKey<T>(string, Func<T>)");

            dictionary.Add(key, new FuncSingletonInstanceProducer<T>(instanceCreator));
        }

        /// <summary>
        /// Registers the specified delegate that allows returning singletons of <typeparamref name="T"/> by
        /// a string key. The delegate will get called at most once per given (ordinal) case-sensitive key.
        /// </summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="keyedInstanceCreator">The keyed instance creator.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, or when the <typeparamref name="T"/> 
        /// already been registered using one of the overloads that take a <b>string</b> key.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or the
        /// <paramref name="instanceCreator"/> is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> contains an empty string.</exception>
        public void RegisterSingleByKey<T>(Func<string, T> keyedInstanceCreator) where T : class
        {
            if (keyedInstanceCreator == null)
            {
                throw new ArgumentNullException("keyedInstanceCreator");
            }

            this.ThrowWhenContainerIsLocked();
            this.ThrowWhenKeyedFuncIsAlreadyRegisteredFor<T>();

            IKeyedInstanceProducer locator = new KeyedFuncSingletonInstanceProducer<T>(keyedInstanceCreator);
            
            this.keyedInstanceProducers[typeof(T)] = locator;
        }

        /// <summary>
        /// Registers a collection of elements of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="collection">The collection to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, or when a <paramref name="collection"/>
        /// for <typeparamref name="T"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is a null
        /// reference.</exception>
        public void RegisterAll<T>(IEnumerable<T> collection) where T : class
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            this.ThrowWhenContainerIsLocked();

            this.ThrowWhenRegisteredCollectionsAlreadyContainsKeyFor<T>();

            this.collectionsToValidate[typeof(T)] = collection;

            this.registrations[typeof(IEnumerable<T>)] = 
                new SingletonInstanceProducer<IEnumerable<T>>(collection);
        }

        /// <summary>
        /// Registers a collection of elements of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="collection">The collection to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, or when a <paramref name="collection"/>
        /// for <typeparamref name="T"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is a null
        /// reference.</exception>
        public void RegisterAll<T>(params T[] collection) where T : class
        {
            this.RegisterAll<T>((IEnumerable<T>)collection);
        }

        /// <summary>
        /// Validates the <b>SimpleServiceLocator</b>. Validate will call all delegates registered with
        /// <b>Register</b> and iterate collections registered with 
        /// <see cref="RegisterAll{T}(IEnumerable{T})"/> and
        /// throws an exception if there was an error.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the registration of instances was
        /// invalid.</exception>
        public void Validate()
        {
            // Note: keyed registrations can not be checked, because we don't know what are valid string
            // arguments for the Func<string, object> delegates.
            this.ValidateRegistrations();
            this.ValidateKeyedRegistrations();
            this.ValidateRegisteredCollections();
        }

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

        private void ThrowWhenContainerIsLocked()
        {
            // By using a lock, we have the certainty that all threads will see the new value for 'locked'
            // immediately.
            lock (this.locker)
            {
                if (this.locked)
                {
                    throw new InvalidOperationException(
                        StringResources.SimpleServiceLocatorCanNotBeChangedAfterUse(this.GetType()));
                }
            }
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

        private void ValidateRegistrations()
        {
            foreach (var pair in this.registrations)
            {
                Type serviceType = pair.Key;
                IInstanceProducer instanceProducer = pair.Value;

                instanceProducer.Verify(serviceType);
            }
        }

        private void ValidateKeyedRegistrations()
        {
            foreach (var pair in this.keyedInstanceProducers)
            {
                IKeyedInstanceProducer keyedInstanceProducer = pair.Value;

                keyedInstanceProducer.Validate();
            }
        }

        private void ValidateRegisteredCollections()
        {
            foreach (var pair in this.collectionsToValidate)
            {
                Type serviceType = pair.Key;
                IEnumerable collection = pair.Value;

                Helpers.ValidateIfCollectionCanBeIterated(collection, serviceType);
                Helpers.ValidateIfCollectionForNullElements(collection, serviceType);
            }
        }

        private void ThrowWhenUnkeyedTypeAlreadyRegistered(Type type)
        {
            if (this.registrations.ContainsKey(type))
            {
                throw new InvalidOperationException(
                    StringResources.UnkeyedTypeAlreadyRegistered(type));
            }
        }

        private void ThrowWhenKeyedFuncIsAlreadyRegisteredFor<T>()
        {
            if (this.keyedInstanceProducers.ContainsKey(typeof(T)))
            {
                this.keyedInstanceProducers[typeof(T)].ThrowTypeAlreadyRegisteredException();
            }
        }

        private void ThrowWhenKeyIsAlreadyRegisteredFor<T>(string key)
        {
            IKeyedInstanceProducer locator;

            if (this.keyedInstanceProducers.TryGetValue(typeof(T), out locator))
            {
                locator.CheckIfKeyIsAlreadyRegistered(key);
            }
        }

        private void ThrowWhenRegisteredCollectionsAlreadyContainsKeyFor<T>()
        {
            if (this.registrations.ContainsKey(typeof(IEnumerable<T>)))
            {
                throw new InvalidOperationException(
                    StringResources.CollectionTypeAlreadyRegistered(typeof(T)));
            }
        }
    }
}