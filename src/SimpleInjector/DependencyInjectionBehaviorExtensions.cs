// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Reflection;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Extension methods for <see cref="IDependencyInjectionBehavior"/>.
    /// </summary>
    public static class DependencyInjectionBehaviorExtensions
    {
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
            // NOTE: In v5, both IConstructorResolutionBehavior's and IDependencyInjectionBehavior's method
            // signatures changed (see #557). This is both a binary and code breaking change, which affects
            // anyone implementing an IDependencyInjectionBehavior. There's nothing much we can do about
            // this (since fixing #557 was more important). By adding this extension method, however, we can
            // reduce the pain for anyone using (calling) this interface. This extension method duplicates
            // the signature and behavior of the old method. This still makes the change binary incompatible,
            // even from perspective of the caller, but it would allow their code to keep compiling (mostly).
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
    }
}