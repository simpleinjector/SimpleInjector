namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class DiagnosticResultGrouper
    {
        private readonly IContainerAnalyzer analyzer;

        internal DiagnosticResultGrouper(IContainerAnalyzer analyzer)
        {
            this.analyzer = analyzer;
        }

        internal static DiagnosticGroup Group(IContainerAnalyzer analyzer, DiagnosticResult[] results)
        {
            return new DiagnosticResultGrouper(analyzer).Group(results);
        }

        internal DiagnosticGroup Group(DiagnosticResult[] results)
        {
            var childGroups = this.GroupResults(results, level: 0);

            var groupResults = GetGroupResults(results, level: 0);

            return new DiagnosticGroup(
                diagnosticType: this.analyzer.DiagnosticType,
                groupType: typeof(object),
                name: this.analyzer.Name,
                description: this.analyzer.GetRootDescription(results),
                children: childGroups,
                results: groupResults);
        }

        private DiagnosticGroup[] GroupResults(IEnumerable<DiagnosticResult> results, int level)
        {
            return (
                from result in results
                group result by MakeTypePartiallyGenericUpToLevel(result.Type, level) into resultGroup
                where resultGroup.Count() > 1
                select this.BuildDiagnosticGroup(resultGroup.Key, resultGroup, level + 1))
                .ToArray();
        }

        private static Type MakeTypePartiallyGenericUpToLevel(Type serviceType, int level)
        {
            return TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(serviceType, level);
        }

        private DiagnosticGroup BuildDiagnosticGroup(Type groupType,
            IEnumerable<DiagnosticResult> results, int level)
        {
            if (groupType.ContainsGenericParameters)
            {
                return this.BuildGenericGroup(groupType, results, level);
            }
            else
            {
                return this.BuildNonGenericGroup(groupType, results);
            }
        }

        private DiagnosticGroup BuildGenericGroup(Type groupType, IEnumerable<DiagnosticResult> results,
            int level)
        {
            DiagnosticGroup[] childGroups = this.GroupResults(results, level);

            var groupResults = GetGroupResults(results, level);

            if (childGroups.Length == 1)
            {
                // This flatterns the hierarcy when there is just one item in the group.
                return childGroups[0];
            }

            return new DiagnosticGroup(
                diagnosticType: this.analyzer.DiagnosticType,
                groupType: groupType,
                name: Helpers.ToFriendlyName(groupType),
                description: this.analyzer.GetGroupDescription(results),
                children: childGroups,
                results: groupResults);
        }

        private DiagnosticGroup BuildNonGenericGroup(Type closedType, IEnumerable<DiagnosticResult> results)
        {
            return new DiagnosticGroup(
                diagnosticType: this.analyzer.DiagnosticType,
                groupType: closedType,
                name: Helpers.ToFriendlyName(closedType),
                description: this.analyzer.GetGroupDescription(results),
                children: Enumerable.Empty<DiagnosticGroup>(),
                results: results);
        }

        private static DiagnosticResult[] GetGroupResults(IEnumerable<DiagnosticResult> results, int level)
        {
            return (
                from result in results
                group result by MakeTypePartiallyGenericUpToLevel(result.Type, level) into resultGroup
                where resultGroup.Count() == 1
                select resultGroup.Single())
                .ToArray();
        }
    }
}