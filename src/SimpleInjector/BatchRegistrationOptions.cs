// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    /// <summary>
    /// Defines options to control the types registered using the
    /// <b>Container.Register</b> overloads that accept a <see cref="BatchRegistrationOptions"/>.
    /// </summary>
    public sealed class BatchRegistrationOptions
    {
        internal static readonly BatchRegistrationOptions Default = new();

        /// <summary>
        /// Gets or sets a value indicating whether generic type definitions (types that have
        /// TypeInfo.IsGenericTypeDefinition or Type.IsGenericTypeDefinition set to true)
        /// should be registered. The default value for this property is <b>false</b>.
        /// </summary>
        /// <value>A boolean.</value>
        public bool IncludeGenericTypeDefinitions { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether composite types should be included in the result. The default
        /// value of this property is <b>true</b>. A type is considered a composite if the type's constructor
        /// contains a parameter of <code>IEnumerable&lt;T&gt;</code>, <code>ICollection&lt;T&gt;</code>,
        /// <code>IList&lt;T&gt;</code>, <code>IReadOnlyCollection&lt;T&gt;</code>,
        /// <code>IReadOnlyList&lt;T&gt;</code> or <code>T[]</code> (array of T), where <code>T</code>
        /// exactly matches the <code>serviceType</code> argument, supplied to the <b>Register</b> method.
        /// </summary>
        /// <value>A boolean.</value>
        public bool IncludeComposites { get; set; } = true;
    }

}