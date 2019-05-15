// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Decorators
{
    using System;

    internal sealed class DecoratorInfo
    {
        internal readonly Type DecoratorType;
        internal readonly InstanceProducer DecoratorProducer;

        internal DecoratorInfo(Type decoratorType, InstanceProducer decoratorProducer)
        {
            this.DecoratorType = decoratorType;
            this.DecoratorProducer = decoratorProducer;
        }
    }
}