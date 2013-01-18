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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using SimpleInjector.Lifestyles;

    internal class PotentialLifestyleMismatchContainerAnalyzer : IContainerAnalyzer
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
                select new ProducerRelationshipPair(producer, dependency))
                .ToArray();

            if (!mismatches.Any())
            {
                return new DebuggerViewItem(DebuggerViewName, "No possible mismatches found.", null);
            }
            else
            {
                var serviceCount = mismatches.Select(m => m.Producer.ServiceType).Distinct().Count();

                return new DebuggerViewItem(
                    DebuggerViewName,
                    mismatches.Count() + " potential mismatches for " + serviceCount + " services.",
                    BuildViews(mismatches));
            }
        }

        private static DebuggerViewItem[] BuildViews(IEnumerable<ProducerRelationshipPair> mismatches,
            int level = 0)
        {
            return (
                from mismatch in mismatches
                let serviceType = mismatch.Producer.ServiceType
                group mismatch by MakeTypePartiallyGenericUpToLevel(serviceType, level) into mismatchGroup
                select BuildGroupedViewForGroupType(mismatchGroup.Key, mismatchGroup, level))
                .ToArray();
        }

        private static Type MakeTypePartiallyGenericUpToLevel(Type serviceType, int level)
        {
            return TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(serviceType, level);
        }

        private static DebuggerViewItem BuildGroupedViewForGroupType(Type groupType,
            IEnumerable<ProducerRelationshipPair> mismatches, int level)
        {
            if (groupType.ContainsGenericParameters)
            {
                return BuildGenericTypeGroupView(groupType, mismatches, level);
            }
            else
            {
                return BuildSingleInstanceView(groupType, mismatches);
            }
        }

        private static DebuggerViewItem BuildGenericTypeGroupView(Type groupType, 
            IEnumerable<ProducerRelationshipPair> mismatches, int level)
        {
            DebuggerViewItem[] views = BuildViews(mismatches, level + 1);

            if (views.Length == 1)
            {
                // This flatterns the hierarcy when there is just one item in the group.
                return views[0];
            }

            var mismatchCount = mismatches.Count();
            var serviceCount = mismatches.Select(m => m.Producer.ServiceType).Distinct().Count();

            return new DebuggerViewItem(
                name: Helpers.ToFriendlyName(groupType),
                description: mismatchCount + " possible mismatches for " + serviceCount + " services.",
                value: views);
        }

        private static DebuggerViewItem BuildSingleInstanceView(Type serviceType,
            IEnumerable<ProducerRelationshipPair> mismatches)
        {
            var mismatchViews = (
                from mismatch in mismatches
                select BuildMismatchView(mismatch))
                .ToArray();

            string description = mismatchViews.Length == 1 ? (string)mismatchViews[0].Description :
                mismatchViews.Length + " possible mismatches.";

            return new DebuggerViewItem(
                name: Helpers.ToFriendlyName(serviceType),
                description: description,
                value: mismatchViews);
        }

        private static DebuggerViewItem BuildMismatchView(ProducerRelationshipPair mismatch)
        {
            string description = BuildRelationshipDescription(mismatch.Relationship);

            return new DebuggerViewItem("Mismatch", description, mismatch.Relationship);
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

        private sealed class ProducerRelationshipPair
        {
            public ProducerRelationshipPair(InstanceProducer producer, KnownRelationship relationship)
            {
                this.Producer = producer;
                this.Relationship = relationship;
            }

            public InstanceProducer Producer { get; private set; }

            public KnownRelationship Relationship { get; private set; }
        }
    }
}