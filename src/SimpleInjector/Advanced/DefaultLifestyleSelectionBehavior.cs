// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{Lifestyle.Name,nq}LifestyleSelectionBehavior")]
    internal sealed class DefaultLifestyleSelectionBehavior : ILifestyleSelectionBehavior
    {
        private readonly ContainerOptions options;

        internal DefaultLifestyleSelectionBehavior(ContainerOptions options) => this.options = options;

        public Lifestyle SelectLifestyle(Type implementationType)
        {
            Requires.IsNotNull(implementationType, nameof(implementationType));

            return this.options.DefaultLifestyle;
        }
    }
}