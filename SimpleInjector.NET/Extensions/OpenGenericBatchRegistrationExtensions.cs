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

namespace SimpleInjector.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using SimpleInjector.Extensions.Decorators;

    /// <summary>
    /// Represents the method that will called to register one or multiple concrete non-generic
    /// <paramref name="implementations"/> of the given closed generic type 
    /// <paramref name="closedServiceType"/>.
    /// </summary>
    /// <param name="closedServiceType">The service type that needs to be registered.</param>
    /// <param name="implementations">One or more concrete types that implement the given 
    /// <paramref name="closedServiceType"/>.</param>
    /// <example>
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    /// 
    /// BatchRegistrationCallback registerAsCollectionAsSingletons = (closedServiceType, implementations) =>
    /// {
    ///     foreach (Type implementation in implementations)
    ///     {
    ///         container.RegisterSingle(implementation);
    ///     }
    ///     
    ///     container.RegisterAll(closedServiceType, implementations);
    /// };
    /// 
    /// container.RegisterManyForOpenGeneric(
    ///     typeof(ICommandHandler<>),
    ///     registerAsCollectionAsSingletons, 
    ///     typeof(ICommandHandler<>).Assembly);
    /// ]]></code>
    /// The <b>BatchRegistrationCallback</b> can be supplied to some overloads of the
    /// <see cref="OpenGenericBatchRegistrationExtensions">RegisterManyForOpenGeneric</see> extension methods.
    /// The default behavior of the <b>RegisterManyForOpenGeneric</b> methods is to register a closed generic
    /// type with the corresponding implementation (and will throw when multiple implementations are found for
    /// a single closed generic service type). The given example overrides this default registration by 
    /// registering the found list of implementations (one or more) as collection of singletons for the given 
    /// closed generic service type.
    /// </example>
    public delegate void BatchRegistrationCallback(Type closedServiceType, Type[] implementations);

    /// <summary>Defines the accessibility of the types to search.</summary>
    /// <remarks>This type is not available in Silverlight.</remarks>
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
        private const string GetTypesToRegisterObsoleteMessage =
            "Use the overload that takes SimpleInjector.Container as an argument instead. This method might " +
            "incorrectly return any decorators.";

        /// <summary>
        /// Registers all concrete, non-generic, public and internal types in the given set of
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
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, params Assembly[] assemblies)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, Lifestyle.Transient);
        }

        /// <summary>
        /// Registers all concrete, non-generic, public and internal types that are located in the given 
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
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, IEnumerable<Assembly> assemblies)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, Lifestyle.Transient);
        }

        /// <summary>
        /// Registers all concrete, non-generic, public and internal types in the given set of
        /// <paramref name="assemblies"/> that implement the given <paramref name="openGenericServiceType"/> 
        /// with the supplied <paramref name="lifestyle"/>. When a found type implements multiple 
        /// closed-generic versions of the given <paramref name="openGenericServiceType"/>, both closed-generic
        /// service types will point at the same registration and return the same instance based on the caching
        /// behavior of the supplied <paramref name="lifestyle"/>.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="lifestyle">The lifestyle that will be used for the registration of the types.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, <paramref name="lifestyle"/> or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, Lifestyle lifestyle, params Assembly[] assemblies)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, lifestyle);
        }

        /// <summary>
        /// Registers all concrete, non-generic, public and internal types that are located in the given 
        /// <paramref name="assemblies"/> that implement the given <paramref name="openGenericServiceType"/> 
        /// with the supplied <paramref name="lifestyle"/>. When a found type implements multiple 
        /// closed-generic versions of the given <paramref name="openGenericServiceType"/>, both closed-generic
        /// service types will point at the same registration and return the same instance based on the caching
        /// behavior of the supplied <paramref name="lifestyle"/>.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="lifestyle">The lifestyle that will be used for the registration of the types.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, <paramref name="lifestyle"/> or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, Lifestyle lifestyle, IEnumerable<Assembly> assemblies)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, lifestyle);
        }

        /// <summary>
        /// Allows registration of all concrete, public and internal, non-generic types that are located in the given set of 
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
            Justification = @"
                By using the 'this Container' argument, we allow this extension method to show when using 
                IntelliSense over the Container.")]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, BatchRegistrationCallback callback,
            params Assembly[] assemblies)
        {
            RegisterManyForOpenGeneric(container, openGenericServiceType, callback,
                (IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Allows registration of all concrete, public and internal, non-generic types that are located in the given set of 
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
            "show when using IntelliSense over the Container.")]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, BatchRegistrationCallback callback,
            IEnumerable<Assembly> assemblies)
        {
            Requires.IsNotNull(container, "container");

            RegisterManyForOpenGenericInternal(container, openGenericServiceType, assemblies, callback);
        }

        /// <summary>
        /// Registers all concrete, non-generic, public and internal types that are located in the given 
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
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManySinglesForOpenGeneric(this Container container,
            Type openGenericServiceType, params Assembly[] assemblies)
        {
            Requires.IsNotNull(container, "container");

            RegisterManySinglesForOpenGenericInternal(container, openGenericServiceType, assemblies);
        }

        /// <summary>
        /// Registers all concrete, non-generic, public and internal types that are located in the given 
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
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManySinglesForOpenGeneric(this Container container,
            Type openGenericServiceType, IEnumerable<Assembly> assemblies)
        {
            Requires.IsNotNull(container, "container");

            RegisterManySinglesForOpenGenericInternal(container, openGenericServiceType, assemblies);
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
            BatchRegistrationCallback callback = (closedServiceType, types) =>
            {
                RequiresSingleImplementation(closedServiceType, types);
                container.Register(closedServiceType, types.Single(), Lifestyle.Transient);
            };

            RegisterManyForOpenGeneric(openGenericServiceType, typesToRegister, callback);
        }

        /// <summary>
        /// Allows registration of all supplied <paramref name="typesToRegister"/> by a closed generic 
        /// definition of the given <paramref name="openGenericServiceType"/>, by supplying a 
        /// <see cref="BatchRegistrationCallback"/> delegate, that will be called for each found closed generic 
        /// implementation.
        /// If the list contains open generic types, matching closed generic versions of each open generic
        /// type will be added to the list of implementations that is passed on to the 
        /// <paramref name="callback"/> delegate.
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
            Justification = @"
                By using the 'this Container' argument, we allow this extension method to show when using 
                IntelliSense over the Container.")]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, BatchRegistrationCallback callback, IEnumerable<Type> typesToRegister)
        {
            RegisterManyForOpenGenericWithOpenGenericTypes(openGenericServiceType, typesToRegister, callback);
        }

        /// <summary>
        /// Registers all supplied <paramref name="typesToRegister"/> by a closed generic definition of the
        /// given <paramref name="openGenericServiceType"/> with a singleton lifetime.
        /// When a found type implements multiple 
        /// closed-generic versions of the given <paramref name="openGenericServiceType"/>, both closed-generic
        /// service types will return the exact same instance.
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
        /// When a found type implements multiple 
        /// closed-generic versions of the given <paramref name="openGenericServiceType"/>, both closed-generic
        /// service types will return the exact same instance.
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
                container.Register(closedServiceType, types.Single(), Lifestyle.Singleton);
            };

            RegisterManyForOpenGeneric(openGenericServiceType, typesToRegister, callback);
        }

        /// <summary>
        /// Registers  all concrete, non-generic types with the given <paramref name="accessibility"/>
        /// that are located in the given <paramref name="assemblies"/> that implement the given 
        /// <paramref name="openGenericServiceType"/> with a transient lifetime.
        /// </summary>
        /// <remarks><b>This method is not available in Silverlight.</b></remarks>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, or <paramref name="assemblies"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type or when <paramref name="accessibility"/> contains an invalid value.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same closed generic 
        /// version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, params Assembly[] assemblies)
        {
            RegisterManyForOpenGeneric(container, openGenericServiceType, accessibility, Lifestyle.Transient,
                (IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Registers all concrete, non-generic types with the given <paramref name="accessibility"/> 
        /// that are located in the given <paramref name="assemblies"/> that implement the given 
        /// <paramref name="openGenericServiceType"/> with a transient lifetime.
        /// </summary>
        /// <remarks><b>This method is not available in Silverlight.</b></remarks>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, or <paramref name="assemblies"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type or when the <paramref name="accessibility"/> argument contains an invalid
        /// value.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            RegisterManyForOpenGenericInternal(container, openGenericServiceType, assemblies,
                Lifestyle.Transient, accessibility);
        }

        /// <summary>
        /// Registers  all concrete, non-generic types with the given <paramref name="accessibility"/>
        /// that are located in the given <paramref name="assemblies"/> that implement the given 
        /// <paramref name="openGenericServiceType"/> with the supplied <paramref name="lifestyle"/>.
        /// When a found type implements multiple 
        /// closed-generic versions of the given <paramref name="openGenericServiceType"/>, both closed-generic
        /// service types will point at the same registration and return the same instance based on the caching
        /// behavior of the supplied <paramref name="lifestyle"/>.
        /// </summary>
        /// <remarks><b>This method is not available in Silverlight.</b></remarks>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="lifestyle">The lifestyle that will be used for the registration of the types.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, <paramref name="lifestyle"/> or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type or when <paramref name="accessibility"/> contains an invalid value.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same closed generic 
        /// version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, Lifestyle lifestyle,
            params Assembly[] assemblies)
        {
            RegisterManyForOpenGeneric(container, openGenericServiceType, accessibility, lifestyle,
                (IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Registers all concrete, non-generic types with the given <paramref name="accessibility"/> 
        /// that are located in the given <paramref name="assemblies"/> that implement the given 
        /// <paramref name="openGenericServiceType"/> with the supplied <paramref name="lifestyle"/>.
        /// When a found type implements multiple 
        /// closed-generic versions of the given <paramref name="openGenericServiceType"/>, both closed-generic
        /// service types will point at the same registration and return the same instance based on the caching
        /// behavior of the supplied <paramref name="lifestyle"/>.
        /// </summary>
        /// <remarks><b>This method is not available in Silverlight.</b></remarks>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="lifestyle">The lifestyle that will be used for the registration of the types.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, <paramref name="lifestyle"/> or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type or when <paramref name="accessibility"/> contains an invalid value.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, Lifestyle lifestyle,
            IEnumerable<Assembly> assemblies)
        {
            container.RegisterManyForOpenGenericInternal(openGenericServiceType, assemblies, lifestyle,
                accessibility);
        }

        /// <summary>
        /// Allows registration of all concrete, non-generic types with the given 
        /// <paramref name="accessibility"/> that are located in the given set of <paramref name="assemblies"/> 
        /// that implement the given <paramref name="openGenericServiceType"/>, by supplying a 
        /// <see cref="BatchRegistrationCallback"/> delegate, that will be called for each found closed generic 
        /// implementation of the given <paramref name="openGenericServiceType"/>.
        /// </summary>
        /// <remarks><b>This method is not available in Silverlight.</b></remarks>
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
        /// an open generic type or when <paramref name="accessibility"/> contains an invalid value.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "container",
            Justification = @"
                By using the 'this Container' argument, we allow this extension method to show when using 
                IntelliSense over the Container.")]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, BatchRegistrationCallback callback,
            params Assembly[] assemblies)
        {
            RegisterManyForOpenGenericInternal(container, openGenericServiceType, assemblies, callback, accessibility);
        }

        /// <summary>
        /// Allows registration of all concrete, non-generic types with the given 
        /// <paramref name="accessibility"/> that are located in the given set of <paramref name="assemblies"/> 
        /// that implement the given <paramref name="openGenericServiceType"/>, by supplying a 
        /// <see cref="BatchRegistrationCallback"/> delegate, that will be called for each found closed generic 
        /// implementation of the given <paramref name="openGenericServiceType"/>.
        /// </summary>
        /// <remarks><b>This method is not available in Silverlight.</b></remarks>
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
        /// an open generic type or when <paramref name="accessibility"/> contains an invalid value.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "container",
            Justification = @"
                By using the 'this Container' argument, we allow this extension method to show when using 
                IntelliSense over the Container.")]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, BatchRegistrationCallback callback,
            IEnumerable<Assembly> assemblies)
        {
            RegisterManyForOpenGenericInternal(container, openGenericServiceType, assemblies, callback, accessibility);
        }

        /// <summary>
        /// Registers  all concrete, non-generic types with the given <paramref name="accessibility"/> 
        /// that are located in the given <paramref name="assemblies"/> that implement the given 
        /// <paramref name="openGenericServiceType"/> with a singleton lifetime.
        /// </summary>
        /// <remarks><b>This method is not available in Silverlight.</b></remarks>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, or <paramref name="assemblies"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type or when <paramref name="accessibility"/> contains an invalid value.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same closed generic 
        /// version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManySinglesForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, params Assembly[] assemblies)
        {
            RegisterManySinglesForOpenGenericInternal(container, openGenericServiceType, assemblies, accessibility);
        }

        /// <summary>
        /// Registers all concrete, non-generic types with the given <paramref name="accessibility"/> 
        /// that are located in the given <paramref name="assemblies"/> that implement the given 
        /// <paramref name="openGenericServiceType"/> with a singleton lifetime.
        /// </summary>
        /// <remarks><b>This method is not available in Silverlight.</b></remarks>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/>,
        /// <paramref name="openGenericServiceType"/>, or <paramref name="assemblies"/> contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type or when <paramref name="accessibility"/> contains an invalid value.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public static void RegisterManySinglesForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            RegisterManySinglesForOpenGenericInternal(container, openGenericServiceType, assemblies,
                accessibility);
        }

        /// <summary>
        /// Returns all types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="openGenericServiceType"/>.
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
        /// <exception cref="ArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="openGenericServiceType"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        [Obsolete(GetTypesToRegisterObsoleteMessage)]
        public static IEnumerable<Type> GetTypesToRegister(Type openGenericServiceType,
            AccessibilityOption accessibility, params Assembly[] assemblies)
        {
            return GetTypesToRegisterInternal(null, openGenericServiceType, assemblies, accessibility);
        }

        /// <summary>
        /// Returns all types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="openGenericServiceType"/>.
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
        /// <exception cref="ArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="openGenericServiceType"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        [Obsolete(GetTypesToRegisterObsoleteMessage)]
        public static IEnumerable<Type> GetTypesToRegister(Type openGenericServiceType,
            AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            return GetTypesToRegisterInternal(null, openGenericServiceType, assemblies, accessibility);
        }

        /// <summary>
        /// Returns all types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="openGenericServiceType"/>.
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
        /// <param name="container">The container to use.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A list of types.</returns>
        /// <exception cref="ArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="openGenericServiceType"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        public static IEnumerable<Type> GetTypesToRegister(Container container, Type openGenericServiceType,
            AccessibilityOption accessibility, params Assembly[] assemblies)
        {
            return GetTypesToRegisterInternal(container, openGenericServiceType, assemblies, accessibility);
        }

        /// <summary>
        /// Returns all types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="openGenericServiceType"/>.
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
        /// <param name="container">The container to use.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="accessibility">Defines which types should be used from the given assemblies.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A list of types.</returns>
        /// <exception cref="ArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="openGenericServiceType"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        public static IEnumerable<Type> GetTypesToRegister(Container container, Type openGenericServiceType,
            AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            return GetTypesToRegisterInternal(container, openGenericServiceType, assemblies, accessibility);
        }

        /// <summary>
        /// Returns all public and internal types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="openGenericServiceType"/>.
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
        [Obsolete(GetTypesToRegisterObsoleteMessage)]
        public static IEnumerable<Type> GetTypesToRegister(Type openGenericServiceType,
            params Assembly[] assemblies)
        {
            return GetTypesToRegisterInternal(null, openGenericServiceType, assemblies, includeInternals: true);
        }

        /// <summary>
        /// Returns all public and internal types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="openGenericServiceType"/>.
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
        [Obsolete(GetTypesToRegisterObsoleteMessage)]
        public static IEnumerable<Type> GetTypesToRegister(Type openGenericServiceType,
            IEnumerable<Assembly> assemblies)
        {
            return GetTypesToRegisterInternal(null, openGenericServiceType, assemblies: assemblies,
                includeInternals: true);
        }

        /// <summary>
        /// Returns all public and internal types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="openGenericServiceType"/>.
        /// Types that are considered to be decorators are not returned.
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
        /// <param name="container">The container to use.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A list of types.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="openGenericServiceType"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        public static IEnumerable<Type> GetTypesToRegister(Container container, Type openGenericServiceType,
            params Assembly[] assemblies)
        {
            Requires.IsNotNull(container, "container");

            return GetTypesToRegisterInternal(container, openGenericServiceType, assemblies,
                includeInternals: true);
        }

        /// <summary>
        /// Returns all public and internal types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="openGenericServiceType"/>.
        /// Types that are considered to be decorators are not returned.
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
        /// <param name="container">The container to use.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A list of types.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="openGenericServiceType"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        public static IEnumerable<Type> GetTypesToRegister(Container container, Type openGenericServiceType,
            IEnumerable<Assembly> assemblies)
        {
            Requires.IsNotNull(container, "container");

            return GetTypesToRegisterInternal(container, openGenericServiceType, assemblies,
                includeInternals: true);
        }

        private static void RegisterManyForOpenGenericInternal(this Container container,
            Type openGenericServiceType, IEnumerable<Assembly> assemblies, Lifestyle lifestyle,
            bool includeInternal = true)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(lifestyle, "lifestyle");

            var typesToRegister = new Dictionary<Type, List<Type>>();

            BatchRegistrationCallback callback = (closedServiceType, implementations) =>
            {
                RequiresSingleImplementation(closedServiceType, implementations);

                var implementation = implementations.Single();

                Add(typesToRegister, implementation, closedServiceType);
            };

            RegisterManyForOpenGenericInternal(container, openGenericServiceType, assemblies, callback, 
                includeInternal);

            RegisterTypes(container, lifestyle, typesToRegister);
        }

        private static void Add(Dictionary<Type, List<Type>> typesToRegister, Type implementation,
            Type closedServiceType)
        {
            List<Type> closedServiceTypes;

            if (!typesToRegister.TryGetValue(implementation, out closedServiceTypes))
            {
                typesToRegister[implementation] = closedServiceTypes = new List<Type>(1);
            }

            closedServiceTypes.Add(closedServiceType);
        }
        
        private static void RegisterTypes(Container container, Lifestyle lifestyle, 
            Dictionary<Type, List<Type>> typesToRegister)
        {
            foreach (var pair in typesToRegister)
            {
                RegisterType(container, lifestyle, implementation: pair.Key, closedServiceTypes: pair.Value);
            }
        }

        private static void RegisterType(Container container, Lifestyle lifestyle, Type implementation, 
            List<Type> closedServiceTypes)
        {
            var registration = lifestyle.CreateRegistration(closedServiceTypes[0], implementation, container);

            foreach (var closedServiceType in closedServiceTypes)
            {
                container.AddRegistration(closedServiceType, registration);
            }
        }

        private static void RegisterManySinglesForOpenGenericInternal(Container container,
            Type openGenericServiceType, IEnumerable<Assembly> assemblies, bool includeInternals = true)
        {
            BatchRegistrationCallback callback = (closedServiceType, implementations) =>
            {
                RequiresSingleImplementation(closedServiceType, implementations);
                container.Register(closedServiceType, implementations.Single(), Lifestyle.Singleton);
            };

            RegisterManyForOpenGenericInternal(container, openGenericServiceType, assemblies, callback, includeInternals);
        }

        private static void RegisterManyForOpenGenericInternal(Container container, Type openGenericServiceType, 
            IEnumerable<Assembly> assemblies, BatchRegistrationCallback callback, bool includeInternals = true)
        {
            Requires.IsNotNull(assemblies, "assemblies");

            var types = GetTypesToRegisterInternal(container, openGenericServiceType, assemblies, includeInternals);

            RegisterManyForOpenGeneric(openGenericServiceType, types, callback);
        }

        private static IEnumerable<Type> GetTypesToRegisterInternal(Container container, Type openGenericServiceType,
            IEnumerable<Assembly> assemblies, bool includeInternals)
        {
            Requires.IsNotNull(openGenericServiceType, "openGenericServiceType");
            Requires.IsNotNull(assemblies, "assemblies");

            return
                from assembly in assemblies
                where !assembly.IsDynamic
                from type in ExtensionHelpers.GetTypesFromAssembly(assembly, includeInternals)
                where ExtensionHelpers.IsConcreteType(type)
                where ExtensionHelpers.ServiceIsAssignableFromImplementation(openGenericServiceType, type)
                where container == null || !DecoratorHelpers.IsDecorator(container, openGenericServiceType, type)
                select type;
        }

        private static void RegisterManyForOpenGeneric(Type openGenericServiceType,
            IEnumerable<Type> typesToRegister, BatchRegistrationCallback callback)
        {
            // Make a copy of the collection for performance and correctness.
            typesToRegister = typesToRegister != null ? typesToRegister.ToArray() : null;

            Requires.CollectionDoesNotContainOpenGenericTypes(typesToRegister, "typesToRegister");

            RegisterManyForOpenGenericCore(openGenericServiceType, typesToRegister, callback);
        }

        private static void RegisterManyForOpenGenericWithOpenGenericTypes(Type openGenericServiceType,
            IEnumerable<Type> typesToRegister, BatchRegistrationCallback callback)
        {
            // Make a copy of the collection for performance and correctness.
            typesToRegister = typesToRegister != null ? typesToRegister.ToArray() : null;

            RegisterManyForOpenGenericCore(openGenericServiceType, typesToRegister, callback);
        }

        private static void RegisterManyForOpenGenericCore(Type openGenericServiceType,
            IEnumerable<Type> typesToRegister, BatchRegistrationCallback callback)
        {
            Requires.IsNotNull(typesToRegister, "typesToRegister");
            Requires.IsNotNull(openGenericServiceType, "openGenericServiceType");
            Requires.IsNotNull(callback, "callback");
            Requires.DoesNotContainNullValues(typesToRegister, "typesToRegister");
            Requires.TypeIsOpenGeneric(openGenericServiceType, "openGenericServiceType");
            Requires.ServiceIsAssignableFromImplementations(openGenericServiceType, typesToRegister, "typesToRegister");

            RegisterOpenGenericInternal(openGenericServiceType, typesToRegister, callback);
        }

        private static void RegisterOpenGenericInternal(Type openGenericType,
            IEnumerable<Type> typesToRegister, BatchRegistrationCallback callback)
        {
            var openGenericImplementations = (
                from implementation in typesToRegister
                where implementation.ContainsGenericParameters
                select implementation)
                .ToArray();

            // A single type to register can implement multiple closed versions of a open generic type, so
            // we can end up with multiple registrations per type.
            // Example: class StrangeValidator : IValidator<Person>, IValidator<Customer> { }
            var registrations =
                from implementation in typesToRegister
                where !implementation.ContainsGenericParameters
                from service in implementation.GetBaseTypesAndInterfacesFor(openGenericType)
                let registration = new { service, implementation }
                group registration by registration.service into g
                let matchingClosedGenericImplementations = 
                    GetMatchingClosedGenericTypesForOpenGenericTypes(g.Key, openGenericImplementations)
                select new
                {
                    ServiceType = g.Key,
                    Implementations = g.Select(r => r.implementation)
                        .Concat(matchingClosedGenericImplementations)
                        .ToArray()
                };

            foreach (var registration in registrations)
            {
                callback(registration.ServiceType, registration.Implementations);
            }
        }

        private static IEnumerable<Type> GetMatchingClosedGenericTypesForOpenGenericTypes(
            Type closedGenericServiceType, Type[] openGenericImplementations)
        {
            if (openGenericImplementations.Length == 0)
            {
                return Enumerable.Empty<Type>();
            }

            return
                from openGenericImplementation in openGenericImplementations
                let type = BuildClosedGenericOrNull(closedGenericServiceType, openGenericImplementation)
                where type != null
                select type;
        }
        
        private static Type BuildClosedGenericOrNull(Type closedGenericBaseType, 
            Type openGenericImplementation)
        {
            var builder = new GenericTypeBuilder(closedGenericBaseType, openGenericImplementation);

            return builder.BuildClosedGenericImplementation().ClosedGenericImplementation;
        }

        private static void RequiresSingleImplementation(Type closedServiceType, Type[] implementations)
        {
            if (implementations.Length > 1)
            {
                throw new InvalidOperationException(
                    StringResources.MultipleTypesThatRepresentClosedGenericType(closedServiceType,
                    implementations));
            }
        }
        
        private static void RegisterManyForOpenGenericInternal(this Container container, Type openGenericServiceType,
            IEnumerable<Assembly> assemblies, Lifestyle lifestyle, AccessibilityOption accessibility)
        {
            IsValidValue(accessibility, "accessibility");

            RegisterManyForOpenGenericInternal(container, openGenericServiceType, assemblies,
                lifestyle, accessibility == AccessibilityOption.AllTypes);
        }

        private static void RegisterManySinglesForOpenGenericInternal(Container container, Type openGenericServiceType,
            IEnumerable<Assembly> assemblies, AccessibilityOption accessibility)
        {
            Requires.IsNotNull(container, "container");
            IsValidValue(accessibility, "accessibility");

            RegisterManySinglesForOpenGenericInternal(container, openGenericServiceType, assemblies,
                includeInternals: accessibility == AccessibilityOption.AllTypes);
        }

        private static IEnumerable<Type> GetTypesToRegisterInternal(Container container, Type openGenericServiceType,
            IEnumerable<Assembly> assemblies, AccessibilityOption accessibility)
        {
            IsValidValue(accessibility, "accessibility");

            return GetTypesToRegisterInternal(container, openGenericServiceType, assemblies,
                includeInternals: accessibility == AccessibilityOption.AllTypes);
        }

        private static void RegisterManyForOpenGenericInternal(Container container, Type openGenericServiceType,
            IEnumerable<Assembly> assemblies, BatchRegistrationCallback callback, AccessibilityOption accessibility)
        {
            Requires.IsNotNull(container, "container");
            IsValidValue(accessibility, "accessibility");

            RegisterManyForOpenGenericInternal(container, openGenericServiceType, assemblies, callback,
                includeInternals: accessibility == AccessibilityOption.AllTypes);
        }

        private static void IsValidValue(AccessibilityOption accessibility, string paramName)
        {
            if (accessibility != AccessibilityOption.AllTypes &&
                accessibility != AccessibilityOption.PublicTypesOnly)
            {
                throw new ArgumentException(
                    StringResources.ValueIsInvalidForEnumType((int)accessibility, typeof(AccessibilityOption)),
                    paramName);
            }
        }
    }
}