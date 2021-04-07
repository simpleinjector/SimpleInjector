// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics.Analyzers
{
    using System.Linq;
    using SimpleInjector.Diagnostics.Debugger;

    internal static class DebuggerGeneralWarningsContainerAnalyzer
    {
        internal static DebuggerViewItem Analyze(Container container)
        {
            const string WarningsName = "Configuration Warnings";

            var producersToAnalyze = Analyzer.GetProducersToAnalyze(container);

            var analysisResults = (
                from analyzer in ContainerAnalyzerProvider.Analyzers
                let results = analyzer.Analyze(producersToAnalyze)
                where results.Any()
                let diagnosticGroup = DiagnosticResultGrouper.Group(analyzer, results)
                select new DebuggerViewItem(
                    name: diagnosticGroup.Name,
                    description: diagnosticGroup.Description,
                    value: ConvertToDebuggerViewItems(diagnosticGroup)))
                .ToArray();

            if (!analysisResults.Any())
            {
                return new DebuggerViewItem(WarningsName, "No warnings detected.");
            }
            else if (analysisResults.Length == 1)
            {
                return analysisResults.Single();
            }
            else
            {
                return new DebuggerViewItem(
                    WarningsName, "Warnings in multiple groups have been detected.", analysisResults);
            }
        }

        private static DebuggerViewItem[] ConvertToDebuggerViewItems(DiagnosticGroup group)
        {
            var childItems =
                from child in @group.Children
                select new DebuggerViewItem(
                    name: child.Name,
                    description: child.Description,
                    value: ConvertToDebuggerViewItems(child));

            var resultItems =
                from result in @group.Results
                select new DebuggerViewItem(
                    name: result.ServiceType.ToFriendlyName(),
                    description: result.Description,
                    value: result.Value);

            return childItems.Concat(resultItems)
                .OrderBy(item => item.Name)
                .ToArray();
        }
    }
}