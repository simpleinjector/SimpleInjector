// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics.Analyzers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    internal sealed class AmbiguousLifestylesAnalyzer : IContainerAnalyzer
    {
        public DiagnosticType DiagnosticType => DiagnosticType.AmbiguousLifestyles;

        public string Name => "Component with ambiguous lifestyles";

        public string GetRootDescription(DiagnosticResult[] results) =>
            $"{results.Length} possible {RegistrationsPlural(results.Length)} found with ambiguous lifestyles.";

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();
            return $"{count} ambiguous {LifestylesPlural(count)}.";
        }

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
            "not allow returning anonymous types through private methods. I think keeping everything in " +
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
                let componentLifestylePair =
                    new { r.ImplementationType, Lifestyle = r.Lifestyle.IdentificationKey }
                group registrationWithProducers by componentLifestylePair into g
                select new
                {
                    g.Key.ImplementationType,
                    Producers = g.SelectMany(registration => registration.Producers),
                };

            var components =
                from componentLifestylePair in componentLifestylePairs
                group componentLifestylePair by componentLifestylePair.ImplementationType into g
                select new { componentLifestylePairs = g.ToArray() };

            var ambiguousComponents =
                from component in components
                where component.componentLifestylePairs!.Length > 1
                select component;

            return
                from component in ambiguousComponents
                from componentLifestylePair in component.componentLifestylePairs
                from diagnosedProducer in componentLifestylePair.Producers
                let conflictingPairs =
                    component.componentLifestylePairs.Except(new[] { componentLifestylePair })
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

            return new AmbiguousLifestylesDiagnosticResult(
                serviceType,
                description,
                lifestyles.ToArray(),
                implementationType,
                diagnosedProducer,
                conflictingProducers);
        }

        private static string BuildDescription(
            InstanceProducer diagnosedProducer, InstanceProducer[] conflictingProducers) =>
            string.Format(
                CultureInfo.InvariantCulture,
                "The registration for {0} ({1}) maps to the same implementation ({2}) as the {3} for {4} " +
                "{5}, but the {3} {6} to a different lifestyle. This will cause each registration to " +
                "resolve to a different instance.",
                diagnosedProducer.ServiceType.FriendlyName(),
                diagnosedProducer.Registration.Lifestyle.Name,
                diagnosedProducer.Registration.ImplementationType.FriendlyName(),
                conflictingProducers.Length == 1 ? "registration" : "registrations",
                conflictingProducers.Select(ToFriendlyNameWithLifestyle).ToCommaSeparatedText(),
                conflictingProducers.Length == 1 ? "does" : "do",
                conflictingProducers.Length == 1 ? "maps" : "map");

        private static string ToFriendlyNameWithLifestyle(InstanceProducer producer) =>
            producer.ServiceType.FriendlyName() + " (" + producer.Registration.Lifestyle.Name + ")";

        private static string RegistrationsPlural(int number) => number == 1 ? "registration" : "registrations";
        private static string LifestylesPlural(int number) => number == 1 ? "lifestyle" : "lifestyles";
    }
}