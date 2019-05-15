// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    [DebuggerDisplay(nameof(DefaultPropertySelectionBehavior))]
    internal sealed class DefaultPropertySelectionBehavior : IPropertySelectionBehavior
    {
        // The default behavior is to not inject any properties. This has the following rational:
        // 1. We don't want to do implicit property injection (where all properties are skipped that
        //    can't be injected), because this leads to a configuration that is hard to verify.
        // 2. We can't do explicit property injection, because this required users to use a
        //    framework-defined attribute and application code should not depend on the DI container.
        // 3. In general, property injection should not be used, since this leads to Temporal Coupling.
        //    Constructor injection should be used, and if a constructor gets too many parameters
        //    (constructor over-injection), this is an indication of a violation of the SRP.
        public bool SelectProperty(Type implementationType, PropertyInfo propertyInfo) => false;
    }
}