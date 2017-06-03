#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2017 Simple Injector Contributors
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

    internal sealed class DefaultFallbackScopedHybridLifestyle : ScopedLifestyle, IHybridLifestyle
    {
        private readonly ScopedLifestyle defaultLifestyle;
        private readonly ScopedLifestyle fallbackLifestyle;

        internal DefaultFallbackScopedHybridLifestyle(ScopedLifestyle defaultLifestyle, ScopedLifestyle fallbackLifestyle)
            : base("Hybrid " + GetHybridName(defaultLifestyle) + " / " + GetHybridName(fallbackLifestyle))
        {
            this.defaultLifestyle = defaultLifestyle;
            this.fallbackLifestyle = fallbackLifestyle;
        }

        string IHybridLifestyle.GetHybridName() =>
            GetHybridName(this.defaultLifestyle) + " / " + GetHybridName(this.fallbackLifestyle);

        internal override int ComponentLength(Container container) =>
            Math.Max(
                this.defaultLifestyle.ComponentLength(container),
                this.fallbackLifestyle.ComponentLength(container));

        internal override int DependencyLength(Container container) =>
            Math.Min(
                this.defaultLifestyle.DependencyLength(container),
                this.fallbackLifestyle.DependencyLength(container));

        protected internal override Func<Scope> CreateCurrentScopeProvider(Container container)
        {
            var defaultProvider = this.defaultLifestyle.CreateCurrentScopeProvider(container);
            var fallbackProvider = this.fallbackLifestyle.CreateCurrentScopeProvider(container);

            // NOTE: It is important to return a delegate that evaluates the lifestyleSelector on each call,
            // instead of evaluating the lifestyleSelector directly and returning either the trueProvider or
            // falseProvider. That behavior would be completely flawed, because that would burn the lifestyle
            // that is active during the compilation of the InstanceProducer's delegate right into that
            // delegate making the other lifestyle unavailable.
            return () => defaultProvider() ?? fallbackProvider();
        }

        protected override Scope GetCurrentScopeCore(Container container) =>
            this.defaultLifestyle.GetCurrentScope(container) ?? this.fallbackLifestyle.GetCurrentScope(container);

        private static string GetHybridName(Lifestyle lifestyle) => HybridLifestyle.GetHybridName(lifestyle);
    }
}