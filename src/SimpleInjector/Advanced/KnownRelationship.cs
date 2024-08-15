// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using SimpleInjector.Diagnostics;

    /// <summary>
    /// A known relationship defines a relationship between two types. The Diagnostics Debug View uses this
    /// information to spot possible misconfigurations.
    /// </summary>
    [DebuggerDisplay("{" + nameof(KnownRelationship.DebuggerDisplay) + ",nq}")]
    public sealed class KnownRelationship : IEquatable<KnownRelationship?>
    {
        private Dictionary<DiagnosticType, string>? additionalInformation;

        // This constructor is here for backwards compatibility: the library itself uses the internal ctor.

        /// <summary>Initializes a new instance of the <see cref="KnownRelationship"/> class.</summary>
        /// <param name="implementationType">The implementation type of the parent type.</param>
        /// <param name="lifestyle">The lifestyle of the parent type.</param>
        /// <param name="dependency">The type that the parent depends on (it is injected into the parent).</param>
        public KnownRelationship(Type implementationType, Lifestyle lifestyle, InstanceProducer dependency)
        {
            Requires.IsNotNull(implementationType, nameof(implementationType));
            Requires.IsNotNull(lifestyle, nameof(lifestyle));
            Requires.IsNotNull(dependency, nameof(dependency));

            this.ImplementationType = implementationType;
            this.Lifestyle = lifestyle;
            this.Dependency = dependency;
            this.Consumer = InjectionConsumerInfo.Root;
        }

        internal KnownRelationship(
            Type implementationType,
            Lifestyle lifestyle,
            InjectionConsumerInfo consumer,
            InstanceProducer dependency)
        {
            Requires.IsNotNull(implementationType, nameof(implementationType));
            Requires.IsNotNull(lifestyle, nameof(lifestyle));
            Requires.IsNotNull(consumer, nameof(consumer));
            Requires.IsNotNull(dependency, nameof(dependency));

            this.ImplementationType = implementationType;
            this.Lifestyle = lifestyle;
            this.Consumer = consumer;
            this.Dependency = dependency;
        }

        /// <summary>Gets the implementation type of the parent type of the relationship.</summary>
        /// <value>The implementation type of the parent type of the relationship.</value>
        [DebuggerDisplay("{" + nameof(ImplementationTypeDebuggerDisplay) + ", nq}")]
        public Type ImplementationType { get; }

        /// <summary>Gets the lifestyle of the parent type of the relationship.</summary>
        /// <value>The lifestyle of the parent type of the relationship.</value>
        public Lifestyle Lifestyle { get; }

        /// <summary>Gets the type that the parent depends on (it is injected into the parent).</summary>
        /// <value>The type that the parent depends on.</value>
        public InstanceProducer Dependency { get; }

        internal void AddAdditionalInformation(DiagnosticType type, string message)
        {
            this.additionalInformation ??= [];

            this.additionalInformation[type] = message ?? string.Empty;
        }

        internal string GetAdditionalInformation(DiagnosticType type)
        {
            if (this.additionalInformation != null
                && this.additionalInformation.TryGetValue(type, out string message))
            {
                return message;
            }

            return string.Empty;
        }

        internal bool UseForVerification { get; set; } = true;

        internal InjectionConsumerInfo Consumer { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => string.Format(
            CultureInfo.InvariantCulture,
            "{0} = {1}, {2} = {3}, {4} = {{{5}}}",
            nameof(this.ImplementationType),
            this.ImplementationTypeDebuggerDisplay,
            nameof(this.Lifestyle),
            this.Lifestyle.Name,
            nameof(this.Dependency),
            this.Dependency.DebuggerDisplay);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string ImplementationTypeDebuggerDisplay => this.ImplementationType.ToFriendlyName();

        /// <inheritdoc />
        public override int GetHashCode() =>
            Helpers.Hash(this.ImplementationType, this.Lifestyle, this.Consumer, this.Dependency);

        /// <inheritdoc />
        public override bool Equals(object obj) => this.Equals(obj as KnownRelationship);

        /// <inheritdoc />
        public bool Equals(KnownRelationship? other)
        {
            if (other is null)
            {
                return false;
            }

            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            return
                this.ImplementationType.Equals(other.ImplementationType)
                && this.Lifestyle.Equals(other.Lifestyle)
                && this.Dependency.Equals(other.Dependency)
                && this.Consumer.Equals(other.Consumer);
        }

        internal KnownRelationship ReplaceLifestyle(Lifestyle lifestyle)
        {
            var newRelationship = new KnownRelationship(
                this.ImplementationType,
                lifestyle,
                this.Consumer,
                this.Dependency);

            if (this.additionalInformation != null)
            {
                newRelationship.additionalInformation = this.additionalInformation.MakeCopy();
            }

            return newRelationship;
        }
    }
}