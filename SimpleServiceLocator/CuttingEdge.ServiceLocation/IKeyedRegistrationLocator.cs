namespace CuttingEdge.ServiceLocation
{
    /// <summary>Defines an interface for retrieving instances by a key.</summary>
    internal interface IKeyedRegistrationLocator
    {
        /// <summary>Gets the instance using the specified <paramref name="key"/>.</summary>
        /// <param name="key">The key to get the instance with.</param>
        /// <returns>Gets an instance by a given <paramref name="key"/>; never returns null.</returns>
        /// <exception cref="ActivationException">Thrown when something went wrong :-).</exception>
        object Get(string key);
    }
}