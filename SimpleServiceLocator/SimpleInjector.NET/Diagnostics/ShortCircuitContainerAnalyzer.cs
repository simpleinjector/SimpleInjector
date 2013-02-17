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

namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using SimpleInjector.Advanced;

    internal sealed class ShortCircuitContainerAnalyzer : IContainerAnalyzer
    {
        public DebuggerViewItem Analyse(Container container)
        {
            var warnings = FindRegistrationsWithPossibleShortCircuitedDependencies(container);

            if (!warnings.Any())
            {
                return null;
            }

            int numberOfComponents = warnings.Select(r => r.Type).Distinct().Count();

            return new DebuggerViewItem(
                "Possible Short Circuited Dependencies",
                string.Format(CultureInfo.InvariantCulture,
                    "{0} {1} {2} been found that possibly short circuit to concrete unregistered types.",
                    numberOfComponents, ComponentPlural(numberOfComponents), HasPlural(numberOfComponents)),
                GroupWarnings(warnings));
        }

        private static DebuggerViewItemType[] FindRegistrationsWithPossibleShortCircuitedDependencies(
            Container container)
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
                from relationship in registration.GetRelationships()
                where autoRegisteredRegistrationsWithLifestyleMismatch.ContainsKey(
                    relationship.Dependency.ServiceType)
                let possibleSkippedRegistrations =
                    registeredImplementationTypes[relationship.Dependency.ServiceType]
                let viewItem =
                    BuildWarningViewItem(registration, relationship, possibleSkippedRegistrations.ToArray())
                select new DebuggerViewItemType(registration.ServiceType, viewItem))
                .ToArray();
        }

        private static DebuggerViewItem BuildWarningViewItem(InstanceProducer registration,
            KnownRelationship actualDependency, InstanceProducer[] possibleSkippedRegistrations)
        {
            DebuggerViewItem[] value = new[]
            {
                new DebuggerViewItem("Registration", registration.ServiceType.ToFriendlyName(), registration),
                new DebuggerViewItem("Actual Dependency", actualDependency.Dependency.ServiceType.ToFriendlyName(), 
                    actualDependency),
                new DebuggerViewItem("Expected Dependency", 
                    possibleSkippedRegistrations.First().ServiceType.ToFriendlyName(),
                    possibleSkippedRegistrations.Length == 1 ? 
                        (object)possibleSkippedRegistrations[0] : 
                        possibleSkippedRegistrations),
            };

            string description = BuildDescription(actualDependency, possibleSkippedRegistrations);

            return new DebuggerViewItem("Warning", description, value);
        }

        private static string BuildDescription(KnownRelationship relationship,
            InstanceProducer[] possibleSkippedRegistrations)
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

        private static DebuggerViewItem[] GroupWarnings(DebuggerViewItemType[] warnings)
        {
            var grouper = new DebuggerViewItemGenericTypeGrouper(DescribeGroup, DescribeItem);

            return grouper.Group(warnings);
        }
        
        private static string DescribeGroup(IEnumerable<DebuggerViewItemType> group)
        {
            int count = group.Count();

            return count + " " + ComponentPlural(count) + 
                " possibly short circuit to concrete unregistered types.";
        }

        private static string DescribeItem(IEnumerable<DebuggerViewItem> item)
        {
            return (string)item.First().Description;
        }

        private static string ComponentPlural(int number)
        {
            return "component" + (number != 1 ? "s" : string.Empty);
        }

        private static string HasPlural(int number)
        {
            return number != 1 ? "have" : "has";
        }
    }
}