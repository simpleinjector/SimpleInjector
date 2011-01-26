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
        internal static bool IsConcreteType(Type type)
        {
            return !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsArray;
        }

        internal static object GetInstanceForTypeFromRegistrations(
            Dictionary<Type, Func<object>> registrations, Type serviceType)
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

        internal static Dictionary<Type, Func<object>> MakeCopyOf(Dictionary<Type, Func<object>> source)
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

        internal static void ThrowWhenTypeIsAbstract(Type serviceType)
        {
            if (!IsConcreteType(serviceType))
            {
                throw new InvalidOperationException(
                    StringResources.TypeShouldBeConcreteToBeUsedOnRegisterSingle(serviceType));
            }
        }

        internal static void CheckIfCollectionCanBeIterated(IEnumerable collection, Type serviceType)
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

        internal static void CheckIfCollectionForNullElements(IEnumerable collection, Type serviceType)
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