﻿#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014 Simple Injector Contributors
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

namespace SimpleInjector.Lifestyles
{
    using System;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Forwards CreateRegistration calls to the lifestyle that is returned from the registered
    /// container.Options.LifestyleSelectionBehavior.
    /// </summary>
    internal sealed class LifestyleSelectionBehaviorProxyLifestyle : Lifestyle
    {
        private readonly ContainerOptions options;

        public LifestyleSelectionBehaviorProxyLifestyle(ContainerOptions options)
            : base("Based On LifestyleSelectionBehavior")
        {
            this.options = options;
        }

        protected override int Length
        {
            get { throw new NotImplementedException(); }
        }

        protected override Registration CreateRegistrationCore<TService, TImplementation>(Container container)
        {
            Lifestyle lifestyle = this.options.SelectLifestyle(typeof(TService), typeof(TImplementation));

            return lifestyle.CreateRegistration<TService, TImplementation>(container);
        }

        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container)
        {
            Lifestyle lifestyle = this.options.SelectLifestyle(typeof(TService), typeof(TService));

            return lifestyle.CreateRegistration<TService>(instanceCreator, container);
        }
    }
}