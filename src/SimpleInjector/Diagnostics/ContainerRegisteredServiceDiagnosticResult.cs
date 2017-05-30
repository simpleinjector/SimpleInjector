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
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public class ContainerRegisteredServiceDiagnosticResult : DiagnosticResult
    {
        internal ContainerRegisteredServiceDiagnosticResult(Type serviceType, string description,
            IEnumerable<KnownRelationship> relationships)
            : base(serviceType, description, DiagnosticType.ContainerRegisteredComponent,
                DiagnosticSeverity.Information, relationships.ToArray())
        {
            this.Relationships = new ReadOnlyCollection<KnownRelationship>(relationships.ToList());
        }

        /// <summary>Gets a collection of <see cref="KnownRelationship"/> instances that describe all 
        /// container-registered dependencies for the given component.</summary>
        /// <value>List of <see cref="KnownRelationship"/> objects.</value>
        public ReadOnlyCollection<KnownRelationship> Relationships { get; }
    }
}