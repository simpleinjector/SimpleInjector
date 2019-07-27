// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics.Analyzers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    internal sealed class SingleResponsibilityViolationsAnalyzer : IContainerAnalyzer
    {
        private const int MaximumValidNumberOfDependencies = 7;

        public DiagnosticType DiagnosticType => DiagnosticType.SingleResponsibilityViolation;

        public string Name => "Potential Single Responsibility Violations";

        public string GetRootDescription(DiagnosticResult[] results) =>
            $"{results.Length} possible single responsibility {ViolationPlural(results.Length)}.";

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();

            return $"{count} possible {ViolationPlural(count)}.";
        }

        public DiagnosticResult[] Analyze(IEnumerable<InstanceProducer> producers) => (
            from producer in producers
            where producer.Registration.ShouldNotBeSuppressed(this.DiagnosticType)
            where IsAnalyzable(producer)
            from relationship in producer.GetRelationships()
            group relationship by new { relationship.ImplementationType, producer } into g
            let numberOfUniqueDependencies = g.Select(i => i.Dependency.ServiceType).Distinct().Count()
            where numberOfUniqueDependencies > MaximumValidNumberOfDependencies
            let dependencies = g.Select(r => r.Dependency).ToArray()
            select new SingleResponsibilityViolationDiagnosticResult(
                serviceType: g.Key.producer.ServiceType,
                description: BuildRelationshipDescription(g.Key.ImplementationType, dependencies.Length),
                implementationType: g.Key.ImplementationType,
                dependencies: dependencies))
            .ToArray();

        private static bool IsAnalyzable(InstanceProducer producer)
        {
            // We can't analyze collections, because this would lead to false positives when decorators are
            // applied to the collection. For a decorator, each collection element it decorates is a
            // dependency, which will make it look as if the decorator has too many dependencies. Since the
            // container will delegate the creation of those elements back to the container, those elements
            // would by them selves still get analyzed, so the only thing we'd miss here is the decorator.
            return !typeof(IEnumerable<>).IsGenericTypeDefinitionOf(producer.ServiceType);
        }

        private static string BuildRelationshipDescription(Type implementationType, int numberOfDependencies) =>
            string.Format(CultureInfo.InvariantCulture,
                "{0} has {1} dependencies which might indicate a SRP violation.",
                implementationType.FriendlyName(),
                numberOfDependencies);

        private static string ViolationPlural(int count) => count == 1 ? "violation" : "violations";
    }
}