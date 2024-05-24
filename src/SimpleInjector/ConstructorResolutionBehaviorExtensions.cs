// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Reflection;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Extension methods for <see cref="IConstructorResolutionBehavior"/>.
    /// </summary>
    public static class ConstructorResolutionBehaviorExtensions
    {
        /// <summary>
        /// Gets the given <paramref name="implementationType"/>'s constructor that can be used by the
        /// container to create that instance.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        /// <param name="implementationType">Type of the implementation to find a suitable constructor for.</param>
        /// <returns>
        /// The <see cref="ConstructorInfo"/>. This method never returns null.
        /// </returns>
        /// <exception cref="ActivationException">Thrown when no suitable constructor could be found.</exception>
        public static ConstructorInfo GetConstructor(
            this IConstructorResolutionBehavior behavior, Type implementationType)
        {
            // NOTE: In v5, both IConstructorResolutionBehavior's and IDependencyInjectionBehavior's method
            // signatures changed (see #557). This is both a binary and code breaking change, which affects
            // anyone implementing an IConstructorResolutionBehavior. There's nothing much we can do about
            // this (since fixing #557 was more important). By adding this extension method, however, we can
            // reduce the pain for anyone using (calling) this interface. This extension method duplicates the
            // signature and behavior of the old method. This still makes the change binary incompatible, even
            // from perspective of the caller, but it would allow their code to keep compiling (in most cases).
            Requires.IsNotNull(behavior, nameof(behavior));
            Requires.IsNotNull(implementationType, nameof(implementationType));

            return behavior.TryGetConstructor(implementationType, out string? message)
                ?? throw BuildActivationException(behavior, implementationType, message);
        }

        private static ActivationException BuildActivationException(
            IConstructorResolutionBehavior behavior, Type implementationType, string? message) => new(
                message is null || string.IsNullOrWhiteSpace(message)
                    ? StringResources.TypeHasNoInjectableConstructorAccordingToCustomResolutionBehavior(
                        behavior, implementationType)
                    : message);
    }
}