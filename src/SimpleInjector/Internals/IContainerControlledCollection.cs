// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector.Advanced;

    /// <summary>This interface is not meant for public use.</summary>
    internal interface IContainerControlledCollection : IEnumerable
    {
        bool AllProducersVerified { get; }

        /// <summary>Please do not use.</summary>
        /// <returns>Do not use.</returns>
        KnownRelationship[] GetRelationships();

        /// <summary>PLease do not use.</summary>
        /// <param name="registration">Do not use.</param>
        void Append(ContainerControlledItem registration);
        
        void Clear();

        void VerifyCreatingProducers();
    }

    internal static class ContainerControlledCollectionExtensions
    {
        internal static void AppendAll(this IContainerControlledCollection collection,
            IEnumerable<ContainerControlledItem> registrations)
        {
            foreach (ContainerControlledItem registration in registrations)
            {
                collection.Append(registration);
            }
        }

        internal static void AppendAll(this IContainerControlledCollection collection,
            IEnumerable<Registration> registrations)
        {
            collection.AppendAll(registrations.Select(ContainerControlledItem.CreateFromRegistration));
        }

        internal static void AppendAll(this IContainerControlledCollection collection,
            IEnumerable<Type> types)
        {
            collection.AppendAll(types.Select(ContainerControlledItem.CreateFromType));
        }
    }
}