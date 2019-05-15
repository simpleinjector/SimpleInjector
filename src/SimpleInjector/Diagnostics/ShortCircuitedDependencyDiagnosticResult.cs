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
    using SimpleInjector.Diagnostics.Debugger;

    /// <summary>
    /// Diagnostic result that warns about a
    /// component that depends on an unregistered concrete type and this concrete type has a lifestyle that is 
    /// different than the lifestyle of an explicitly registered type that uses this concrete type as its 
    /// implementation.
    /// For more information, see: https://simpleinjector.org/diasc.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ShortCircuitedDependencyDiagnosticResult.DebuggerDisplay) + ", nq}")]
    public class ShortCircuitedDependencyDiagnosticResult : DiagnosticResult
    {
        internal ShortCircuitedDependencyDiagnosticResult(
            Type serviceType,
            string description,
            InstanceProducer registration,
            KnownRelationship relationship,
            IEnumerable<InstanceProducer> expectedDependencies)
            : base(
                serviceType,
                description,
                DiagnosticType.ShortCircuitedDependency,
                DiagnosticSeverity.Warning,
                CreateDebugValue(registration, relationship, expectedDependencies.ToArray()))
        {
            this.Relationship = relationship;
            this.ExpectedDependencies = new ReadOnlyCollection<InstanceProducer>(expectedDependencies.ToList());
        }

        /// <summary>Gets the instance that describes the current relationship between the checked component
        /// and the short-circuited dependency.</summary>
        /// <value>The <see cref="KnownRelationship"/>.</value>
        public KnownRelationship Relationship { get; }

        /// <summary>
        /// Gets the collection of registrations that have the component's current dependency as 
        /// implementation type, but have a lifestyle that is different than the current dependency.
        /// </summary>
        /// <value>A collection of <see cref="InstanceProducer"/> instances.</value>
        public ReadOnlyCollection<InstanceProducer> ExpectedDependencies { get; }

        private static DebuggerViewItem[] CreateDebugValue(InstanceProducer registration,
            KnownRelationship actualDependency,
            InstanceProducer[] possibleSkippedRegistrations)
        {
            return new[]
            {
                new DebuggerViewItem(
                    name: "Registration",
                    description: registration.ServiceType.ToFriendlyName(),
                    value: registration),
                new DebuggerViewItem(
                    name: "Actual Dependency",
                    description: actualDependency.Dependency.ServiceType.ToFriendlyName(),
                    value: actualDependency),
                new DebuggerViewItem(
                    name: "Expected Dependency",
                    description: possibleSkippedRegistrations[0].ServiceType.ToFriendlyName(),
                    value: possibleSkippedRegistrations.Length == 1 ?
                        (object)possibleSkippedRegistrations[0] :
                        possibleSkippedRegistrations),
            };
        }
    }
}