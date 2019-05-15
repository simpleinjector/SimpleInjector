// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics.Debugger
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    [DebuggerDisplay("{" + nameof(DebuggerViewItem.Description) + ", nq}",
        Name = "{" + nameof(DebuggerViewItem.Name) + ", nq}")]
    internal class DebuggerViewItem
    {
        internal DebuggerViewItem(string name, string description, object value = null)
        {
            this.Name = name;
            this.Description = description;
            this.Value = value;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Called by the Visual Studio debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Description { get; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Called by the Visual Studio debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Name { get; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Called by the Visual Studio debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object Value { get; }
    }
}