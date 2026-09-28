// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;

    internal record struct FoundInstanceProducer(
        Type ServiceType,
        Type ImplementationType,
        InstanceProducer Producer);
}