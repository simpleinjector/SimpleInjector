#region Copyright Simple Injector Contributors
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

namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal class DiagnosticResultGrouper
    {
        private readonly IContainerAnalyzer analyzer;

        internal DiagnosticResultGrouper(IContainerAnalyzer analyzer)
        {
            this.analyzer = analyzer;
        }

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

        private DiagnosticGroup BuildDiagnosticGroup(Type groupType, IEnumerable<DiagnosticResult> results, 
            int level) => 
            groupType.ContainsGenericParameters()
                ? this.BuildGenericGroup(groupType, results, level)
                : this.BuildNonGenericGroup(groupType, results);

        private DiagnosticGroup BuildGenericGroup(Type groupType, IEnumerable<DiagnosticResult> results,
            int level)
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