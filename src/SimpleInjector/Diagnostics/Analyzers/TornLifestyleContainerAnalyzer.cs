#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014-2015 Simple Injector Contributors
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

namespace SimpleInjector.Diagnostics.Analyzers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using SimpleInjector.Lifestyles;

    internal sealed class TornLifestyleContainerAnalyzer : IContainerAnalyzer
    {
        internal static readonly IContainerAnalyzer Instance = new TornLifestyleContainerAnalyzer();

        private TornLifestyleContainerAnalyzer()
        {
        }

        public DiagnosticType DiagnosticType => DiagnosticType.TornLifestyle;

        public string Name => "Torn Lifestyle";

        public string GetRootDescription(IEnumerable<DiagnosticResult> results) =>
            results.Count() + " possible registrations found with a torn lifestyle.";

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results) =>
            results.Count() + " torn registrations.";

        public DiagnosticResult[] Analyze(IEnumerable<InstanceProducer> producers)
        {
            IEnumerable<InstanceProducer[]> tornRegistrationGroups = GetTornRegistrationGroups(producers);

            var results =
                from tornProducerGroup in tornRegistrationGroups
                from producer in tornProducerGroup
                where producer.Registration.ShouldNotBeSuppressed(DiagnosticType.TornLifestyle)
                select CreateDiagnosticResult(
                    diagnosedProducer: producer,
                    affectedProducers: tornProducerGroup);

            return results.ToArray();
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification = "FxCop is unable to recognize nicely written LINQ statements from complex code.")]
        private static IEnumerable<InstanceProducer[]> GetTornRegistrationGroups(
            IEnumerable<InstanceProducer> producers) => 
            from producer in producers
            where !producer.IsDecorated
            where producer.Registration.Lifestyle != Lifestyle.Transient
            where !SingletonLifestyle.IsSingletonInstanceRegistration(producer.Registration)
            where !producer.Registration.WrapsInstanceCreationDelegate
            group producer by producer.Registration into registrationGroup
            let registration = registrationGroup.Key
            let key = new { registration.ImplementationType, Lifestyle = registration.Lifestyle.IdentificationKey }
            group registrationGroup by key into registrationLifestyleGroup
            let hasConflict = registrationLifestyleGroup.Count() > 1
            where hasConflict
            select registrationLifestyleGroup.SelectMany(p => p).ToArray();

        private static TornLifestyleDiagnosticResult CreateDiagnosticResult(
            InstanceProducer diagnosedProducer,
            InstanceProducer[] affectedProducers)
        {
            Type serviceType = diagnosedProducer.ServiceType;
            Type implementationType = diagnosedProducer.Registration.ImplementationType;
            Lifestyle lifestyle = diagnosedProducer.Registration.Lifestyle;
            string description = BuildDescription(diagnosedProducer, affectedProducers);

            return new TornLifestyleDiagnosticResult(serviceType, description, lifestyle, implementationType,
                affectedProducers);
        }

        private static string BuildDescription(InstanceProducer diagnosedProducer,
             InstanceProducer[] affectedProducers)
        {
            Lifestyle lifestyle = diagnosedProducer.Registration.Lifestyle;

            var tornProducers = (
                from producer in affectedProducers
                where producer.Registration != diagnosedProducer.Registration
                select producer)
                .ToArray();

            return string.Format(CultureInfo.InvariantCulture,
                "The registration for {0} maps to the same implementation and lifestyle as the {1} " +
                "for {2} {3}. They {4} map to {5} ({6}). This will cause each registration to resolve to " +
                "a different instance: each registration will have its own instance{7}.",
                diagnosedProducer.ServiceType.ToFriendlyName(),
                tornProducers.Length == 1 ? "registration" : "registrations",
                tornProducers.Select(producer => producer.ServiceType.ToFriendlyName()).ToCommaSeparatedText(),
                tornProducers.Length == 1 ? "does" : "do",
                tornProducers.Length == 1 ? "both" : "all",
                diagnosedProducer.Registration.ImplementationType.ToFriendlyName(),
                lifestyle.Name,
                lifestyle == Lifestyle.Singleton ? string.Empty : " during a single " + lifestyle.Name);
        }
    }
}