namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using SimpleInjector.Diagnostics.Analyzers;

    public class Analyzer
    {
        private readonly Container container;

        public Analyzer(Container container)
        {
            this.container = container;
        }

        public DiagnosticResult[] Analyze()
        {
            // TODO: Check if container is verified.

            var resultGroups = (
                from analyzer in this.GetAnalyzers()
                let results = analyzer.Analyze(this.container)
                where results.Any()
                select new { results, analyzer })
                .ToArray();

            foreach (var resultGroup in resultGroups)
            {
                DiagnosticResultGrouper.Group(resultGroup.analyzer, resultGroup.results);
            }

            return (
                from resultGroup in resultGroups
                from result in resultGroup.results
                select result)
                .ToArray();
        }

        private IEnumerable<IContainerAnalyzer> GetAnalyzers()
        {
            yield return new PotentialLifestyleMismatchContainerAnalyzer();
            yield return new ShortCircuitedDependencyContainerAnalyzer();
            yield return new SingleResponsibilityViolationsAnalyzer();
            yield return new ContainerRegisteredServiceContainerAnalyzer();
        }
    }
}
