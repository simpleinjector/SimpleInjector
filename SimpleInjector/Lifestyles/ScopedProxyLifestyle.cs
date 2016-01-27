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

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class ScopedProxyLifestyle : ScopedLifestyle
    {
        public ScopedProxyLifestyle() : base("Scoped")
        {
        }

        internal override int ComponentLength(Container container)
        {
            return GetDefaultScopedLifestyle(container).ComponentLength(container);
        }

        internal override int DependencyLength(Container container)
        {
            return GetDefaultScopedLifestyle(container).DependencyLength(container);
        }

        protected internal override Func<Scope> CreateCurrentScopeProvider(Container container)
        {
            ScopedLifestyle lifestyle = GetDefaultScopedLifestyle(container);

            return lifestyle.CreateCurrentScopeProvider(container);
        }

        protected override Scope GetCurrentScopeCore(Container container)
        {
            ScopedLifestyle lifestyle = GetDefaultScopedLifestyle(container);

            return lifestyle.GetCurrentScope(container);
        }

        protected override Registration CreateRegistrationCore<TService, TImplementation>(Container container)
        {
            ScopedLifestyle lifestyle = GetDefaultScopedLifestyle(container);

            return lifestyle.CreateRegistration<TService, TImplementation>(container);
        }

        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container)
        {
            ScopedLifestyle lifestyle = GetDefaultScopedLifestyle(container);

            return lifestyle.CreateRegistration<TService>(instanceCreator, container);
        }

#if NET45
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static ScopedLifestyle GetDefaultScopedLifestyle(Container container)
        {
            var lifestyle = container.Options.DefaultScopedLifestyle;

            if (lifestyle == null)
            {
                ThrowDefaultScopeLifestyleIsNotSet();
            }

            return lifestyle;
        }

        private static void ThrowDefaultScopeLifestyleIsNotSet()
        {
            throw new InvalidOperationException(
                "To be able to use the Lifestyle.Scoped property, please ensure that the container is " +
                "configured with a default scoped lifestyle by setting the Container.Options." +
                "DefaultScopedLifestyle property with the required scoped lifestyle for your type of " +
                "application.");
        }
    }
}