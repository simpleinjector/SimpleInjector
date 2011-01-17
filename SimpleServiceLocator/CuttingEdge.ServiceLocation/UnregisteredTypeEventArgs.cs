using System;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Provides data for and interaction with the 
    /// <see cref="SimpleServiceLocator.ResolveUnregisteredType">ResolveUnregisteredType</see> event of 
    /// the <see cref="SimpleServiceLocator"/>. An observer can check the 
    /// <see cref="UnregisteredServiceType"/> to see whether the unregistered type can be handled. The
    /// <see cref="Register"/> method can be called to register a <see cref="Func{T}"/> delegate that
    /// allows creation of instances of the unregistered for this and future requests.
    /// </summary>
    public class UnregisteredTypeEventArgs : EventArgs
    {
        /// <summary>Initializes a new instance of the UnregisteredTypeEventArgs class.</summary>
        /// <param name="unregisteredServiceType">The unregistered service type.</param>
        public UnregisteredTypeEventArgs(Type unregisteredServiceType)
        {
            this.UnregisteredServiceType = unregisteredServiceType;
        }

        /// <summary>Gets the unregistered service type that is currently requested.</summary>
        /// <value>The unregistered service type that is currently requested.</value>
        public Type UnregisteredServiceType { get; private set; }
        
        internal Func<object> InstanceCreator { get; private set; }

        internal bool Handled
        {
            get { return this.InstanceCreator != null; }
        }

        /// <summary>
        /// Registers a <see cref="Func{T}"/> delegate that allows creation of instances of the type
        /// expressed by the <see cref="UnregisteredServiceType"/> for this and future requests. The delegate
        /// will be caches and future requests will directly call that delegate.
        /// </summary>
        /// <param name="instanceCreator">The delegate that allows creation of instances of the type
        /// expressed by the <see cref="UnregisteredServiceType"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="instanceCreator"/> is a
        /// null reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown when multiple observers that have registered to
        /// the <see cref="SimpleServiceLocator.ResolveUnregisteredType">ResolveUnregisteredType</see> event
        /// called this method for the same type.</exception>
        public void Register(Func<object> instanceCreator)
        {
            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            if (this.Handled)
            {
                throw new InvalidOperationException(StringResources.MultipleObserversRegisteredTheSameType(
                    this.UnregisteredServiceType));
            }

            this.InstanceCreator = instanceCreator;
        }
    }
}