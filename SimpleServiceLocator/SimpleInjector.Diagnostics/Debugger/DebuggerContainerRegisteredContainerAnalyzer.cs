namespace SimpleInjector.Diagnostics.Debugger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector.Advanced;
    using SimpleInjector.Diagnostics.Analyzers;

    internal sealed class DebuggerContainerRegisteredContainerAnalyzer : IDebuggerContainerAnalyzer
    {
        private const string DebuggerViewName = "Unregistered types";

        public DebuggerViewItem Analyze(Container container)
        {
            var analyzer = new ContainerRegisteredServiceContainerAnalyzer();

            var warnings = analyzer.Analyze(container);

            if (!warnings.Any())
            {
                return null;
            }

            int numberOfAutoRegisteredServices = GetNumberOfAutoRegisteredServices(warnings);

            int numberOfRegistrations = warnings.Select(warning => warning.Type).Distinct().Count();

            string description = numberOfAutoRegisteredServices + " container-registered " +
                TypePlural(numberOfAutoRegisteredServices) + " " + HasPlural(numberOfAutoRegisteredServices) +
                " been detected that " + IsPlural(numberOfAutoRegisteredServices) + " referenced by " +
                numberOfRegistrations + " " + ComponentPlural(numberOfRegistrations) + ".";

            return new DebuggerViewItem(
                DebuggerViewName,
                description,
                GroupWarnings(warnings));
        }

        private static DebuggerViewItemType[] FindContainerRegisteredRegistrations(Container container)
        {
            var registrations = container.GetCurrentRegistrations();

            var autoRegisteredServices = new HashSet<Type>(
                from producer in container.GetCurrentRegistrations()
                where producer.IsContainerAutoRegistered
                select producer.ServiceType);

            return (
                from registration in registrations
                from relationship in registration.GetRelationships()
                where autoRegisteredServices.Contains(relationship.Dependency.ServiceType)
                group relationship by registration into relationshipGroup
                let viewItem = BuildUnregisteredTypeViewItem(relationshipGroup.Key, relationshipGroup.ToArray())
                select new DebuggerViewItemType(relationshipGroup.Key.ServiceType, viewItem))
                .ToArray();
        }

        private static int GetNumberOfAutoRegisteredServices(
            IEnumerable<ContainerRegisteredServiceDiagnosticResult> warnings)
        {
            return (
                from warning in warnings
                from relationship in warning.Relationships
                select relationship.Dependency.ServiceType)
                .Distinct()
                .Count();
        }

        private static DebuggerViewItem[] GroupWarnings(ContainerRegisteredServiceDiagnosticResult[] warnings)
        {
            var items =
                from warning in warnings
                select new DebuggerViewItemType(warning.Type,
                    new DebuggerViewItem(warning.Name, warning.Description, warning.Relationships));

            var grouper = new DebuggerViewItemGenericTypeGrouper(DescribeGroup, DescribeItem);

            return grouper.Group(items.ToArray());
        }

        private static string DescribeGroup(IEnumerable<DebuggerViewItemType> group)
        {
            int unregisteredServicesCount = (
                from typedItem in @group
                let relationships = typedItem.Item.Value as IEnumerable<KnownRelationship>
                from relationship in relationships
                select relationship.Dependency.ServiceType)
                .Distinct()
                .Count();

            var componentCount = group.Select(item => item.Type).Distinct().Count();

            return
                componentCount + " " + ComponentPlural(componentCount) + " depend on " +
                unregisteredServicesCount + " container-registered " +
                TypePlural(unregisteredServicesCount) + ".";
        }

        private static string DescribeItem(IEnumerable<DebuggerViewItem> item)
        {
            return (string)item.First().Description;
        }

        private static DebuggerViewItem BuildUnregisteredTypeViewItem(InstanceProducer registration,
            KnownRelationship[] relationships)
        {
            string description = BuildDescription(registration, relationships);

            return new DebuggerViewItem("Unregistered dependency", description, relationships);
        }

        private static string BuildDescription(InstanceProducer registration, KnownRelationship[] relationships)
        {
            string componentName = BuildComponentName(registration, relationships);
            string unregisteredTypeName = BuildUnregisteredTypeDescription(relationships);

            return componentName + " depends on " + unregisteredTypeName + ".";
        }

        private static string BuildComponentName(InstanceProducer registration, KnownRelationship[] relationships)
        {
            var consumingTypes = (
                from relationship in relationships
                select relationship.ImplementationType)
                .Distinct()
                .ToArray();

            if (consumingTypes.Length == 1)
            {
                return consumingTypes.First().ToFriendlyName();
            }
            else
            {
                return registration.ServiceType.ToFriendlyName();
            }
        }

        private static string BuildUnregisteredTypeDescription(KnownRelationship[] relationships)
        {
            var unregisteredTypes = (
                from relationship in relationships
                select relationship.Dependency.ServiceType)
                .Distinct()
                .ToArray();

            if (unregisteredTypes.Length == 1)
            {
                return "container-registered " + TypePlural(1) + " " + unregisteredTypes[0].ToFriendlyName();
            }
            else
            {
                return unregisteredTypes.Length + " container-registered " + TypePlural(unregisteredTypes.Length);
            }
        }

        private static string ComponentPlural(int number)
        {
            return "component" + (number != 1 ? "s" : string.Empty);
        }

        private static string TypePlural(int number)
        {
            return "type" + (number != 1 ? "s" : string.Empty);
        }

        private static string HasPlural(int number)
        {
            return number != 1 ? "have" : "has";
        }

        private static string IsPlural(int number)
        {
            return number != 1 ? "are" : "is";
        }
    }
}