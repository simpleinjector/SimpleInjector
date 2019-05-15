// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    using SimpleInjector.Advanced;

    internal static class LifestyleMismatchChecker
    {
        internal static bool HasLifestyleMismatch(Container container, KnownRelationship relationship)
        {
            Lifestyle componentLifestyle = relationship.Lifestyle;
            Lifestyle dependencyLifestyle = relationship.Dependency.Lifestyle;

            // If the lifestyles are the same instance, we consider them valid, even though in theory
            // an hybrid lifestyle could screw things up. In practice this would be very unlikely, since
            // the Func<bool> test delegate would typically return the same value within a given context.
            if (object.ReferenceEquals(componentLifestyle, dependencyLifestyle)
                && componentLifestyle != Lifestyle.Unknown)
            {
                return false;
            }

            return componentLifestyle.ComponentLength(container) > dependencyLifestyle.DependencyLength(container);
        }
    }
}