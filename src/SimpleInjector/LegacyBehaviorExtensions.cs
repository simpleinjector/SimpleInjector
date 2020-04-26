// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Reflection;
    using SimpleInjector.Advanced;

    // NOTE: In v5, both IConstructorResolutionBehavior's and IDependencyInjectionBehavior's method signatures
    // changed (see #557). This is both a binary and code breaking change, which will affect anyone
    // implementing an IConstructorResolutionBehavior. There's nothing much we can do about this (since fixing
    // #557 was more important). By adding these extension methods, however, we can reduce the pain for anyone
    // using (calling) these interfaces. These extension methods duplicate the signature and behavior of the
    // old methods. This still makes the change binary incompatible, even from perspective of the caller, but
    // it would allow their code to keep compiling (in most cases).

    /// <summary>
    /// Extension methods for <see cref="IConstructorResolutionBehavior"/> and
    /// <see cref="IDependencyInjectionBehavior"/> to mimic v4.x behavior.
    /// </summary>
    public static class LegacyBehaviorExtensions
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
            Requires.IsNotNull(behavior, nameof(behavior));
            Requires.IsNotNull(implementationType, nameof(implementationType));

            return behavior.TryGetConstructor(implementationType, out string? message)
                ?? throw BuildActivationException(behavior, implementationType, message);
        }

        /// <summary>Verifies the specified <paramref name="consumer"/>.</summary>
        /// <param name="behavior">The behavior.</param>
        /// <param name="consumer">Contextual information about the consumer where the built dependency is
        /// injected into.</param>
        /// <exception cref="ActivationException">
        /// Thrown when the type of the <see cref="InjectionConsumerInfo.Target">target</see> supplied with
        /// the supplied <paramref name="consumer"/> cannot be used for auto wiring.</exception>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference.</exception>
        public static void Verify(
            this IDependencyInjectionBehavior behavior, InjectionConsumerInfo consumer)
        {
            Requires.IsNotNull(behavior, nameof(behavior));
            Requires.IsNotNull(consumer, nameof(consumer));

            if (!behavior.VerifyDependency(consumer, out string? message))
            {
                throw new ActivationException(
                     message is null || string.IsNullOrWhiteSpace(message)
                        ? StringResources.DependencyNotValidForInjectionAccordingToCustomInjectionBehavior(
                            behavior, consumer)
                        : message);
            }
        }

        internal static string? VerifyConstructor(
            this IDependencyInjectionBehavior behavior, ConstructorInfo constructor)
        {
            foreach (ParameterInfo parameter in constructor.GetParameters())
            {
                if (!behavior.VerifyDependency(new InjectionConsumerInfo(parameter), out string? message))
                {
                    return message;
                }
            }

            return null;
        }

        private static ActivationException BuildActivationException(
            IConstructorResolutionBehavior behavior, Type implementationType, string? message) =>
            new ActivationException(
                message is null || string.IsNullOrWhiteSpace(message)
                    ? StringResources.TypeHasNoInjectableConstructorAccordingToCustomResolutionBehavior(
                        behavior, implementationType)
                    : message);
    }
}