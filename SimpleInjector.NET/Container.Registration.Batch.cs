#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015 Simple Injector Contributors
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

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using SimpleInjector.Decorators;
    
#if !PUBLISH
    /// <summary>Methods for batch registration.</summary>
#endif
    public partial class Container
    {
        /// <summary>
        /// Registers all concrete, non-generic, public and internal types in the given set of
        /// <paramref name="assemblies"/> that implement the given <paramref name="openGenericServiceType"/> 
        /// with the transient lifestyle.
        /// </summary>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public void Register(Type openGenericServiceType, IEnumerable<Assembly> assemblies)
        {
            this.Register(openGenericServiceType, assemblies, this.SelectionBasedLifestyle);
        }

        /// <summary>
        /// Registers all concrete, non-generic, public and internal types in the given set of
        /// <paramref name="assemblies"/> that implement the given <paramref name="openGenericServiceType"/> 
        /// with the supplied <paramref name="lifestyle"/>.
        /// </summary>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <param name="lifestyle">The lifestyle to register instances with.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public void Register(Type openGenericServiceType, IEnumerable<Assembly> assemblies, Lifestyle lifestyle)
        {
            Requires.IsNotNull(openGenericServiceType, "openGenericServiceType");
            Requires.IsNotNull(lifestyle, "lifestyle");
            Requires.IsNotNull(assemblies, "assemblies");
            Requires.IsNotPartiallyClosed(openGenericServiceType, "openGenericServiceType");
            Requires.IsOpenGenericType(openGenericServiceType, "openGenericServiceType", 
                guidance: 
                    "You can use the RegisterCollection(Type, IEnumerable<Assembly>) overload to " +
                    "register a collection of instances.");

            var implementationTypes = this.GetTypesToRegister(openGenericServiceType, assemblies);

            this.Register(openGenericServiceType, implementationTypes, lifestyle);
        }

        /// <summary>
        /// Registers all supplied <paramref name="implementationTypes"/> based on the closed-generic version
        /// of the given <paramref name="openGenericServiceType"/> with the transient lifestyle.
        /// </summary>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="implementationTypes">A list types to be registered.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type or when one of the supplied types from the 
        /// <paramref name="implementationTypes"/> collection does not derive from 
        /// <paramref name="openGenericServiceType"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="implementationTypes"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public void Register(Type openGenericServiceType, IEnumerable<Type> implementationTypes)
        {
            this.Register(openGenericServiceType, implementationTypes, this.SelectionBasedLifestyle);
        }

        /// <summary>
        /// Registers all supplied <paramref name="implementationTypes"/> based on the closed-generic version
        /// of the given <paramref name="openGenericServiceType"/> with the given <paramref name="lifestyle"/>.
        /// </summary>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="implementationTypes">A list types to be registered.</param>
        /// <param name="lifestyle">The lifestyle to register instances with.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type or when one of the supplied types from the 
        /// <paramref name="implementationTypes"/> collection does not derive from 
        /// <paramref name="openGenericServiceType"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="implementationTypes"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public void Register(Type openGenericServiceType, IEnumerable<Type> implementationTypes, Lifestyle lifestyle)
        {
            Requires.IsNotNull(openGenericServiceType, "openGenericServiceType");
            Requires.IsNotNull(lifestyle, "lifestyle");
            Requires.IsNotNull(implementationTypes, "implementationTypes");
            Requires.IsNotPartiallyClosed(openGenericServiceType, "openGenericServiceType");
            Requires.IsOpenGenericType(openGenericServiceType, "openGenericServiceType",
                guidance: 
                    "You can use the RegisterCollection(Type, IEnumerable<Type>) overload to " +
                    "register a collection of instances.");

            implementationTypes = implementationTypes.Distinct().ToArray();

            Requires.DoesNotContainNullValues(implementationTypes, "implementationTypes");
            Requires.CollectionDoesNotContainOpenGenericTypes(implementationTypes, "implementationTypes");
            Requires.ServiceIsAssignableFromImplementations(openGenericServiceType, implementationTypes,
                "implementationTypes", typeCanBeServiceType: false);

            var mappings =
                from mapping in BatchMapping.Build(openGenericServiceType, implementationTypes)
                let registration = lifestyle.CreateRegistration(mapping.ImplementationType, this)
                from serviceType in mapping.ClosedServiceTypes
                select new { serviceType, registration };

            foreach (var mapping in mappings)
            {
                this.AddRegistration(mapping.serviceType, mapping.registration);
            }
        }

        /// <summary>
        /// Registers all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <typeparamref name="TService"/>
        /// with a default lifestyle and register them as a collection of <typeparamref name="TService"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// </summary>
        /// <typeparam name="TService">The element type of the collections to register. This can be either
        /// a non-generic, closed-generic or open-generic type.</typeparam>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        public void RegisterCollection<TService>(IEnumerable<Assembly> assemblies) where TService : class
        {
            this.RegisterCollection(typeof(TService), assemblies);
        }

        /// <summary>
        /// Registers all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <paramref name="serviceType"/> 
        /// with a default lifestyle and register them as a collection of <paramref name="serviceType"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// </summary>
        /// <param name="serviceType">The element type of the collections to register. This can be either
        /// a non-generic, closed-generic or open-generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        public void RegisterCollection(Type serviceType, params Assembly[] assemblies)
        {
            this.RegisterCollection(serviceType, (IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Registers all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <paramref name="serviceType"/> 
        /// with a default lifestyle and register them as a collection of <paramref name="serviceType"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// </summary>
        /// <param name="serviceType">The element type of the collections to register. This can be either
        /// a non-generic, closed-generic or open-generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        public void RegisterCollection(Type serviceType, IEnumerable<Assembly> assemblies)
        {
            this.RegisterCollection(serviceType, this.GetTypesToRegister(serviceType, assemblies));
        }

        /// <summary>
        /// Returns all concrete non-generic types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="serviceType"/>. 
        /// <paramref name="serviceType"/> can be an open-generic type.
        /// </summary>
        /// <remarks>
        /// Use this method when you need influence the types that are registered using 
        /// <see cref="Register(Type,IEnumerable{Assembly})">Register</see>. 
        /// The <b>Register</b> overloads that take a collection of <see cref="Assembly"/> 
        /// objects use this method internally to get the list of types that need to be registered. Instead of
        /// calling  such overload, you can call an overload that takes a list of <see cref="Type"/> objects 
        /// and pass  in a filtered result from this <b>GetTypesToRegister</b> method.
        /// <code lang="cs"><![CDATA[
        /// var container = new Container();
        /// 
        /// var assemblies = new[] { typeof(ICommandHandler<>).Assembly };
        /// var types = container.GetTypesToRegister(typeof(ICommandHandler<>), assemblies)
        ///     .Where(type => !type.IsPublic);
        /// 
        /// container.Register(typeof(ICommandHandler<>), types);
        /// ]]></code>
        /// This example calls the <b>GetTypesToRegister</b> method to request a list of concrete implementations
        /// of the <b>ICommandHandler&lt;T&gt;</b> interface from the assembly of that interface. After that
        /// all internal types are filtered out. This list is supplied to the
        /// <see cref="Register(Type,IEnumerable{Type})">Register(Type, IEnumerable&lt;Type&gt;)</see>
        /// overload to finish the registration.
        /// </remarks>
        /// <param name="serviceType">The base type or interface to find derived types for. This can be both
        /// a non-generic and open-generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A collection of types.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments contain a null reference 
        /// (Nothing in VB).</exception>
        public IEnumerable<Type> GetTypesToRegister(Type serviceType, IEnumerable<Assembly> assemblies)
        {
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(assemblies, "assemblies");

            var types =
                from assembly in assemblies.Distinct()
                where !assembly.IsDynamic
                from type in GetTypesFromAssembly(assembly)
                where Helpers.IsConcreteType(type)
                where !type.IsGenericType
                where Helpers.ServiceIsAssignableFromImplementation(serviceType, type)
                where !DecoratorHelpers.IsDecorator(this, serviceType, type)
                select type;

            return types.ToArray();
        }

        private static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (NotSupportedException)
            {
                // A type load exception would typically happen on an Anonymously Hosted DynamicMethods 
                // Assembly and it would be safe to skip this exception.
                return Enumerable.Empty<Type>();
            }
        }

        private sealed class BatchMapping
        {
            internal Type ImplementationType { get; private set; }

            internal IEnumerable<Type> ClosedServiceTypes { get; private set; }

            public static BatchMapping[] Build(Type openServiceType, IEnumerable<Type> implementationTypes)
            {
                var mappings = (
                    from implemenationType in implementationTypes
                    select Build(openServiceType, implemenationType))
                    .ToArray();

                RequiresNoDuplicateRegistrations(mappings);

                return mappings;
            }

            public static BatchMapping Build(Type openServiceType, Type implementationType)
            {
                return new BatchMapping()
                {
                    ImplementationType = implementationType,
                    ClosedServiceTypes = implementationType.GetBaseTypesAndInterfacesFor(openServiceType)
                };
            }

            private static void RequiresNoDuplicateRegistrations(BatchMapping[] mappings)
            {
                var duplicateServiceTypes =
                    from mapping in mappings
                    from closedServiceType in mapping.ClosedServiceTypes
                    group mapping.ImplementationType by closedServiceType into serviceTypeGroup
                    where serviceTypeGroup.Count() > 1
                    select new { service = serviceTypeGroup.Key, implementations = serviceTypeGroup.ToArray() };

                var invalidRegistration = duplicateServiceTypes.FirstOrDefault();

                if (invalidRegistration != null)
                {
                    throw new InvalidOperationException(
                        StringResources.MultipleTypesThatRepresentClosedGenericType(
                            invalidRegistration.service, invalidRegistration.implementations));
                }
            }
        }
    }
}