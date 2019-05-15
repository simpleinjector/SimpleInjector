// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics.Analyzers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using SimpleInjector.Advanced;

    internal sealed class LifestyleMismatchAnalyzer : IContainerAnalyzer
    {
        public DiagnosticType DiagnosticType => DiagnosticType.LifestyleMismatch;

        public string Name => "Lifestyle Mismatches";

        public string GetRootDescription(DiagnosticResult[] results)
        {
            var serviceCount = results.Select(result => result.ServiceType).Distinct().Count();

            return
                $"{results.Length} lifestyle {MismatchPlural(results.Length)} " +
                $"for {serviceCount} {ServicePlural(serviceCount)}.";
        }

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();

            return $"{count} {MismatchPlural(count)}.";
        }

        public DiagnosticResult[] Analyze(IEnumerable<InstanceProducer> producers) => (
            from producer in producers
            from relationship in producer.GetRelationships()
            where relationship.Dependency.Registration.ShouldNotBeSuppressed(this.DiagnosticType)
            let container = producer.Registration.Container
            where LifestyleMismatchChecker.HasLifestyleMismatch(container, relationship)
            select new LifestyleMismatchDiagnosticResult(
                serviceType: producer.ServiceType,
                description: BuildRelationshipDescription(relationship),
                relationship: relationship))
            .ToArray();

        private static string BuildRelationshipDescription(KnownRelationship relationship) =>
            string.Format(
                CultureInfo.InvariantCulture,
                "{0} ({1}) depends on {2}{3} ({4}).{5}{6}",
                relationship.ImplementationType.ToFriendlyName(),
                relationship.Lifestyle.Name,
                relationship.Dependency.ServiceType.ToFriendlyName(),
                relationship.Dependency.ServiceType != relationship.Dependency.ImplementationType
                    ? " implemented by " + relationship.Dependency.ImplementationType.ToFriendlyName()
                    : string.Empty,
                relationship.Dependency.Lifestyle.Name,
                relationship.AdditionalInformation == string.Empty ? string.Empty : " ",
                relationship.AdditionalInformation);

        private static string ServicePlural(int number) => number == 1 ? "service" : "services";

        private static string MismatchPlural(int number) => number == 1 ? "mismatch" : "mismatches";
    }
}