namespace SimpleInjector.Diagnostics.Debugger
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using SimpleInjector.Advanced;
    using SimpleInjector.Diagnostics.Analyzers;

    internal sealed class DebuggerShortCircuitContainerAnalyzer : IDebuggerContainerAnalyzer
    {
        private const string DebuggerViewName = "Unregistered Types";

        public DebuggerViewItem Analyze(Container container)
        {
            var analyzer = new ShortCircuitedDependencyContainerAnalyzer();

            var warnings = analyzer.Analyze(container);

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

        private static DebuggerViewItem[] GroupWarnings(ShortCircuitedDependencyDiagnosticResult[] warnings)
        {
            var items =
                from warning in warnings
                let expected = warning.ExpectedDependencies
                select new DebuggerViewItemType(warning.Type,
                    new DebuggerViewItem(warning.Name, warning.Description, new[]
                    {
                        new DebuggerViewItem("Registration", 
                            warning.Registration.ServiceType.ToFriendlyName(), warning.Registration),
                        new DebuggerViewItem("Actual Dependency", 
                            warning.ActualDependency.Dependency.ServiceType.ToFriendlyName(), 
                            warning.ActualDependency),
                        new DebuggerViewItem(
                            name: expected.Count == 1 ? "Expected Dependency" : "Expected Dependencies",
                            description: string.Join(", ",
                                from dependency in expected
                                select dependency.ServiceType.ToFriendlyName()),
                            value: expected.Count == 1 ? expected.First() : (object)expected)
                    }));

            var grouper = new DebuggerViewItemGenericTypeGrouper(DescribeGroup, DescribeItem);

            return grouper.Group(items.ToArray());
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