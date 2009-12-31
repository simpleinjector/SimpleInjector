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
        private readonly Dictionary<Type, Func<object>> registrations = new Dictionary<Type, Func<object>>();
        
        private readonly Dictionary<Type, IKeyedRegistrationLocator> keyedRegistrations = 
            new Dictionary<Type, IKeyedRegistrationLocator>();

        private readonly Dictionary<Type, IEnumerable<object>> registeredCollections =
            new Dictionary<Type, IEnumerable<object>>();

        private readonly object locker = new object();
        
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
        public void RegisterSingle<T>(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            this.ThrowIfLocked();

            Type serviceType = typeof(T);

            this.ThrowWhenUnkeyedTypeAlreadyRegistered(serviceType);

            this.registrations[serviceType] = () => instance;           
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        public void Register<T>(Func<T> instanceCreator)
        {
            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            this.ThrowIfLocked();

            Type serviceType = typeof(T);

            this.ThrowWhenUnkeyedTypeAlreadyRegistered(serviceType);

            this.registrations[serviceType] = () => instanceCreator();
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="T"/> by
        /// a string key.
        /// </summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="keyedInstanceCreator">The keyed instance creator.</param>
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
            this.ThrowWhenRegistrationsAlreadyContainKeyFor<T>(key);

            var registrations = this.GetDictionaryWithKeyedSinglesFor<T>();
            registrations.Add(key, instance);
        }

        /// <summary>
        /// Registers a collection of elements of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="collection">The collection to register.</param>
        public void RegisterAll<T>(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

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
        /// <exception cref="ConfigurationErrorsException">Thrown when the registration of instances was
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
            this.Lock();

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
            this.Lock();

            if (!this.registeredCollections.ContainsKey(serviceType))
            {
                return Enumerable.Empty<object>();
            }

            return this.registeredCollections[serviceType];
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
            if (this.locked)
            {
                throw new InvalidOperationException(
                    StringResources.SimpleServiceLocatorCanNotBeChangedAfterUse(this.GetType()));
            }
        }

        private void Lock()
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

        private object GetInstanceForType(Type serviceType)
        {
            if (this.registrations.ContainsKey(serviceType))
            {
                object instance = this.registrations[serviceType]();

                if (instance != null)
                {
                    return instance;
                }

                throw new ActivationException(StringResources.DelegateForTypeReturnedNull(serviceType));                    
            }

            throw new ActivationException(StringResources.NoRegistrationForTypeFound(serviceType));
        }

        private Dictionary<string, object> GetDictionaryWithKeyedSinglesFor<T>()
        {
            Type type = typeof(T);

            IKeyedRegistrationLocator registrations;

            if (!this.keyedRegistrations.TryGetValue(type, out registrations))
            {
                registrations = new RegistrationDictionary(type);
                this.keyedRegistrations.Add(type, registrations);
            }

            var dictionary = registrations as Dictionary<string, object>;

            if (dictionary != null)
            {
                return dictionary;
            }

            throw new InvalidOperationException(StringResources.TypeAlreadyRegisteredForRegisterByKey(type));
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
                    throw new ConfigurationErrorsException(
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
                    throw new ConfigurationErrorsException(
                        StringResources.ConfigurationInvalidIteratingCollectionFailed(pair.Key, ex), ex);
                }
            }

            if (firstInvalidType != null)
            {
                throw new ConfigurationErrorsException(
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

        private void ThrowWhenRegistrationsAlreadyContainKeyFor<T>(string key)
        {
            var registrations = this.GetDictionaryWithKeyedSinglesFor<T>();

            if (registrations.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    StringResources.TypeAlreadyRegisteredWithKey(typeof(T), key));
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