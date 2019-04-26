#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2019 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// A known relationship defines a relationship between two types. The Diagnostics Debug View uses this
    /// information to spot possible misconfigurations. 
    /// </summary>
    [DebuggerDisplay("{" + nameof(KnownRelationship.DebuggerDisplay) + ",nq}")]
    public sealed class KnownRelationship : IEquatable<KnownRelationship>
    {
        // This constructor is here for backwards compatibility: the library itself uses the internal ctor.

        /// <summary>Initializes a new instance of the <see cref="KnownRelationship"/> class.</summary>
        /// <param name="implementationType">The implementation type of the parent type.</param>
        /// <param name="lifestyle">The lifestyle of the parent type.</param>
        /// <param name="dependency">The type that the parent depends on (it is injected into the parent).</param>
        public KnownRelationship(
            Type implementationType, Lifestyle lifestyle, InstanceProducer dependency)
        {
            Requires.IsNotNull(implementationType, nameof(implementationType));
            Requires.IsNotNull(lifestyle, nameof(lifestyle));
            Requires.IsNotNull(dependency, nameof(dependency));

            this.ImplementationType = implementationType;
            this.Lifestyle = lifestyle;
            this.Dependency = dependency;
        }

        internal KnownRelationship(
            Type implementationType,
            Lifestyle lifestyle,
            InjectionConsumerInfo consumer,
            InstanceProducer dependency,
            string additionalInformation = null)
        {
            Requires.IsNotNull(implementationType, nameof(implementationType));
            Requires.IsNotNull(lifestyle, nameof(lifestyle));
            Requires.IsNotNull(dependency, nameof(dependency));

            this.ImplementationType = implementationType;
            this.Lifestyle = lifestyle;
            this.Consumer = consumer;
            this.Dependency = dependency;
            this.AdditionalInformation = additionalInformation ?? string.Empty;
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

        // WARNING: Can be null.
        internal InjectionConsumerInfo Consumer { get; }

        internal string AdditionalInformation { get; } = string.Empty;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "This method is called by the debugger.")]
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

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "This method is called by the debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string ImplementationTypeDebuggerDisplay => this.ImplementationType.ToFriendlyName();

        /// <inheritdoc />
        public override int GetHashCode() =>
            this.ImplementationType.GetHashCode()
            ^ this.Lifestyle.GetHashCode()
            ^ this.Consumer?.GetHashCode() ?? 0
            ^ this.Dependency.GetHashCode();

        /// <inheritdoc />
        public override bool Equals(object obj) => this.Equals(obj as KnownRelationship);

        /// <inheritdoc />
        public bool Equals(KnownRelationship other)
        {
            if (other == null)
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
                && (this.Consumer?.Equals(other.Consumer)
                    ?? other.Consumer?.Equals(this.Consumer)
                    ?? true);
        }
    }
}