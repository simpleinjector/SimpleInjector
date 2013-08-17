namespace SimpleInjector.Diagnostics.Debugger
{
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector.Diagnostics.Analyzers;

    internal sealed class DebuggerPotentialLifestyleMismatchContainerAnalyzer : IDebuggerContainerAnalyzer
    {
        private const string DebuggerViewName = "Potential Lifestyle Mismatches";

        internal DebuggerPotentialLifestyleMismatchContainerAnalyzer()
        {
        }

        public DebuggerViewItem Analyze(Container container)
        {
            var analyzer = new PotentialLifestyleMismatchContainerAnalyzer();

            var mismatches = analyzer.Analyze(container);

            if (!mismatches.Any())
            {
                return null;
            }

            return new DebuggerViewItem(
                DebuggerViewName,
                DescribeGroup(mismatches.Select(m => new DebuggerViewItemType(m.Type, null))),
                GroupMismatches(mismatches));
        }

        private static DebuggerViewItem[] GroupMismatches(PotentialLifestyleMismatchDiagnosticResult[] mismatches)
        {
            var items =
                from mismatch in mismatches
                select new DebuggerViewItemType(mismatch.Type,
                    new DebuggerViewItem(mismatch.Name, mismatch.Description, mismatch.Relationship));

            var grouper = new DebuggerViewItemGenericTypeGrouper(DescribeGroup, DescribeItem);

            return grouper.Group(items.ToArray());
        }

        private static string DescribeGroup(IEnumerable<DebuggerViewItemType> group)
        {
            var mismatchCount = group.Count();
            var serviceCount = group.Select(item => item.Type).Distinct().Count();

            return
                mismatchCount + " possible " + MismatchPlural(mismatchCount) +
                " for " + serviceCount + " " + ServicePlural(serviceCount) + ".";
        }

        private static string DescribeItem(IEnumerable<DebuggerViewItem> item)
        {
            int count = item.Count();

            return count + " possible " + MismatchPlural(count) + ".";
        }

        private static string ServicePlural(int number)
        {
            return number == 1 ? "service" : "services";
        }

        private static string MismatchPlural(int number)
        {
            return number == 1 ? "mismatch" : "mismatches";
        }
    }
}