#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2015 Simple Injector Contributors
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
    using System.Reflection;

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
    ///         container.Register(implementation, implementation, Lifestyle.Singleton);
    ///     }
    ///     
    ///     container.RegisterCollection(closedServiceType, implementations);
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
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static partial class OpenGenericBatchRegistrationExtensions
    {
        private const string ObsoleteOneToOneWithAccessibilityMessage =
            "This extension method has been removed. In case you supply AccessibilityOption.AllTypes, " +
            "please call Container.Register(Type, IEnumerable<Assembly>). In case you supply " +
            "AccessibilityOption.PublicTypesOnly, please call Container.Register(serviceType, Container." +
            "GetTypesToRegister(serviceType, IEnumerable<Assembly>).Where(t => t.IsPublic)).";

        private const string ObsoleteOneToOneWithAccessibilityAndLifestyleMessage =
            "This extension method has been removed. In case you supply AccessibilityOption.AllTypes, " +
            "please call Container.Register(Type, IEnumerable<Assembly>, Lifestyle). In case you supply " +
            "AccessibilityOption.PublicTypesOnly, please call Container.Register(serviceType, Container." +
            "GetTypesToRegister(serviceType, IEnumerable<Assembly>).Where(t => t.IsPublic), Lifestyle).";

        private const string ObsoleteCollectionWithAccessibilityMessage =
            "This extension method has been removed. In case you supply AccessibilityOption.AllTypes, " +
            "please call Container.RegisterCollection(Type, IEnumerable<Assembly>). In case you supply " +
            "AccessibilityOption.PublicTypesOnly, please call Container.RegisterCollection(serviceType, " +
            "Container.GetTypesToRegister(serviceType, IEnumerable<Assembly>).Where(t => t.IsPublic)).";

        private const string ObsoleteGetTypesToRegisterMessage =
            "This method has been replaced. Please call Container.GetTypesToRegister instead.";

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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. Please use Container.Register(Type, IEnumerable<Assembly>) instead.",
            error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, params Assembly[] assemblies)
        {
            container.Register(openGenericServiceType, assemblies);
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. Please use Container.Register(Type, IEnumerable<Assembly>) instead.",
            error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, IEnumerable<Assembly> assemblies)
        {
            container.Register(openGenericServiceType, assemblies);
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. Please use Container.Register(Type, IEnumerable<Assembly>, Lifestyle) instead.",
            error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, Lifestyle lifestyle, params Assembly[] assemblies)
        {
            container.Register(openGenericServiceType, assemblies, lifestyle);
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. Please use Container.Register(Type, IEnumerable<Assembly>, Lifestyle) instead.",
            error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, Lifestyle lifestyle, IEnumerable<Assembly> assemblies)
        {
            container.Register(openGenericServiceType, assemblies, lifestyle);
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. Please use Container.RegisterCollection(Type, IEnumerable<Assembly>) instead.",
            error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, BatchRegistrationCallback callback,
            params Assembly[] assemblies)
        {
            Requires.IsNotNull(callback, "callback");
            container.RegisterCollection(openGenericServiceType, assemblies);
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. Please use Container.RegisterCollection(Type, IEnumerable<Assembly>) instead.",
            error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, BatchRegistrationCallback callback,
            IEnumerable<Assembly> assemblies)
        {
            Requires.IsNotNull(callback, "callback");
            container.RegisterCollection(openGenericServiceType, assemblies);
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. Please use Container.Register(Type, IEnumerable<Type>) instead.",
            error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, params Type[] typesToRegister)
        {
            container.Register(openGenericServiceType, typesToRegister);
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. Please use Container.Register(Type, IEnumerable<Type>) instead.",
            error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, IEnumerable<Type> typesToRegister)
        {
            container.Register(openGenericServiceType, typesToRegister);
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. Please use Container.RegisterCollection(Type, IEnumerable<Type>) instead.",
            error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, BatchRegistrationCallback callback, IEnumerable<Type> typesToRegister)
        {
            Requires.IsNotNull(callback, "callback");
            container.RegisterCollection(openGenericServiceType, typesToRegister);
        }

        /// <summary>
        /// Registers all supplied <paramref name="typesToRegister"/> by a closed generic definition of the
        /// given <paramref name="openGenericServiceType"/> with the supplied <paramref name="lifestyle"/>.
        /// When a found type implements multiple 
        /// closed-generic versions of the given <paramref name="openGenericServiceType"/>, both closed-generic
        /// service types will return the exact same instance.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="lifestyle">The lifestyle the registrations are made in.</param>
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. Please use Container.Register(Type, IEnumerable<Type>, Lifestyle) instead.",
            error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, Lifestyle lifestyle, params Type[] typesToRegister)
        {
            container.Register(openGenericServiceType, typesToRegister, lifestyle);
        }

        /// <summary>
        /// Registers all supplied <paramref name="typesToRegister"/> by a closed generic definition of the
        /// given <paramref name="openGenericServiceType"/> with the supplied <paramref name="lifestyle"/>
        /// When a found type implements multiple 
        /// closed-generic versions of the given <paramref name="openGenericServiceType"/>, both closed-generic
        /// service types will return the exact same instance.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="lifestyle">The lifestyle the registrations are made in.</param>
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. Please use Container.Register(Type, IEnumerable<Type>, Lifestyle) instead.",
            error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, Lifestyle lifestyle, IEnumerable<Type> typesToRegister)
        {
            container.Register(openGenericServiceType, typesToRegister, lifestyle);
        }

        /// <summary>
        /// Registers  all concrete, non-generic types with the given <paramref name="accessibility"/>
        /// that are located in the given <paramref name="assemblies"/> that implement the given 
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
        /// an open generic type or when <paramref name="accessibility"/> contains an invalid value.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same closed generic 
        /// version of the given <paramref name="openGenericServiceType"/>.</exception>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(ObsoleteOneToOneWithAccessibilityMessage, error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, params Assembly[] assemblies)
        {
            var types = 
                GetTypesToRegisterInternal(container, openGenericServiceType, accessibility, assemblies);

            container.Register(openGenericServiceType, types);
        }

        /// <summary>
        /// Registers all concrete, non-generic types with the given <paramref name="accessibility"/> 
        /// that are located in the given <paramref name="assemblies"/> that implement the given 
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
        /// an open generic type or when the <paramref name="accessibility"/> argument contains an invalid
        /// value.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(ObsoleteOneToOneWithAccessibilityMessage, error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            var types = GetTypesToRegisterInternal(container, openGenericServiceType, accessibility, assemblies);

            container.Register(openGenericServiceType, types);
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(ObsoleteOneToOneWithAccessibilityAndLifestyleMessage, error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, Lifestyle lifestyle,
            params Assembly[] assemblies)
        {
            var types = GetTypesToRegisterInternal(container, openGenericServiceType, accessibility, assemblies);

            container.Register(openGenericServiceType, types, lifestyle);
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(ObsoleteOneToOneWithAccessibilityAndLifestyleMessage, error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, Lifestyle lifestyle,
            IEnumerable<Assembly> assemblies)
        {
            var types = GetTypesToRegisterInternal(container, openGenericServiceType, accessibility, assemblies);

            container.Register(openGenericServiceType, types, lifestyle);
        }

        /// <summary>
        /// Allows registration of all concrete, non-generic types with the given 
        /// <paramref name="accessibility"/> that are located in the given set of <paramref name="assemblies"/> 
        /// that implement the given <paramref name="openGenericServiceType"/>, by supplying a 
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
        /// an open generic type or when <paramref name="accessibility"/> contains an invalid value.</exception>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(ObsoleteCollectionWithAccessibilityMessage, error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, BatchRegistrationCallback callback,
            params Assembly[] assemblies)
        {
            Requires.IsNotNull(callback, "callback");

            var types = GetTypesToRegisterInternal(container, openGenericServiceType, accessibility, assemblies);

            container.RegisterCollection(openGenericServiceType, types);
        }

        /// <summary>
        /// Allows registration of all concrete, non-generic types with the given 
        /// <paramref name="accessibility"/> that are located in the given set of <paramref name="assemblies"/> 
        /// that implement the given <paramref name="openGenericServiceType"/>, by supplying a 
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
        /// an open generic type or when <paramref name="accessibility"/> contains an invalid value.</exception>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(ObsoleteCollectionWithAccessibilityMessage, error: true)]
        public static void RegisterManyForOpenGeneric(this Container container,
            Type openGenericServiceType, AccessibilityOption accessibility, BatchRegistrationCallback callback,
            IEnumerable<Assembly> assemblies)
        {
            Requires.IsNotNull(callback, "callback");

            var types = GetTypesToRegisterInternal(container, openGenericServiceType, accessibility, assemblies);

            container.RegisterCollection(openGenericServiceType, types);
        }

        /// <summary>
        /// Returns all types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="openGenericServiceType"/>.
        /// </summary>
        /// <remarks>
        /// Use this method when you need influence the types that are registered using 
        /// <see cref="RegisterManyForOpenGeneric(Container, System.Type, System.Reflection.Assembly[])">RegisterManyForOpenGeneric</see>. 
        /// The <b>RegisterManyForOpenGeneric</b> overloads that take a collection of <see cref="Assembly"/> 
        /// objects use this method to get the list of types that need to be registered. Instead of calling 
        /// such overload, you can call an overload that takes a list of <see cref="System.Type"/> objects and pass 
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
        /// <see cref="RegisterManyForOpenGeneric(Container, System.Type, System.Reflection.Assembly[])">RegisterManyForOpenGeneric</see>
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(ObsoleteGetTypesToRegisterMessage, error: true)]
        public static IEnumerable<Type> GetTypesToRegister(Container container, Type openGenericServiceType,
            AccessibilityOption accessibility, params Assembly[] assemblies)
        {
            return GetTypesToRegisterInternal(container, openGenericServiceType, accessibility, assemblies);
        }

        /// <summary>
        /// Returns all types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="openGenericServiceType"/>.
        /// </summary>
        /// <remarks>
        /// Use this method when you need influence the types that are registered using 
        /// <see cref="RegisterManyForOpenGeneric(Container, System.Type, System.Reflection.Assembly[])">RegisterManyForOpenGeneric</see>.
        /// The <b>RegisterManyForOpenGeneric</b> overloads that take a collection of <see cref="Assembly"/> 
        /// objects use this method to get the list of types that need to be registered. Instead of calling 
        /// such overload, you can call an overload that takes a list of <see cref="System.Type"/> objects and pass 
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
        /// <see cref="RegisterManyForOpenGeneric(Container, System.Type, System.Reflection.Assembly[])">RegisterManyForOpenGeneric</see>
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(ObsoleteGetTypesToRegisterMessage, error: true)]
        public static IEnumerable<Type> GetTypesToRegister(Container container, Type openGenericServiceType,
            AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            return GetTypesToRegisterInternal(container, openGenericServiceType, accessibility, assemblies);
        }

        /// <summary>
        /// Returns all public and internal types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="openGenericServiceType"/>.
        /// Types that are considered to be decorators are not returned.
        /// </summary>
        /// <remarks>
        /// Use this method when you need influence the types that are registered using 
        /// <see cref="RegisterManyForOpenGeneric(Container, System.Type, System.Reflection.Assembly[])">RegisterManyForOpenGeneric</see>. 
        /// The <b>RegisterManyForOpenGeneric</b> overloads that take a collection of <see cref="Assembly"/> 
        /// objects use this method to get the list of types that need to be registered. Instead of calling 
        /// such overload, you can call an overload that takes a list of <see cref="System.Type"/> objects and pass 
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
        /// <see cref="RegisterManyForOpenGeneric(Container, System.Type, System.Reflection.Assembly[])">RegisterManyForOpenGeneric</see>
        /// overload that takes a list of types to finish the
        /// registration.
        /// </remarks>
        /// <param name="container">The container to use.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A list of types.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="openGenericServiceType"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(ObsoleteGetTypesToRegisterMessage, error: true)]
        public static IEnumerable<Type> GetTypesToRegister(Container container, Type openGenericServiceType,
            params Assembly[] assemblies)
        {
            return GetTypesToRegisterInternal(container, openGenericServiceType, AccessibilityOption.AllTypes, 
                assemblies);
        }

        /// <summary>
        /// Returns all public and internal types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="openGenericServiceType"/>.
        /// Types that are considered to be decorators are not returned.
        /// </summary>
        /// <remarks>
        /// Use this method when you need influence the types that are registered using 
        /// <see cref="RegisterManyForOpenGeneric(Container, System.Type, System.Reflection.Assembly[])">RegisterManyForOpenGeneric</see>. 
        /// The <b>RegisterManyForOpenGeneric</b> overloads that take a collection of <see cref="Assembly"/> 
        /// objects use this method to get the list of types that need to be registered. Instead of calling 
        /// such overload, you can call an overload that takes a list of <see cref="System.Type"/> objects and pass 
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
        /// <see cref="RegisterManyForOpenGeneric(Container, System.Type, System.Reflection.Assembly[])">RegisterManyForOpenGeneric</see>
        /// overload that takes a list of types to finish the registration.
        /// </remarks>
        /// <param name="container">The container to use.</param>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A list of types.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="openGenericServiceType"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(ObsoleteGetTypesToRegisterMessage, error: true)]
        public static IEnumerable<Type> GetTypesToRegister(Container container, Type openGenericServiceType,
            IEnumerable<Assembly> assemblies)
        {
            return GetTypesToRegisterInternal(container, openGenericServiceType, AccessibilityOption.AllTypes, 
                assemblies);
        }

        private static IEnumerable<Type> GetTypesToRegisterInternal(Container container, 
            Type openGenericServiceType, AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            Requires.IsValidEnum(accessibility, "accessibility");

            return container.GetTypesToRegister(openGenericServiceType, assemblies)
                .Where(GetAccessibilityFilter(accessibility));
        }

        private static Func<Type, bool> GetAccessibilityFilter(AccessibilityOption accessibility)
        {
            if (accessibility == AccessibilityOption.AllTypes)
            {
                return type => true;
            }
            else
            {
                return type => type.IsPublic;
            }
        }
    }
}