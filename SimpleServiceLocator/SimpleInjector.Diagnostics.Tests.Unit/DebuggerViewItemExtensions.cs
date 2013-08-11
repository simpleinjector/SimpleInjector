namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector.Diagnostics;

#if DEBUG
    internal static class DebuggerViewItemExtensions
    {
        internal static IEnumerable<DebuggerViewItem> Items(this DebuggerViewItem item)
        {
            if (item.Value is IEnumerable<DebuggerViewItem>)
            {
                return item.Value as IEnumerable<DebuggerViewItem>;
            }

            return Enumerable.Empty<DebuggerViewItem>();
        }
    }
#endif
}