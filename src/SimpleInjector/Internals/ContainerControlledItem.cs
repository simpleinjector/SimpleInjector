#region Copyright Simple Injector Contributors
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

namespace SimpleInjector.Internals
{
    using System;
    
    /// <summary>
    /// Container controlled collections can be supplied with both Type objects or direct Registration
    /// instances.
    /// </summary>
    internal sealed class ContainerControlledItem
    {
        /// <summary>Will never be null. Can be open-generic.</summary>
        public readonly Type ImplementationType;

        /// <summary>Can be null.</summary>
        public readonly Registration Registration;

        private ContainerControlledItem(Registration registration)
        {
            Requires.IsNotNull(registration, nameof(registration));
            this.Registration = registration;
            this.ImplementationType = registration.ImplementationType;
        }

        private ContainerControlledItem(Type implementationType)
        {
            Requires.IsNotNull(implementationType, nameof(implementationType));
            this.ImplementationType = implementationType;
        }

        public static ContainerControlledItem CreateFromRegistration(Registration registration) => 
            new ContainerControlledItem(registration);

        public static ContainerControlledItem CreateFromType(Type implementationType) => 
            new ContainerControlledItem(implementationType);
    }
}