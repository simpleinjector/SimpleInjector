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
using System.Collections.ObjectModel;
using System.Linq;

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
    ///     where result.DiagnosticType == DiagnosticType.PotentialLifestyleMismatch
    ///     let mismatch = (PotentialLifestyleMismatchDiagnosticResult)result
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
            Requires.IsNotNull(container, "container");
            RequiresContainerToBeVerified(container);

            var producersToAnalyse = container.GetCurrentRegistrations();

            var analyzerResultsCollection = (
                from analyzer in ContainerAnalyzerProvider.Analyzers
                let results = analyzer.Analyze(producersToAnalyse)
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

        internal static IEnumerable<RegistrationInfo> GetRootRegistrations(Container container)
        {
            Requires.IsNotNull(container, "container");
            RequiresContainerToBeVerified(container);

            return container.GetRootRegistrations().Select(ToRegistrationInfo);
        }

        private static RegistrationInfo ToRegistrationInfo(InstanceProducer producer)
        {
            // TODO: Recreate decorator structure.
            // TODO: How to handle collections?
            // TODO: How to deal with groups of types  (Generic registrations?)? Should there be a 'group' thing?
            return new RegistrationInfo(
                producer.ServiceType,
                producer.Registration.ImplementationType,
                producer.Lifestyle, // This is not the right lifestyle.
                producer.GetRelationships().Select(r => ToRegistrationInfo(r.Dependency)));
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

    internal class RegistrationInfo
    {
        internal RegistrationInfo(Type serviceType, Type implementationType, Lifestyle lifestyle,
            IEnumerable<RegistrationInfo> dependencies)
        {
            this.ServiceType = serviceType;
            this.ImplementationType = implementationType;
            this.Lifestyle = lifestyle;
            this.Dependencies = new ReadOnlyCollection<RegistrationInfo>(dependencies.ToList());
        }

        public Type ServiceType { get; set; }

        public Type ImplementationType { get; set; }

        public Lifestyle Lifestyle { get; private set; }

        public ReadOnlyCollection<RegistrationInfo> Dependencies { get; private set; }

        public override string ToString()
        {
            return this.Visualize(indentingDepth: 0);
        }

        internal string Visualize(int indentingDepth)
        {
            var visualizedDependencies =
                from dependency in this.Dependencies
                select Environment.NewLine + dependency.Visualize(indentingDepth + 1);

            return string.Format("{0}new {1}({2})",
                new string('\t', indentingDepth) +
                this.ImplementationType.ToFriendlyName(),
                string.Join(",", visualizedDependencies));
        }
    }
}