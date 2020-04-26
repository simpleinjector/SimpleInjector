// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
#if NET45
    using System.Runtime.Serialization;
#endif

    /// <summary>
    /// The standard exception thrown when a container has an error in resolving an object.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public partial class ActivationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationException" /> class.
        /// </summary>
        public ActivationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationException" /> class with a specified error
        /// message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ActivationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationException" /> class with a specified error
        /// message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference (Nothing in Visual
        /// Basic) if no inner exception is specified.
        /// </param>
        public ActivationException(string message, Exception innerException) : base(message, innerException)
        {
        }

#if NET45
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationException" /> class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception
        /// being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or
        /// destination.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="info" /> parameter is null.
        /// </exception>
        /// <exception cref="System.Runtime.Serialization.SerializationException">
        /// The class name is null or hresult is zero (0).
        /// </exception>
        protected ActivationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}