// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector.Internals;

    /// <summary>
    /// Entry point for doing diagnostic analysis on <see cref="Container"/> instances.
    /// </summary>
    /// <example>
    /// The following example shows the usage of the <b>Analyzer</b> class:
    /// <code lang="cs"><![CDATA[
    /// DiagnosticResult[] results = Analyzer.Analyze(container);
    ///
    /// var typesWithAShortedLifetimeThanTheirDependencies =
    ///     from result in results
    ///     where result.DiagnosticType == DiagnosticType.LifestyleMismatch
    ///     let mismatch = (LifestyleMismatchDiagnosticResult)result
    ///     select mismatch.Relationship.ImplementationType;
    /// ]]></code>
    /// </example>
    public static class Analyzer
    {
        /// <summary>
        /// Analyzes the supplied <paramref name="container"/> instance.
        /// </summary>
        /// <param name="container">The container instance to analyze.</param>
        /// <returns>A collection of <see cref="DiagnosticResult"/> sub types that describe the diagnostic
        /// warnings and messages.</returns>
        public static DiagnosticResult[] Analyze(Container container)
        {
            Requires.IsNotNull(container, nameof(container));
            RequiresContainerToBeVerified(container);

            var producersToAnalyze = GetProducersToAnalyze(container);

            var analyzerResultsCollection = (
                from analyzer in ContainerAnalyzerProvider.Analyzers
                let results = analyzer.Analyze(producersToAnalyze)
                where results.Any()
                select new { Results = results, Analyzer = analyzer })
                .ToArray();

            foreach (var analyzerResults in analyzerResultsCollection)
            {
                DiagnosticResultGrouper.Group(analyzerResults.Analyzer!, analyzerResults.Results!);
            }

            return (
                from analyzerResults in analyzerResultsCollection
                from result in analyzerResults.Results
                select result)
                .ToArray();
        }

        internal static InstanceProducer[] GetProducersToAnalyze(Container container) =>
            container
            .GetCurrentRegistrations()
            .SelectMany(SelfAndWrappedProducers)
            .SelectMany(GetSelfAndDependentProducers)
            .Distinct(InstanceProducer.EqualityComparer)
            .ToArray();

        private static IEnumerable<InstanceProducer> SelfAndWrappedProducers(InstanceProducer producer) =>
            producer.SelfAndWrappedProducers;

        private static IEnumerable<InstanceProducer> GetSelfAndDependentProducers(InstanceProducer producer) =>
            GetSelfAndDependentProducers(producer,
                new HashSet<InstanceProducer>(InstanceProducer.EqualityComparer));

        private static IEnumerable<InstanceProducer> GetSelfAndDependentProducers(InstanceProducer producer,
            HashSet<InstanceProducer> set)
        {
            // Prevent stack overflow exception in case the graph is cyclic.
            if (set.Contains(producer))
            {
                yield break;
            }

            // Return self
            yield return set.AddReturn(producer);

            // Return dependent producers
            foreach (var relationship in producer.GetRelationships())
            {
                if (relationship.UseForVerification)
                {
                    foreach (var dependentProducer in GetSelfAndDependentProducers(relationship.Dependency, set))
                    {
                        yield return set.AddReturn(dependentProducer);
                    }
                }
            }
        }

        private static void RequiresContainerToBeVerified(Container container)
        {
            if (!container.SuccesfullyVerified)
            {
                throw new InvalidOperationException(
                    "Please make sure that Container.Verify() is called on the supplied container instance. " +
                    "Only successfully verified container instance can be analyzed.");
            }
        }
    }
}