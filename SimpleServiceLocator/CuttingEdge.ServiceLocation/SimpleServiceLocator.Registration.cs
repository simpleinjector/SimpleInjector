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

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Methods for registration.
    /// </summary>
    public partial class SimpleServiceLocator
    {
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

            var immutableCollection = Helpers.CreateImmutableCollection(collection);

            this.collectionsToValidate[typeof(T)] = immutableCollection;

            this.registrations[typeof(IEnumerable<T>)] =
                new SingletonInstanceProducer<IEnumerable<T>>(immutableCollection);
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
    }
}