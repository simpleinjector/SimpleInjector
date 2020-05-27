// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class DiagnosticResultGrouper
    {
        private readonly IContainerAnalyzer analyzer;

        internal DiagnosticResultGrouper(IContainerAnalyzer analyzer) => this.analyzer = analyzer;

        internal static DiagnosticGroup Group(IContainerAnalyzer analyzer, DiagnosticResult[] results) =>
            new DiagnosticResultGrouper(analyzer).Group(results);

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

        private DiagnosticGroup[] GroupResults(IEnumerable<DiagnosticResult> results, int level) => (
            from result in results
            group result by MakeTypePartiallyGenericUpToLevel(result.ServiceType, level) into resultGroup
            where resultGroup.Count() > 1
            select this.BuildDiagnosticGroup(resultGroup.Key, resultGroup, level + 1))
            .ToArray();

        private static Type MakeTypePartiallyGenericUpToLevel(Type serviceType, int level) =>
            TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(serviceType, level);

        private DiagnosticGroup BuildDiagnosticGroup(
            Type groupType, IEnumerable<DiagnosticResult> results, int level) =>
            groupType.ContainsGenericParameters()
                ? this.BuildGenericGroup(groupType, results, level)
                : this.BuildNonGenericGroup(groupType, results);

        private DiagnosticGroup BuildGenericGroup(
            Type groupType, IEnumerable<DiagnosticResult> results, int level)
        {
            DiagnosticGroup[] childGroups = this.GroupResults(results, level);

            var groupResults = GetGroupResults(results, level);

            if (childGroups.Length == 1)
            {
                // This flattens the hierarchy when there is just one item in the group.
                return childGroups[0];
            }

            return new DiagnosticGroup(
                diagnosticType: this.analyzer.DiagnosticType,
                groupType: groupType,
                name: groupType.ToFriendlyName(),
                description: this.analyzer.GetGroupDescription(results),
                children: childGroups,
                results: groupResults);
        }

        private DiagnosticGroup BuildNonGenericGroup(Type closedType, IEnumerable<DiagnosticResult> results) =>
            new DiagnosticGroup(
                diagnosticType: this.analyzer.DiagnosticType,
                groupType: closedType,
                name: closedType.ToFriendlyName(),
                description: this.analyzer.GetGroupDescription(results),
                children: Enumerable.Empty<DiagnosticGroup>(),
                results: results);

        private static DiagnosticResult[] GetGroupResults(IEnumerable<DiagnosticResult> results, int level) => (
            from result in results
            group result by MakeTypePartiallyGenericUpToLevel(result.ServiceType, level) into resultGroup
            where resultGroup.Count() == 1
            select resultGroup.Single())
            .ToArray();
    }
}