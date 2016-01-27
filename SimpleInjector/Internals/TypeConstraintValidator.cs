﻿#region Copyright Simple Injector Contributors
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
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Allows validating an ArgumentMapping.
    /// </summary>
    internal sealed class TypeConstraintValidator
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

            if (this.Mapping.ConcreteType.Info().IsValueType)
            {
                // Value types always have a default constructor.
                return true;
            }

            bool typeHasDefaultCtor = this.Mapping.ConcreteType.GetConstructor(Helpers.Array<Type>.Empty) != null;

            return typeHasDefaultCtor;
        }

        private bool ParameterSatisfiesReferenceTypeConstraint()
        {
            if (!this.MappingHasConstraint(GenericParameterAttributes.ReferenceTypeConstraint))
            {
                return true;
            }

            return !this.Mapping.ConcreteType.Info().IsValueType;
        }

        private bool ParameterSatisfiesNotNullableValueTypeConstraint()
        {
            if (!this.MappingHasConstraint(GenericParameterAttributes.NotNullableValueTypeConstraint))
            {
                return true;
            }

            if (!this.Mapping.ConcreteType.Info().IsValueType)
            {
                return false;
            }

            bool isNullable = this.Mapping.ConcreteType.Info().IsGenericType &&
                this.Mapping.ConcreteType.GetGenericTypeDefinition() == typeof(Nullable<>);

            return !isNullable;
        }

        private bool ParameterSatisfiesGenericParameterConstraints()
        {
            var unsatisfiedConstraints =
                from constraint in this.Mapping.Argument.Info().GetGenericParameterConstraints()
                where !this.MappingMightBeCompatibleWithTypeConstraint(constraint)
                select constraint;

            return !unsatisfiedConstraints.Any();
        }

        private bool MappingMightBeCompatibleWithTypeConstraint(Type constraint)
        {
            // We return true in PCL, because there's no System.Type.GUID in PCL and GUID is needed to compare
            // if the constraint matches one of the base type. Returning true does not change the functional 
            // behavior and correctness of the framework, but does lower the performance when resolving a
            // service for the first time. This can especially add up when calling Verify. In a benchmark of
            // a complex application that made heavily use of generic registrations with type constraints,
            // we've seen a call to Verify() take up to 6 times as long (from 8.5 seconds to 55 seconds), when 
            // we don't do these checks here (and simply return true). 
            // That's why we need to have these checks in the full version.
#if PCL
            return true;
#else
            if (constraint.IsAssignableFrom(this.Mapping.ConcreteType))
            {
                return true;
            }

            if (constraint.ContainsGenericParameters)
            {
                // The constraint is one of the other generic parameters, but this class checks a single
                // mapping, so we cannot check whether this constraint holds. We just return true and
                // have to check later on whether this constraint holds.
                return true;
            }

            var baseTypes = this.Mapping.ConcreteType.GetBaseTypesAndInterfaces();

            // This doesn't feel right, but have no idea how to reliably do this check without the GUID.
            return baseTypes.Any(type => type.GUID == constraint.GUID);
#endif
        }

        private bool MappingHasConstraint(GenericParameterAttributes constraint)
        {
            var constraints = this.Mapping.Argument.Info().GenericParameterAttributes;
            return (constraints & constraint) != GenericParameterAttributes.None;
        }
    }
}