// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Provides access to an injected dependency and its metadata.
    /// </summary>
    /// <typeparam name="TService">The dependency type.</typeparam>
    public sealed class DependencyMetadata<TService> : ApiObject, IEquatable<DependencyMetadata<TService>?>
        where TService : class
    {
        internal DependencyMetadata(InstanceProducer dependency)
        {
            this.Dependency = dependency;
        }

        /// <summary>
        /// Gets the type of the implementation that is created by the container and for which the decorator
        /// is about to be applied. The original implementation type will be returned, even if other decorators
        /// have already been applied to this type. Please note that the implementation type can not always be
        /// determined. In that case the closed generic service type will be returned.
        /// </summary>
        /// <value>The implementation type.</value>
        public Type ImplementationType => this.Dependency.ImplementationType;

        /// <summary>Gets the type that the parent depends on (it is injected into the parent).</summary>
        /// <value>The type that the parent depends on.</value>
        public InstanceProducer Dependency { get; }

        /// <summary>Produces an instance.</summary>
        /// <returns>An instance. Will never return null.</returns>
        /// <exception cref="ActivationException">When the instance could not be retrieved or is null.
        /// </exception>
        public TService GetInstance() => (TService)this.Dependency.GetInstance();

        /// <inheritdoc />
        public bool Equals(DependencyMetadata<TService>? other) =>
            other != null
            && this.Dependency.Equals(other.Dependency);

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is DependencyMetadata<TService> other && this.Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => this.Dependency.GetHashCode();
    }
}