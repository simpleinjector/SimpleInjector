// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Diagnostic result for a warning about a concrete type that was not registered explicitly and was not 
    /// resolved using unregistered type resolution, but was created by the container using the transient 
    /// lifestyle.
    /// For more information, see: https://simpleinjector.org/diaut.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ContainerRegisteredServiceDiagnosticResult.DebuggerDisplay) + ", nq}")]
    public class ContainerRegisteredServiceDiagnosticResult : DiagnosticResult
    {
        internal ContainerRegisteredServiceDiagnosticResult(
            Type serviceType, string description, IEnumerable<KnownRelationship> relationships)
            : base(
                serviceType,
                description,
                DiagnosticType.ContainerRegisteredComponent,
                DiagnosticSeverity.Information,
                relationships.ToArray())
        {
            this.Relationships = new ReadOnlyCollection<KnownRelationship>(relationships.ToList());
        }

        /// <summary>Gets a collection of <see cref="KnownRelationship"/> instances that describe all 
        /// container-registered dependencies for the given component.</summary>
        /// <value>List of <see cref="KnownRelationship"/> objects.</value>
        public ReadOnlyCollection<KnownRelationship> Relationships { get; }
    }
}