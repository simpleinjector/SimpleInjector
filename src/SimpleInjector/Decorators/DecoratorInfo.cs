// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Decorators
{
    using System;

    internal sealed record DecoratorInfo(Type DecoratorType, InstanceProducer DecoratorProducer);
}