// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Allows verifying whether a given type has a direct or indirect dependency on itself. Verifying is done
    /// by preventing recursive calls to an InstanceProducer. A CyclicDependencyValidator instance checks a
    /// single InstanceProducer and therefore a single service type.
    /// </summary>
    /// <remarks>
    /// DESIGN: An instance or expression can be requested from multiple threads simultaniously, which means
    /// that this class must be thread-safe but also prevent reporting false positives in cyclic dependencies
    /// when called from multiple thread. Originally, this was done by wrapping a ThreadLocal{bool}, but this
    /// lead to memory leaks (see #956 and #540), as in most cases the ThreadLocal{bool} couldn't be disposed.
    /// THat's why this class now wraps a list of currently entered threads. Because the list is nulled when
    /// there are no entering threads, it is a very memory-efficient solution.
    /// </remarks>
    internal sealed class CyclicDependencyValidator
    {
        // If needed, we can do an extra optimization, which is to have an extra int field for the first
        // entered thread. This prevents the list from having to be newed in most cases, as typically there
        // is only on thread entering at the same time. This does complicate the code of this class though.
        // Typically, however, we're not that concerned with producing garbage during the warmup phase.
        // There are quite a few cases were we produce extensive garbage. Producing garbage in the happy
        // path, however, is somethings we prevent at all costs.
        private List<int>? enteredThreads;

        // Checks whether this is a recursive call (and thus a cyclic dependency) and throw in that case.
        // MEMORY: By passing the instance producer in through this method instead of through the ctor,
        // this class becomes 8 bytes smaller (on 64 bits) and results in less memory used. This is
        // important, because every InstanceProducer gets its own validator and 95% of all validators
        // stay in memory.
        // TODO: We can reduce an extra 8 bytes of memory per InstanceProducer by merging this class into
        // the InstanceProducer itself.
        internal void Check(InstanceProducer producer)
        {
            lock (this)
            {                
                if (this.IsCurrentThreadReentering())
                {
                    throw new CyclicDependencyException(
                        producer, producer.Registration.ImplementationType);
                }

                this.MarkCurrentThreadAsEntering();
            }
        }

        // Resets the validator to its initial state.
        internal void Reset()
        {
            lock (this)
            {
                this.MarkCurrentThreadAsLeaving();
            }
        }

        private bool IsCurrentThreadReentering()
        {
            if (this.enteredThreads is null) return false;

            int currentThreadId = Environment.CurrentManagedThreadId;

            foreach (int threadId in this.enteredThreads)
            {
                if (threadId == currentThreadId) return true;
            }

            return false;
        }

        private void MarkCurrentThreadAsEntering()
        {
            if (this.enteredThreads is null)
            {
                // Create a list with an array of one. In all but the rarest cases we'll see multiple threads
                // entering at the same time.
                this.enteredThreads = new(1);
            }

            this.enteredThreads.Add(Environment.CurrentManagedThreadId);
        }

        private void MarkCurrentThreadAsLeaving()
        {
            if (this.enteredThreads is null) return;

            this.enteredThreads.Remove(Environment.CurrentManagedThreadId);

            if (this.enteredThreads.Count == 0)
            {
                // Dereference the list. Although this causes the production of way
                // more garbage during the warm-up phase when the instance producer
                // is depended upon a lot, it ensures that the least amount of memory
                // is used when the warmup phase is complete and all expression trees
                // have been compiled.
                this.enteredThreads = null;
            }
        }
    }
}