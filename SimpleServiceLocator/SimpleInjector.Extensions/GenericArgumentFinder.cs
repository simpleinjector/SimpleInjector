#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2010 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
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

namespace SimpleInjector.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Allows retrieving the concrete types of the generic type arguments of that must be used to create a
    /// closed generic implementation of a given open generic implementation, based on on the concrete
    /// arguments of the given closed base type.
    /// </summary>
    internal sealed class GenericArgumentFinder
    {
        internal Type[] OpenServiceGenericTypeArguments { get; set; }

        internal Type[] ClosedServiceConcreteTypeArguments { get; set; }

        internal IList<Type> OpenGenericImplementationTypeArguments { get; set; }

        internal Type[] GetConcreteTypeArgumentsForClosedImplementation()
        {
            // The arguments must be in the same order as those of the open implementation.
            return (
                from mapping in this.FindArgumentMappings()
                orderby this.OpenGenericImplementationTypeArguments.IndexOf(mapping.Argument)
                select mapping.ConcreteType)
                .ToArray();
        }

        private ArgumentMapping[] FindArgumentMappings()
        {
            // An Argument mapping is a mapping between a generic type argument and a concrete type. For
            // instance: { Argument = T, ConcreteType = Int32 } is the mapping from generic type argument T
            // to Int32.
            var argumentMappings = this.GetOpenServiceArgumentToConcreteTypeMappings();

            this.ConvertToOpenImplementationArgumentMappings(ref argumentMappings);

            RemoveMappingsThatDoNotSatisfyAllTypeConstraints(ref argumentMappings);

            RemoveDuplicateTypeArguments(ref argumentMappings);

            return argumentMappings.ToArray();
        }

        private IEnumerable<ArgumentMapping> GetOpenServiceArgumentToConcreteTypeMappings()
        {
            int index = 0;

            // Here we 'zip' the generic types (T, TKey, TValue) together with their concrete counter parts.
            while (index < this.OpenServiceGenericTypeArguments.Length &&
                index < this.ClosedServiceConcreteTypeArguments.Length)
            {
                var argument = this.OpenServiceGenericTypeArguments[index];
                var concreteType = this.ClosedServiceConcreteTypeArguments[index];

                yield return new ArgumentMapping(argument, concreteType);

                index++;
            }
        }

        private void ConvertToOpenImplementationArgumentMappings(ref IEnumerable<ArgumentMapping> mappings)
        {
            mappings = (
                from mapping in mappings
                from newMapping in this.ConvertToOpenImplementationArgumentMappings(mapping)
                select newMapping)
                .Distinct();
        }

        private static void RemoveMappingsThatDoNotSatisfyAllTypeConstraints(
            ref IEnumerable<ArgumentMapping> mappings)
        {
            mappings =
                from mapping in mappings
                where mapping.TypeConstraintsAreSatisfied
                select mapping;
        }
        
        private static void RemoveDuplicateTypeArguments(ref IEnumerable<ArgumentMapping> mappings)
        {
            // When a single type argument satisfies multiple concrete types (i.e. an TKey that can both be an
            // Int32 and Double), it is impossible to resolve it. Those duplicates will be removed. This means
            // that the open generic implementation is incompatible with the given arguments and will later on
            // prevent a closed generic implementation to be returned.
            mappings =
                from mapping in mappings
                group mapping by mapping.Argument into mappingGroup
                where mappingGroup.Count() == 1
                select mappingGroup.First();
        }

        private IEnumerable<ArgumentMapping> ConvertToOpenImplementationArgumentMappings(
            ArgumentMapping mapping)
        {
            // We are only interested in generic parameters
            if (mapping.Argument.IsGenericArgument())
            {
                if (this.OpenGenericImplementationTypeArguments.Contains(mapping.Argument))
                {
                    // The argument is one of the type's generic arguments. We can directly return it.
                    yield return mapping;

                    foreach (var arg in this.GetTypeConstraintArgumentMappingsRecursive(mapping))
                    {
                        yield return arg;
                    }
                }
                else
                {
                    // The argument is not in the type's list, which means that the real type is (or are)
                    // buried in a generic type (i.e. Nullable<KeyValueType<TKey, TValue>>). This can result
                    // in multiple values.
                    foreach (var arg in this.ConvertToOpenImplementationArgumentMappingsRecursive(mapping))
                    {
                        yield return arg;
                    }
                }
            }
        }

        private IEnumerable<ArgumentMapping> GetTypeConstraintArgumentMappingsRecursive(ArgumentMapping mapping)
        {
            return
                from constraint in mapping.Argument.GetGenericParameterConstraints()
                let constraintMapping = new ArgumentMapping(constraint, mapping.ConcreteType)
                from arg in this.ConvertToOpenImplementationArgumentMappings(constraintMapping)
                select arg;
        }

        private IEnumerable<ArgumentMapping> ConvertToOpenImplementationArgumentMappingsRecursive(
            ArgumentMapping mapping)
        {
            var argumentTypeDefinition = mapping.Argument.GetGenericTypeDefinition();

            // Try to get mappings for each type in the type hierarchy that is compatible to the  argument.
            return
                from type in mapping.ConcreteType.GetTypeBaseTypesAndInterfacesFor(argumentTypeDefinition)
                from arg in this.ConvertToOpenImplementationArgumentMappingsForType(mapping, type)
                select arg;
        }

        private IEnumerable<ArgumentMapping> ConvertToOpenImplementationArgumentMappingsForType(
           ArgumentMapping mapping, Type type)
        {
            var arguments = mapping.Argument.GetGenericArguments();
            var concreteTypes = type.GetGenericArguments();

            if (concreteTypes.Length != arguments.Length)
            {
                // The length of the concrete list and the generic argument list does not match. This normally
                // means that the generic argument contains a argument that is not generic (so Int32 instead
                // of T). In that case we can ignore everything, because the type will be unusable.
                return Enumerable.Empty<ArgumentMapping>();        
            }

            return
                from subMapping in ArgumentMapping.Zip(arguments, concreteTypes)
                from arg in this.ConvertToOpenImplementationArgumentMappings(subMapping)
                select arg;
        }

        /// <summary>
        /// A map containing a generic argument (such as T) and the concrete type (such as Int32) that it
        /// represents.
        /// </summary>
        private sealed class ArgumentMapping : IEquatable<ArgumentMapping>
        {
            internal ArgumentMapping(Type argument, Type concreteType)
            {
                this.Argument = argument;
                this.ConcreteType = concreteType;
            }

            internal Type Argument { get; private set; }

            internal Type ConcreteType { get; private set; }
            
            internal bool TypeConstraintsAreSatisfied
            {
                get { return (new TypeConstraintValidator { Mapping = this }).AreTypeConstraintsSatisfied(); }
            }

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

            internal static IEnumerable<ArgumentMapping> Zip(Type[] arguments, Type[] concreteTypes)
            {
                for (int index = 0; index < arguments.Length; index++)
                {
                    yield return new ArgumentMapping(arguments[index], concreteTypes[index]);
                }
            }
        }

        /// <summary>
        /// Allows validating an ArgumentMapping.
        /// </summary>
        private sealed class TypeConstraintValidator
        {
            internal ArgumentMapping Mapping { get; set; }

            internal bool AreTypeConstraintsSatisfied()
            {
                return this.ParameterSatisfiesNotNullableValueTypeConstraint() &&
                    this.ParameterSatisfiesDefaultConstructorConstraint() &&
                    this.ParameterSatisfiesReferenceTypeConstraint() &&
                    this.ParameterSatisfiesGenericParameterConstraints();
            }

            private bool ParameterSatisfiesDefaultConstructorConstraint()
            {
                if (!this.MappingHasConstraint(GenericParameterAttributes.DefaultConstructorConstraint))
                {
                    return true;
                }

                if (this.Mapping.ConcreteType.IsValueType)
                {
                    // Value types always have a default constructor.
                    return true;
                }

                bool typeHasDefaultCtor = this.Mapping.ConcreteType.GetConstructor(Type.EmptyTypes) != null;

                return typeHasDefaultCtor;
            }

            private bool ParameterSatisfiesReferenceTypeConstraint()
            {
                if (!this.MappingHasConstraint(GenericParameterAttributes.ReferenceTypeConstraint))
                {
                    return true;
                }

                return !this.Mapping.ConcreteType.IsValueType;
            }

            private bool ParameterSatisfiesNotNullableValueTypeConstraint()
            {
                if (!this.MappingHasConstraint(GenericParameterAttributes.NotNullableValueTypeConstraint))
                {
                    return true;
                }

                if (!this.Mapping.ConcreteType.IsValueType)
                {
                    return false;
                }

                bool isNullable = this.Mapping.ConcreteType.IsGenericType &&
                    this.Mapping.ConcreteType.GetGenericTypeDefinition() == typeof(Nullable<>);

                return !isNullable;
            }

            private bool ParameterSatisfiesGenericParameterConstraints()
            {
                var unsatisfiedConstraints =
                    from constraint in this.Mapping.Argument.GetGenericParameterConstraints()
                    where !this.MappingIsCompatibleWithTypeConstraint(constraint)
                    select constraint;

                if (unsatisfiedConstraints.Any())
                {
                    return false;
                }

                return true;
            }

            private bool MappingIsCompatibleWithTypeConstraint(Type constraint)
            {
                if (constraint.IsAssignableFrom(this.Mapping.ConcreteType))
                {
                    return true;
                }

                return this.Mapping.ConcreteType.GetBaseTypesAndInterfaces()
                    .Any(type => type.GUID == constraint.GUID);
            }

            private bool MappingHasConstraint(GenericParameterAttributes constraint)
            {
                var constraints = this.Mapping.Argument.GenericParameterAttributes;
                return (constraints & constraint) != GenericParameterAttributes.None;
            }
        }
    }
}