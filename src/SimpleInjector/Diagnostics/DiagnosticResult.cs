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
        internal DiagnosticResult(Type serviceType, string description, DiagnosticType diagnosticType,
            DiagnosticSeverity severity, object value)
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
        [DebuggerDisplay("{" + TypesExtensions.FriendlyName + "(ServiceType),nq}")]
        public Type ServiceType { get; }

        /// <summary>Gets the description of the diagnostic result.</summary>
        /// <value>A <see cref="string"/> with the description.</value>
        [DebuggerDisplay("{Description, nq}")]
        public string Description { get; }

        /// <summary>Gets the documentation URL of the diagnostic result.</summary>
        /// <value>A <see cref="string"/> with the URL.</value>
        [DebuggerDisplay("{DocumentationUrl, nq}")]
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