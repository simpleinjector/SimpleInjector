// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    internal class ServiceCreatedListenerArgs
    {
        public ServiceCreatedListenerArgs(InstanceProducer producer)
        {
            this.Producer = producer;
        }

        public bool Handled { get; set; }
        public InstanceProducer Producer { get; }
    }
}