#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2015 Simple Injector Contributors
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
                DiagnosticResultGrouper.Group(analyzerResults.Analyzer, analyzerResults.Results);
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
            .SelectMany(GetSelfAndDependentProducers)
            .Distinct(ReferenceEqualityComparer<InstanceProducer>.Instance)
            .ToArray();

        private static IEnumerable<InstanceProducer> GetSelfAndDependentProducers(InstanceProducer producer) =>
            GetSelfAndDependentProducers(producer,
                new HashSet<InstanceProducer>(ReferenceEqualityComparer<InstanceProducer>.Instance));

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
                foreach (var dependentProducer in GetSelfAndDependentProducers(relationship.Dependency, set))
                {
                    yield return set.AddReturn(dependentProducer);
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