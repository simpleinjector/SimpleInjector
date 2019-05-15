// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;

    internal sealed class LifestyleSelectorScopedHybridLifestyle : ScopedLifestyle, IHybridLifestyle
    {
        private readonly Predicate<Container> selector;
        private readonly ScopedLifestyle trueLifestyle;
        private readonly ScopedLifestyle falseLifestyle;

        internal LifestyleSelectorScopedHybridLifestyle(
            Predicate<Container> lifestyleSelector,
            ScopedLifestyle trueLifestyle,
            ScopedLifestyle falseLifestyle)
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

        protected internal override Func<Scope?> CreateCurrentScopeProvider(Container container)
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

        protected override Scope? GetCurrentScopeCore(Container container) =>
            this.CurrentLifestyle(container).GetCurrentScope(container);

        private ScopedLifestyle CurrentLifestyle(Container container) =>
            this.selector(container) ? this.trueLifestyle : this.falseLifestyle;

        private static string GetHybridName(Lifestyle lifestyle) => HybridLifestyle.GetHybridName(lifestyle);
    }
}