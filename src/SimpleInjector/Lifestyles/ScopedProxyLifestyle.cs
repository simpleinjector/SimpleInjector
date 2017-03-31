#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015-2016 Simple Injector Contributors
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

    internal sealed class ScopedProxyLifestyle : ScopedLifestyle
    {
        public ScopedProxyLifestyle() : base("Scoped")
        {
        }

        internal override int ComponentLength(Container container) => 
            GetDefaultScopedLifestyle(container).ComponentLength(container);

        internal override int DependencyLength(Container container) =>
            GetDefaultScopedLifestyle(container).DependencyLength(container);

        protected internal override Func<Scope> CreateCurrentScopeProvider(Container container) => 
            GetDefaultScopedLifestyle(container).CreateCurrentScopeProvider(container);

        protected internal override Registration CreateRegistrationCore<TConcrete>(Container container) =>
            GetDefaultScopedLifestyle(container).CreateRegistrationCore<TConcrete>(container);

        protected internal override Registration CreateRegistrationCore<TService>(Func<TService> creator, Container c) => 
            GetDefaultScopedLifestyle(c).CreateRegistrationCore<TService>(creator, c);

        protected override Scope GetCurrentScopeCore(Container container) =>
            GetDefaultScopedLifestyle(container).GetCurrentScope(container);

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static ScopedLifestyle GetDefaultScopedLifestyle(Container container) => 
            container.Options.DefaultScopedLifestyle ?? ThrowDefaultScopeLifestyleIsNotSet();

        private static ScopedLifestyle ThrowDefaultScopeLifestyleIsNotSet()
        {
            throw new InvalidOperationException(
                "To be able to use the Lifestyle.Scoped property, please ensure that the container is " +
                "configured with a default scoped lifestyle by setting the Container.Options." +
                "DefaultScopedLifestyle property with the required scoped lifestyle for your type of " +
                "application. See: https://simpleinjector.org/lifestyles#scoped");
        }
    }
}