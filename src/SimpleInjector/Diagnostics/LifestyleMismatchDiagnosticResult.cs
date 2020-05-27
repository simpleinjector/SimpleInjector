// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Diagnostics;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Diagnostic result for a warning about a
    /// component that depends on a service with a lifestyle that is shorter than that of the component.
    /// For more information, see: https://simpleinjector.org/dialm.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public class LifestyleMismatchDiagnosticResult : DiagnosticResult
    {
        internal LifestyleMismatchDiagnosticResult(
            Type serviceType, string description, KnownRelationship relationship)
            : base(
                serviceType,
                description,
                DiagnosticType.LifestyleMismatch,
                DiagnosticSeverity.Warning,
                relationship)
        {
            this.Relationship = relationship;
        }

        /// <summary>Gets the object that describes the relationship between the component and its dependency.</summary>
        /// <value>A <see cref="KnownRelationship"/> instance.</value>
        public KnownRelationship Relationship { get; }
    }
}