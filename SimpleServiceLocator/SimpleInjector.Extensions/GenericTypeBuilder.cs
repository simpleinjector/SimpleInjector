using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleInjector.Extensions
{
    /// <summary>
    /// Helper class for building closed generic type for a given open generic type and a closed generic base.
    /// </summary>
    internal sealed class GenericTypeBuilder
    {
        internal Type ClosedGenericBaseType { get; set; }

        internal Type OpenGenericImplementation { get; set; }

        internal BuildResult BuildClosedGenericImplementation()
        {
            var serviceType = this.FindMatchingOpenGenericServiceType();

            if (serviceType == null)
            {
                return new BuildResult
                {
                    ClosedServiceTypeSatisfiesAllTypeConstraints = false
                };
            }
            else
            {
                var arguments = this.GetMatchingGenericArgumentsForOpenImplementationBasedOn(serviceType);

                return new BuildResult
                {
                    ClosedServiceTypeSatisfiesAllTypeConstraints = true,
                    ClosedGenericImplementation = this.OpenGenericImplementation.MakeGenericType(arguments)
                };
            }
        }

        private Type FindMatchingOpenGenericServiceType()
        {
            // There can be more than one service that exactly matches, but they will never have a different
            // set of generic type arguments; the type system ensures this. 
            return (
                from serviceType in this.GetCandidateServiceTypes()
                where this.SatisfiesGenericTypeConstraints(serviceType)
                select serviceType)
                .FirstOrDefault();
        }

        private IEnumerable<Type> GetCandidateServiceTypes()
        {
            var openGenericBaseType = this.ClosedGenericBaseType.GetGenericTypeDefinition();

            return (
                from baseType in this.OpenGenericImplementation.GetBaseTypesAndInterfaces()
                where openGenericBaseType.IsGenericTypeDefinitionOf(baseType)
                select baseType)
                .Distinct();
        }

        private bool SatisfiesGenericTypeConstraints(Type serviceType)
        {
            var arguments = this.GetMatchingGenericArgumentsForOpenImplementationBasedOn(serviceType);

            // Type arguments that don't match are left out. When the length of the result does not match the
            // actual length, this means that the generic type constraints don't match and the given service 
            // type does not satisft the generic type constraints.
            return arguments.Length == this.OpenGenericImplementation.GetGenericArguments().Length;
        }

        private Type[] GetMatchingGenericArgumentsForOpenImplementationBasedOn(Type openGenericServiceType)
        {
            var finder = new GenericArgumentFinder
            {
                OpenServiceGenericTypeArguments = openGenericServiceType.GetGenericArguments(),
                ClosedServiceConcreteTypeArguments = this.ClosedGenericBaseType.GetGenericArguments(),
                OpenGenericImplementationTypeArguments = this.OpenGenericImplementation.GetGenericArguments()
            };

            return finder.GetConcreteTypeArgumentsForClosedImplementation();
        }

        /// <summary>Result of the GenericTypeBuilder.</summary>
        internal sealed class BuildResult
        {
            internal bool ClosedServiceTypeSatisfiesAllTypeConstraints { get; set; }

            internal Type ClosedGenericImplementation { get; set; }
        }
    }
}