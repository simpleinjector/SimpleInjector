// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;

    /// <summary>
    /// The standard exception thrown when a container has an error in resolving an object.
    /// </summary>
    [Serializable]
    public class ActivationException : Exception
    {
        /// <inheritdoc />
        public ActivationException()
        {
        }

        /// <inheritdoc />
        public ActivationException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public ActivationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}