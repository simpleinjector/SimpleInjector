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
    using System.ServiceModel;

    /// <summary>
    /// This service host is used to set up the service behavior that replaces the instance provider to use 
    /// dependency injection.
    /// </summary>
    public class SimpleInjectorServiceHost : ServiceHost
    {
        private readonly Container container;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorServiceHost"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="baseAddresses">The base addresses.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/> is a null
        /// reference or <paramref name="serviceType"/> is a null reference.</exception>
        public SimpleInjectorServiceHost(Container container, Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            Requires.IsNotNull(container, "container");

            this.container = container;
        }

        /// <summary>Opens the channel dispatchers.</summary>
        protected override void OnOpening()
        {
            this.Description.Behaviors.Add(new SimpleInjectorServiceBehavior(this.container));

            base.OnOpening();
        }
    }
}