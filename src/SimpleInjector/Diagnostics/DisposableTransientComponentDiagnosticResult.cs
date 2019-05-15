// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Diagnostics;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Diagnostic result for a warning about a component that is registered as transient, but implements 
    /// <see cref="IDisposable"/>.
    /// For more information, see: https://simpleinjector.org/diadt.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DisposableTransientComponentDiagnosticResult.DebuggerDisplay) + ", nq}")]
    public class DisposableTransientComponentDiagnosticResult : DiagnosticResult
    {
        internal DisposableTransientComponentDiagnosticResult(
            Type serviceType, InstanceProducer registration, string description)
            : base(
                serviceType,
                description,
                DiagnosticType.DisposableTransientComponent,
                DiagnosticSeverity.Warning,
                registration)
        {
            this.Registration = registration;
        }

        /// <summary>Gets the object that describes the relationship between the component and its dependency.</summary>
        /// <value>A <see cref="KnownRelationship"/> instance.</value>
        public InstanceProducer Registration { get; }
    }
}