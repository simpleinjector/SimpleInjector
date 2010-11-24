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

        private Dictionary<Type, Func<object>> registrations = new Dictionary<Type, Func<object>>();

        // This dictionary is only used for validation. After validation is gets erased.
        private Dictionary<Type, IEnumerable> collectionsToValidate = new Dictionary<Type, IEnumerable>();

        private bool locked;
        private bool validated;

        /// <summary>Initializes a new instance of the <see cref="SimpleServiceLocator"/> class.</summary>
        public SimpleServiceLocator()
        {
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

            // Create a delegate that calls the Func<T> delegate and returns T as object.
            Func<object> objectInstanceCreator = () => instanceCreator();

            this.registrations[typeof(T)] = objectInstanceCreator;
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
            ThrowWhenTypeIsAbstract(typeof(TConcrete));

            if (instanceInitializer == null)
            {
                throw new ArgumentNullException("instanceInitializer");
            }

            this.ThrowWhenContainerIsLocked();

            this.ThrowWhenUnkeyedTypeAlreadyRegistered(typeof(TConcrete));

            // Create a delegate that always returns this particular instance.
            Func<TConcrete> constructorInvoker = DelegateBuilder.Build<TConcrete>(this);

            Func<object> instanceCreator = () =>
            {
                TConcrete instance = constructorInvoker();

                instanceInitializer(instance);

                return instance;
            };

            this.registrations[typeof(TConcrete)] = instanceCreator;
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

            IKeyedInstanceProducer locator = new KeyedFuncTransientInstanceProducer<T>(keyedInstanceCreator);
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

            dictionary.Add(key, new TransientInstanceProducer<T>(instanceCreator));
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
            ThrowWhenTypeIsAbstract(typeof(TConcrete));

            this.ThrowWhenContainerIsLocked();

            this.ThrowWhenUnkeyedTypeAlreadyRegistered(typeof(TConcrete));

            // Create a delegate that always returns this particular instance.
            Func<object> instanceCreator = DelegateBuilder.Build(typeof(TConcrete), this);

            Func<object> singletonCreator = 
                new FuncSingletonInstanceProducer<object>(instanceCreator).GetInstance;

            this.registrations[typeof(TConcrete)] = singletonCreator;
        }

        /// <summary>Registers a single instance. This <paramref name="instance"/> must be thread-safe.</summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve the instance.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, or when an <paramref name="instance"/>
        /// for <typeparamref name="T"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is a null reference.</exception>
        public void RegisterSingle<T>(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            this.ThrowWhenContainerIsLocked();

            this.ThrowWhenUnkeyedTypeAlreadyRegistered(typeof(T));

            // Create a delegate that always returns this particular instance.
            Func<object> instanceCreator = () => instance;

            this.registrations[typeof(T)] = instanceCreator;
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

            // Create a delegate that always returns this particular instance.
            Func<object> singleInstanceCreator = 
                new FuncSingletonInstanceProducer<T>(instanceCreator).GetInstance;

            this.registrations[typeof(T)] = singleInstanceCreator;
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
            ThrowWhenTypeIsAbstract(typeof(TConcrete));

            if (instanceInitializer == null)
            {
                throw new ArgumentNullException("instanceInitializer");
            }

            this.ThrowWhenContainerIsLocked();

            this.ThrowWhenUnkeyedTypeAlreadyRegistered(typeof(TConcrete));

            Func<TConcrete> constructorInvoker = DelegateBuilder.Build<TConcrete>(this);

            Func<TConcrete> instanceCreator = () =>
            {
                TConcrete instance = constructorInvoker();

                instanceInitializer(instance);

                return instance;
            };

            this.RegisterSingle<TConcrete>(instanceCreator);
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

            dictionary.Add(key, new SingletonInstanceProducer(instance));
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

            this.registrations[typeof(IEnumerable<T>)] = () => collection;
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
            this.LockContainer();

            if (!this.validated)
            {
                // Note: keyed registrations can not be checked, because we don't know what are valid string
                // arguments for the Func<string, object> delegates.
                this.ValidateRegistrations();
                this.ValidateKeyedRegistrations();
                this.ValidateRegisteredCollections();

                this.validated = true;
            }
        }

        internal static bool IsConcreteType(Type type)
        {
            return !type.IsAbstract && !type.IsGenericTypeDefinition;
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

            if (key == null)
            {
                return this.GetInstanceForType(serviceType);
            }
            else
            {
                return this.GetKeyedInstanceForType(serviceType, key);
            }
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

            Func<object> getCollection;

            Type collectionType = typeof(IEnumerable<>).MakeGenericType(serviceType);

            // Create a copy, because the reference may change during this method call.
            var snapshot = this.registrations;

            if (!snapshot.TryGetValue(collectionType, out getCollection))
            {
                return Enumerable.Empty<object>();
            }

            IEnumerable registeredCollection = (IEnumerable)getCollection();

            return registeredCollection.Cast<object>();
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

            object instance = GetInstanceForTypeFromRegistrations(snapshot, serviceType);

            if (instance != null)
            {
                return instance;
            }

            if (!IsConcreteType(serviceType))
            {
                // Only delegates for concrete types can be generated.
                throw new ActivationException(StringResources.NoRegistrationForTypeFound(serviceType));
            }

            var instanceCreator = this.RegisterDelegateForConcreteType(serviceType, snapshot);

            return instanceCreator();
        }

        private static object GetInstanceForTypeFromRegistrations(Dictionary<Type, Func<object>> registrations,
            Type serviceType)
        {
            Func<object> instanceCreator = null;

            if (registrations.TryGetValue(serviceType, out instanceCreator))
            {
                object instance = instanceCreator();

                if (instance != null)
                {
                    return instance;
                }

                throw new ActivationException(StringResources.DelegateForTypeReturnedNull(serviceType));
            }

            return null;
        }

        // We're registering a service type after lock here and that means that the type is added to a copy of
        // the registrations dictionary and the original replaced with a new one. This 'reference swapping' is
        // thread-safe, but can result in types disappearing again from the registrations when multiple
        // threads simultaneously add different types. This however, does not result in a consistency problem,
        // because the missing type will be again added later. This type of swapping safes us from using locks.
        private Func<object> RegisterDelegateForConcreteType(Type serviceType,
            Dictionary<Type, Func<object>> registrationsSnapshot)
        {
            Func<object> instanceCreator =
                DelegateBuilder.Build(serviceType, registrationsSnapshot, this);

            var registrationsCopy = MakeCopyOf(registrationsSnapshot);

            registrationsCopy.Add(serviceType, instanceCreator);

            // Replace the original with the new version that includes the serviceType.
            this.registrations = registrationsCopy;

            return instanceCreator;
        }

        private object GetKeyedInstanceForType(Type serviceType, string key)
        {
            // the keyedRegistrations will never change after the container is locked down; there is no need
            // for making a local copy of its reference.
            if (this.keyedInstanceProducers.ContainsKey(serviceType))
            {
                return this.keyedInstanceProducers[serviceType].GetInstance(key);
            }

            throw new ActivationException(StringResources.NoRegistrationFoundForType(serviceType));
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
                try
                {
                    // Test the creator
                    // NOTE: We've got our first quirk in the design here: The returned object could implement
                    // IDisposable, but there is no way for us to know if we should actually dispose this 
                    // instance or not :-(. Disposing it could make us prevent a singleton from ever being
                    // used; not disposing it could make us leak resources :-(.
                    this.GetInstance(pair.Key);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        StringResources.ConfigurationInvalidCreatingInstanceFailed(pair.Key, ex), ex);
                }
            }
        }

        private void ValidateKeyedRegistrations()
        {
            foreach (var pair in this.keyedInstanceProducers)
            {
                pair.Value.Validate();
            }
        }

        private void ValidateRegisteredCollections()
        {
            foreach (var pair in this.collectionsToValidate)
            {
                Type serviceType = pair.Key;
                IEnumerable collection = pair.Value;
                
                CheckIfCollectionCanBeIterated(collection, serviceType);

                CheckIfCollectionForNullElements(collection, serviceType);
            }

            this.collectionsToValidate = null;
        }

        private static void CheckIfCollectionCanBeIterated(IEnumerable collection, Type serviceType)
        {
            try
            {
                var enumerator = collection.GetEnumerator();
                try
                {
                    // Just iterate the collection.
                    while (enumerator.MoveNext())
                    {
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator as IDisposable;

                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    StringResources.ConfigurationInvalidIteratingCollectionFailed(serviceType, ex), ex);
            }
        }

        private static void CheckIfCollectionForNullElements(IEnumerable collection, Type serviceType)
        {
            bool collectionContainsNullItems = collection.Cast<object>().Any(c => c == null);

            if (collectionContainsNullItems)
            {
                throw new InvalidOperationException(
                    StringResources.ConfigurationInvalidCollectionContainsNullElements(serviceType));
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

        private static Dictionary<Type, Func<object>> MakeCopyOf(Dictionary<Type, Func<object>> source)
        {
            // We choose an initial capacity of count + 1, because we'll be adding 1 item to this copy.
            int initialCapacity = source.Count + 1;

            var copy = new Dictionary<Type, Func<object>>(initialCapacity);

            foreach (var pair in source)
            {
                copy.Add(pair.Key, pair.Value);
            }

            return copy;
        }

        private static void ThrowWhenTypeIsAbstract(Type serviceType)
        {
            if (!IsConcreteType(serviceType))
            {
                throw new InvalidOperationException(
                    StringResources.TypeShouldBeConcreteToBeUsedOnRegisterSingle(serviceType));
            }
        }
    }
}