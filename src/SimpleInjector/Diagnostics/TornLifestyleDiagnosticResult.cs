#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014 Simple Injector Contributors
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
    /// Diagnostic result that warns about when a multiple registrations map to the same implementation type 
    /// and lifestyle, which might cause multiple instances to be created during the lifespan of that lifestyle.
    /// For more information, see: https://simpleinjector.org/diatl.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public class TornLifestyleDiagnosticResult : DiagnosticResult
    {
        internal TornLifestyleDiagnosticResult(Type serviceType, string description, Lifestyle lifestyle,
            Type implementationType, InstanceProducer[] affectedRegistrations)
            : base(serviceType, description, DiagnosticType.TornLifestyle, DiagnosticSeverity.Warning,
                CreateDebugValue(implementationType, lifestyle, affectedRegistrations))
        {
            this.Lifestyle = lifestyle;
            this.ImplementationType = implementationType;
            this.AffectedRegistrations = new ReadOnlyCollection<InstanceProducer>(affectedRegistrations.ToList());
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

        private static DebuggerViewItem[] CreateDebugValue(Type implementationType, Lifestyle lifestyle,
            InstanceProducer[] affectedRegistrations)
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