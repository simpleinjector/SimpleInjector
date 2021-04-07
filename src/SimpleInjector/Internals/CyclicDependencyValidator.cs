// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System.Threading;

    /// <summary>
    /// Allows verifying whether a given type has a direct or indirect dependency on itself. Verifying is done
    /// by preventing recursive calls to an InstanceProducer. A CyclicDependencyValidator instance checks a
    /// single InstanceProducer and therefore a single service type.
    /// </summary>
    internal sealed class CyclicDependencyValidator
    {
        private readonly ThreadLocal<bool> producerVisited = new ThreadLocal<bool>();
        private readonly InstanceProducer producer;

        internal CyclicDependencyValidator(InstanceProducer producer)
        {
            this.producer = producer;
        }

        // Checks whether this is a recursive call (and thus a cyclic dependency) and throw in that case.
        internal void Check()
        {
            if (this.producerVisited.Value)
            {
                throw new CyclicDependencyException(
                    this.producer, this.producer.Registration.ImplementationType);
            }

            this.producerVisited.Value = true;
        }

        // Resets the validator to its initial state.
        internal void Reset() => this.producerVisited.Value = false;
    }
}