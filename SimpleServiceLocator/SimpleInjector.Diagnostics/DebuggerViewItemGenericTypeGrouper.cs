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
    using System.Linq;

    /// <summary>
    /// Allows grouping a supplied collection of DebuggerViewItemType items by their generic type.
    /// </summary>
    internal class DebuggerViewItemGenericTypeGrouper
    {
        private readonly Func<IEnumerable<DebuggerViewItemType>, string> groupDescriptor;
        private readonly Func<IEnumerable<DebuggerViewItem>, string> itemDescriptor;

        public DebuggerViewItemGenericTypeGrouper(
            Func<IEnumerable<DebuggerViewItemType>, string> groupDescriptor,
            Func<IEnumerable<DebuggerViewItem>, string> itemDescriptor)
        {
            this.groupDescriptor = groupDescriptor;
            this.itemDescriptor = itemDescriptor;
        }

        internal DebuggerViewItem[] Group(DebuggerViewItemType[] items)
        {
            return this.GroupViews(items, level: 0);
        }

        private DebuggerViewItem[] GroupViews(IEnumerable<DebuggerViewItemType> typedItems, int level)
        {
            return (
                from typedItem in typedItems
                group typedItem by MakeTypePartiallyGenericUpToLevel(typedItem.Type, level) into itemGroup
                select this.BuildGroupedViewForGroupType(itemGroup.Key, itemGroup, level))
                .ToArray();
        }

        private static Type MakeTypePartiallyGenericUpToLevel(Type serviceType, int level)
        {
            return TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(serviceType, level);
        }

        private DebuggerViewItem BuildGroupedViewForGroupType(Type groupType,
            IEnumerable<DebuggerViewItemType> typedItems, int level)
        {
            if (groupType.ContainsGenericParameters)
            {
                return this.BuildGenericTypeGroupView(groupType, typedItems, level);
            }
            else
            {
                return this.BuildSingleInstanceView(groupType, typedItems);
            }
        }

        private DebuggerViewItem BuildGenericTypeGroupView(Type groupType,
            IEnumerable<DebuggerViewItemType> typedItems, int level)
        {
            DebuggerViewItem[] views = this.GroupViews(typedItems, level + 1);

            if (views.Length == 1)
            {
                // This flatterns the hierarcy when there is just one item in the group.
                return views[0];
            }

            return new DebuggerViewItem(
                name: Helpers.ToFriendlyName(groupType),
                description: this.groupDescriptor(typedItems),
                value: views);
        }

        private DebuggerViewItem BuildSingleInstanceView(Type closedType,
            IEnumerable<DebuggerViewItemType> typedItems)
        {
            var items = (
                from typedItem in typedItems
                select typedItem.Item)
                .ToArray();

            string description = items.Length == 1 ? (string)items[0].Description : this.itemDescriptor(items);

            return new DebuggerViewItem(
                name: Helpers.ToFriendlyName(closedType),
                description: description,
                value: items);
        }
    }
}