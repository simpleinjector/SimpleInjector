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
    using System.Diagnostics.CodeAnalysis;
    using SimpleInjector;

    /// <summary>
    /// Provides a set of static (Shared in Visual Basic) methods for registration of open generic service
    /// types in the <see cref="Container"/>.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [ExcludeFromCodeCoverage]
    public static class OpenGenericRegistrationExtensions
    {
        /// <summary>
        /// Registers that a new instance of <paramref name="openGenericImplementation"/> will be returned 
        /// every time a <paramref name="openGenericServiceType"/> is requested.
        /// </summary>
        /// <example>
        /// Please see the 
        /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric(Container,Type,Type,Lifestyle)">RegisterOpenGeneric(Container,Type,Type,Lifestyle)</see>
        /// overload for an example.
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances.</param>
        /// <param name="openGenericImplementation">The definition of the open generic implementation type
        /// that will be returned when a <paramref name="openGenericServiceType"/> is requested.</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. " +
            "Please use Container.Register(Type, Type) to register a generic type. In case this " +
            "registration acts as fallback registration (in case an explicit registration is missing), " +
            "please use Container.RegisterConditional(Type, Type, c => !c.Handled) instead.",
            error: true)]
        public static void RegisterOpenGeneric(this Container container,
            Type openGenericServiceType, Type openGenericImplementation)
        {
            container.RegisterConditional(openGenericServiceType, openGenericImplementation, Fallback);
        }

        /// <summary>
        /// Registers that the same instance of <paramref name="openGenericImplementation"/> will be returned 
        /// every time a <paramref name="openGenericServiceType"/> is requested.
        /// </summary>
        /// <example>
        /// Please see the 
        /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric(Container,Type,Type,Lifestyle)">RegisterOpenGeneric(Container,Type,Type,Lifestyle)</see>
        /// overload for an example.
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances..</param>
        /// <param name="openGenericImplementation">The definition of the open generic implementation type
        /// that will be returned when a <paramref name="openGenericServiceType"/> is requested.</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. " +
            "Please use Container.Register(Type, Type, Lifestyle.Singleton) to register a generic type. In " +
            "case this registration acts as fallback registration (in case an explicit registration is " +
            "missing), please use Container.RegisterConditional(Type, Type, Lifestyle.Singleton, c => !c.Handled) " +
            "instead.",
            error: true)]
        public static void RegisterSingleOpenGeneric(this Container container,
            Type openGenericServiceType, Type openGenericImplementation)
        {
            container.RegisterConditional(openGenericServiceType, openGenericImplementation,
                Lifestyle.Singleton, Fallback);
        }

        /// <summary>
        /// Registers that a closed generic instance of the supplied 
        /// <paramref name="openGenericImplementation"/> will be returned when a closed generic version of
        /// the <paramref name="openGenericServiceType"/> is requested. The instance will be cached 
        /// according to the specified <paramref name="lifestyle"/>.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances.</param>
        /// <param name="openGenericImplementation">The definition of the open generic implementation type
        /// that will be returned when a <paramref name="openGenericServiceType"/> is requested.</param>
        /// <param name="lifestyle">The lifestyle that defines how returned instances are cached.</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. " +
            "Please use Container.Register(Type, Type, Lifestyle) to register a generic type. In case this " +
            "registration acts as fallback registration (in case an explicit registration is missing), " +
            "please use Container.RegisterConditional(Type, Type, Lifestyle, c => !c.Handled) " +
            "instead.",
            error: true)]
        public static void RegisterOpenGeneric(this Container container,
            Type openGenericServiceType, Type openGenericImplementation, Lifestyle lifestyle)
        {
            container.RegisterConditional(openGenericServiceType, openGenericImplementation, lifestyle, Fallback);
        }

        /// <summary>
        /// Registers that the same instance of <paramref name="openGenericImplementationType"/> will be returned 
        /// every time a <paramref name="openGenericServiceType"/> is requested.
        /// </summary>
        /// <example>
        /// Please see the 
        /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric(Container,Type,Type,Lifestyle)">RegisterOpenGeneric(Container,Type,Type,Lifestyle)</see>
        /// overload for an example.
        /// </example>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances..</param>
        /// <param name="openGenericImplementationType">The definition of the open generic implementation type
        /// that will be returned when a <paramref name="openGenericServiceType"/> is requested.</param>
        /// <param name="lifestyle">The lifestyle that defines how returned instances are cached.</param>
        /// <param name="predicate">The predicate that determines whether the 
        /// <paramref name="openGenericImplementationType"/> can implement the service type.</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. " +
            "Please use Container.RegisterConditional(Type, Type, Lifestyle, Predicate<PredicateContext>) " + 
            "to make conditional registrations. In case this registration acts as fallback registration, " +
            "add the !c.Handled check to the supplied predicate. For instance: " +
            "Container.RegisterConditional(Type, Type, Lifestyle, c => !c.Handled && yourOriginalPredicate).",
            error: true)]
        public static void RegisterOpenGeneric(this Container container,
            Type openGenericServiceType, Type openGenericImplementationType, Lifestyle lifestyle,
            Predicate<PredicateContext> predicate)
        {
            container.RegisterConditional(openGenericServiceType, openGenericImplementationType, lifestyle, 
                c => Fallback(c) && predicate(c));
        }

        /// <summary>This method has been removed.</summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances.</param>
        /// <param name="openGenericImplementations">The list of open generic implementation types
        /// that will be returned when a collection of <paramref name="openGenericServiceType"/> is requested.
        /// </param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete(
            "This extension method has been removed. " +
            "Please use Container.RegisterCollection(Type, IEnumerable<Type>) instead.",
            error: true)]
        public static void RegisterAllOpenGeneric(this Container container,
            Type openGenericServiceType, params Type[] openGenericImplementations)
        {
            RegisterAllOpenGeneric(container, openGenericServiceType,
                (IEnumerable<Type>)openGenericImplementations);
        }

        /// <summary>This method has been removed.</summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances.</param>
        /// <param name="openGenericImplementations">The list of open generic implementation types
        /// that will be returned when a collection of <paramref name="openGenericServiceType"/> is requested.
        /// </param>
        [Obsolete(
            "This extension method has been removed. " +
            "Please use Container.RegisterCollection(Type, Type[]) instead.",
            error: true)]
        public static void RegisterAllOpenGeneric(this Container container,
            Type openGenericServiceType, IEnumerable<Type> openGenericImplementations)
        {
            Requires.IsNotNull(container, nameof(container));

            RegisterAllOpenGeneric(container, openGenericServiceType, container.SelectionBasedLifestyle,
                (IEnumerable<Type>)openGenericImplementations);
        }

        /// <summary>This method has been removed.</summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances.</param>
        /// <param name="lifestyle">The lifestyle that defines how returned instances are cached.</param>
        /// <param name="openGenericImplementations">The list of open generic implementation types
        /// that will be returned when a collection of <paramref name="openGenericServiceType"/> is requested.
        /// </param>
        [Obsolete(
            "This extension method has been removed. " +
            "Please use Container.RegisterCollection(Type, Type[]) instead.",
            error: true)]
        public static void RegisterAllOpenGeneric(this Container container,
          Type openGenericServiceType, Lifestyle lifestyle, params Type[] openGenericImplementations)
        {
            RegisterAllOpenGeneric(container, openGenericServiceType, lifestyle,
                (IEnumerable<Type>)openGenericImplementations);
        }

        /// <summary>This method has been removed.</summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="openGenericServiceType">The definition of the open generic service type that can be 
        /// used to retrieve instances.</param>
        /// <param name="lifestyle">The lifestyle that defines how returned instances are cached.</param>
        /// <param name="openGenericImplementations">The list of open generic implementation types
        /// that will be returned when a collection of <paramref name="openGenericServiceType"/> is requested.
        /// </param>
        [Obsolete(
            "This extension method has been removed. " +
            "Please use Container.RegisterCollection(Type, Type[]) instead.",
            error: true)]
        public static void RegisterAllOpenGeneric(this Container container,
            Type openGenericServiceType, Lifestyle lifestyle, IEnumerable<Type> openGenericImplementations)
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(openGenericServiceType, nameof(openGenericServiceType));
            Requires.IsNotNull(lifestyle, nameof(lifestyle));
            Requires.IsNotNull(openGenericImplementations, nameof(openGenericImplementations));

            throw new InvalidOperationException("This extension method has been removed. " +
                "Please use one of the Container.RegisterCollection() overloads instead.");
        }

        private static bool Fallback(PredicateContext context) => !context.Handled;
    }
}