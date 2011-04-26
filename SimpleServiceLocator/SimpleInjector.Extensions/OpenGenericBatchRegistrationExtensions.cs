using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SimpleInjector.Extensions
{
    /// <summary>
    /// Defines the accessibility of the types to search.
    /// </summary>
    public enum AccessibilityOption
    {
        /// <summary>Load both public as internal types from the given assemblies.</summary>
        AllTypes = 0,

        /// <summary>Only load publicly exposed types from the given assemblies.</summary>
        PublicTypesOnly = 1,
    }

    /// <summary>
    /// Provides a set of static (Shared in Visual Basic) methods for registration many concrete types at
    /// once that implement the same open generic service types in the <see cref="Container"/>.
    /// </summary>
    public static partial class OpenGenericBatchRegistrationExtensions
    {
        private enum Lifetime
        {
            Transient = 0,
            Singleton = 1
        }

        /// <summary>
        /// Registers all concrete, non-generic, publicly exposed types in the given 
        /// <paramref name="assemblies"/> that implement the given <paramref name="openGenericServiceType"/> 
        /// with a transient lifetime.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, or <paramref name="assemblies"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple publicly exposed types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManyForOpenGeneric(this Container container, 
            Type openGenericServiceType, params Assembly[] assemblies)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, 
                AccessibilityOption.PublicTypesOnly);
        }

        /// <summary>
        /// Registers all concrete, non-generic, publicly exposed types in the given 
        /// <paramref name="assemblies"/> that implement the given <paramref name="openGenericServiceType"/> 
        /// with a transient lifetime.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, or <paramref name="assemblies"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple publicly exposed types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, IEnumerable<Assembly> assemblies)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, 
                AccessibilityOption.PublicTypesOnly);
        }

        /// <summary>
        /// Registers  all concrete, non-generic types with the given <paramref name="accessibility"/> in the 
        /// given <paramref name="assemblies"/> that implement the given 
        /// <paramref name="openGenericServiceType"/> with a transient lifetime.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, or <paramref name="assemblies"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same closed generic 
        /// version of the given <paramref name="openGenericServiceType"/>.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="accessibility"/> 
        /// contains an invalid value.</exception>
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, params Assembly[] assemblies)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, accessibility);
        }

        /// <summary>
        /// Registers all concrete, non-generic types with the given <paramref name="accessibility"/> in the 
        /// given <paramref name="assemblies"/> that implement the given 
        /// <paramref name="openGenericServiceType"/> with a transient lifetime.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, or <paramref name="assemblies"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="accessibility"/> 
        /// contains an invalid value.</exception>
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, accessibility);
        }

        /// <summary>
        /// Registers all concrete, non-generic, publicly exposed types in the given 
        /// <paramref name="assemblies"/> that implement the given 
        /// <paramref name="openGenericServiceType"/> with a singleton lifetime.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, or <paramref name="assemblies"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple publicly exposed types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManySinglesForOpenGeneric(this Container container,
            Type openGenericServiceType, params Assembly[] assemblies)
        {
            container.RegisterManySinglesForOpenGenericInternal(openGenericServiceType, assemblies,
                AccessibilityOption.PublicTypesOnly);
        }

        /// <summary>
        /// Registers all concrete, non-generic, publicly exposed types in the given 
        /// <paramref name="assemblies"/> that implement the given <paramref name="openGenericServiceType"/> 
        /// with a singleton lifetime.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, or <paramref name="assemblies"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple publicly exposed types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManySinglesForOpenGeneric(this Container container,
            Type openGenericServiceType, IEnumerable<Assembly> assemblies)
        {
            container.RegisterManySinglesForOpenGenericInternal(openGenericServiceType, assemblies,
                AccessibilityOption.PublicTypesOnly);
        }

        /// <summary>
        /// Registers  all concrete, non-generic types with the given <paramref name="accessibility"/> in the 
        /// given <paramref name="assemblies"/> that implement the given 
        /// <paramref name="openGenericServiceType"/> with a singleton lifetime.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, or <paramref name="assemblies"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same closed generic 
        /// version of the given <paramref name="openGenericServiceType"/>.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="accessibility"/> 
        /// contains an invalid value.</exception>
        public static void RegisterManySinglesForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, params Assembly[] assemblies)
        {
            container.RegisterManySinglesForOpenGenericInternal(openGenericServiceType, assemblies, accessibility);
        }

        /// <summary>
        /// Registers all concrete, non-generic types with the given <paramref name="accessibility"/> in the 
        /// given <paramref name="assemblies"/> that implement the given 
        /// <paramref name="openGenericServiceType"/> with a singleton lifetime.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, or <paramref name="assemblies"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="accessibility"/> 
        /// contains an invalid value.</exception>
        public static void RegisterManySinglesForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            container.RegisterManySinglesForOpenGenericInternal(openGenericServiceType, assemblies, accessibility);
        }

        /// <summary>
        /// Registers all supplied <paramref name="typesToRegister"/> by a closed generic definition of the
        /// given <paramref name="openGenericServiceType"/> with a transient lifetime.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="typesToRegister">The list of types that must be registered according to the given
        /// <paramref name="openGenericServiceType"/> definition.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>, 
        /// <paramref name="openGenericServiceType"/>, or <paramref name="typesToRegister"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="typesToRegister"/> contains a null
        /// (Nothing in VB) element, when the <paramref name="openGenericServiceType"/> is not an open generic
        /// type, or one of the types supplied in <paramref name="typesToRegister"/> does not implement a 
        /// closed version of <paramref name="openGenericServiceType"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when there are multiple types in the given
        /// <paramref name="typesToRegister"/> collection that implement the same closed version of the
        /// supplied <paramref name="openGenericServiceType"/>.
        /// </exception>
        public static void RegisterManyForOpenGeneric(this Container container, 
            Type openGenericServiceType, params Type[] typesToRegister)
        {
            RegisterManyForOpenGeneric(container, openGenericServiceType, (IEnumerable<Type>)typesToRegister);
        }

        /// <summary>
        /// Registers all supplied <paramref name="typesToRegister"/> by a closed generic definition of the
        /// given <paramref name="openGenericServiceType"/> with a transient lifetime.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="typesToRegister">The list of types that must be registered according to the given
        /// <paramref name="openGenericServiceType"/> definition.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>, 
        /// <paramref name="openGenericServiceType"/>, or <paramref name="typesToRegister"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="typesToRegister"/> contains a null
        /// (Nothing in VB) element, when the <paramref name="openGenericServiceType"/> is not an open generic
        /// type, or one of the types supplied in <paramref name="typesToRegister"/> does not implement a 
        /// closed version of <paramref name="openGenericServiceType"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when there are multiple types in the given
        /// <paramref name="typesToRegister"/> collection that implement the same closed version of the
        /// supplied <paramref name="openGenericServiceType"/>.
        /// </exception>
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, IEnumerable<Type> typesToRegister)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, typesToRegister, 
                Lifetime.Transient);
        }

        /// <summary>
        /// Registers all supplied <paramref name="typesToRegister"/> by a closed generic definition of the
        /// given <paramref name="openGenericServiceType"/> with a singleton lifetime.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="typesToRegister">The list of types that must be registered according to the given
        /// <paramref name="openGenericServiceType"/> definition.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>, 
        /// <paramref name="openGenericServiceType"/>, or <paramref name="typesToRegister"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="typesToRegister"/> contains a null
        /// (Nothing in VB) element, when the <paramref name="openGenericServiceType"/> is not an open generic
        /// type, or one of the types supplied in <paramref name="typesToRegister"/> does not implement a 
        /// closed version of <paramref name="openGenericServiceType"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when there are multiple types in the given
        /// <paramref name="typesToRegister"/> collection that implement the same closed version of the
        /// supplied <paramref name="openGenericServiceType"/>.
        /// </exception>
        public static void RegisterManySinglesForOpenGeneric(this Container container,
            Type openGenericServiceType, params Type[] typesToRegister)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, typesToRegister,
                Lifetime.Singleton);
        }

        /// <summary>
        /// Registers all supplied <paramref name="typesToRegister"/> by a closed generic definition of the
        /// given <paramref name="openGenericServiceType"/> with a singleton lifetime.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="typesToRegister">The list of types that must be registered according to the given
        /// <paramref name="openGenericServiceType"/> definition.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>, 
        /// <paramref name="openGenericServiceType"/>, or <paramref name="typesToRegister"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="typesToRegister"/> contains a null
        /// (Nothing in VB) element, when the <paramref name="openGenericServiceType"/> is not an open generic
        /// type, or one of the types supplied in <paramref name="typesToRegister"/> does not implement a 
        /// closed version of <paramref name="openGenericServiceType"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when there are multiple types in the given
        /// <paramref name="typesToRegister"/> collection that implement the same closed version of the
        /// supplied <paramref name="openGenericServiceType"/>.
        /// </exception>
        public static void RegisterManySinglesForOpenGeneric(this Container container,
            Type openGenericServiceType, IEnumerable<Type> typesToRegister)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, typesToRegister,
                Lifetime.Singleton);
        }

        private static void RegisterManyForOpenGenericInternal(this Container container,
            Type openGenericServiceType, IEnumerable<Assembly> assemblies, AccessibilityOption accessibility)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, accessibility,
                Lifetime.Transient);
        }

        private static void RegisterManySinglesForOpenGenericInternal(this Container container,
            Type openGenericServiceType, IEnumerable<Assembly> assemblies, AccessibilityOption loadOptions)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, loadOptions,
                Lifetime.Singleton);
        }

        private static void RegisterManyForOpenGenericInternal(this Container container,
            Type openGenericServiceType, IEnumerable<Assembly> assemblies, AccessibilityOption accessibility,
            Lifetime lifetime)
        {
            Requires.IsNotNull(assemblies, "assemblies");
            Requires.IsValidValue(accessibility, "accessibility");
            
            var typesToRegister =
                from assembly in assemblies
                from type in Helpers.GetTypesFromAssembly(assembly, accessibility)
                where Helpers.IsConcreteType(type)
                where Helpers.ServiceIsAssignableFromImplementation(openGenericServiceType, type)
                select type;

            container.RegisterManyForOpenGenericInternal(openGenericServiceType, typesToRegister, lifetime);
        }

        private static void RegisterManyForOpenGenericInternal(this Container container, 
            Type openGenericServiceType, IEnumerable<Type> typesToRegister, Lifetime lifetime)
        {
            // Make a copy of the collection for performance and correctness.
            typesToRegister = typesToRegister != null ? typesToRegister.ToArray() : null;

            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(openGenericServiceType, "openGenericServiceType");
            Requires.IsNotNull(typesToRegister, "typesToRegister");
            Requires.DoesNotContainNullValues(typesToRegister, "typesToRegister");
            Requires.TypeIsOpenGeneric(openGenericServiceType, "openGenericServiceType");
            Requires.ServiceIsAssignableFromImplementations(openGenericServiceType, typesToRegister, "typesToRegister");
            Requires.NoDuplicateRegistrations(openGenericServiceType, typesToRegister);

            container.RegisterOpenGenericInternal(openGenericServiceType, typesToRegister, lifetime);
        }

        private static void RegisterOpenGenericInternal(this Container container,
            Type openGenericType, IEnumerable<Type> typesToRegister, Lifetime lifetime)
        {
            // A single type to register can implement multiple closed versions of a open generic type, so
            // we can end up with multiple registrations per type.
            // Example: class StrangeValidator : IValidator<Person>, IValidator<Customer> { }
            var registrations =
                from implementation in typesToRegister
                from service in implementation.GetBaseTypesAndInterfaces(openGenericType)
                select new { ServiceType = service, Implementation = implementation };

            foreach (var registration in registrations)
            {
                container.Register(registration.ServiceType, registration.Implementation, lifetime);
            }
        }

        private static void Register(this Container container, Type serviceType, 
            Type implementationType, Lifetime lifetime)
        {
            if (lifetime == Lifetime.Singleton)
            {
                container.RegisterSingle(serviceType, implementationType);
            }
            else
            {
                container.Register(serviceType, implementationType);
            }
        }
    }
}