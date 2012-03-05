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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents the method that will called to register one or multiple concrete. non-generic
    /// <paramref name="implementations"/> of the given closed generic type 
    /// <paramref name="closedServiceType"/>.
    /// </summary>
    /// <param name="closedServiceType">The service type that needs to be registered.</param>
    /// <param name="implementations">One or more concrete types that implement the given 
    /// <paramref name="closedServiceType"/>.</param>
    public delegate void BatchRegistrationCallback(Type closedServiceType, Type[] implementations);

#if !SILVERLIGHT
    /// <summary>Defines the accessibility of the types to search.</summary>
    public enum AccessibilityOption
#else
    internal enum AccessibilityOption
#endif
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
        /// <summary>
        /// Registers all concrete, non-generic, publicly exposed types in the given set of
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

#if !SILVERLIGHT
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
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, params Assembly[] assemblies)
        {
            RegisterManyForOpenGeneric(container, openGenericServiceType, accessibility,
                (IEnumerable<Assembly>)assemblies);
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
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, accessibility);
        }
#endif

        /// <summary>
        /// Allows registration of all concrete, public, non-generic types in the given set of 
        /// <paramref name="assemblies"/> that implement the given <paramref name="openGenericServiceType"/>, 
        /// by supplying a <see cref="BatchRegistrationCallback"/> delegate, that will be called for each 
        /// found closed generic implementation of the given <paramref name="openGenericServiceType"/>.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="callback">The delegate that will be called for each found closed generic version of
        /// the given open generic <paramref name="openGenericServiceType"/> to do the actual registration.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, <paramref name="callback"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "container",
            Justification = "By using the 'this Container' argument, we allow this extension method to " +
            "show when using Intellisense over the Container.")]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, BatchRegistrationCallback callback,
            params Assembly[] assemblies)
        {
            RegisterManyForOpenGeneric(container, openGenericServiceType, callback,
                (IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Allows registration of all concrete, public, non-generic types in the given set of 
        /// <paramref name="assemblies"/> that implement the given <paramref name="openGenericServiceType"/>, 
        /// by supplying a <see cref="BatchRegistrationCallback"/> delegate, that will be called for each 
        /// found closed generic implementation of the given <paramref name="openGenericServiceType"/>.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="callback">The delegate that will be called for each found closed generic version of
        /// the given open generic <paramref name="openGenericServiceType"/> to do the actual registration.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, <paramref name="callback"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "container",
            Justification = "By using the 'this Container' argument, we allow this extension method to " +
            "show when using Intellisense over the Container.")]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, BatchRegistrationCallback callback,
            IEnumerable<Assembly> assemblies)
        {
            RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies,
                AccessibilityOption.PublicTypesOnly, callback);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Allows registration of all concrete, non-generic types with the given 
        /// <paramref name="accessibility"/> in the given set of <paramref name="assemblies"/> that implement 
        /// the given <paramref name="openGenericServiceType"/>, by supplying a 
        /// <see cref="BatchRegistrationCallback"/> delegate, that will be called for each found closed generic 
        /// implementation of the given <paramref name="openGenericServiceType"/>.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="callback">The delegate that will be called for each found closed generic version of
        /// the given open generic <paramref name="openGenericServiceType"/> to do the actual registration.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, <paramref name="callback"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "container",
            Justification = "By using the 'this Container' argument, we allow this extension method to " +
            "show when using Intellisense over the Container.")]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, BatchRegistrationCallback callback,
            params Assembly[] assemblies)
        {
            RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, accessibility, callback);
        }

        /// <summary>
        /// Allows registration of all concrete, non-generic types with the given 
        /// <paramref name="accessibility"/> in the given set of <paramref name="assemblies"/> that implement 
        /// the given <paramref name="openGenericServiceType"/>, by supplying a 
        /// <see cref="BatchRegistrationCallback"/> delegate, that will be called for each found closed generic 
        /// implementation of the given <paramref name="openGenericServiceType"/>.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="callback">The delegate that will be called for each found closed generic version of
        /// the given open generic <paramref name="openGenericServiceType"/> to do the actual registration.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, <paramref name="callback"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "container",
            Justification = "By using the 'this Container' argument, we allow this extension method to " +
            "show when using Intellisense over the Container.")]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, BatchRegistrationCallback callback,
            IEnumerable<Assembly> assemblies)
        {
            RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, accessibility, callback);
        }
#endif

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

#if !SILVERLIGHT
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
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
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
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
        public static void RegisterManySinglesForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            container.RegisterManySinglesForOpenGenericInternal(openGenericServiceType, assemblies, accessibility);
        }
#endif

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
            BatchRegistrationCallback callback = (closedServiceType, types) =>
            {
                RequiresSingleImplementation(closedServiceType, types);
                container.Register(closedServiceType, types.Single());
            };

            RegisterManyForOpenGenericInternal(openGenericServiceType, typesToRegister, callback);
        }

        /// <summary>
        /// Allows registration of all supplied <paramref name="typesToRegister"/> by a closed generic 
        /// definition of the given <paramref name="openGenericServiceType"/>, by supplying a 
        /// <see cref="BatchRegistrationCallback"/> delegate, that will be called for each found closed generic 
        /// implementation.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="callback">The delegate that will be called for each found closed generic version of
        /// the given open generic <paramref name="openGenericServiceType"/> to do the actual registration.</param>
        /// <param name="typesToRegister">The list of types that must be registered according to the given
        /// <paramref name="openGenericServiceType"/> definition.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>, 
        /// <paramref name="openGenericServiceType"/>, <paramref name="callback"/>, or 
        /// <paramref name="typesToRegister"/> contain a null reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="typesToRegister"/> contains a null
        /// (Nothing in VB) element, when the <paramref name="openGenericServiceType"/> is not an open generic
        /// type, or one of the types supplied in <paramref name="typesToRegister"/> does not implement a 
        /// closed version of <paramref name="openGenericServiceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "container",
            Justification = "By using the 'this Container' argument, we allow this extension method to " +
            "show when using Intellisense over the Container.")]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, BatchRegistrationCallback callback, IEnumerable<Type> typesToRegister)
        {
            RegisterManyForOpenGenericInternal(openGenericServiceType, typesToRegister, callback);
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
            RegisterManySinglesForOpenGeneric(container, openGenericServiceType,
                (IEnumerable<Type>)typesToRegister);
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
            BatchRegistrationCallback callback = (closedServiceType, types) =>
            {
                RequiresSingleImplementation(closedServiceType, types);
                container.RegisterSingle(closedServiceType, types.Single());
            };

            RegisterManyForOpenGenericInternal(openGenericServiceType, typesToRegister, callback);
        }

        /// <summary>
        /// Returns all public types from the supplied <paramref name="assemblies"/> that implement or inherit
        /// from the supplied <paramref name="openGenericServiceType"/>.
        /// </summary>
        /// <remarks>
        /// Use this method when you need influence the types that are registered using 
        /// <see cref="RegisterManyForOpenGeneric(Container,Type,Assembly[])">RegisterManyForOpenGeneric</see>. 
        /// The <b>RegisterManyForOpenGeneric</b> overloads that take a collection of <see cref="Assembly"/> 
        /// objects use this method to get the list of types that need to be registered. Instead of calling 
        /// such overload, you can call an overload that takes a list of <see cref="Type"/> objects and pass 
        /// in a filtered result from this <b>GetTypesToRegister</b> method.
        /// <code lang="cs"><![CDATA[
        /// var container = new Container();
        /// 
        /// var types = OpenGenericBatchRegistrationExtensions
        ///     .GetTypesToRegister(typeof(ICommandHandler<>), typeof(ICommandHandler<>).Assembly)
        ///     .Where(type => !type.Name.EndsWith("Decorator"));
        /// 
        /// container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), types);
        /// ]]></code>
        /// This example calls the <b>GetTypesToRegister</b> method to request a list of concrete implementations
        /// of the <b>ICommandHandler&lt;T&gt;</b> interface from the assembly of that interface. After that
        /// all types which name ends with 'Decorator' are filtered out. This list is supplied to an
        /// <see cref="RegisterManyForOpenGeneric(Container,Type,Assembly[])">RegisterManyForOpenGeneric</see>
        /// overload that takes a list of types to finish the
        /// registration.
        /// </remarks>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A list of types.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="openGenericServiceType"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        public static IEnumerable<Type> GetTypesToRegister(Type openGenericServiceType,
            params Assembly[] assemblies)
        {
            return GetTypesToRegisterInternal(openGenericServiceType, AccessibilityOption.PublicTypesOnly, assemblies);
        }

        /// <summary>
        /// Returns all public types from the supplied <paramref name="assemblies"/> that implement or inherit
        /// from the supplied <paramref name="openGenericServiceType"/>.
        /// </summary>
        /// <remarks>
        /// Use this method when you need influence the types that are registered using 
        /// <see cref="RegisterManyForOpenGeneric(Container,Type,Assembly[])">RegisterManyForOpenGeneric</see>. 
        /// The <b>RegisterManyForOpenGeneric</b> overloads that take a collection of <see cref="Assembly"/> 
        /// objects use this method to get the list of types that need to be registered. Instead of calling 
        /// such overload, you can call an overload that takes a list of <see cref="Type"/> objects and pass 
        /// in a filtered result from this <b>GetTypesToRegister</b> method.
        /// <code lang="cs"><![CDATA[
        /// var container = new Container();
        /// 
        /// var types = OpenGenericBatchRegistrationExtensions
        ///     .GetTypesToRegister(typeof(ICommandHandler<>), typeof(ICommandHandler<>).Assembly)
        ///     .Where(type => !type.Name.EndsWith("Decorator"));
        /// 
        /// container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), types);
        /// ]]></code>
        /// This example calls the <b>GetTypesToRegister</b> method to request a list of concrete implementations
        /// of the <b>ICommandHandler&lt;T&gt;</b> interface from the assembly of that interface. After that
        /// all types which name ends with 'Decorator' are filtered out. This list is supplied to an
        /// <see cref="RegisterManyForOpenGeneric(Container,Type,Assembly[])">RegisterManyForOpenGeneric</see>
        /// overload that takes a list of types to finish the registration.
        /// </remarks>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A list of types.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="openGenericServiceType"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        public static IEnumerable<Type> GetTypesToRegister(Type openGenericServiceType,
            IEnumerable<Assembly> assemblies)
        {
            return GetTypesToRegisterInternal(openGenericServiceType, AccessibilityOption.PublicTypesOnly, assemblies);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Returns all types from the supplied <paramref name="assemblies"/> that implement or inherit
        /// from the supplied <paramref name="openGenericServiceType"/>.
        /// </summary>
        /// <remarks>
        /// Use this method when you need influence the types that are registered using 
        /// <see cref="RegisterManyForOpenGeneric(Container,Type,Assembly[])">RegisterManyForOpenGeneric</see>. 
        /// The <b>RegisterManyForOpenGeneric</b> overloads that take a collection of <see cref="Assembly"/> 
        /// objects use this method to get the list of types that need to be registered. Instead of calling 
        /// such overload, you can call an overload that takes a list of <see cref="Type"/> objects and pass 
        /// in a filtered result from this <b>GetTypesToRegister</b> method.
        /// <code lang="cs"><![CDATA[
        /// var container = new Container();
        /// 
        /// var types = OpenGenericBatchRegistrationExtensions
        ///     .GetTypesToRegister(typeof(ICommandHandler<>), AccessibilityOption.PublicTypesOnly,
        ///         typeof(ICommandHandler<>).Assembly)
        ///     .Where(type => !type.Name.EndsWith("Decorator"));
        /// 
        /// container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), types);
        /// ]]></code>
        /// This example calls the <b>GetTypesToRegister</b> method to request a list of concrete implementations
        /// of the <b>ICommandHandler&lt;T&gt;</b> interface from the assembly of that interface. After that
        /// all types which name ends with 'Decorator' are filtered out. This list is supplied to an
        /// <see cref="RegisterManyForOpenGeneric(Container,Type,Assembly[])">RegisterManyForOpenGeneric</see>
        /// overload that takes a list of types to finish the
        /// registration.
        /// </remarks>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A list of types.</returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="openGenericServiceType"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        public static IEnumerable<Type> GetTypesToRegister(Type openGenericServiceType,
            AccessibilityOption accessibility, params Assembly[] assemblies)
        {
            return GetTypesToRegisterInternal(openGenericServiceType, accessibility, assemblies);
        }

        /// <summary>
        /// Returns all types from the supplied <paramref name="assemblies"/> that implement or inherit
        /// from the supplied <paramref name="openGenericServiceType"/>.
        /// </summary>
        /// <remarks>
        /// Use this method when you need influence the types that are registered using 
        /// <see cref="RegisterManyForOpenGeneric(Container,Type,Assembly[])">RegisterManyForOpenGeneric</see>.
        /// The <b>RegisterManyForOpenGeneric</b> overloads that take a collection of <see cref="Assembly"/> 
        /// objects use this method to get the list of types that need to be registered. Instead of calling 
        /// such overload, you can call an overload that takes a list of <see cref="Type"/> objects and pass 
        /// in a filtered result from this <b>GetTypesToRegister</b> method.
        /// <code lang="cs"><![CDATA[
        /// var container = new Container();
        /// 
        /// var types = OpenGenericBatchRegistrationExtensions
        ///     .GetTypesToRegister(typeof(ICommandHandler<>), AccessibilityOption.PublicTypesOnly, 
        ///         typeof(ICommandHandler<>).Assembly)
        ///     .Where(type => !type.Name.EndsWith("Decorator"));
        /// 
        /// container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), types);
        /// ]]></code>
        /// This example calls the <b>GetTypesToRegister</b> method to request a list of concrete implementations
        /// of the <b>ICommandHandler&lt;T&gt;</b> interface from the assembly of that interface. After that
        /// all types which name ends with 'Decorator' are filtered out. This list is supplied to an
        /// <see cref="RegisterManyForOpenGeneric(Container,Type,Assembly[])">RegisterManyForOpenGeneric</see>
        /// overload that takes a list of types to finish the registration.
        /// </remarks>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A list of types.</returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="openGenericServiceType"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        public static IEnumerable<Type> GetTypesToRegister(Type openGenericServiceType,
            AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            return GetTypesToRegisterInternal(openGenericServiceType, accessibility, assemblies);
        }
#endif

        private static void RegisterManyForOpenGenericInternal(this Container container,
            Type openGenericServiceType, IEnumerable<Assembly> assemblies, AccessibilityOption accessibility)
        {
            BatchRegistrationCallback callback = (closedServiceType, implementations) =>
            {
                RequiresSingleImplementation(closedServiceType, implementations);
                container.Register(closedServiceType, implementations.Single());
            };

            RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, accessibility, callback);
        }

        private static void RegisterManySinglesForOpenGenericInternal(this Container container,
            Type openGenericServiceType, IEnumerable<Assembly> assemblies, AccessibilityOption loadOptions)
        {
            BatchRegistrationCallback callback = (closedServiceType, implementations) =>
            {
                RequiresSingleImplementation(closedServiceType, implementations);
                container.RegisterSingle(closedServiceType, implementations.Single());
            };

            RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, loadOptions, callback);
        }

        private static void RegisterManyForOpenGenericInternal(Type openGenericServiceType,
            IEnumerable<Assembly> assemblies, AccessibilityOption accessibility,
            BatchRegistrationCallback callback)
        {
            Requires.IsNotNull(assemblies, "assemblies");
            Requires.IsValidValue(accessibility, "accessibility");

            var types = GetTypesToRegisterInternal(openGenericServiceType, accessibility, assemblies);

            RegisterManyForOpenGenericInternal(openGenericServiceType, types, callback);
        }

        private static IEnumerable<Type> GetTypesToRegisterInternal(Type openGenericServiceType,
            AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            Requires.IsNotNull(openGenericServiceType, "openGenericServiceType");
            Requires.IsNotNull(assemblies, "assemblies");
            Requires.IsValidValue(accessibility, "accessibility");

            return
                from assembly in assemblies
                from type in Helpers.GetTypesFromAssembly(assembly, accessibility)
                where Helpers.IsConcreteType(type)
                where Helpers.ServiceIsAssignableFromImplementation(openGenericServiceType, type)
                select type;
        }

        private static void RegisterManyForOpenGenericInternal(Type openGenericServiceType,
            IEnumerable<Type> typesToRegister, BatchRegistrationCallback callback)
        {
            // Make a copy of the collection for performance and correctness.
            typesToRegister = typesToRegister != null ? typesToRegister.ToArray() : null;

            Requires.IsNotNull(openGenericServiceType, "openGenericServiceType");
            Requires.IsNotNull(typesToRegister, "typesToRegister");
            Requires.IsNotNull(callback, "callback");
            Requires.DoesNotContainNullValues(typesToRegister, "typesToRegister");
            Requires.TypeIsOpenGeneric(openGenericServiceType, "openGenericServiceType");
            Requires.ServiceIsAssignableFromImplementations(openGenericServiceType, typesToRegister, "typesToRegister");

            RegisterOpenGenericInternal(openGenericServiceType, typesToRegister, callback);
        }

        private static void RegisterOpenGenericInternal(Type openGenericType,
            IEnumerable<Type> typesToRegister, BatchRegistrationCallback callback)
        {
            // A single type to register can implement multiple closed versions of a open generic type, so
            // we can end up with multiple registrations per type.
            // Example: class StrangeValidator : IValidator<Person>, IValidator<Customer> { }
            var registrations =
                from implementation in typesToRegister
                from service in implementation.GetBaseTypesAndInterfacesFor(openGenericType)
                let registration = new { service, implementation }
                group registration by registration.service into g
                select new
                {
                    ServiceType = g.Key,
                    Implementations = g.Select(r => r.implementation).ToArray()
                };

            foreach (var registration in registrations)
            {
                callback(registration.ServiceType, registration.Implementations);
            }
        }

        private static void RequiresSingleImplementation(Type closedServiceType, Type[] implementations)
        {
            if (implementations.Length > 1)
            {
                var typeDescription = string.Join(", ", (
                    from type in implementations
                    select string.Format(CultureInfo.InvariantCulture, "'{0}'", type)).ToArray());

                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "There are {0} types that represent the closed generic type '{1}'. Types: {2}.",
                    implementations.Length, closedServiceType, typeDescription));
            }
        }
    }
}