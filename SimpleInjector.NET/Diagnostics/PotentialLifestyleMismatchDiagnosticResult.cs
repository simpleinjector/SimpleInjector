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
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using SimpleInjector.Advanced;

    /// <summary>
    /// This type is obsolete. Please use <see cref="LifestyleMismatchDiagnosticResult"/> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been renamed to LifestyleMismatchDiagnosticResult. " + 
        "Please use LifestyleMismatchDiagnosticResult instead.", error: true)]
    [ExcludeFromCodeCoverage]
    public class PotentialLifestyleMismatchDiagnosticResult : DiagnosticResult
    {
        internal PotentialLifestyleMismatchDiagnosticResult(Type serviceType, string description,
            KnownRelationship relationship)
            : base(serviceType, description, DiagnosticType.LifestyleMismatch,
                DiagnosticSeverity.Warning, relationship)
        {
            this.Relationship = relationship;
        }

        /// <summary>Gets the object that describes the relationship between the component and its dependency.</summary>
        /// <value>A <see cref="KnownRelationship"/> instance.</value>
        public KnownRelationship Relationship { get; }
    }
}