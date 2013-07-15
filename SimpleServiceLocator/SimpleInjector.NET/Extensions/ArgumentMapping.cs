namespace SimpleInjector.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    
    /// <summary>
    /// A map containing a generic argument (such as T) and the concrete type (such as Int32) that it
    /// represents.
    /// </summary>
    [DebuggerDisplay(
        "Argument: {SimpleInjector.Helpers.ToFriendlyName(Argument),nq}, " +
        "ConcreteType: {SimpleInjector.Helpers.ToFriendlyName(ConcreteType),nq}")]
    internal sealed class ArgumentMapping : IEquatable<ArgumentMapping>
    {
        internal ArgumentMapping(Type argument, Type concreteType)
        {
            this.Argument = argument;
            this.ConcreteType = concreteType;
        }

        [DebuggerDisplay("{SimpleInjector.Helpers.ToFriendlyName(Argument),nq}")]
        internal Type Argument { get; private set; }

        [DebuggerDisplay("{SimpleInjector.Helpers.ToFriendlyName(ConcreteType),nq}")]
        internal Type ConcreteType { get; private set; }

        /// <summary>Implements equality. Needed for doing LINQ distinct operations.</summary>
        /// <param name="other">The other to compare to.</param>
        /// <returns>True or false.</returns>
        bool IEquatable<ArgumentMapping>.Equals(ArgumentMapping other)
        {
            return this.Argument == other.Argument && this.ConcreteType == other.ConcreteType;
        }

        /// <summary>Overrides the default hash code. Needed for doing LINQ distinct operations.</summary>
        /// <returns>An 32 bit integer.</returns>
        public override int GetHashCode()
        {
            return this.Argument.GetHashCode() ^ this.ConcreteType.GetHashCode();
        }

        internal static ArgumentMapping Create(Type argument, Type concreteType)
        {
            return new ArgumentMapping(argument, concreteType);
        }

        internal static ArgumentMapping[] Zip(Type[] arguments, Type[] concreteTypes)
        {
            return arguments.Zip(concreteTypes,
                (argument, concreteType) => new ArgumentMapping(argument, concreteType))
                .ToArray();
        }

        internal bool ConcreteTypeMatchesPartialArgument()
        {
            if (this.Argument.IsGenericParameter || this.Argument == this.ConcreteType)
            {
                return true;
            }

            if (!this.ConcreteType.IsGenericType || !this.Argument.IsGenericType)
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