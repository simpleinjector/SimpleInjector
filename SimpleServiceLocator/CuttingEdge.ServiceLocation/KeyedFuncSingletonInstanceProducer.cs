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
    internal sealed class KeyedFuncSingletonInstanceProducer<T> : IKeyedInstanceProducer
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
            // Create a copy to the reference.
            var snapshot = this.instances;

            object instance;

            if (!snapshot.TryGetValue(key, out instance))
            {
                instance = this.GetInternal(key);
            }

            if (instance == null)
            {
                throw new ActivationException(StringResources.RegisteredDelegateForTypeReturnedNull(typeof(T)));
            }

            return instance;
        }

        /// <summary>Does nothing.</summary>
        public void Validate()
        {
            // Validation is not possible, because there is no way to determine with what keyed the
            // keyedCreator should be called. We assume the registration is value.
        }

        /// <summary>Throws an expressive exception.</summary>
        public void ThrowTypeAlreadyRegisteredException()
        {
            throw new InvalidOperationException(
                StringResources.TypeAlreadyRegisteredUsingRegisterSingleByKeyFuncStringT(typeof(T)));
        }

        /// <summary>Throws an expressive exception.</summary>
        /// <param name="key">The key that is used for the registration.</param>
        public void CheckIfKeyIsAlreadyRegistered(string key)
        {
            // When this method is called, the type is registered using a Func<string, T>. Registring the
            // same type using Func<string, T> and (string, Func<T>) is not allowed: We throw.
            throw new InvalidOperationException(
                StringResources.ForKeyTypeAlreadyRegisteredUsingRegisterSingleByKeyFuncStringT(typeof(T)));
        }

        private object GetInternal(string key)
        {
            object instance;

            // We have to have a lock here, because we must guarantee that the keyedCreator is at most called
            // once per key.
            // We can lock on this, because instances of this type are never publicly exposed.
            lock (this)
            {
                // Check again, because the cache could have been replaced by now.
                if (!this.instances.TryGetValue(key, out instance))
                {
                    instance = this.keyedCreator(key);

                    // Create a copy of the original instance cache.
                    var copy = new Dictionary<string, object>(this.instances);

                    // Add the new instance to the copy. We must do this even when instance == null, because
                    // not adding would allow the keyedCreator to be called again with that same key.
                    copy[key] = instance;

                    // Replace the original instance cache with the copy.
                    this.instances = copy;
                }
            }

            return instance;
        }
    }
}