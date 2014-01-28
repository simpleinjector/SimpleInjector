#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2014 Simple Injector Contributors
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

        internal override int ComponentLength
        {
            get { return Math.Max(this.trueLifestyle.ComponentLength, this.falseLifestyle.ComponentLength); }
        }

        internal override int DependencyLength
        {
            get { return Math.Min(this.trueLifestyle.DependencyLength, this.falseLifestyle.DependencyLength); }
        }

        protected override int Length
        {
            get { throw new NotSupportedException("The length property is not supported for this lifestyle."); }
        }

        private ScopedLifestyle CurrentLifestyle
        {
            get { return this.lifestyleSelector() ? this.trueLifestyle : this.falseLifestyle; }
        }

        // NOTE: Since the ScopedLifestyle.WhenScopeEnds is marked as virtual (which is unfortunate legacy), 
        // custom scoped lifestyle implementations can override it, and we must therefore override it here to 
        // make sure the custom WhenScopeEnds is called (not overriding it will skip the WhenScopeEnds of the 
        // lifestyle and will forward the call directly to the Scope.
        public override void WhenScopeEnds(Container container, Action action)
        {
            this.CurrentLifestyle.WhenScopeEnds(container, action);
        }

        // NOTE: This method is overridden for the same reason as WhenScopeEnds is.
        public override void RegisterForDisposal(Container container, IDisposable disposable)
        {
            this.CurrentLifestyle.RegisterForDisposal(container, disposable);
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

        private static string GetHybridName(Lifestyle lifestyle)
        {
            return HybridLifestyle.GetHybridName(lifestyle);
        }
    }
}