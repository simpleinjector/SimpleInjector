// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;

    internal sealed class UnknownLifestyle : Lifestyle
    {
        internal UnknownLifestyle()
            : base("Unknown")
        {
        }

        public override int Length =>
            throw new NotSupportedException("The length property is not supported for this lifestyle.");

        internal override int ComponentLength(Container container) => Singleton.ComponentLength(container);

        internal override int DependencyLength(Container container) => Transient.DependencyLength(container);

        protected internal override Registration CreateRegistrationCore<TConcrete>(Container container) =>
            throw new InvalidOperationException(
                "The unknown lifestyle does not allow creation of registrations.");

        protected internal override Registration CreateRegistrationCore<TService>(
            Func<TService> instanceCreator, Container container) =>
            throw new InvalidOperationException(
                "The unknown lifestyle does not allow creation of registrations.");
    }
}