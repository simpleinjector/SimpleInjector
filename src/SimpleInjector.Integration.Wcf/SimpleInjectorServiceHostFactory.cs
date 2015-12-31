#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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

namespace SimpleInjector.Integration.Wcf
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.ServiceModel.Activation;

    /// <summary>
    /// Factory that provides instances of <see cref="SimpleInjectorServiceHost"/> in managed hosting 
    /// environments where the host instance is created dynamically in response to incoming messages.
    /// </summary>
    public class SimpleInjectorServiceHostFactory : ServiceHostFactory
    {
        private static Container container;

        /// <summary>Sets the container.</summary>
        /// <param name="container">The container.</param>
        public static void SetContainer(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            if (SimpleInjectorServiceHostFactory.container != null)
            {
                throw new InvalidOperationException(
                    "This method can only be called once. The container has already been set.");
            }

            SimpleInjectorServiceHostFactory.container = container;
        }

        /// <summary>
        /// Creates a <see cref="SimpleInjectorServiceHost"/> for a specified type of service with a specific 
        /// base address. 
        /// </summary>
        /// <returns>
        /// A <see cref="SimpleInjectorServiceHost"/> for the type of service specified with a specific base 
        /// address.
        /// </returns>
        /// <param name="serviceType">
        /// Specifies the type of service to host. 
        /// </param>
        /// <param name="baseAddresses">
        /// The <see cref="T:System.Array"/> of type <see cref="T:System.Uri"/> that contains the base 
        /// addresses for the service hosted.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceType"/> is a null 
        /// reference.</exception>
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));

            if (container == null)
            {
                throw new InvalidOperationException("The operation failed. Please call the " +
                    typeof(SimpleInjectorServiceHostFactory).FullName + ".SetContainer(Container) method " +
                    "supplying the application's " + typeof(Container).FullName + " instance during " +
                    "application startup (for instance inside the Application_Start event of the Global.asax). " +
                    "In case you're running on non-HTTP protocols such as net.tcp and net.pipe that is " +
                    "supported by the Windows Activation Service (WAS), please see the WCF integration " +
                    "documentation: https://simpleinjector.org/wcf.");
            }

            return new SimpleInjectorServiceHost(container, serviceType, baseAddresses);
        }
    }
}