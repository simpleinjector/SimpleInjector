// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

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
        public DiagnosticType DiagnosticType => DiagnosticType.TornLifestyle;

        public string Name => "Torn Lifestyle";

        public string GetRootDescription(DiagnosticResult[] results) =>
            $"{results.Length} possible {RegistrationsPlural(results.Length)} found with a torn lifestyle.";

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();
            return $"{count} torn {RegistrationsPlural(count)}.";
        }

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
            let lifestyle = registration.Lifestyle.IdentificationKey
            let key = new { registration.ImplementationType, lifestyle }
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

            return new TornLifestyleDiagnosticResult(
                serviceType,
                description,
                lifestyle,
                implementationType,
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
                diagnosedProducer.ServiceType.FriendlyName(),
                tornProducers.Length == 1 ? "registration" : "registrations",
                tornProducers.Select(producer => producer.ServiceType.FriendlyName()).ToCommaSeparatedText(),
                tornProducers.Length == 1 ? "does" : "do",
                tornProducers.Length == 1 ? "both" : "all",
                diagnosedProducer.Registration.ImplementationType.FriendlyName(),
                lifestyle.Name,
                lifestyle == Lifestyle.Singleton ? string.Empty : " during a single " + lifestyle.Name);
        }

        private static string RegistrationsPlural(int number) => number == 1 ? "registration" : "registrations";
    }
}