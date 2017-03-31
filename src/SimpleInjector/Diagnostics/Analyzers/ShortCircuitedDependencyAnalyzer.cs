#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2015 Simple Injector Contributors
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
    using SimpleInjector.Advanced;

    internal sealed class ShortCircuitedDependencyAnalyzer : IContainerAnalyzer
    {
        internal static readonly IContainerAnalyzer Instance = new ShortCircuitedDependencyAnalyzer();

        private ShortCircuitedDependencyAnalyzer()
        {
        }

        public DiagnosticType DiagnosticType => DiagnosticType.ShortCircuitedDependency;

        public string Name => "Possible Short Circuited Dependencies";

        public string GetRootDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();

            if (count == 1)
            {
                return "1 component possibly short circuits to a concrete unregistered type.";
            }

            return count + " components possibly short circuit to a concrete unregistered type.";
        }

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();

            return count == 1 ? "1 short circuited component." : (count + " short circuited components.");
        }

        public DiagnosticResult[] Analyze(IEnumerable<InstanceProducer> producers)
        {
            Dictionary<Type, IEnumerable<InstanceProducer>> registeredImplementationTypes = 
                GetRegisteredImplementationTypes(producers);

            Dictionary<Type, InstanceProducer> autoRegisteredRegistrationsWithLifestyleMismatch =
                GetAutoRegisteredRegistrationsWithLifestyleMismatch(producers, registeredImplementationTypes);

            var results =
                from producer in producers
                where producer.Registration.ShouldNotBeSuppressed(this.DiagnosticType)
                from actualDependency in producer.GetRelationships()
                where autoRegisteredRegistrationsWithLifestyleMismatch.ContainsKey(
                    actualDependency.Dependency.ServiceType)
                let possibleSkippedRegistrations =
                    registeredImplementationTypes[actualDependency.Dependency.ServiceType]
                select new ShortCircuitedDependencyDiagnosticResult(
                    serviceType: producer.ServiceType,
                    description: BuildDescription(actualDependency, possibleSkippedRegistrations),
                    registration: producer,
                    relationship: actualDependency,
                    expectedDependencies: possibleSkippedRegistrations);

            return results.ToArray();
        }

        private static Dictionary<Type, IEnumerable<InstanceProducer>> GetRegisteredImplementationTypes(
            IEnumerable<InstanceProducer> producers) => (
            from producer in producers
            where producer.ServiceType != producer.ImplementationType
            group producer by producer.ImplementationType into registrationGroup
            select registrationGroup)
            .ToDictionary(g => g.Key, g => (IEnumerable<InstanceProducer>)g);

        private static Dictionary<Type, InstanceProducer> GetAutoRegisteredRegistrationsWithLifestyleMismatch(
            IEnumerable<InstanceProducer> producers,
            Dictionary<Type, IEnumerable<InstanceProducer>> registeredImplementationTypes)
        {
            var containerRegisteredRegistrations =
                from producer in producers
                where producer.IsContainerAutoRegistered
                select producer;

            var autoRegisteredRegistrationsWithLifestyleMismatch =
                from registration in containerRegisteredRegistrations
                let registrationIsPossiblyShortCircuited =
                    registeredImplementationTypes.ContainsKey(registration.ServiceType)
                where registrationIsPossiblyShortCircuited
                select registration;

            return autoRegisteredRegistrationsWithLifestyleMismatch.ToDictionary(producer => producer.ServiceType);
        }

        private static string BuildDescription(KnownRelationship relationship,
            IEnumerable<InstanceProducer> possibleSkippedRegistrations)
        {
            var possibleSkippedRegistrationsDescription = string.Join(" or ",
                from possibleSkippedRegistration in possibleSkippedRegistrations
                let name = possibleSkippedRegistration.ServiceType.ToFriendlyName()
                orderby name
                select string.Format(CultureInfo.InvariantCulture, "{0} ({1})",
                    name, possibleSkippedRegistration.Lifestyle.Name));

            return string.Format(CultureInfo.InvariantCulture,
                "{0} might incorrectly depend on unregistered type {1} ({2}) instead of {3}.",
                relationship.ImplementationType.ToFriendlyName(),
                relationship.Dependency.ServiceType.ToFriendlyName(),
                relationship.Dependency.Lifestyle.Name,
                possibleSkippedRegistrationsDescription);
        }
    }
}