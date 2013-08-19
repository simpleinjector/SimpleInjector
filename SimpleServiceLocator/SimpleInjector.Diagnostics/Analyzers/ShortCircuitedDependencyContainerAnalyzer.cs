#region Copyright (c) 2013 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2013 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
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

    internal sealed class ShortCircuitedDependencyContainerAnalyzer : IContainerAnalyzer
    {
        public DiagnosticType DiagnosticType
        {
            get { return DiagnosticType.ShortCircuitedDependency; }
        }

        public string Name
        {
            get { return "Possible Short Circuited Dependencies"; }
        }

        public string GetRootDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();

            return count + " " + ComponentPlural(count) +
                " possibly short circuits to concrete unregistered types.";
        }

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results)
        {
            int count = results.Count();

            return count + " short circuited " + ComponentPlural(count) + ".";
        }

        DiagnosticResult[] IContainerAnalyzer.Analyze(Container container)
        {
            return this.Analyze(container);
        }

        public ShortCircuitedDependencyDiagnosticResult[] Analyze(Container container)
        {
            var registrations = container.GetCurrentRegistrations();

            var containerRegisteredRegistrations =
                from producer in container.GetCurrentRegistrations()
                where producer.IsContainerAutoRegistered
                select producer;

            Dictionary<Type, IEnumerable<InstanceProducer>> registeredImplementationTypes = (
                from registration in registrations
                where registration.ServiceType != registration.ImplementationType
                group registration by registration.ImplementationType into registrationGroup
                select registrationGroup)
                .ToDictionary(g => g.Key, g => (IEnumerable<InstanceProducer>)g);

            Dictionary<Type, InstanceProducer> autoRegisteredRegistrationsWithLifestyleMismatch = (
                from registration in containerRegisteredRegistrations
                let registrationIsPossiblyShortCircuited =
                    registeredImplementationTypes.ContainsKey(registration.ServiceType)
                where registrationIsPossiblyShortCircuited
                let registrationsWithThisImplementationType =
                    registeredImplementationTypes[registration.ServiceType]
                let hasLifestyleMismatch =
                    registrationsWithThisImplementationType.Any(r => r.Lifestyle != registration.Lifestyle)
                where hasLifestyleMismatch
                select registration)
                .ToDictionary(producer => producer.ServiceType);

            return (
                from registration in registrations
                from actualDependency in registration.GetRelationships()
                where autoRegisteredRegistrationsWithLifestyleMismatch.ContainsKey(
                    actualDependency.Dependency.ServiceType)
                let possibleSkippedRegistrations =
                    registeredImplementationTypes[actualDependency.Dependency.ServiceType]
                select new ShortCircuitedDependencyDiagnosticResult(
                    type: registration.ServiceType,
                    description: BuildDescription(actualDependency, possibleSkippedRegistrations),
                    registration: registration,
                    actualDependency: actualDependency,
                    expectedDependencies: possibleSkippedRegistrations))
                .ToArray();
        }

        private static string BuildDescription(KnownRelationship relationship,
            IEnumerable<InstanceProducer> possibleSkippedRegistrations)
        {
            var possibleSkippedRegistrationsDescription = string.Join(" or ",
                from possibleSkippedRegistration in possibleSkippedRegistrations
                select string.Format(CultureInfo.InvariantCulture, "{0} ({1})",
                    possibleSkippedRegistration.ServiceType.ToFriendlyName(),
                    possibleSkippedRegistration.Lifestyle.Name));

            return string.Format(CultureInfo.InvariantCulture,
                "{0} might incorrectly depend on unregistered type {1} ({2}) instead of {3}.",
                relationship.ImplementationType.ToFriendlyName(),
                relationship.Dependency.ServiceType.ToFriendlyName(),
                relationship.Dependency.Lifestyle.Name,
                possibleSkippedRegistrationsDescription);
        }

        private static string ComponentPlural(int number)
        {
            return number == 1 ? "component" : "components";
        }

        private static string HasPlural(int number)
        {
            return number == 1 ? "has" : "have";
        }
    }
}