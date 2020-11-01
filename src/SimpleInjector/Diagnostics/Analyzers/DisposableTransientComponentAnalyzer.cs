// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics.Analyzers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    internal class DisposableTransientComponentAnalyzer : IContainerAnalyzer
    {
        public DiagnosticType DiagnosticType => DiagnosticType.DisposableTransientComponent;

        public string Name => "Disposable Transient Components";

        public string GetRootDescription(DiagnosticResult[] results) =>
            $"{results.Length} disposable transient {ComponentPlural(results.Length)} found.";

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results)
        {
            var count = results.Count();
            return $"{count} disposable transient {ComponentPlural(count)}.";
        }

        public DiagnosticResult[] Analyze(IEnumerable<InstanceProducer> producers)
        {
            var results =
                from producer in producers
                let registration = producer.Registration
                where registration.Lifestyle == Lifestyle.Transient
                where GetDisposableType(registration.ImplementationType) != null
                where registration.ShouldNotBeSuppressed(this.DiagnosticType)
                group producer by producer.Registration.ImplementationType into g
                from producer in g
                select new DisposableTransientComponentDiagnosticResult(
                    producer.ServiceType, producer, BuildDescription(g.Key, g));

            return results.ToArray();
        }

        private static string BuildDescription(
            Type implementationType, IEnumerable<InstanceProducer> producers) =>
            string.Format(
                CultureInfo.InvariantCulture,
                "{0} is registered as transient, but implements {1}.{2}",
                implementationType.FriendlyName(),
                (GetDisposableType(implementationType)!).ToFriendlyName(),
                OtherServiceTypesThanImplementationType(implementationType, producers)
                    ? BuildServiceDescription(implementationType, producers)
                    : string.Empty);

        private static bool OtherServiceTypesThanImplementationType(
            Type implementationType, IEnumerable<InstanceProducer> producers) =>
            producers.Select(p => p.ServiceType).Distinct().Except(new[] { implementationType }).Any();

        private static string BuildServiceDescription(
            Type implementationType, IEnumerable<InstanceProducer> producers)
        {
            var serviceTypes = producers.Select(p => p.ServiceType).Distinct().ToArray();

            return string.Format(
                CultureInfo.InvariantCulture,
                " {0} was registered for {1} {2}.",
                implementationType.ToFriendlyName(),
                serviceTypes.Length == 1 ? "service" : "services",
                serviceTypes.Select(t => t.FriendlyName()).ToCommaSeparatedText());
        }

        private static string ComponentPlural(int number) => number == 1 ? "component" : "components";

        private static Type? GetDisposableType(Type implementationType)
        {
            if (typeof(IDisposable).IsAssignableFrom(implementationType))
            {
                return typeof(IDisposable);
            }
#if NET461 || NETSTANDARD2_0 || NETSTANDARD2_1
else if (typeof(IAsyncDisposable).IsAssignableFrom(implementationType))
            {
                return typeof(IAsyncDisposable);
            }
#endif
            else
            {
                return null;
            }
        }
    }
}