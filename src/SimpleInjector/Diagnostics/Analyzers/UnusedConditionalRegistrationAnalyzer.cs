// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics.Analyzers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    internal sealed class UnusedConditionalRegistrationAnalyzer : IContainerAnalyzer
    {
        public DiagnosticType DiagnosticType => DiagnosticType.UnusedConditionalRegistration;

        public string Name => "Unused Conditional Registration";

        public string GetRootDescription(DiagnosticResult[] results) =>
            $"{results.Length} unused conditional {RegistrationsPlural(results.Length)} found.";

        private static string RegistrationsPlural(int number) => number == 1 ? "registration" : "registrations";

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();
            return $"{count} unused conditional {RegistrationsPlural(count)}.";
        }

        public DiagnosticResult[] Analyze(IEnumerable<InstanceProducer> producers)
        {
            var results =
                from producerToCheck in producers
                where producerToCheck.IsConditional
                where producerToCheck.Registration
                    .ShouldNotBeSuppressed(DiagnosticType.UnusedConditionalRegistration)
                let consumers =
                    from consumer in producers
                    where consumer != producerToCheck
                    where consumer.GetRelationships().Any(r => r.Dependency == producerToCheck)
                    select consumer
                where !consumers.Any()
                select new UnusedConditionalRegistration(
                    producer: producerToCheck,
                    description: BuildDescription(producerToCheck));

            return results.ToArray();
        }

        private static string BuildDescription(InstanceProducer diagnosedProducer) =>
            string.Format(
                CultureInfo.InvariantCulture,
                "The conditional registration for {0} ({1}) with implementation {2} is never used as a " +
                "dependency.",
                diagnosedProducer.ServiceType.FriendlyName(),
                diagnosedProducer.Registration.Lifestyle.Name,
                diagnosedProducer.Registration.ImplementationType.FriendlyName());
    }
}