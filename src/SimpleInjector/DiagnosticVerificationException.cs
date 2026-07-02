// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using SimpleInjector.Diagnostics;

    /// <summary>
    /// Thrown by the container in case of a diagnostic error.
    /// </summary>
    [Serializable]
    public class DiagnosticVerificationException : Exception
    {
        private static readonly ReadOnlyCollection<DiagnosticResult> Empty = new([]);

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticVerificationException" /> class.
        /// </summary>
        public DiagnosticVerificationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticVerificationException" /> class with a specified error
        /// message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DiagnosticVerificationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticVerificationException" /> class with a specified error
        /// message.
        /// </summary>
        /// <param name="errors">The list of errors.</param>
        public DiagnosticVerificationException(IList<DiagnosticResult> errors)
            : base(BuildMessage(errors))
        {
            this.Errors = new ReadOnlyCollection<DiagnosticResult>(errors.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticVerificationException" /> class with a specified error
        /// message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference if no inner
        /// exception is specified.
        /// </param>
        public DiagnosticVerificationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        internal DiagnosticVerificationException(string message, DiagnosticResult error)
            : base(message)
        {
            this.Errors = new ReadOnlyCollection<DiagnosticResult>([error]);
        }

        /// <summary>Gets the list of <see cref="DiagnosticResult"/> instances.</summary>
        /// <value>A list of <see cref="DiagnosticResult"/> instances.</value>
        public ReadOnlyCollection<DiagnosticResult> Errors { get; } = Empty;

        private static string BuildMessage(IList<DiagnosticResult> errors)
        {
            Requires.IsNotNull(errors, nameof(errors));
            return StringResources.DiagnosticWarningsReported(errors);
        }
    }
}