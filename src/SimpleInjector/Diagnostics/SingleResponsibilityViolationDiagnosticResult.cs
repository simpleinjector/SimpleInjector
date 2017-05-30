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
    using SimpleInjector.Diagnostics.Debugger;

    /// <summary>
    /// Diagnostic result that warns about a component that depends on (too) many services.
    /// For more information, see: https://simpleinjector.org/diasr.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public class SingleResponsibilityViolationDiagnosticResult : DiagnosticResult
    {
        internal SingleResponsibilityViolationDiagnosticResult(Type serviceType, string description,
            Type implementationType, IEnumerable<InstanceProducer> dependencies)
            : base(serviceType, description, DiagnosticType.SingleResponsibilityViolation,
                DiagnosticSeverity.Information, GetDebugValue(implementationType, dependencies.ToArray()))
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