#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2016 Simple Injector Contributors
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

    // Lifestyle explicitly to allow resolve and inject a SimpleInjector.Scope itself.
    internal sealed class ScopedScopeLifestyle : ScopedLifestyle
    {
        internal static readonly ScopedScopeLifestyle Instance = new ScopedScopeLifestyle();

        internal ScopedScopeLifestyle() : base("Scoped")
        {
        }

        protected internal override Func<Scope> CreateCurrentScopeProvider(Container c) => 
            () => this.GetScopeFromDefaultScopedLifestyle(c);

        protected override Scope GetCurrentScopeCore(Container c) => this.GetScopeFromDefaultScopedLifestyle(c);

        private Scope GetScopeFromDefaultScopedLifestyle(Container container)
        {
            ScopedLifestyle lifestyle = container.Options.DefaultScopedLifestyle;

            if (lifestyle != null)
            {
                return lifestyle.GetCurrentScope(container) ?? ThrowThereIsNoActiveScopeException();
            }

            return null;
        }

        private static Scope ThrowThereIsNoActiveScopeException()
        {
            throw new InvalidOperationException("There is no active scope.");
        }
    }
}