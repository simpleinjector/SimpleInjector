// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using SimpleInjector.Diagnostics.Debugger;

    /// <summary>
    /// Diagnostic result that warns about when a multiple registrations map to the same implementation type 
    /// and lifestyle, which might cause multiple instances to be created during the lifespan of that lifestyle.
    /// For more information, see: https://simpleinjector.org/diatl.
    /// </summary>
    [DebuggerDisplay("{" + nameof(TornLifestyleDiagnosticResult.DebuggerDisplay) + ", nq}")]
    public class TornLifestyleDiagnosticResult : DiagnosticResult
    {
        internal TornLifestyleDiagnosticResult(
            Type serviceType,
            string description,
            Lifestyle lifestyle,
            Type implementationType,
            InstanceProducer[] affectedRegistrations)
            : base(
                serviceType,
                description,
                DiagnosticType.TornLifestyle,
                DiagnosticSeverity.Warning,
                CreateDebugValue(implementationType, lifestyle, affectedRegistrations))
        {
            this.Lifestyle = lifestyle;
            this.ImplementationType = implementationType;
            this.AffectedRegistrations = 
                new ReadOnlyCollection<InstanceProducer>(affectedRegistrations.ToList());
        }

        /// <summary>Gets the lifestyle on which instances are torn.</summary>
        /// <value>A <see cref="Lifestyle"/>.</value>
        public Lifestyle Lifestyle { get; }

        /// <summary>Gets the implementation type that the affected registrations map to.</summary>
        /// <value>A <see cref="Type"/>.</value>
        public Type ImplementationType { get; }

        /// <summary>Gets the list of registrations that are affected by this warning.</summary>
        /// <value>A list of <see cref="InstanceProducer"/> instances.</value>
        public ReadOnlyCollection<InstanceProducer> AffectedRegistrations { get; }

        private static DebuggerViewItem[] CreateDebugValue(
            Type implementationType, Lifestyle lifestyle, InstanceProducer[] affectedRegistrations)
        {
            return new[]
            {
                new DebuggerViewItem(
                    name: "ImplementationType",
                    description: implementationType.ToFriendlyName(),
                    value: implementationType),
                new DebuggerViewItem(
                    name: "Lifestyle",
                    description: lifestyle.Name,
                    value: lifestyle),
                new DebuggerViewItem(
                    name: "Affected Registrations",
                    description: ToCommaSeparatedText(affectedRegistrations),
                    value: affectedRegistrations)
            };
        }

        private static string ToCommaSeparatedText(IEnumerable<InstanceProducer> producers) =>
            producers.Select(r => r.ServiceType).Distinct().Select(TypesExtensions.ToFriendlyName)
                .ToCommaSeparatedText();
    }
}