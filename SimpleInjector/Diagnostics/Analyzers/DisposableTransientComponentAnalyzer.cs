﻿#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014 Simple Injector Contributors
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

    internal class DisposableTransientComponentAnalyzer : IContainerAnalyzer
    {
        internal static readonly IContainerAnalyzer Instance = new DisposableTransientComponentAnalyzer();

        private DisposableTransientComponentAnalyzer()
        {
        }

        public DiagnosticType DiagnosticType => DiagnosticType.DisposableTransientComponent;

        public string Name => "Disposable Transient Components";

        public string GetRootDescription(IEnumerable<DiagnosticResult> results)
        {
            var count = results.Count();
            return count + " disposable transient " + ComponentPlural(count) + " found.";
        }

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results)
        {
            var count = results.Count();
            return count + " disposable transient " + ComponentPlural(count) + ".";
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
                select new DisposableTransientComponentDiagnosticResult(producer.ServiceType, producer,
                    BuildDescription(producer));

            return results.ToArray();
        }

        private static string BuildDescription(InstanceProducer producer) => 
            string.Format(CultureInfo.InvariantCulture,
                "{0} is registered as transient, but implements IDisposable.",
                producer.Registration.ImplementationType.ToFriendlyName());

        private static string ComponentPlural(int number) => number == 1 ? "component" : "components";
    }
}