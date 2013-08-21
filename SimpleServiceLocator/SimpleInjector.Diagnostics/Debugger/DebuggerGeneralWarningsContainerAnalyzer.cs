#region Copyright (c) 2013 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2013 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
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
    using System.Linq;
    using SimpleInjector.Diagnostics.Debugger;

    internal static class DebuggerGeneralWarningsContainerAnalyzer
    {
        internal static DebuggerViewItem Analyze(Container container)
        {
            const string WarningsName = "Configuration Warnings";

            var analysisResults = (
                from analyzer in ContainerAnalyzerProvider.Analyzers
                let results = analyzer.Analyze(container)
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