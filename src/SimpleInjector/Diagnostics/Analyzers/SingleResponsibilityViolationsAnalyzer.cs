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

namespace SimpleInjector.Diagnostics.Analyzers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    internal sealed class SingleResponsibilityViolationsAnalyzer : IContainerAnalyzer
    {
        internal static readonly IContainerAnalyzer Instance = new SingleResponsibilityViolationsAnalyzer();

        private const int MaximumValidNumberOfDependencies = 7;

        private SingleResponsibilityViolationsAnalyzer()
        {
        }

        public DiagnosticType DiagnosticType => DiagnosticType.SingleResponsibilityViolation;

        public string Name => "Potential Single Responsibility Violations";

        public string GetRootDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();

            return count + " possible single responsibility " + ViolationPlural(count) + ".";
        }

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();

            return count + " possible " + ViolationPlural(count) + ".";
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
            if (!producer.ServiceType.IsGenericType())
            {
                return true;
            }

            return producer.ServiceType.GetGenericTypeDefinition() != typeof(IEnumerable<>);
        }

        private static string BuildRelationshipDescription(Type implementationType, int numberOfDependencies) =>
            string.Format(CultureInfo.InvariantCulture,
                "{0} has {1} dependencies which might indicate a SRP violation.",
                implementationType.ToFriendlyName(),
                numberOfDependencies);

        private static string ViolationPlural(int count) => count == 1 ? "violation" : "violations";
    }
}