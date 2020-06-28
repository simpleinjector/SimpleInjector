// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;

    internal sealed class FlowingContainerControlledCollection<TService>
        : ContainerControlledCollection<TService>
    {
        private readonly Scope scope;

        public FlowingContainerControlledCollection(
            Scope scope, ContainerControlledCollection<TService> definition)
            : base(scope.Container!, definition)
        {
            this.scope = scope;
        }

        public override TService this[int index]
        {
            get
            {
                using (this.ApplyScoping())
                {
                    return base[index];
                }
            }

            set => base[index] = value;
        }

        public override int IndexOf(TService item)
        {
            using (this.ApplyScoping())
            {
                return base.IndexOf(item);
            }
        }

        public override void CopyTo(TService[] array, int arrayIndex)
        {
            using (this.ApplyScoping())
            {
                base.CopyTo(array, arrayIndex);
            }
        }

        public override IEnumerator<TService> GetEnumerator()
        {
            foreach (var producer in this.GetProducers())
            {
                TService service;

                using (this.ApplyScoping())
                {
                    service = GetInstance(producer);
                }

                yield return service;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private  IDisposable? ApplyScoping()
        {
            Container container = this.scope.Container!;

            Scope? originalScope = container.CurrentThreadResolveScope;
            container.CurrentThreadResolveScope = this.scope;

            // TODO: If needed, this can be further optimized to prevent GC pressure.
            return new Scoper(originalScope, container);
        }

        private sealed class Scoper : IDisposable
        {
            private readonly Container container;
            private readonly Scope? originalScope;

            public Scoper(Scope? originalScope, Container container)
            {
                this.originalScope = originalScope;
                this.container = container;
            }

            public void Dispose() => this.container.CurrentThreadResolveScope = this.originalScope;
        }
    }
}