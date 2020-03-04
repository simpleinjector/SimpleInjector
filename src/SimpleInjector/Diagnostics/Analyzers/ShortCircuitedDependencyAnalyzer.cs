// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics.Analyzers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using SimpleInjector.Advanced;

    internal sealed class ShortCircuitedDependencyAnalyzer : IContainerAnalyzer
    {
        public DiagnosticType DiagnosticType => DiagnosticType.ShortCircuitedDependency;

        public string Name => "Possible Short-Circuited Dependencies";

        public string GetRootDescription(DiagnosticResult[] results)
        {
            return results.Length == 1
                ? "1 component possibly short circuits to a concrete unregistered type."
                : $"{results.Length} components possibly short circuit to a concrete unregistered type.";
        }

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();

            return count == 1
                ? "1 short-circuited component."
                : $"{count} short-circuited components.";
        }

        public DiagnosticResult[] Analyze(IEnumerable<InstanceProducer> producers)
        {
            Dictionary<Type, IEnumerable<InstanceProducer>> registeredImplementationTypes =
                GetRegisteredImplementationTypes(producers);

            Dictionary<Type, InstanceProducer> autoRegisteredRegistrationsWithLifestyleMismatch =
                GetAutoRegisteredRegistrationsWithLifestyleMismatch(producers, registeredImplementationTypes);

            var results =
                from producer in producers
                where producer.Registration.ShouldNotBeSuppressed(this.DiagnosticType)
                from actualDependency in producer.GetRelationships()
                where actualDependency.UseForVerification
                where autoRegisteredRegistrationsWithLifestyleMismatch.ContainsKey(
                    actualDependency.Dependency.ServiceType)
                let possibleSkippedRegistrations =
                    registeredImplementationTypes[actualDependency.Dependency.ServiceType]
                select new ShortCircuitedDependencyDiagnosticResult(
                    serviceType: producer.ServiceType,
                    description: BuildDescription(actualDependency, possibleSkippedRegistrations),
                    registration: producer,
                    relationship: actualDependency,
                    expectedDependencies: possibleSkippedRegistrations);

            return results.ToArray();
        }

        private static Dictionary<Type, IEnumerable<InstanceProducer>> GetRegisteredImplementationTypes(
            IEnumerable<InstanceProducer> producers) => (
            from producer in producers
            where producer.ServiceType != producer.ImplementationType
            group producer by producer.ImplementationType into registrationGroup
            select registrationGroup)
            .ToDictionary(g => g.Key, g => (IEnumerable<InstanceProducer>)g);

        private static Dictionary<Type, InstanceProducer> GetAutoRegisteredRegistrationsWithLifestyleMismatch(
            IEnumerable<InstanceProducer> producers,
            Dictionary<Type, IEnumerable<InstanceProducer>> registeredImplementationTypes)
        {
            var containerRegisteredRegistrations =
                from producer in producers
                where producer.IsContainerAutoRegistered
                select producer;

            var autoRegisteredRegistrationsWithLifestyleMismatch =
                from registration in containerRegisteredRegistrations
                let registrationIsPossiblyShortCircuited =
                    registeredImplementationTypes.ContainsKey(registration.ServiceType)
                where registrationIsPossiblyShortCircuited
                select registration;

            return autoRegisteredRegistrationsWithLifestyleMismatch
                .ToDictionary(producer => producer.ServiceType);
        }

        private static string BuildDescription(KnownRelationship relationship,
            IEnumerable<InstanceProducer> possibleSkippedRegistrations)
        {
            var possibleSkippedRegistrationsDescription = string.Join(" or ",
                from possibleSkippedRegistration in possibleSkippedRegistrations
                let name = possibleSkippedRegistration.ServiceType.FriendlyName()
                orderby name
                select string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} ({1})",
                    name,
                    possibleSkippedRegistration.Lifestyle.Name));

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} might incorrectly depend on unregistered type {1} ({2}) instead of {3}.",
                relationship.ImplementationType.FriendlyName(),
                relationship.Dependency.ServiceType.FriendlyName(),
                relationship.Dependency.Lifestyle.Name,
                possibleSkippedRegistrationsDescription);
        }
    }
}