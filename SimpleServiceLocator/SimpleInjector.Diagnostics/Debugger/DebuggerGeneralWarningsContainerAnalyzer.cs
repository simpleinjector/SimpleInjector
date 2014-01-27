#region Copyright (c) 2013 Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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

namespace SimpleInjector.Diagnostics.Analyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using SimpleInjector.Diagnostics.Debugger;

    internal static class DebuggerGeneralWarningsContainerAnalyzer
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = 
                "FxCop is being smart here. The code is called by a class that is instantiated in the " +
                "debugger.")]
        internal static DebuggerViewItem Analyze(Container container)
        {
            const string WarningsName = "Configuration Warnings";

            var producersToAnalyze = container.GetCurrentRegistrations();

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
                return new DebuggerViewItem(WarningsName, "Warnings in multiple groups have been detected.",
                    analysisResults);
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