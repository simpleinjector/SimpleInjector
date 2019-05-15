// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using SimpleInjector.Advanced;

    internal sealed class KnownRelationshipCollection : Collection<KnownRelationship>
    {
        internal KnownRelationshipCollection(List<KnownRelationship> relationships) : base(relationships)
        {
        }

        public bool HasChanged { get; private set; }

        protected override void InsertItem(int index, KnownRelationship item)
        {
            Requires.IsNotNull(item, nameof(item));

            this.HasChanged = true;

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, KnownRelationship item)
        {
            Requires.IsNotNull(item, nameof(item));

            this.HasChanged = true;

            base.SetItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this.HasChanged = true;

            base.RemoveItem(index);
        }
    }
}