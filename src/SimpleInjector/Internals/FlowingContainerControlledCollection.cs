// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;

    internal sealed class FlowingContainerControlledCollection<TService>(
        Scope scope, ContainerControlledCollection<TService> definition)
        : ContainerControlledCollection<TService>(scope.Container, definition)
    {
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
            Container container = scope.Container;

            Scope? originalScope = container.CurrentThreadResolveScope;
            container.CurrentThreadResolveScope = scope;

            // TODO: If needed, this can be further optimized to prevent GC pressure.
            return new Scoper(originalScope, container);
        }

        private sealed class Scoper(Scope? originalScope, Container container) : IDisposable
        {
            public void Dispose() => container.CurrentThreadResolveScope = originalScope;
        }
    }
}