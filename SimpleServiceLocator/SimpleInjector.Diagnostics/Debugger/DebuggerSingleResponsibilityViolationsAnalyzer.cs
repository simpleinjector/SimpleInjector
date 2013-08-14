namespace SimpleInjector.Diagnostics.Debugger
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using SimpleInjector.Advanced;
    using SimpleInjector.Diagnostics.Analyzers;

    internal sealed class DebuggerSingleResponsibilityViolationsAnalyzer : IDebuggerContainerAnalyzer
    {
        private const int MaximumValidNumberOfDependencies = 6;
        private const string DebuggerViewName = "Potential Single Responsibility Violations";

        internal DebuggerSingleResponsibilityViolationsAnalyzer()
        {
        }

        public DebuggerViewItem Analyze(Container container)
        {
            var analyzer = new SingleResponsibilityViolationsAnalyzer();

            var violations = analyzer.Analyze(container);

            if (!violations.Any())
            {
                return null;
            }

            return new DebuggerViewItem(
                DebuggerViewName,
                DescribeGroup(violations.Count()),
                GroupViolations(violations));
        }

        private static bool IsAnalyzableRegistration(InstanceProducer registration)
        {
            // We can't analyze collections, because this would lead to false positives when decorators are
            // applied to the collection. For a decorator, each collection element it decorates is a 
            // dependency, which will make it look as if the decorator has too many dependencies. Since the
            // container will delegate the creation of those elements back to the container, those elements
            // would by them selve still get analyzed, so the only thing we'd miss here is the decorator.
            return !registration.ServiceType.IsGenericType ||
                registration.ServiceType.GetGenericTypeDefinition() != typeof(IEnumerable<>);
        }

        private static DebuggerViewItem BuildMismatchViewItem(Type implementationType,
            IEnumerable<KnownRelationship> relationships)
        {
            var dependencies = relationships.Select(r => r.Dependency).ToArray();

            string description = BuildRelationshipDescription(implementationType, dependencies.Length);

            var violationInformation = new[]
            {
                new DebuggerViewItem("ImplementationType", implementationType.ToFriendlyName(), implementationType),
                new DebuggerViewItem("Dependencies", dependencies.Length + " dependencies.", dependencies.ToArray()),
            };

            return new DebuggerViewItem("Violation", description, violationInformation);
        }

        private static string BuildRelationshipDescription(Type implementationType, int numberOfDependencies)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0} has {1} dependencies which might indicate a SRP violation.",
                Helpers.ToFriendlyName(implementationType),
                numberOfDependencies);
        }

        private static string DescribeGroup(int violationCount)
        {
            return violationCount + " possible " + ViolationPlural(violationCount) + ".";
        }

        private static string DescribeGroup(IEnumerable<DebuggerViewItemType> violations)
        {
            return DescribeGroup(violations.Count());
        }

        private static DebuggerViewItem[] GroupViolations(
            SingleResponsibilityViolationDiagnosticResult[] violations)
        {
            var items =
                from violation in violations
                select new DebuggerViewItemType(violation.Type,
                    new DebuggerViewItem(violation.Name, violation.Description, new[]
                    {
                        new DebuggerViewItem(
                            "ImplementationType", 
                            violation.ImplementationType.ToFriendlyName(), 
                            violation.ImplementationType),
                        new DebuggerViewItem(
                            "Dependencies", 
                            violation.Dependencies.Count + " dependencies.", 
                            violation.Dependencies.ToArray()),
                    }));

            var grouper = new DebuggerViewItemGenericTypeGrouper(DescribeGroup, DescribeItem);

            return grouper.Group(items.ToArray());
        }

        private static string DescribeItem(IEnumerable<DebuggerViewItem> item)
        {
            int count = item.Count();

            return count + " possible " + ViolationPlural(count) + ".";
        }

        private static string ViolationPlural(int violationCount)
        {
            return "violation" + (violationCount != 1 ? "s" : string.Empty);
        }
    }
}
