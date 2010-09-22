using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Practices.ServiceLocation;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Represents a collection of string keys and object instances for a single interface or base type,
    /// registered with the <see cref="RegisterSingleByKey"/> method.
    /// </summary>
    internal sealed class RegistrationDictionary : Dictionary<string, object>,
        IKeyedRegistrationLocator
    {
        private readonly Type serviceType;

        internal RegistrationDictionary(Type serviceType)
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
}