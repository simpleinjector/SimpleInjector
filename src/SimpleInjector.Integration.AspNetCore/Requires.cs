// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;

    internal static class Requires
    {
        internal static void IsNotNull(object instance, string paramName)
        {
            if (object.ReferenceEquals(instance, null))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        internal static void IsOfType<ShouldBeType>(Type type,string paramName)
        {
            if ( !(type.IsSubclassOf(typeof(ShouldBeType) ) ))
            {
                throw new ArgumentException($"{paramName} should be of type {nameof(ShouldBeType)}");
            }
        }
    }
}