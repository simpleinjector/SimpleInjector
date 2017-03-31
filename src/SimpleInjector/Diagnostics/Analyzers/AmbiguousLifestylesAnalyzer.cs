#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015 Simple Injector Contributors
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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    internal sealed class AmbiguousLifestylesAnalyzer : IContainerAnalyzer
    {
        internal static readonly IContainerAnalyzer Instance = new AmbiguousLifestylesAnalyzer();

        private AmbiguousLifestylesAnalyzer()
        {
        }

        public DiagnosticType DiagnosticType => DiagnosticType.AmbiguousLifestyles;

        public string Name => "Component with ambiguous lifestyles";

        public string GetRootDescription(IEnumerable<DiagnosticResult> results) =>
            results.Count() + " possible registrations found with ambiguous lifestyles.";

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results) =>
            results.Count() + " ambiguous lifestyles.";

        public DiagnosticResult[] Analyze(IEnumerable<InstanceProducer> producers)
        {
            var warnings = GetDiagnosticWarnings(producers);

            var results =
                from warning in warnings
                where warning.DiagnosedRegistration.Registration.ShouldNotBeSuppressed(this.DiagnosticType)
                select warning;

            return results.ToArray();
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification =
            "Reducing cyclomatic complexity of this method would mean breaking it up in smaller methods, " +
            "but that would force us to create a number of small classes just for this, because C# does " +
            "not allow returning anonymous types through private methods. We think keeping everything in " +
            "one method is the cleanest approach.")]
        private static IEnumerable<AmbiguousLifestylesDiagnosticResult> GetDiagnosticWarnings(
            IEnumerable<InstanceProducer> instanceProducers)
        {
            var registrations =
                from instanceProducer in instanceProducers
                where !instanceProducer.Registration.WrapsInstanceCreationDelegate
                group instanceProducer by instanceProducer.Registration into g
                let registration = g.Key
                select new { registration, Producers = g.ToArray() };

            var componentLifestylePairs =
                from registrationWithProducers in registrations
                let r = registrationWithProducers.registration
                let componentLifestylePair = new { r.ImplementationType, Lifestyle = r.Lifestyle.IdentificationKey }
                group registrationWithProducers by componentLifestylePair into g
                select new
                {
                    g.Key.ImplementationType,
                    Producers = g.SelectMany(registration => registration.Producers)
                };

            var components =
                from componentLifestylePair in componentLifestylePairs
                group componentLifestylePair by componentLifestylePair.ImplementationType into g
                select new { componentLifestylePairs = g.ToArray() };

            var ambiguousComponents =
                from component in components
                where component.componentLifestylePairs.Length > 1
                select component;

            return
                from component in ambiguousComponents
                from componentLifestylePair in component.componentLifestylePairs
                from diagnosedProducer in componentLifestylePair.Producers
                let conflictingPairs = component.componentLifestylePairs.Except(new[] { componentLifestylePair })
                let conflictingProducers = conflictingPairs.SelectMany(pair => pair.Producers)
                select CreateDiagnosticResult(diagnosedProducer, conflictingProducers.ToArray());
        }

        private static AmbiguousLifestylesDiagnosticResult CreateDiagnosticResult(
            InstanceProducer diagnosedProducer,
            InstanceProducer[] conflictingProducers)
        {
            Type serviceType = diagnosedProducer.ServiceType;
            Type implementationType = diagnosedProducer.Registration.ImplementationType;

            var lifestyles =
                from producer in conflictingProducers.Concat(new[] { diagnosedProducer })
                let lifestyle = producer.Registration.Lifestyle
                group lifestyle by lifestyle.IdentificationKey into g
                select g.First();

            string description = BuildDescription(diagnosedProducer, conflictingProducers);

            return new AmbiguousLifestylesDiagnosticResult(serviceType, description,
                lifestyles.ToArray(), implementationType, diagnosedProducer, conflictingProducers);
        }

        private static string BuildDescription(InstanceProducer diagnosedProducer,
            InstanceProducer[] conflictingProducers) =>
            string.Format(CultureInfo.InvariantCulture,
                "The registration for {0} ({1}) maps to the same implementation ({2}) as the {3} for {4} " +
                "{5}, but the {3} {6} to a different lifestyle. This will cause each registration to " +
                "resolve to a different instance.",
                diagnosedProducer.ServiceType.ToFriendlyName(),
                diagnosedProducer.Registration.Lifestyle.Name,
                diagnosedProducer.Registration.ImplementationType.ToFriendlyName(),
                conflictingProducers.Length == 1 ? "registration" : "registrations",
                conflictingProducers.Select(ToFriendlyNameWithLifestyle).ToCommaSeparatedText(),
                conflictingProducers.Length == 1 ? "does" : "do",
                conflictingProducers.Length == 1 ? "maps" : "map");

        private static string ToFriendlyNameWithLifestyle(InstanceProducer producer) =>
            producer.ServiceType.ToFriendlyName() + " (" + producer.Registration.Lifestyle.Name + ")";
    }
}