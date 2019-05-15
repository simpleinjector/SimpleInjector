// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;

    internal static class RegistrationEntry
    {
        internal static IRegistrationEntry Create(Type serviceType, Container container) =>
            serviceType.IsGenericType()
                ? (IRegistrationEntry)new GenericRegistrationEntry(container)
                : (IRegistrationEntry)new NonGenericRegistrationEntry(serviceType, container);
    }
}