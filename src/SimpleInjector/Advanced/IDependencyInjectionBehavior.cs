// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;

    /// <summary>
    /// Defines the container's behavior for building an expression tree for an dependency to inject, based on
    /// the information of the consuming type the dependency is injected into.
    /// Set the <see cref="ContainerOptions.DependencyInjectionBehavior">ConstructorInjectionBehavior</see>
    /// property of the container's <see cref="Container.Options"/> property to change the default behavior
    /// of the container.
    /// </summary>
    public interface IDependencyInjectionBehavior
    {
        /// <summary>Verifies whether the dependency can be injected into the given consumer by the container,
        /// based on the supplied type information of both the consumer and the dependency.</summary>
        /// <param name="dependency">Contextual information about the dependency.</param>
        /// <param name="errorMessage">The description of why the dependency is invalid, when the method
        /// returns <b>false</b>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the supplied argument is a null reference.</exception>
        /// <returns><b>True</b> when the dependency is valid, otherwise <b>false</b> and the
        /// <paramref name="errorMessage"/> is set with an explanation.</returns>
        bool VerifyDependency(InjectionConsumerInfo dependency, out string? errorMessage);

        /// <summary>
        /// Gets the <see cref="InstanceProducer"/> for the
        /// <see cref="InjectionConsumerInfo.Target">Target</see> of the supplied <paramref name="dependency"/>.
        /// </summary>
        /// <param name="dependency">Contextual information about the consumer where the built dependency is
        /// injected into.</param>
        /// <param name="throwOnFailure">The indication whether the method should return null or throw
        /// an exception when the type is not registered.</param>
        /// <returns>An <see cref="InstanceProducer"/> that describes the intend of creating that
        /// <see cref="InjectionConsumerInfo.Target">Target</see>. This method never returns null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the argument is a null reference.</exception>
        InstanceProducer? GetInstanceProducer(InjectionConsumerInfo dependency, bool throwOnFailure);
    }
}