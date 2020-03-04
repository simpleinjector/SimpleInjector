// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal interface IContainerControlledCollection : IEnumerable
    {
        bool AllProducersVerified { get; }

        InstanceProducer[] GetProducers();

        void Append(ContainerControlledItem item);

        void Clear();

        void VerifyCreatingProducers();
    }

    internal static class ContainerControlledCollectionExtensions
    {
        internal static void AppendAll(
            this IContainerControlledCollection collection, IEnumerable<ContainerControlledItem> items)
        {
            foreach (ContainerControlledItem item in items)
            {
                collection.Append(item);
            }
        }

        internal static void AppendAll(
            this IContainerControlledCollection collection, IEnumerable<Registration> registrations)
        {
            collection.AppendAll(registrations.Select(ContainerControlledItem.CreateFromRegistration));
        }

        internal static void AppendAll(
            this IContainerControlledCollection collection, IEnumerable<Type> types)
        {
            collection.AppendAll(types.Select(ContainerControlledItem.CreateFromType));
        }
    }
}