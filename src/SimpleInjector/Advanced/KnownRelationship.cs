#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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
    [DebuggerDisplay(nameof(KnownRelationship))]
    public sealed class KnownRelationship : IEquatable<KnownRelationship>
    {
        /// <summary>Initializes a new instance of the <see cref="KnownRelationship"/> class.</summary>
        /// <param name="implementationType">The implementation type of the parent type.</param>
        /// <param name="lifestyle">The lifestyle of the parent type.</param>
        /// <param name="dependency">The type that the parent depends on (it is injected into the parent).</param>
        public KnownRelationship(Type implementationType, Lifestyle lifestyle, 
            InstanceProducer dependency)
        {
            Requires.IsNotNull(implementationType, nameof(implementationType));
            Requires.IsNotNull(lifestyle, nameof(lifestyle));
            Requires.IsNotNull(dependency, nameof(dependency));

            this.ImplementationType = implementationType;
            this.Lifestyle = lifestyle;
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

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "This method is called by the debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => string.Format(CultureInfo.InvariantCulture,
            "{0} = {1}, {2} = {3}, {4} = {{{5}}}",
            nameof(this.ImplementationType), this.ImplementationTypeDebuggerDisplay,
            nameof(this.Lifestyle), this.Lifestyle.Name,
            nameof(this.Dependency), this.Dependency.DebuggerDisplay);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "This method is called by the debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string ImplementationTypeDebuggerDisplay => this.ImplementationType.ToFriendlyName();

        /// <summary>Serves as a hash function for a particular type.</summary>
        /// <returns>A hash code for the current <see cref="KnownRelationship"/>.</returns>
        public override int GetHashCode() => 
            this.ImplementationType.GetHashCode() ^ this.Lifestyle.GetHashCode() ^ this.Dependency.GetHashCode();

        /// <summary>
        /// Determines whether the specified <see cref="KnownRelationship"/> is equal to the current 
        /// <see cref="KnownRelationship"/>.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified <see cref="KnownRelationship"/> is equal to the current 
        /// <see cref="KnownRelationship"/>; otherwise, false.</returns>
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
                this.ImplementationType == other.ImplementationType &&
                this.Lifestyle == other.Lifestyle &&
                this.Dependency == other.Dependency;
        }
    }
}