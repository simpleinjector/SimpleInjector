#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Internals
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A map containing a generic argument (such as T) and the concrete type (such as Int32) that it
    /// represents.
    /// </summary>
    [DebuggerDisplay(
        nameof(Argument) + ": {SimpleInjector.Helpers.ToFriendlyName(" + nameof(Argument) + "), nq}, " +
        nameof(ConcreteType) + ": {SimpleInjector.Helpers.ToFriendlyName(" + nameof(ConcreteType) + "), nq}")]
    internal sealed class ArgumentMapping : IEquatable<ArgumentMapping>
    {
        internal ArgumentMapping(Type argument, Type concreteType)
        {
            this.Argument = argument;
            this.ConcreteType = concreteType;
        }

        [DebuggerDisplay("{SimpleInjector.Helpers.ToFriendlyName(" + nameof(Argument) + "), nq}")]
        internal Type Argument { get; }

        [DebuggerDisplay("{SimpleInjector.Helpers.ToFriendlyName(" + nameof(ConcreteType) + "), nq}")]
        internal Type ConcreteType { get; }

        internal bool TypeConstraintsAreSatisfied => 
            new TypeConstraintValidator { Mapping = this }.AreTypeConstraintsSatisfied();

        /// <summary>Implements equality. Needed for doing LINQ distinct operations.</summary>
        /// <param name="other">The other to compare to.</param>
        /// <returns>True or false.</returns>
        bool IEquatable<ArgumentMapping>.Equals(ArgumentMapping other) => 
            this.Argument == other.Argument && this.ConcreteType == other.ConcreteType;

        /// <summary>Overrides the default hash code. Needed for doing LINQ distinct operations.</summary>
        /// <returns>An 32 bit integer.</returns>
        public override int GetHashCode() =>
            this.Argument.GetHashCode() ^ this.ConcreteType.GetHashCode();

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

            if (!this.ConcreteType.Info().IsGenericType || !this.Argument.Info().IsGenericType)
            {
                return false;
            }

            if (this.ConcreteType.GetGenericTypeDefinition() != this.Argument.GetGenericTypeDefinition())
            {
                return false;
            }

            return this.Argument.GetGenericArguments()
                .Zip(this.ConcreteType.GetGenericArguments(), ArgumentMapping.Create)
                .All(mapping => mapping.ConcreteTypeMatchesPartialArgument());
        }
    }
}