namespace SimpleInjector.Extensions
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

            if (this.Mapping.ConcreteType.IsValueType)
            {
                // Value types always have a default constructor.
                return true;
            }

            bool typeHasDefaultCtor = this.Mapping.ConcreteType.GetConstructor(new Type[0]) != null;

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

            return !unsatisfiedConstraints.Any();
        }

        private bool MappingIsCompatibleWithTypeConstraint(Type constraint)
        {
            // We return true in PCL, because there's no System.Type.GUID in PCL and GUID is needed to compare
            // if the constraint matches one of the base type. Returning true does not change the functional 
            // behavior and correctness of the framework, but does lower the performance (especially during 
            // a call to Verify).
#if PCL
            return true;
#else
            if (constraint.IsAssignableFrom(this.Mapping.ConcreteType))
            {
                return true;
            }

            if (constraint.IsGenericParameter)
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
            var constraints = this.Mapping.Argument.GenericParameterAttributes;
            return (constraints & constraint) != GenericParameterAttributes.None;
        }
    }
}