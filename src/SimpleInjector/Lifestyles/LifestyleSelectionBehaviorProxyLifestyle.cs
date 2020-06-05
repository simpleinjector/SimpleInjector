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

        // TODO: CreateRegistrationCore calls into CreateRegistration of the selected lifestyle, but I'm
        // wondering whether this is correct. A call to CreateRegistrationCore means always creating a new
        // instance, but CreateRegistration might return a cached one. Calling CreateRegistrationCore instead
        // doesn't break a test, so we're clearly missing a test. Now the question becomes: what is the
        // correct behavior???
        protected internal override Registration CreateRegistrationCore(Type concreteType, Container container) =>
            this.options.SelectLifestyle(concreteType)
                .CreateRegistration(concreteType, container);

        protected internal override Registration CreateRegistrationCore<TService>(
            Func<TService> instanceCreator, Container container) =>
            this.options.SelectLifestyle(typeof(TService))
                .CreateRegistration(instanceCreator, container);
    }
}