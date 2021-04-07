// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Internals
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// A map containing a generic argument (such as T) and the concrete type (such as Int32) that it
    /// represents.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    internal sealed class ArgumentMapping : IEquatable<ArgumentMapping>
    {
        internal ArgumentMapping(Type argument, Type concreteType)
        {
            this.Argument = argument;
            this.ConcreteType = concreteType;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebuggerDisplay =>
            $"Argument: {this.Argument.ToFriendlyName()}, " +
            $"ConcreteType: {this.ConcreteType.ToFriendlyName()}";

        [DebuggerDisplay("{Argument, nq}")]
        internal Type Argument { get; }

        [DebuggerDisplay("{ConcreteType, nq}")]
        internal Type ConcreteType { get; }

        internal bool TypeConstraintsAreSatisfied => this.Validator.AreTypeConstraintsSatisfied();

        private TypeConstraintValidator Validator => new TypeConstraintValidator(this);

        /// <summary>Implements equality. Needed for doing LINQ distinct operations.</summary>
        /// <param name="other">The other to compare to.</param>
        /// <returns>True or false.</returns>
        bool IEquatable<ArgumentMapping>.Equals(ArgumentMapping other) =>
            this.Argument == other.Argument && this.ConcreteType == other.ConcreteType;

        /// <inheritdoc />
        public override int GetHashCode() => Helpers.Hash(this.Argument, this.ConcreteType);

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is ArgumentMapping other && ((IEquatable<ArgumentMapping>)this).Equals(other);

        internal static ArgumentMapping Create(Type argument, Type concreteType) =>
            new ArgumentMapping(argument, concreteType);

        internal static ArgumentMapping[] Zip(Type[] arguments, Type[] concreteTypes) =>
            arguments.Zip(concreteTypes, Create).ToArray();

        internal bool ConcreteTypeMatchesPartialArgument()
        {
            if (this.Argument.IsGenericParameter || this.Argument == this.ConcreteType)
            {
                return true;
            }
            else if (!this.ConcreteType.IsGenericType() || !this.Argument.IsGenericType())
            {
                return false;
            }
            else if (this.ConcreteType.GetGenericTypeDefinition() != this.Argument.GetGenericTypeDefinition())
            {
                return false;
            }
            else
            {
                return this.Argument.GetGenericArguments()
                    .Zip(this.ConcreteType.GetGenericArguments(), Create)
                    .All(mapping => mapping.ConcreteTypeMatchesPartialArgument());
            }
        }
    }
}