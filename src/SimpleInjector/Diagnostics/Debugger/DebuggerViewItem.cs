// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics.Debugger
{
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(Description) + ", nq}", Name = "{" + nameof(Name) + ", nq}")]
    internal class DebuggerViewItem
    {
        internal DebuggerViewItem(string name, string description, object? value = null)
        {
            this.Name = name;
            this.Description = description;
            this.Value = value;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Description { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Name { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object? Value { get; }
    }
}