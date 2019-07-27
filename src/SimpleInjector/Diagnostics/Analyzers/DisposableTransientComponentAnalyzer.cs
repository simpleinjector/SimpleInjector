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
            var invalidProducers =
                from producer in producers
                let registration = producer.Registration
                where registration.Lifestyle == Lifestyle.Transient
                where typeof(IDisposable).IsAssignableFrom(registration.ImplementationType)
                where registration.ShouldNotBeSuppressed(this.DiagnosticType)
                select producer;

            var results =
                from producer in invalidProducers
                select new DisposableTransientComponentDiagnosticResult(
                    producer.ServiceType, producer, BuildDescription(producer));

            return results.ToArray();
        }

        private static string BuildDescription(InstanceProducer producer) =>
            string.Format(
                CultureInfo.InvariantCulture,
                "{0} is registered as transient, but implements IDisposable.",
                producer.Registration.ImplementationType.FriendlyName());

        private static string ComponentPlural(int number) => number == 1 ? "component" : "components";
    }
}