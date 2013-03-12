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

    internal sealed class SingleResponsibilityViolationsAnalyzer : IContainerAnalyzer
    {
        private const int MaximumValidNumberOfDependencies = 6;
        private const string DebuggerViewName = "Potential Single Responsibility Violations";

        internal SingleResponsibilityViolationsAnalyzer()
        {
        }

        public DebuggerViewItem Analyze(Container container)
        {
            var violations = (
                from registration in container.GetCurrentRegistrations()
                where IsAnalyzableRegistration(registration)
                from relationship in registration.GetRelationships()
                group relationship by new { relationship.ImplementationType, registration } into g
                where g.Count() > MaximumValidNumberOfDependencies
                let item = BuildMismatchViewItem(g.Key.ImplementationType, g)
                select new DebuggerViewItemType(g.Key.registration.ServiceType, item))
                .ToArray();

            if (!violations.Any())
            {
                return null;
            }

            return new DebuggerViewItem(
                DebuggerViewName,
                DescribeGroup(violations),
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
            IEnumerable<KnownRelationship> dependencies)
        {
            string description = BuildRelationshipDescription(implementationType, dependencies);

            DebuggerViewItem[] violationInformation = new[]
            {
                new DebuggerViewItem("ImplementationType", implementationType.ToFriendlyName(), implementationType),
                new DebuggerViewItem("Dependencies", dependencies.Count() + " dependencies.", dependencies.ToArray()),
            };

            return new DebuggerViewItem("Violation", description, violationInformation);
        }

        private static string BuildRelationshipDescription(Type implementationType, 
            IEnumerable<KnownRelationship> dependencies)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0} has {1} dependencies which might indicate a SRP violation.",
                Helpers.ToFriendlyName(implementationType),
                dependencies.Count());
        }

        private static string DescribeGroup(IEnumerable<DebuggerViewItemType> violations)
        {
            var violationCount = violations.Count();

            return violationCount + " possible " + ViolationPlural(violationCount) + ".";
        }

        private static DebuggerViewItem[] GroupViolations(DebuggerViewItemType[] violations)
        {
            var grouper = new DebuggerViewItemGenericTypeGrouper(DescribeGroup, DescribeItem);

            return grouper.Group(violations);
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