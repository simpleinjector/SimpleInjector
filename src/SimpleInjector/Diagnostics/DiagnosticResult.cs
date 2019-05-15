// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Base class for types that hold information about a single diagnostic message or warning for a
    /// particular type or part of the configuration.
    /// </summary>
    public abstract class DiagnosticResult
    {
        internal DiagnosticResult(
            Type serviceType,
            string description,
            DiagnosticType diagnosticType,
            DiagnosticSeverity severity,
            object value)
        {
            this.ServiceType = serviceType;
            this.Description = description;
            this.DiagnosticType = diagnosticType;
            this.Severity = severity;
            this.Value = value;
        }

        /// <summary>Gets the severity of this result.</summary>
        /// <value>The <see cref="DiagnosticSeverity"/>.</value>
        public DiagnosticSeverity Severity { get; }

        /// <summary>Gets the diagnostic type of this result.</summary>
        /// <value>The <see cref="DiagnosticType"/>.</value>
        public DiagnosticType DiagnosticType { get; }

        /// <summary>Gets the service type to which this warning is related.</summary>
        /// <value>A <see cref="Type"/>.</value>
        [DebuggerDisplay("{" + TypesExtensions.FriendlyName + "(" + nameof(ServiceType) + "),nq}")]
        public Type ServiceType { get; }

        /// <summary>Gets the description of the diagnostic result.</summary>
        /// <value>A <see cref="string"/> with the description.</value>
        [DebuggerDisplay("{" + nameof(Description) + ", nq}")]
        public string Description { get; }

        /// <summary>Gets the documentation URL of the diagnostic result.</summary>
        /// <value>A <see cref="string"/> with the URL.</value>
        [DebuggerDisplay("{" + nameof(DocumentationUrl) + ", nq}")]
        public Uri DocumentationUrl =>
            DocumentationAttribute.GetDocumentationAttribute(this.DiagnosticType).DocumentationUrl;

        /// <summary>Gets the hierarchical group to which this diagnostic result belongs.</summary>
        /// <value>The <see cref="DiagnosticGroup"/>.</value>
        public DiagnosticGroup Group { get; internal set; }

        [DebuggerHidden]
        internal object Value { get; }

        [DebuggerHidden]
        internal string Name => DocumentationAttribute.GetDocumentationAttribute(this.DiagnosticType).Name;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "This method is called by the debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebuggerDisplay => string.Format(
            CultureInfo.InvariantCulture,
            "{0} {1}: {2}",
            this.Name,
            this.ServiceType.ToFriendlyName(),
            this.Description);
    }
}