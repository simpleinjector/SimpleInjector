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
    /// Represents a collection of string keys and object instances for a single interface or base type,
    /// registered with on of the XXXByKey(string, T) or XXXByKey(T, Func{T}) methods.
    /// </summary>
    internal sealed class KeyedInstanceProducer : IKeyedInstanceProducer
    {
        private readonly string methodUsedForRegistration;
        private readonly Type serviceType;
        private readonly Dictionary<string, IInstanceProducer> instanceProducers =
            new Dictionary<string, IInstanceProducer>(SimpleServiceLocator.StringComparer);

        internal KeyedInstanceProducer(Type serviceType, string methodUsedForRegistration)
        {
            this.serviceType = serviceType;
            this.methodUsedForRegistration = methodUsedForRegistration;
        }

        /// <summary>Gets the instance using the specified <paramref name="key"/>.</summary>
        /// <param name="key">The key to get the instance with.</param>
        /// <returns>Gets an instance by a given <paramref name="key"/>; never returns null.</returns>
        /// <exception cref="ActivationException">Thrown when something went wrong.</exception>
        object IKeyedInstanceProducer.GetInstance(string key)
        {
            IInstanceProducer instanceCreator = null;

            if (this.instanceProducers.TryGetValue(key, out instanceCreator))
            {
                return instanceCreator.GetInstance();
            }

            throw new ActivationException(StringResources.KeyForTypeNotFound(this.serviceType, key));
        }

        /// <summary>Validates the registered instance producers.</summary>
        public void Validate()
        {
            foreach (var pair in this.instanceProducers)
            {
                IInstanceProducer instanceProducer = pair.Value;

                instanceProducer.Validate();
            }
        }

        /// <summary>Throws an expressive exception.</summary>
        public void ThrowTypeAlreadyRegisteredException()
        {
            throw new InvalidOperationException(
                StringResources.TypeAlreadyRegisteredUsingByKeyString(this.serviceType,
                    this.methodUsedForRegistration));
        }

        /// <summary>Throws an expressive exception in case the key has already been registered.</summary>
        /// <param name="key">The key that is used for the registration.</param>
        public void CheckIfKeyIsAlreadyRegistered(string key)
        {
            if (this.instanceProducers.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    StringResources.TypeAlreadyRegisteredWithKey(this.serviceType, key));
            }
        }

        internal void Add(string key, IInstanceProducer instanceProducer)
        {
            this.instanceProducers.Add(key, instanceProducer);
        }
    }
}