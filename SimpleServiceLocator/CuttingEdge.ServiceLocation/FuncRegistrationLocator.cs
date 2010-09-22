using System;

using Microsoft.Practices.ServiceLocation;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Locates instances based on the supplied <see cref="Func{T, TResult}"/> delegate.
    /// </summary>
    /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
    internal sealed class FuncRegistrationLocator<T> : IKeyedRegistrationLocator
    {
        private readonly Func<string, T> keyedCreator;

        internal FuncRegistrationLocator(Func<string, T> keyedCreator)
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