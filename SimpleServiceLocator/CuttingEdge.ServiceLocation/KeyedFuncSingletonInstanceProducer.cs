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

using Microsoft.Practices.ServiceLocation;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Caches instances by key as they get created by the supplied delegate.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    internal sealed class KeyedFuncSingletonInstanceProducer<T> : IKeyedInstanceProducer where T : class
    {
        private readonly Func<string, T> keyedCreator;

        // This dictionary is never changed, but only completely replaced by a new one.
        private Dictionary<string, object> instances = 
            new Dictionary<string, object>(SimpleServiceLocator.StringComparer);

        internal KeyedFuncSingletonInstanceProducer(Func<string, T> keyedCreator)
        {
            this.keyedCreator = keyedCreator;
        }

        /// <summary>Produces an instance by a given key.</summary>
        /// <param name="key">The key that produces the instance.</param>
        /// <returns>An produced instance.</returns>
        object IKeyedInstanceProducer.GetInstance(string key)
        {
            object instance;

            // We use a lock to prevent the delegate to be called more than once per type during the lifetime
            // of the application. We use a double checked lock to prevent the lock statement from being 
            // called again after a keyed instance was created.
            if (!this.instances.TryGetValue(key, out instance))
            {
                // We can take a lock on this, because instances of this type are never publicly exposed.
                lock (this)
                {
                    if (!this.instances.TryGetValue(key, out instance))
                    {
                        instance = this.GetInstanceInternal(key);
                            
                        this.CacheInstance(key, instance);
                    }
                }
            }

            return instance;
        }

        /// <summary>Does nothing.</summary>
        void IKeyedInstanceProducer.Validate()
        {
            // Validation is not possible, because there is no way to determine with what keyed the
            // keyedCreator should be called. We assume the registration is valid.
        }

        /// <summary>Throws an expressive exception.</summary>
        void IKeyedInstanceProducer.ThrowTypeAlreadyRegisteredException()
        {
            throw new InvalidOperationException(
                StringResources.TypeAlreadyRegisteredUsingRegisterSingleByKeyFuncStringT(typeof(T)));
        }

        /// <summary>Throws an expressive exception.</summary>
        /// <param name="key">The key that is used for the registration.</param>
        void IKeyedInstanceProducer.CheckIfKeyIsAlreadyRegistered(string key)
        {
            // When this method is called, the type is registered using a Func<string, T>. Registering the
            // same type using Func<string, T> and (string, Func<T>) is not allowed: We throw.
            throw new InvalidOperationException(
                StringResources.ForKeyTypeAlreadyRegisteredUsingRegisterSingleByKeyFuncStringT(typeof(T)));
        }

        private object GetInstanceInternal(string key)
        {
            object instance = null;

            try
            {
                instance = this.keyedCreator(key);
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    StringResources.DelegateForTypeThrewAnException(typeof(T), ex), ex);
            }

            if (instance == null)
            {
                throw new ActivationException(
                    StringResources.RegisteredDelegateForTypeReturnedNull(typeof(T)));
            }

            return instance;
        }

        private void CacheInstance(string key, object instance)
        {
            // Create a copy of the original instance cache. We don't change the original for thread-safety.
            var copy = Helpers.MakeCopyOf(this.instances);

            copy.Add(key, instance);

            // Replace the original instance cache with the copy.
            this.instances = copy;
        }
    }
}