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
    /// For more information, see: https://simpleinjector.org/diaal.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public class AmbiguousLifestylesDiagnosticResult : DiagnosticResult
    {
        internal AmbiguousLifestylesDiagnosticResult(Type serviceType, string description, 
            Lifestyle[] lifestyles, Type implementationType, InstanceProducer diagnosedProducer, 
            InstanceProducer[] conflictingProducers)
            : base(serviceType, description, DiagnosticType.AmbiguousLifestyles, DiagnosticSeverity.Warning,
                CreateDebugValue(implementationType, lifestyles, conflictingProducers))
        {
            this.Lifestyles = new ReadOnlyCollection<Lifestyle>(lifestyles.ToList());
            this.ImplementationType = implementationType;
            this.DiagnosedRegistration = diagnosedProducer;
            this.ConflictingRegistrations = new ReadOnlyCollection<InstanceProducer>(conflictingProducers.ToList());
        }

        /// <summary>Gets the lifestyles that causes the registrations to be conflicting.</summary>
        /// <value><see cref="Lifestyle"/> instances.</value>
        public ReadOnlyCollection<Lifestyle> Lifestyles { get; }

        /// <summary>Gets the implementation type that the affected registrations map to.</summary>
        /// <value>A <see cref="Type"/>.</value>
        public Type ImplementationType { get; }

        /// <summary>Gets the registration that caused this warning.</summary>
        /// /// <value>An <see cref="InstanceProducer"/>.</value>
        public InstanceProducer DiagnosedRegistration { get; }

        /// <summary>
        /// Gets the list of registrations that are in conflict with the <see cref="DiagnosedRegistration"/>.
        /// </summary>
        /// <value>A list of <see cref="InstanceProducer"/> instances.</value>
        public ReadOnlyCollection<InstanceProducer> ConflictingRegistrations { get; }

        private static DebuggerViewItem[] CreateDebugValue(Type implementationType, Lifestyle[] lifestyles,
            InstanceProducer[] conflictingRegistrations)
        {
            return new[]
            {
                new DebuggerViewItem(
                    name: "ImplementationType", 
                    description: implementationType.ToFriendlyName(), 
                    value: implementationType),
                new DebuggerViewItem(
                    name: "Lifestyles", 
                    description: ToCommaSeparatedText(lifestyles), 
                    value: lifestyles),
                new DebuggerViewItem(
                    name: "Conflicting Registrations", 
                    description: ToCommaSeparatedText(conflictingRegistrations), 
                    value: conflictingRegistrations)
            };
        }

        private static string ToCommaSeparatedText(IEnumerable<Lifestyle> lifestyles) => 
            lifestyles.Select(lifestyle => lifestyle.Name).ToCommaSeparatedText();

        private static string ToCommaSeparatedText(IEnumerable<InstanceProducer> producers) => 
            producers.Select(r => r.ServiceType).Distinct().Select(TypesExtensions.ToFriendlyName)
                .ToCommaSeparatedText();
    }
}