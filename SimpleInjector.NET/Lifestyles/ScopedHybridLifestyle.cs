#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2015 Simple Injector Contributors
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
    using System.Diagnostics.CodeAnalysis;

    internal sealed class ScopedHybridLifestyle : ScopedLifestyle
    {
        private readonly Func<bool> lifestyleSelector;
        private readonly ScopedLifestyle trueLifestyle;
        private readonly ScopedLifestyle falseLifestyle;

        internal ScopedHybridLifestyle(Func<bool> lifestyleSelector, ScopedLifestyle trueLifestyle,
            ScopedLifestyle falseLifestyle)
            : base("Hybrid " + GetHybridName(trueLifestyle) + " / " + GetHybridName(falseLifestyle))
        {
            this.lifestyleSelector = lifestyleSelector;
            this.trueLifestyle = trueLifestyle;
            this.falseLifestyle = falseLifestyle;
        }

        protected override int Length
        {
            get { throw new NotSupportedException("The length property is not supported for this lifestyle."); }
        }

        private ScopedLifestyle CurrentLifestyle
        {
            get { return this.lifestyleSelector() ? this.trueLifestyle : this.falseLifestyle; }
        }

        internal override int ComponentLength(Container container)
        {
            return Math.Max(
                this.trueLifestyle.ComponentLength(container), 
                this.falseLifestyle.ComponentLength(container));
        }

        internal override int DependencyLength(Container container)
        {
            return Math.Min(
                this.trueLifestyle.DependencyLength(container), 
                this.falseLifestyle.DependencyLength(container));
        }

        internal string GetHybridName()
        {
            return GetHybridName(this.trueLifestyle) + " / " + GetHybridName(this.falseLifestyle);
        }

        protected internal override Func<Scope> CreateCurrentScopeProvider(Container container)
        {
            var trueProvider = this.trueLifestyle.CreateCurrentScopeProvider(container);
            var falseProvider = this.falseLifestyle.CreateCurrentScopeProvider(container);

            // NOTE: It is important to return a delegate that evaluates the lifestyleSelector on each call,
            // instead of evaluating the lifestyleSelector directly and returning either the trueProvider or
            // falseProvider. That behavior would be completely flawed, because that would burn the lifestyle
            // that is active during the compilation of the InstanceProducer's delegate right into that
            // delegate making the other lifestyle unavailable.
            return () => this.lifestyleSelector() ? trueProvider() : falseProvider();
        }

        protected override Scope GetCurrentScopeCore(Container container)
        {
            return this.CurrentLifestyle.GetCurrentScope(container);
        }

        private static string GetHybridName(Lifestyle lifestyle)
        {
            return HybridLifestyle.GetHybridName(lifestyle);
        }
    }
}