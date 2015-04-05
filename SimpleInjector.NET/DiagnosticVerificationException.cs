#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015 Simple Injector Contributors
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
    public partial class DiagnosticVerificationException : Exception
    {
        private static readonly ReadOnlyCollection<DiagnosticResult> Empty = 
            new ReadOnlyCollection<DiagnosticResult>(new DiagnosticResult[0]);

#if !PCL
        [NonSerializedAttribute]
#endif
        private readonly ReadOnlyCollection<DiagnosticResult> errors = Empty;

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
        public DiagnosticVerificationException(IList<DiagnosticResult> errors) : base(BuildMessage(errors)) 
        {
            this.errors = new ReadOnlyCollection<DiagnosticResult>(errors.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticVerificationException" /> class with a specified error 
        /// message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception. 
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference (Nothing in Visual 
        /// Basic) if no inner exception is specified. 
        /// </param>
        public DiagnosticVerificationException(string message, Exception innerException)
            : base(message, innerException) 
        {
        }

        /// <summary>
        /// Gets the list of <see cref="DiagnosticResult"/> instances.
        /// </summary>
        /// <value>A list of <see cref="DiagnosticResult"/> instances.</value>
        public ReadOnlyCollection<DiagnosticResult> Errors 
        {
            get { return this.errors; }
        }

        private static string BuildMessage(IList<DiagnosticResult> errors)
        {
            Requires.IsNotNull(errors, "errors");
            return StringResources.DiagnosticWarningsReported(errors);
        }
    }
}
