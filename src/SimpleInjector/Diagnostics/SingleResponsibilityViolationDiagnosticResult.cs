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
    /// Diagnostic result that warns about a component that depends on (too) many services.
    /// For more information, see: https://simpleinjector.org/diasr.
    /// </summary>
    [DebuggerDisplay("{" + nameof(SingleResponsibilityViolationDiagnosticResult.DebuggerDisplay) + ", nq}")]
    public class SingleResponsibilityViolationDiagnosticResult : DiagnosticResult
    {
        internal SingleResponsibilityViolationDiagnosticResult(
            Type serviceType,
            string description,
            Type implementationType,
            IEnumerable<InstanceProducer> dependencies)
            : base(
                serviceType,
                description,
                DiagnosticType.SingleResponsibilityViolation,
                DiagnosticSeverity.Information,
                GetDebugValue(implementationType, dependencies.ToArray()))
        {
            this.ImplementationType = implementationType;
            this.Dependencies = new ReadOnlyCollection<InstanceProducer>(dependencies.ToList());
        }

        /// <summary>Gets the created type.</summary>
        /// <value>A <see cref="Type"/>.</value>
        public Type ImplementationType { get; }

        /// <summary>Gets the list of registrations that are dependencies of the <see cref="ImplementationType"/>.</summary>
        /// <value>A collection of <see cref="InstanceProducer"/> instances.</value>
        public ReadOnlyCollection<InstanceProducer> Dependencies { get; }

        private static DebuggerViewItem[] GetDebugValue(Type implementationType, InstanceProducer[] dependencies)
        {
            return new[]
            {
                new DebuggerViewItem("ImplementationType", implementationType.ToFriendlyName(), implementationType),
                new DebuggerViewItem("Dependencies", dependencies.Length + " dependencies.", dependencies),
            };
        }
    }
}