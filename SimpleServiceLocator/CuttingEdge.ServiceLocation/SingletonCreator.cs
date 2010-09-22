using System;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Ensures that the wrapped delegate will only be executed once.
    /// </summary>
    /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
    internal sealed class SingletonCreator<T> where T : class
    {
        private Func<T> singleInstanceCreator;
        private bool instanceCreated;
        private T instance;

        internal SingletonCreator(Func<T> singleInstanceCreator)
        {
            this.singleInstanceCreator = singleInstanceCreator;
        }

        internal object GetInstance()
        {
            // We use a lock to prevent the delegate to be called more than once during the lifetime of
            // the application. We use a double checked lock to prevent the lock statement from being 
            // called again after the instance was created.
            if (!this.instanceCreated)
            {
                // We can take a lock on this, because this class is private.
                lock (this)
                {
                    if (!this.instanceCreated)
                    {
                        this.instance = this.singleInstanceCreator();
                        this.instanceCreated = true;

                        // Remove the reference to the delegate; it is not needed anymore.
                        this.singleInstanceCreator = null;
                    }
                }
            }

            return this.instance;
        }
    }
}