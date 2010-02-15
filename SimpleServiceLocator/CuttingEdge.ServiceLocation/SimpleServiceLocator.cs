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
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

using Microsoft.Practices.ServiceLocation;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Simple service locator.
    /// </summary>
    public class SimpleServiceLocator : ServiceLocatorImplBase
    {
        private readonly Dictionary<Type, IKeyedRegistrationLocator> keyedRegistrations =
            new Dictionary<Type, IKeyedRegistrationLocator>();

        private readonly Dictionary<Type, IEnumerable<object>> registeredCollections =
            new Dictionary<Type, IEnumerable<object>>();

        private readonly object locker = new object();

        private Dictionary<Type, Func<object>> registrations = new Dictionary<Type, Func<object>>();

        private bool locked;

        /// <summary>Initializes a new instance of the <see cref="SimpleServiceLocator"/> class.</summary>
        public SimpleServiceLocator()
        {
        }

        /// <summary>Defines an interface for retrieving instances by a key.</summary>
        private interface IKeyedRegistrationLocator
        {
            /// <summary>Gets the instance using the specified <paramref name="key"/>.</summary>
            /// <param name="key">The key to get the instance with.</param>
            /// <returns>Gets an instance by a given <paramref name="key"/>; never returns null.</returns>
            /// <exception cref="ActivationException">Thrown when something went wrong :-).</exception>
            object Get(string key);
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

            this.ThrowIfLocked();

            Type serviceType = typeof(T);

            this.ThrowWhenUnkeyedTypeAlreadyRegistered(serviceType);

            // Create a delegate that always returns this particular instance.
            Func<object> instanceCreator = () => instance;

            this.registrations[serviceType] = instanceCreator;
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
        public void Register<T>(Func<T> instanceCreator)
        {
            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            this.ThrowIfLocked();

            this.ThrowWhenUnkeyedTypeAlreadyRegistered(typeof(T));

            // Create a delegate that calls the Func<T> delegate and returns T as object.
            Func<object> objectInstanceCreator = () => instanceCreator();

            this.registrations[typeof(T)] = objectInstanceCreator;
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
        public void RegisterByKey<T>(Func<string, T> keyedInstanceCreator)
        {
            if (keyedInstanceCreator == null)
            {
                throw new ArgumentNullException("keydInstanceCreator");
            }

            this.ThrowIfLocked();
            this.ThrowWhenKeyedTypeAlreadyRegistered<T>();

            IKeyedRegistrationLocator locator = new FuncRegistrationLocator<T>(keyedInstanceCreator);
            this.keyedRegistrations[typeof(T)] = locator;
        }

        /// <summary>Registers a single instance by a given <paramref name="key"/>. 
        /// This <paramref name="instance"/> must be thread-safe.</summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="key">The key that can be used to retrieve the instance.</param>
        /// <param name="instance">The instance to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance is locked and can not be altered, or when an 
        /// <paramref name="instance"/> with <paramref name="key"/> for <typeparamref name="T"/> has already
        /// been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or 
        /// <paramref name="instance"/> are null references.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is an empty string.</exception>
        public void RegisterSingleByKey<T>(string key, T instance)
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

            this.ThrowIfLocked();
            this.ThrowWhenRegistrationsAlreadyContainKeyFor(typeof(T), key);

            var registrationDictionary = this.GetRegisteredDictionaryWithKeyedSinglesFor(typeof(T));

            if (registrationDictionary == null)
            {
                registrationDictionary = new RegistrationDictionary(typeof(T));
                this.RegisterDictionaryWithKeyedSinglesFor(typeof(T), registrationDictionary);
            }

            registrationDictionary.Add(key, instance);
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
        public void RegisterAll<T>(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            this.ThrowIfLocked();

            Type serviceType = typeof(T);

            this.ThrowWhenRegisteredCollectionsAlreadyContainsKeyFor(serviceType);

            var collectionOfObjects = collection.Cast<object>();

            this.registeredCollections[serviceType] = collectionOfObjects;
        }

        /// <summary>
        /// Validates the <b>SimpleServiceLocator</b>. Validate will call all delegates registered with
        /// <see cref="Register"/> and iterate collections registered with <see cref="RegisterAll"/> and
        /// throws an exception if there was an error.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the registration of instances was
        /// invalid.</exception>
        public void Validate()
        {
            // Note: keyed registrations can not be checked, because we don't know what are valid string
            // arguments for the Func<string, object> delegates.
            this.ValidateRegistrations();
            this.ValidateRegisteredCollections();
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

            // Create a copy. Not strictly needed, since we don't mutate this dictionary later on, but this
            // saves us a bug when we do this in a later release :-).
            var registeredCollections = this.registeredCollections;

            if (!registeredCollections.ContainsKey(serviceType))
            {
                return Enumerable.Empty<object>();
            }

            return registeredCollections[serviceType];
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
            // HACK: We add a period after the exception message, because the ServiceLocatorImplBase does
            // not produce an exception message ending with a period (which is a bug).
            const string Period = ".";

            string message = base.FormatActivateAllExceptionMessage(actualException, serviceType);

            if (actualException != null && !String.IsNullOrEmpty(actualException.Message))
            {
                return message + Period + " " + actualException.Message;
            }

            return message + Period;
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
            // HACK: We add a period after the exception message, because the ServiceLocatorImplBase does
            // not produce an exception message ending with a period (which is a bug).
            const string Period = ".";

            string message = base.FormatActivationExceptionMessage(actualException, serviceType, key);

            if (actualException != null && !String.IsNullOrEmpty(actualException.Message))
            {
                return message + Period + " " + actualException.Message;
            }

            return message + Period;
        }

        private object GetInstanceForType(Type serviceType)
        {
            // Create a copy, because the reference may change during this method call.
            var localRegistrations = this.registrations;

            object instance = GetInstanceForTypeFromRegistrations(localRegistrations, serviceType);

            if (instance != null)
            {
                return instance;
            }

            if (serviceType.IsAbstract || serviceType.IsGenericTypeDefinition)
            {
                // Only delegates for concrete types can be generated.
                throw new ActivationException(StringResources.NoRegistrationForTypeFound(serviceType));
            }

            var instanceCreator = this.RegisterDelegateForType(serviceType, localRegistrations);

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
        private Func<object> RegisterDelegateForType(Type serviceType,
            Dictionary<Type, Func<object>> registrations)
        {
            Func<object> instanceCreator = DelegateBuilder.Build(serviceType, registrations, this);

            var registrationsCopy = MakeCopyOf(registrations);

            registrationsCopy.Add(serviceType, instanceCreator);

            // Replace the original with the new version that includes the serviceType.
            this.registrations = registrationsCopy;

            return instanceCreator;
        }

        private object GetKeyedInstanceForType(Type serviceType, string key)
        {
            if (this.keyedRegistrations.ContainsKey(serviceType))
            {
                return this.keyedRegistrations[serviceType].Get(key);
            }

            throw new ActivationException(StringResources.NoRegistrationFoundForType(serviceType));
        }

        private void ThrowIfLocked()
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

        private RegistrationDictionary GetRegisteredDictionaryWithKeyedSinglesFor(Type serviceType)
        {
            IKeyedRegistrationLocator registrationLocator;

            if (!this.keyedRegistrations.TryGetValue(serviceType, out registrationLocator))
            {
                return null;
            }

            var registrationDictionary = registrationLocator as RegistrationDictionary;

            if (registrationDictionary != null)
            {
                return registrationDictionary;
            }

            throw new InvalidOperationException(
                StringResources.TypeAlreadyRegisteredForRegisterByKey(serviceType));
        }

        private void RegisterDictionaryWithKeyedSinglesFor(Type serviceType,
            RegistrationDictionary registrationDictionary)
        {
            this.keyedRegistrations.Add(serviceType, registrationDictionary);
        }

        private void ValidateRegistrations()
        {
            foreach (var pair in this.registrations)
            {
                try
                {
                    Func<object> instanceCreator = pair.Value;

                    // Test the creator
                    instanceCreator();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        StringResources.ConfigurationInvalidCreatingInstanceFailed(pair.Key, ex), ex);
                }
            }
        }

        private void ValidateRegisteredCollections()
        {
            Type firstInvalidType = null;

            foreach (var pair in this.registeredCollections)
            {
                try
                {
                    IEnumerable<object> registeredCollection = pair.Value;

                    VerifyIfListCanBeIterated(registeredCollection);

                    bool collectionContainsNullItems = registeredCollection.Where(i => i == null).Count() > 0;

                    if (collectionContainsNullItems && firstInvalidType == null)
                    {
                        firstInvalidType = pair.Key;
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        StringResources.ConfigurationInvalidIteratingCollectionFailed(pair.Key, ex), ex);
                }
            }

            if (firstInvalidType != null)
            {
                throw new InvalidOperationException(
                    StringResources.ConfigurationInvalidCollectionContainsNullElements(firstInvalidType));
            }
        }

        private static void VerifyIfListCanBeIterated(IEnumerable<object> registeredCollection)
        {
            foreach (var item in registeredCollection)
            {
                // Do nothing inside the loop, iterating the list is enough.
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

        private void ThrowWhenKeyedTypeAlreadyRegistered<T>()
        {
            // This check is only performed within RegisterByKey.
            if (this.keyedRegistrations.ContainsKey(typeof(T)))
            {
                if (this.keyedRegistrations[typeof(T)] is FuncRegistrationLocator<T>)
                {
                    throw new InvalidOperationException(
                        StringResources.TypeAlreadyRegisteredRegisterByKeyAlreadyCalled(typeof(T)));
                }
                else
                {
                    throw new InvalidOperationException(
                        StringResources.TypeAlreadyRegisteredRegisterSingleByKeyAlreadyCalled(typeof(T)));
                }
            }
        }

        private void ThrowWhenRegistrationsAlreadyContainKeyFor(Type serviceType, string key)
        {
            var registrationDictionary = this.GetRegisteredDictionaryWithKeyedSinglesFor(serviceType);

            if (registrationDictionary != null && registrationDictionary.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    StringResources.TypeAlreadyRegisteredWithKey(serviceType, key));
            }
        }

        private void ThrowWhenRegisteredCollectionsAlreadyContainsKeyFor(Type serviceType)
        {
            if (this.registeredCollections.ContainsKey(serviceType))
            {
                throw new InvalidOperationException(
                    StringResources.CollectionTypeAlreadyRegistered(serviceType));
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

        /// <summary>
        /// Represents a collection of string keys and object instances for a single interface or base type,
        /// registered with the <see cref="RegisterSingleByKey"/> method.
        /// </summary>
        private sealed class RegistrationDictionary : Dictionary<string, object>,
            IKeyedRegistrationLocator
        {
            private readonly Type serviceType;

            /// <summary>
            /// Initializes a new instance of the <see cref="RegistrationDictionary"/> class.
            /// </summary>
            /// <param name="serviceType">Type of the service.</param>
            public RegistrationDictionary(Type serviceType)
            {
                this.serviceType = serviceType;
            }

            /// <summary>Gets the instance using the specified <paramref name="key"/>.</summary>
            /// <param name="key">The key to get the instance with.</param>
            /// <returns>Gets an instance by a given <paramref name="key"/>; never returns null.</returns>
            /// <exception cref="ActivationException">Thrown when something went wrong.</exception>
            object IKeyedRegistrationLocator.Get(string key)
            {
                if (this.ContainsKey(key))
                {
                    object instance = this[key];

                    Debug.Assert(instance != null, "It should be impossible for null values to be registered.");

                    return instance;
                }

                throw new ActivationException(StringResources.KeyForTypeNotFound(this.serviceType, key));
            }
        }

        /// <summary>
        /// Locates instances based on the supplied <see cref="Func{T, TResult}"/> delegate.
        /// </summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
        private sealed class FuncRegistrationLocator<T> : IKeyedRegistrationLocator
        {
            private readonly Func<string, T> keyedCreator;

            /// <summary>
            /// Initializes a new instance of the <see cref="FuncRegistrationLocator{T}"/> class.
            /// </summary>
            /// <param name="keyedCreator">The keyed creator.</param>
            public FuncRegistrationLocator(Func<string, T> keyedCreator)
            {
                this.keyedCreator = keyedCreator;
            }

            /// <summary>Gets the instance using the specified <paramref name="key"/>.</summary>
            /// <param name="key">The key to get the instance with.</param>
            /// <returns>Gets an instance by a given <paramref name="key"/>; never returns null.</returns>
            /// <exception cref="ActivationException">Thrown when something went wrong.</exception>
            object IKeyedRegistrationLocator.Get(string key)
            {
                object instance = this.keyedCreator(key);

                if (instance != null)
                {
                    return instance;
                }

                throw new ActivationException(StringResources.RegisteredDelegateForTypeReturnedNull(typeof(T)));
            }
        }
    }
}