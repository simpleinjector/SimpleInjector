// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;

    /// <summary>
    /// Provides data for and interaction with the
    /// <see cref="ContainerOptions.ContainerLocking">ContainerLocking</see> event of
    /// the <see cref="ContainerOptions"/>.
    /// </summary>
    public class ContainerLockingEventArgs : EventArgs
    {
        internal ContainerLockingEventArgs()
        {
        }
    }
}