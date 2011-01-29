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
    /// Locates instances based on the supplied <see cref="Func{T, TResult}"/> delegate.
    /// </summary>
    /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
    internal sealed class KeyedFuncInstanceProducer<T> : IKeyedInstanceProducer
    {
        private readonly Func<string, T> keyedCreator;

        internal KeyedFuncInstanceProducer(Func<string, T> keyedCreator)
        {
            this.keyedCreator = keyedCreator;
        }

        /// <summary>Gets the instance using the specified <paramref name="key"/>.</summary>
        /// <param name="key">The key to get the instance with.</param>
        /// <returns>Gets an instance by a given <paramref name="key"/>; never returns null.</returns>
        /// <exception cref="ActivationException">Thrown when something went wrong.</exception>
        object IKeyedInstanceProducer.GetInstance(string key)
        {
            object instance;

            try
            {
                instance = this.keyedCreator(key);
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    StringResources.DelegateForTypeThrewAnException(typeof(T), ex));
            }

            if (instance == null)
            {
                throw new ActivationException(
                    StringResources.RegisteredDelegateForTypeReturnedNull(typeof(T)));
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
                StringResources.TypeAlreadyRegisteredUsingRegisterByKeyFuncStringT(typeof(T)));
        }

        /// <summary>Throws an expressive exception.</summary>
        /// <param name="key">The key to register the type with.</param>
        void IKeyedInstanceProducer.CheckIfKeyIsAlreadyRegistered(string key)
        {
            // When this method is called, the type is registered using a Func<string, T>. Registering the
            // same type using Func<string, T> and (string, Func<T>) is not allowed: We throw.
            throw new InvalidOperationException(
                StringResources.ForKeyTypeAlreadyRegisteredUsingRegisterByKeyFuncStringT(typeof(T)));
        }
    }
}