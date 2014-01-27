#region Copyright (c) 2013 Simple Injector Contributors
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

    /// <summary>Defines the accessibility of the types to search.</summary>
    /// <remarks>This type is not available in Silverlight.</remarks>
    public enum AccessibilityOption
    {
        /// <summary>Load both public as internal types from the given assemblies.</summary>
        AllTypes = 0,

        /// <summary>Only load publicly exposed types from the given assemblies.</summary>
        PublicTypesOnly = 1,
    }

#if !PUBLISH
    /// <summary>Behavior for the full .NET version of Simple Injector.</summary>
#endif
    public static partial class OpenGenericBatchRegistrationExtensions
    {
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
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same closed generic 
        /// version of the given <paramref name="openGenericServiceType"/>.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
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
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
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
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same closed generic 
        /// version of the given <paramref name="openGenericServiceType"/>.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
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
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
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
        /// an open generic type.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "container",
            Justification = @"
                By using the 'this Container' argument, we allow this extension method to show when using 
                Intellisense over the Container.")]
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
        /// an open generic type.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "container",
            Justification = @"
                By using the 'this Container' argument, we allow this extension method to show when using 
                Intellisense over the Container.")]
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
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same closed generic 
        /// version of the given <paramref name="openGenericServiceType"/>.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
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
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given set of 
        /// <paramref name="assemblies"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
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
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
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
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
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
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
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
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when 
        /// <paramref name="accessibility"/> contains an invalid value.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="openGenericServiceType"/>, or 
        /// <paramref name="assemblies"/> contain a null reference (Nothing in VB).</exception>
        public static IEnumerable<Type> GetTypesToRegister(Container container, Type openGenericServiceType,
            AccessibilityOption accessibility, IEnumerable<Assembly> assemblies)
        {
            return GetTypesToRegisterInternal(container, openGenericServiceType, assemblies, accessibility);
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