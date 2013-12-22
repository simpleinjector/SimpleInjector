#region Copyright (c) 2013 Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
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

        public override void WhenScopeEnds(Container container, Action action)
        {
            this.CurrentLifestyle.WhenScopeEnds(container, action);
        }

        public override void RegisterForDisposal(Container container, IDisposable disposable)
        {
            this.CurrentLifestyle.RegisterForDisposal(container, disposable);
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "See base.CreateRegistration for more info.")]
        protected override Registration CreateRegistrationCore<TService, TImplementation>(Container container)
        {
            return new HybridRegistration(typeof(TService), typeof(TImplementation), this.lifestyleSelector,
                this.trueLifestyle.CreateRegistration<TService, TImplementation>(container),
                this.falseLifestyle.CreateRegistration<TService, TImplementation>(container),
                this, container);
        }

        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container)
        {
            return new HybridRegistration(typeof(TService), typeof(TService), this.lifestyleSelector,
                this.trueLifestyle.CreateRegistration<TService>(instanceCreator, container),
                this.falseLifestyle.CreateRegistration<TService>(instanceCreator, container),
                this, container);
        }

        private string GetHybridName()
        {
            return GetHybridName(this.trueLifestyle) + " / " + GetHybridName(this.falseLifestyle);
        }

        private static string GetHybridName(Lifestyle lifestyle)
        {
            var hybrid = lifestyle as ScopedHybridLifestyle;

            return hybrid != null ? hybrid.GetHybridName() : (lifestyle != null ? lifestyle.Name : "Null");
        }
    }
}
