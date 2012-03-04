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
    using System.Linq;

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

            bool isGenericType = this.OpenGenericImplementation.GetGenericArguments().Length > 0;

            if (isGenericType)
            {
                var arguments = this.GetMatchingGenericArgumentsForOpenImplementationBasedOn(serviceType);

                return new BuildResult
                {
                    ClosedServiceTypeSatisfiesAllTypeConstraints = true,
                    ClosedGenericImplementation = this.OpenGenericImplementation.MakeGenericType(arguments)
                };
            }
            else
            {
                return new BuildResult
                {
                    ClosedServiceTypeSatisfiesAllTypeConstraints = true,
                    ClosedGenericImplementation = this.OpenGenericImplementation
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
            bool implementationHasGenericArguments =
                this.OpenGenericImplementation.GetGenericArguments().Length == 0;

            if (implementationHasGenericArguments)
            {
                // When there are no generic type arguments, there are (obviously) no generic type constraints
                // so checking for the number of argument would always succeed, while this is not correct.
                // Instead we should check whether the given service type is the requested closed generic base
                // type.
                return this.ClosedGenericBaseType == serviceType;
            }
            else
            {
                var arguments = this.GetMatchingGenericArgumentsForOpenImplementationBasedOn(serviceType);

                // Type arguments that don't match are left out. When the length of the result does not match 
                // the actual length, this means that the generic type constraints don't match and the given 
                // service type does not satisft the generic type constraints.
                return arguments.Length == this.OpenGenericImplementation.GetGenericArguments().Length;
            }
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