// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;

    /// <summary>
    /// Forwards CreateRegistration calls to the lifestyle that is returned from the registered
    /// container.Options.LifestyleSelectionBehavior.
    /// </summary>
    internal sealed class LifestyleSelectionBehaviorProxyLifestyle : Lifestyle
    {
        private readonly ContainerOptions options;

        public LifestyleSelectionBehaviorProxyLifestyle(ContainerOptions options)
            : base("Based On LifestyleSelectionBehavior")
        {
            this.options = options;
        }

        public override int Length => throw new NotImplementedException();

        protected internal override Registration CreateRegistrationCore<TConcrete>(Container container) =>
            this.options.SelectLifestyle(typeof(TConcrete))
                .CreateRegistration<TConcrete>(container);

        protected internal override Registration CreateRegistrationCore<TService>(
            Func<TService> instanceCreator, Container container) =>
            this.options.SelectLifestyle(typeof(TService))
                .CreateRegistration(instanceCreator, container);
    }
}