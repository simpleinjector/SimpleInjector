// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
#if NET45 || NET461
    using System.Runtime.Serialization;
#endif

    /// <summary>
    /// The standard exception thrown when a container has an error in resolving an object.
    /// </summary>
#if NET45 || NET461
    [Serializable]
#endif
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

#if NET45 || NET461
        /// <inheritdoc />
        protected ActivationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}