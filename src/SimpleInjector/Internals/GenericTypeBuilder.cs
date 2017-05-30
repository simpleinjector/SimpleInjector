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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Helper class for building closed generic type for a given open generic type and a closed generic base.
    /// </summary>
    internal sealed class GenericTypeBuilder
    {
        [DebuggerDisplay("{" + TypesExtensions.FriendlyName + "(closedGenericBaseType),nq}")]
        private readonly Type closedGenericBaseType;

        [DebuggerDisplay("{" + TypesExtensions.FriendlyName + "(openGenericImplementation),nq}")]
        private readonly Type openGenericImplementation;

        [DebuggerDisplay("{(partialOpenGenericImplementation == null ? \"null\" : " +
            TypesExtensions.FriendlyName + "(partialOpenGenericImplementation)),nq}")]
        private readonly Type partialOpenGenericImplementation;

        private readonly bool isPartialOpenGenericImplementation;

        internal GenericTypeBuilder(Type closedGenericBaseType, Type openGenericImplementation)
        {
            this.closedGenericBaseType = closedGenericBaseType;

            this.openGenericImplementation = openGenericImplementation;

            if (openGenericImplementation.IsGenericType() && 
                !openGenericImplementation.IsGenericTypeDefinition())
            {
                this.openGenericImplementation = openGenericImplementation.GetGenericTypeDefinition();
                this.partialOpenGenericImplementation = openGenericImplementation;
                this.isPartialOpenGenericImplementation = true;
            }
        }

        internal static bool IsImplementationApplicableToEveryGenericType(Type openAbstraction,
            Type openImplementation)
        {
            try
            {
                return MakeClosedImplementation(openAbstraction, openImplementation) != null;
            }
            catch (InvalidOperationException)
            {
                // This can happen in case there are some weird as type constraints, as with the curiously
                // recurring template pattern. In that case we know that the implementation can't be applied
                // to every generic type
                return false;
            }
        }

        internal static Type MakeClosedImplementation(Type closedAbstraction, Type openImplementation)
        {
            var builder = new GenericTypeBuilder(closedAbstraction, openImplementation);
            var results = builder.BuildClosedGenericImplementation();
            return results.ClosedGenericImplementation;
        }

        internal bool OpenGenericImplementationCanBeAppliedToServiceType()
        {
            var openGenericBaseType = this.closedGenericBaseType.GetGenericTypeDefinition();

            var openGenericBaseTypes = (
                from baseType in this.openGenericImplementation.GetTypeBaseTypesAndInterfaces()
                where openGenericBaseType.IsGenericTypeDefinitionOf(baseType)
                select baseType)
                .Distinct()
                .ToArray();

            return openGenericBaseTypes.Any(type =>
            {
                var typeArguments = GetNestedTypeArgumentsForType(type);

                var partialOpenImplementation =
                    this.partialOpenGenericImplementation ?? this.openGenericImplementation;

                var unmappedArguments = partialOpenImplementation.GetGenericArguments().Except(typeArguments);

                return unmappedArguments.All(argument => !argument.IsGenericParameter);
            });
        }

        internal BuildResult BuildClosedGenericImplementation()
        {
            // Performance optimization: In case the user registers a very large set with mainly non-generic
            // types, we see a considerable performance improvement by adding this simple check.
            if (this.openGenericImplementation.IsGenericType() ||
                this.closedGenericBaseType.IsAssignableFrom(this.openGenericImplementation))
            {
                var serviceType = this.FindMatchingOpenGenericServiceType();

                if (serviceType != null && this.SafisfiesPartialTypeArguments(serviceType))
                {
                    Type closedGenericImplementation =
                        this.BuildClosedGenericImplementationBasedOnMatchingServiceType(serviceType);

                    // closedGenericImplementation will be null when there was a mismatch on type constraints.
                    if (closedGenericImplementation != null &&
                        this.closedGenericBaseType.IsAssignableFrom(closedGenericImplementation))
                    {
                        return BuildResult.Valid(closedGenericImplementation);
                    }
                }
            }

            return BuildResult.Invalid();
        }

        private CandicateServiceType FindMatchingOpenGenericServiceType()
        {
            // There can be more than one service that exactly matches, but they will never have a different
            // set of generic type arguments; the type system ensures this. 
            return (
                from openCandidateServiceType in this.GetOpenCandidateServiceTypes()
                where this.MatchesClosedGenericBaseType(openCandidateServiceType)
                select openCandidateServiceType)
                .FirstOrDefault();
        }

        private Type BuildClosedGenericImplementationBasedOnMatchingServiceType(
            CandicateServiceType candicateServiceType)
        {
            if (this.openGenericImplementation.IsGenericType())
            {
                try
                {
                    return this.openGenericImplementation.MakeGenericType(candicateServiceType.Arguments);
                }
                catch (ArgumentException)
                {
                    // This can happen when there is a type constraint that we didn't check. For instance
                    // the constraint where TIn : TOut is one we cannot check and have to do here (bit ugly).
                    return null;
                }
            }
            else
            {
                return this.openGenericImplementation;
            }
        }

        private IEnumerable<CandicateServiceType> GetOpenCandidateServiceTypes()
        {
            var openGenericBaseType = this.closedGenericBaseType.GetGenericTypeDefinition();

            var openGenericBaseTypes = (
                from baseType in this.openGenericImplementation.GetTypeBaseTypesAndInterfaces()
                where openGenericBaseType.IsGenericTypeDefinitionOf(baseType)
                select baseType)
                .Distinct()
                .ToArray();

            var candidates = (
                from type in openGenericBaseTypes
                select this.ToCandicateServiceType(type))
                .ToArray();

            return candidates;
        }

        private CandicateServiceType ToCandicateServiceType(Type openCandidateServiceType)
        {
            if (openCandidateServiceType.IsGenericType())
            {
                return new CandicateServiceType(openCandidateServiceType,
                    this.GetMatchingGenericArgumentsForOpenImplementationBasedOn(openCandidateServiceType));
            }

            return new CandicateServiceType(openCandidateServiceType, Helpers.Array<Type>.Empty);
        }

        private bool MatchesClosedGenericBaseType(CandicateServiceType openCandidateServiceType)
        {
            if (this.openGenericImplementation.IsGenericType())
            {
                return this.SatisfiesGenericTypeConstraints(openCandidateServiceType);
            }

            // When there are no generic type arguments, there are (obviously) no generic type constraints
            // so checking for the number of argument would always succeed, while this is not correct.
            // Instead we should check whether the given service type is the requested closed generic base
            // type.
            return this.closedGenericBaseType == openCandidateServiceType.ServiceType;
        }

        private bool SatisfiesGenericTypeConstraints(CandicateServiceType openCandidateServiceType)
        {
            // Type arguments that don't match are left out of the list. 
            // When the length of the result does not match the actual length, this means that the generic 
            // type constraints don't match and the given service type does not satisfy the generic type 
            // constraints.
            return openCandidateServiceType.Arguments.Length ==
                this.openGenericImplementation.GetGenericArguments().Length;
        }

        private bool SafisfiesPartialTypeArguments(CandicateServiceType candicateServiceType)
        {
            if (!this.isPartialOpenGenericImplementation)
            {
                return true;
            }

            return this.SafisfiesPartialTypeArguments(candicateServiceType.Arguments);
        }

        private bool SafisfiesPartialTypeArguments(Type[] arguments)
        {
            // Map the partial open generic type arguments to the concrete arguments.
            var mappings =
                this.partialOpenGenericImplementation.GetGenericArguments()
                .Zip(arguments, ArgumentMapping.Create);

            return mappings.All(mapping => mapping.ConcreteTypeMatchesPartialArgument());
        }

        private Type[] GetMatchingGenericArgumentsForOpenImplementationBasedOn(Type openCandidateServiceType)
        {
            var finder = new GenericArgumentFinder(openCandidateServiceType, this.closedGenericBaseType,
                this.openGenericImplementation, this.partialOpenGenericImplementation);

            return finder.GetConcreteTypeArgumentsForClosedImplementation();
        }

        private static IEnumerable<Type> GetNestedTypeArgumentsForType(Type type) => (
            from argument in type.GetGenericArguments()
            from nestedArgument in GetNestedTypeArgumentsForTypeArgument(argument, new List<Type>())
            select nestedArgument)
            .Distinct()
            .ToArray();

        private static IEnumerable<Type> GetNestedTypeArgumentsForTypeArgument(Type argument, IList<Type> processedArguments)
        {
            processedArguments.Add(argument);

            if (argument.IsGenericParameter)
            {
                var nestedArguments =
                    from constraint in argument.GetGenericParameterConstraints()
                    from arg in GetNestedTypeArgumentsForTypeArgument(constraint, processedArguments)
                    select arg;

                return nestedArguments.Concat(new[] { argument });
            }

            if (!argument.IsGenericType())
            {
                return Enumerable.Empty<Type>();
            }

            return
                from genericArgument in argument.GetGenericArguments().Except(processedArguments)
                from arg in GetNestedTypeArgumentsForTypeArgument(genericArgument, processedArguments)
                select arg;
        }

        /// <summary>Result of the GenericTypeBuilder.</summary>
        internal sealed class BuildResult
        {
            private BuildResult()
            {
            }

            internal bool ClosedServiceTypeSatisfiesAllTypeConstraints { get; private set; }

            internal Type ClosedGenericImplementation { get; private set; }

            internal static BuildResult Invalid() => 
                new BuildResult { ClosedServiceTypeSatisfiesAllTypeConstraints = false };

            internal static BuildResult Valid(Type closedGenericImplementation)
            {
                return new BuildResult
                {
                    ClosedServiceTypeSatisfiesAllTypeConstraints = true,
                    ClosedGenericImplementation = closedGenericImplementation,
                };
            }
        }

        /// <summary>
        /// A open generic type with the concrete arguments that can be used to create a closed generic type.
        /// </summary>
        private sealed class CandicateServiceType
        {
            internal readonly Type ServiceType;
            internal readonly Type[] Arguments;

            public CandicateServiceType(Type serviceType, Type[] arguments)
            {
                this.ServiceType = serviceType;
                this.Arguments = arguments;
            }

#if DEBUG
            public override string ToString()
            {
                // This is for our own debugging purposes. We don't use the DebuggerDisplayAttribute, since
                // this code is hard to write (and maintain) as debugger display string.
                return string.Format(CultureInfo.InvariantCulture, "ServiceType: {0}, Arguments: {1}",
                    this.ServiceType.ToFriendlyName(),
                    this.Arguments.Select(type => type.ToFriendlyName()).ToCommaSeparatedText());
            }
#endif
        }
    }
}