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
                where GetDisposableType(registration.ImplementationType) != null
                where registration.ShouldNotBeSuppressed(this.DiagnosticType)
                select producer;

            var results =
                from producer in invalidProducers
                select new DisposableTransientComponentDiagnosticResult(
                    producer.ServiceType, producer, BuildDescription(producer.Registration.ImplementationType));

            return results.ToArray();
        }

        private static string BuildDescription(Type implementationType) =>
            string.Format(
                CultureInfo.InvariantCulture,
                "{0} is registered as transient, but implements {1}.",
                implementationType.FriendlyName(),
                GetDisposableType(implementationType)!.ToFriendlyName());

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