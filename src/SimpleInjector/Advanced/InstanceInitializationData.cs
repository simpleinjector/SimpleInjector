// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Contains data that can be used to initialize a created instance. This data includes the actual
    /// created <see cref="Instance"/> and the <see cref="Context"/> information about the created instance.
    /// </summary>
    [DebuggerDisplay(nameof(InstanceInitializationData) +
        " ({" + nameof(InstanceInitializationData.DebuggerDisplay) + ", nq})")]
    public struct InstanceInitializationData : IEquatable<InstanceInitializationData>
    {
        /// <summary>Initializes a new instance of the <see cref="InstanceInitializationData"/> struct.</summary>
        /// <param name="context">The <see cref="InitializerContext"/> that contains contextual information
        /// about the created instance.</param>
        /// <param name="instance">The created instance.</param>
        public InstanceInitializationData(InitializerContext context, object instance)
        {
            Requires.IsNotNull(context, nameof(context));
            Requires.IsNotNull(instance, nameof(instance));

            this.Context = context;
            this.Instance = instance;
        }

        /// <summary>Gets the <see cref="InitializationContext"/> with contextual information about the 
        /// created instance.</summary>
        /// <value>The <see cref="InitializationContext"/>.</value>
        public InitializerContext Context { get; }

        /// <summary>Gets the created instance.</summary>
        /// <value>The created instance.</value>
        public object Instance { get; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "This method is called by the debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebuggerDisplay => this.Context.DebuggerDisplay;

        /// <inheritdoc />
        public override int GetHashCode() =>
            (this.Context == null ? 0 : this.Context.GetHashCode()) ^
            (this.Instance == null ? 0 : this.Instance.GetHashCode());

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is InstanceInitializationData && this.Equals((InstanceInitializationData)obj);

        /// <inheritdoc />
        public bool Equals(InstanceInitializationData other) => this == other;

        /// <summary>
        /// Indicates whether the values of two specified <see cref="InstanceInitializationData"/> objects are equal.
        /// </summary>
        /// <param name="first">The first object to compare.</param>
        /// <param name="second">The second object to compare.</param>
        /// <returns>True if a and b are equal; otherwise, false.</returns>
        public static bool operator ==(InstanceInitializationData first, InstanceInitializationData second) =>
            object.ReferenceEquals(first.Context, second.Context) &&
            object.ReferenceEquals(first.Instance, second.Instance);

        /// <summary>
        /// Indicates whether the values of two specified  <see cref="InstanceInitializationData"/>  objects are 
        /// not equal.
        /// </summary>
        /// <param name="first">The first object to compare.</param>
        /// <param name="second">The second object to compare.</param>
        /// <returns>True if a and b are not equal; otherwise, false.</returns>
        public static bool operator !=(InstanceInitializationData first, InstanceInitializationData second) =>
            !(first == second);
    }
}