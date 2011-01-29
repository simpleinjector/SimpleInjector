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
using System.Linq;

using Microsoft.Practices.ServiceLocation;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Helper methods for the Simple Service Locator.
    /// </summary>
    internal static class Helpers
    {
        // Throws an InvalidOperationException on failure.
        internal static void Validate(this IInstanceProducer instanceProducer, Type serviceType)
        {
            try
            {
                // Test the creator
                // NOTE: We've got our first quirk in the design here: The returned object could implement
                // IDisposable, but there is no way for us to know if we should actually dispose this 
                // instance or not :-(. Disposing it could make us prevent a singleton from ever being
                // used; not disposing it could make us leak resources :-(.
                instanceProducer.GetInstance();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    StringResources.ConfigurationInvalidCreatingInstanceFailed(serviceType, ex), ex);
            }
        }

        internal static bool IsConcreteType(Type type)
        {
            // While array types are in fact concrete, we can not create them and creating them would be
            // pretty useless.
            return !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsArray;
        }

        internal static object GetInstanceFromUnhandledTypeDelegate(Type serviceType,
            Func<object> instanceCreator)
        {
            object instance;

            try
            {
                instance = instanceCreator();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(StringResources
                    .HandlerReturnedADelegateThatThrewAnException(serviceType, ex.Message), ex);
            }

            if (instance == null)
            {
                throw new InvalidOperationException(
                    StringResources.HandlerReturnedADelegateThatReturnedNull(serviceType));
            }

            if (!serviceType.IsAssignableFrom(instance.GetType()))
            {
                throw new InvalidOperationException(
                    StringResources.HandlerReturnedDelegateThatReturnedAnUnassignableFrom(serviceType,
                    instance.GetType()));
            }

            return instance;
        }

        internal static Dictionary<TKey, TValue> MakeCopyOf<TKey, TValue>(Dictionary<TKey, TValue> source)
        {
            // We choose an initial capacity of count + 1, because we'll be adding 1 item to this copy.
            int initialCapacity = source.Count + 1;

            var copy = new Dictionary<TKey, TValue>(initialCapacity);

            foreach (var pair in source)
            {
                copy.Add(pair.Key, pair.Value);
            }

            return copy;
        }

        internal static void ThrowWhenTypeIsAbstract(Type serviceType)
        {
            if (!IsConcreteType(serviceType))
            {
                throw new InvalidOperationException(
                    StringResources.TypeShouldBeConcreteToBeUsedOnRegisterSingle(serviceType));
            }
        }

        internal static void ValidateIfCollectionCanBeIterated(IEnumerable collection, Type serviceType)
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

        internal static void ValidateIfCollectionForNullElements(IEnumerable collection, Type serviceType)
        {
            bool collectionContainsNullItems = collection.Cast<object>().Any(c => c == null);

            if (collectionContainsNullItems)
            {
                throw new InvalidOperationException(
                    StringResources.ConfigurationInvalidCollectionContainsNullElements(serviceType));
            }
        }
    }
}