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

namespace SimpleInjector.Analysis
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    internal sealed class PotentialLifestyleMismatchContainerAnalyzer : IContainerAnalyzer
    {
        private const string DebuggerViewName = "Potential Lifestyle Mismatches";

        internal PotentialLifestyleMismatchContainerAnalyzer()
        {
        }

        public DebuggerViewItem Analyse(Container container)
        {
            var mismatches = (
                from producer in container.GetCurrentRegistrations()
                from dependency in producer.GetRelationships()
                where LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency)
                select new DebuggerViewItemType(producer.ServiceType, BuildMismatchViewItem(dependency)))
                .ToArray();

            if (!mismatches.Any())
            {
                return null;
            }

            var serviceCount = mismatches.Select(m => m.Type).Distinct().Count();

            return new DebuggerViewItem(
                "Potential Lifestyle Mismatches", 
                DescribeGroup(mismatches),
                GroupMismatches(mismatches));
        }

        private static DebuggerViewItem[] GroupMismatches(DebuggerViewItemType[] mismatches)
        {
            var grouper = new DebuggerViewItemGenericTypeGrouper(DescribeGroup, DescribeItem);

            return grouper.Group(mismatches);
        }

        private static string DescribeGroup(IEnumerable<DebuggerViewItemType> group)
        {
            var mismatchCount = group.Count();
            var serviceCount = group.Select(item => item.Type).Distinct().Count();

            return 
                mismatchCount + " possible " + MismatchPlural(mismatchCount) +  
                " for " + serviceCount + " " + ServicePlural(serviceCount) + ".";
        }

        private static string ServicePlural(int number)
        {
            return "service" + (number != 1 ? "s" : string.Empty);
        }

        private static string MismatchPlural(int number)
        {
            return "mismatch" + (number != 1 ? "es" : string.Empty);
        }

        private static string DescribeItem(IEnumerable<DebuggerViewItem> item)
        {
            int count = item.Count();

            return count + " possible " + MismatchPlural(count) + ".";
        }

        private static DebuggerViewItem BuildMismatchViewItem(KnownRelationship relationship)
        {
            string description = BuildRelationshipDescription(relationship);

            return new DebuggerViewItem("Mismatch", description, relationship);
        }

        private static string BuildRelationshipDescription(KnownRelationship relationship)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0} ({1}) depends on {2} ({3}).",
                Helpers.ToFriendlyName(relationship.ImplementationType),
                relationship.Lifestyle.Name,
                Helpers.ToFriendlyName(relationship.Dependency.ServiceType),
                relationship.Dependency.Lifestyle.Name);
        }
    }
}