#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
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

using SimpleInjector;

namespace CommonServiceLocator.SimpleInjectorAdapter
{
    /// <summary>
    /// Translates calls from <see cref="IServiceLocator"/> to Simple Injector's container.
    /// </summary>
    public class SimpleInjectorServiceLocatorAdapter : IServiceLocator
    {
        private const string NotSupportedMessage = "Keyed registration is not supported by the Simple Injector.";

        private readonly Container container;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorServiceLocatorAdapter"/> class.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> to adapt.</param>
        public SimpleInjectorServiceLocatorAdapter(Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            this.container = container;
        }

        /// <summary>
        /// Get all instances of the given <typeparamref name="TService" /> currently
        /// registered in the container.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <exception cref="T:Microsoft.Practices.ServiceLocation.ActivationException">if there is are errors resolving
        /// the service instance.</exception>
        /// <returns>A sequence of instances of the requested <typeparamref name="TService" />.</returns>
        public IEnumerable<TService> GetAllInstances<TService>()
        {
            try
            {
                return this.container.GetAllInstances<TService>();
            }
            catch (SimpleInjector.ActivationException ex)
            {
                throw new Microsoft.Practices.ServiceLocation.ActivationException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Get all instances of the given <paramref name="serviceType" /> currently
        /// registered in the container.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <exception cref="T:Microsoft.Practices.ServiceLocation.ActivationException">if there is are errors resolving
        /// the service instance.</exception>
        /// <returns>A sequence of instances of the requested <paramref name="serviceType" />.</returns>
        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            try
            {
                return this.container.GetAllInstances(serviceType);
            }
            catch (SimpleInjector.ActivationException ex)
            {
                throw new Microsoft.Practices.ServiceLocation.ActivationException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Get an instance of the given named <typeparamref name="TService" />.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <param name="key">Name the object was registered with.</param>
        /// <exception cref="T:Microsoft.Practices.ServiceLocation.ActivationException">if there is are errors resolving
        /// the service instance.</exception>
        /// <exception cref="NotSupportedException">Thrown when a non-null key is requested. Keyed 
        /// registration is not supported by the Simple Injector.</exception>
        /// <returns>The requested service instance.</returns>
        public TService GetInstance<TService>(string key)
        {
            if (key == null)
            {
                return (TService)this.GetInstance(typeof(TService));
            }
            else
            {
                throw new NotSupportedException(NotSupportedMessage);
            }
        }

        /// <summary>
        /// Get an instance of the given <typeparamref name="TService" />.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <exception cref="T:Microsoft.Practices.ServiceLocation.ActivationException">if there is are errors resolving
        /// the service instance.</exception>
        /// <returns>The requested service instance.</returns>
        public TService GetInstance<TService>()
        {
            try
            {
                return (TService)this.container.GetInstance(typeof(TService));
            }
            catch (SimpleInjector.ActivationException ex)
            {
                throw new Microsoft.Practices.ServiceLocation.ActivationException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Get an instance of the given named <paramref name="serviceType" />.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <param name="key">Name the object was registered with.</param>
        /// <exception cref="T:Microsoft.Practices.ServiceLocation.ActivationException">if there is an error resolving
        /// the service instance.</exception>
        /// <exception cref="NotSupportedException">Thrown when a non-null key is requested. Keyed 
        /// registration is not supported by the Simple Injector.</exception>
        /// <returns>The requested service instance.</returns>
        public object GetInstance(Type serviceType, string key)
        {
            if (key == null)
            {
                return this.GetInstance(serviceType);
            }
            else
            {
                throw new NotSupportedException(NotSupportedMessage);
            }
        }

        /// <summary>
        /// Get an instance of the given <paramref name="serviceType" />.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <exception cref="T:Microsoft.Practices.ServiceLocation.ActivationException">if there is an error resolving
        /// the service instance.</exception>
        /// <returns>The requested service instance.</returns>
        public object GetInstance(Type serviceType)
        {
            try
            {
                return this.container.GetInstance(serviceType);
            }
            catch (SimpleInjector.ActivationException ex)
            {
                throw new Microsoft.Practices.ServiceLocation.ActivationException(ex.Message, ex);
            }  
        }

        /// <summary>Gets the service object of the specified type.</summary>
        /// <returns>A service object of type serviceType.-or- null if there is no service object of type serviceType.</returns>
        /// <param name="serviceType">An object that specifies the type of service object to get. </param>
        object IServiceProvider.GetService(Type serviceType)
        {
            try
            {
                IServiceProvider serviceProvider = this.container;

                return serviceProvider.GetService(serviceType);
            }
            catch (SimpleInjector.ActivationException ex)
            {
                throw new Microsoft.Practices.ServiceLocation.ActivationException(ex.Message, ex);
            }
        }
    }
}