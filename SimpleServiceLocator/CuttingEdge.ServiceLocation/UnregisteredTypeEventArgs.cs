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

using Microsoft.Practices.ServiceLocation;

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
                throw new ActivationException(StringResources.MultipleObserversRegisteredTheSameType(
                    this.UnregisteredServiceType));
            }

            this.InstanceCreator = instanceCreator;
        }
    }
}