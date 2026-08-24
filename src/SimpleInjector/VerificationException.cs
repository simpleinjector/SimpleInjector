// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// The standard exception thrown when a container has an error in resolving an object.
    /// </summary>
    [Serializable]
    public class VerificationException : Exception
    {
        private static readonly ReadOnlyCollection<Exception> Empty = new([]);

        /// <summary>The list of errors returned by the verification process.</summary>
        public ReadOnlyCollection<Exception> Errors { get; private set; } = Empty;

        /// <inheritdoc />
        public VerificationException()
        {
        }

        /// <inheritdoc />
        public VerificationException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public VerificationException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.Errors = new([innerException]);
        }

        internal VerificationException(string message, IList<Exception> innerExceptions)
            : base(message, innerExceptions?.FirstOrDefault())
        {
            this.Errors = new(innerExceptions ?? []);
        }
    }
}