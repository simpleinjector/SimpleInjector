#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2016 Simple Injector Contributors
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

    internal sealed class LifestyleSelectorScopedHybridLifestyle : ScopedLifestyle, IHybridLifestyle
    {
        private readonly Predicate<Container> selector;
        private readonly ScopedLifestyle trueLifestyle;
        private readonly ScopedLifestyle falseLifestyle;

        internal LifestyleSelectorScopedHybridLifestyle(
            Predicate<Container> lifestyleSelector, ScopedLifestyle trueLifestyle, ScopedLifestyle falseLifestyle)
            : base("Hybrid " + GetHybridName(trueLifestyle) + " / " + GetHybridName(falseLifestyle))
        {
            this.selector = lifestyleSelector;
            this.trueLifestyle = trueLifestyle;
            this.falseLifestyle = falseLifestyle;
        }

        string IHybridLifestyle.GetHybridName() =>
            GetHybridName(this.trueLifestyle) + " / " + GetHybridName(this.falseLifestyle);

        internal override int ComponentLength(Container container) =>
            Math.Max(
                this.trueLifestyle.ComponentLength(container),
                this.falseLifestyle.ComponentLength(container));

        internal override int DependencyLength(Container container) =>
            Math.Min(
                this.trueLifestyle.DependencyLength(container),
                this.falseLifestyle.DependencyLength(container));

        protected internal override Func<Scope> CreateCurrentScopeProvider(Container container)
        {
            var selector = this.selector;
            var trueProvider = this.trueLifestyle.CreateCurrentScopeProvider(container);
            var falseProvider = this.falseLifestyle.CreateCurrentScopeProvider(container);

            // NOTE: It is important to return a delegate that evaluates the lifestyleSelector on each call,
            // instead of evaluating the lifestyleSelector directly and returning either the trueProvider or
            // falseProvider. That behavior would be completely flawed, because that would burn the lifestyle
            // that is active during the compilation of the InstanceProducer's delegate right into that
            // delegate making the other lifestyle unavailable.
            return () => selector(container) ? trueProvider() : falseProvider();
        }

        protected override Scope GetCurrentScopeCore(Container container) =>
            this.CurrentLifestyle(container).GetCurrentScope(container);

        private ScopedLifestyle CurrentLifestyle(Container container) =>
            this.selector(container) ? this.trueLifestyle : this.falseLifestyle;

        private static string GetHybridName(Lifestyle lifestyle) => HybridLifestyle.GetHybridName(lifestyle);
    }
}