﻿#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015 Simple Injector Contributors
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

namespace SimpleInjector.Integration.AspNet
{
    using System;
    using Microsoft.AspNet.Mvc;
    using Microsoft.AspNet.Mvc.Controllers;

    /// <summary>
    /// Controller activator for Simple Injector.
    /// </summary>
    public class SimpleInjectorControllerActivator : IControllerActivator
    {
        private readonly Container container;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorControllerActivator"/> class.
        /// </summary>
        /// <param name="container">The container instance.</param>
        public SimpleInjectorControllerActivator(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            this.container = container;
        }

        /// <summary>Creates a controller.</summary>
        /// <param name="context">The Microsoft.AspNet.Mvc.ActionContext for the executing action.</param>
        /// <param name="controllerType">The controller type to create.</param>
        /// <returns>A new controller instance.</returns>
        public object Create(ActionContext context, Type controllerType) => 
            this.container.GetInstance(controllerType);
    }
}