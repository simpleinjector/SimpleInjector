#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015-2018 Simple Injector Contributors
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
        private readonly Dictionary<Type, List<Type>> skippedNonGenericDecorators =
            new Dictionary<Type, List<Type>>();

        /// <summary>
        /// Registers all concrete, non-generic, public and internal types in the given set of
        /// <paramref name="assemblies"/> that implement the given <paramref name="openGenericServiceType"/> 
        /// with container's default lifestyle (which is transient by default).
        /// <see cref="TypesToRegisterOptions.IncludeDecorators">Decorators</see> and
        /// <see cref="TypesToRegisterOptions.IncludeGenericTypeDefinitions">generic type definitions</see>
        /// will be excluded from registration, while 
        /// <see cref="TypesToRegisterOptions.IncludeComposites">composites</see> are included.
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
        public void Register(Type openGenericServiceType, params Assembly[] assemblies)
        {
            this.Register(openGenericServiceType, assemblies, this.SelectionBasedLifestyle);
        }

        /// <summary>
        /// Registers all concrete, non-generic, public and internal types in the given set of
        /// <paramref name="assemblies"/> that implement the given <paramref name="openGenericServiceType"/> 
        /// with container's default lifestyle (which is transient by default).
        /// <see cref="TypesToRegisterOptions.IncludeDecorators">Decorators</see> and
        /// <see cref="TypesToRegisterOptions.IncludeGenericTypeDefinitions">generic type definitions</see>
        /// will be excluded from registration, while 
        /// <see cref="TypesToRegisterOptions.IncludeComposites">composites</see> are included.
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
        /// Registers all concrete, non-generic, public and internal types in the given
        /// <paramref name="assembly"/> that implement the given <paramref name="openGenericServiceType"/> 
        /// with the supplied <paramref name="lifestyle"/>.
        /// <see cref="TypesToRegisterOptions.IncludeDecorators">Decorators</see> and
        /// <see cref="TypesToRegisterOptions.IncludeGenericTypeDefinitions">generic type definitions</see>
        /// will be excluded from registration, while 
        /// <see cref="TypesToRegisterOptions.IncludeComposites">composites</see> are included.
        /// </summary>
        /// <param name="openGenericServiceType">The definition of the open generic type.</param>
        /// <param name="assembly">An assembly that will be searched.</param>
        /// <param name="lifestyle">The lifestyle to register instances with.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments contain a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="openGenericServiceType"/> is not
        /// an open generic type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the given 
        /// <paramref name="assembly"/> contain multiple types that implement the same 
        /// closed generic version of the given <paramref name="openGenericServiceType"/>.</exception>
        public void Register(Type openGenericServiceType, Assembly assembly, Lifestyle lifestyle)
        {
            Requires.IsNotNull(assembly, nameof(assembly));

            this.Register(openGenericServiceType, new[] { assembly }, lifestyle);
        }

        /// <summary>
        /// Registers all concrete, non-generic, public and internal types in the given set of
        /// <paramref name="assemblies"/> that implement the given <paramref name="openGenericServiceType"/> 
        /// with the supplied <paramref name="lifestyle"/>.
        /// <see cref="TypesToRegisterOptions.IncludeDecorators">Decorators</see> and
        /// <see cref="TypesToRegisterOptions.IncludeGenericTypeDefinitions">generic type definitions</see>
        /// will be excluded from registration, while 
        /// <see cref="TypesToRegisterOptions.IncludeComposites">composites</see> are included.
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
        public void Register(
            Type openGenericServiceType, IEnumerable<Assembly> assemblies, Lifestyle lifestyle)
        {
            Requires.IsNotNull(openGenericServiceType, nameof(openGenericServiceType));
            Requires.IsNotNull(lifestyle, nameof(lifestyle));
            Requires.IsNotNull(assemblies, nameof(assemblies));

            Requires.IsGenericType(
                openGenericServiceType,
                nameof(openGenericServiceType),
                guidance: StringResources.SuppliedTypeIsNotGenericExplainingAlternativesWithAssemblies);

            Requires.IsNotPartiallyClosed(openGenericServiceType, nameof(openGenericServiceType));

            Requires.IsOpenGenericType(
                openGenericServiceType,
                nameof(openGenericServiceType),
                guidance: StringResources.SuppliedTypeIsNotOpenGenericExplainingAlternativesWithAssemblies);

            var results =
                this.GetNonGenericTypesToRegisterForOneToOneMapping(openGenericServiceType, assemblies);

            this.Register(openGenericServiceType, results.ImplementationTypes, lifestyle);

            this.AddSkippedDecorators(openGenericServiceType, results.SkippedDecorators);
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
        public void Register(
            Type openGenericServiceType, IEnumerable<Type> implementationTypes, Lifestyle lifestyle)
        {
            Requires.IsNotNull(openGenericServiceType, nameof(openGenericServiceType));
            Requires.IsNotNull(lifestyle, nameof(lifestyle));
            Requires.IsNotNull(implementationTypes, nameof(implementationTypes));

            Requires.IsGenericType(
                openGenericServiceType,
                nameof(openGenericServiceType),
                guidance: StringResources.SuppliedTypeIsNotGenericExplainingAlternativesWithTypes);

            Requires.IsNotPartiallyClosed(openGenericServiceType, nameof(openGenericServiceType));

            Requires.IsOpenGenericType(
                openGenericServiceType,
                paramName: nameof(openGenericServiceType),
                guidance: StringResources.SuppliedTypeIsNotOpenGenericExplainingAlternativesWithTypes);

            implementationTypes = implementationTypes.Distinct().ToArray();

            Requires.DoesNotContainNullValues(implementationTypes, nameof(implementationTypes));
            Requires.CollectionDoesNotContainOpenGenericTypes(
                implementationTypes, nameof(implementationTypes));

            Requires.ServiceIsAssignableFromImplementations(
                openGenericServiceType,
                implementationTypes,
                paramName: nameof(implementationTypes),
                typeCanBeServiceType: false);

            var mappings =
                from mapping in BatchMapping.Build(openGenericServiceType, implementationTypes)
                let registration = lifestyle.CreateRegistration(mapping.ImplementationType, this)
                from serviceType in mapping.ClosedServiceTypes
                select new { serviceType, registration };

            foreach (var mapping in mappings)
            {
                this.AddRegistration(mapping.serviceType!, mapping.registration!);
            }
        }

        /// <summary>
        /// Registers all concrete, non-generic, public and internal types in the given set of
        /// <paramref name="assemblies"/> that implement the given <paramref name="openGenericServiceType"/> 
        /// with <see cref="Lifestyle.Singleton" /> lifestyle.
        /// <see cref="TypesToRegisterOptions.IncludeDecorators">Decorators</see> and
        /// <see cref="TypesToRegisterOptions.IncludeGenericTypeDefinitions">generic type definitions</see>
        /// will be excluded from registration, while 
        /// <see cref="TypesToRegisterOptions.IncludeComposites">composites</see> are included.
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
        public void RegisterSingleton(Type openGenericServiceType, params Assembly[] assemblies)
        {
            this.RegisterSingleton(openGenericServiceType, (IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Registers all concrete, non-generic, public and internal types in the given set of
        /// <paramref name="assemblies"/> that implement the given <paramref name="openGenericServiceType"/> 
        /// with <see cref="Lifestyle.Singleton" /> lifestyle.
        /// <see cref="TypesToRegisterOptions.IncludeDecorators">Decorators</see> and
        /// <see cref="TypesToRegisterOptions.IncludeGenericTypeDefinitions">generic type definitions</see>
        /// will be excluded from registration, while 
        /// <see cref="TypesToRegisterOptions.IncludeComposites">composites</see> are included.
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
        public void RegisterSingleton(Type openGenericServiceType, IEnumerable<Assembly> assemblies)
        {
            this.Register(openGenericServiceType, assemblies, Lifestyle.Singleton);
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
        [Obsolete("Please use Container." + nameof(Container.Collection) + "." +
            nameof(ContainerCollectionRegistrator.Register) + " instead. " +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        public void RegisterCollection<TService>(IEnumerable<Assembly> assemblies) where TService : class
        {
            this.Collection.Register<TService>(assemblies);
        }

        /// <summary>
        /// Registers all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <paramref name="serviceType"/> 
        /// with a default lifestyle and register them as a collection of <paramref name="serviceType"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>. 
        /// <see cref="TypesToRegisterOptions.IncludeComposites">Composites</see>,
        /// <see cref="TypesToRegisterOptions.IncludeDecorators">decorators</see> and
        /// <see cref="TypesToRegisterOptions.IncludeGenericTypeDefinitions">generic type definitions</see>
        /// will be excluded from registration.
        /// </summary>
        /// <param name="serviceType">The element type of the collections to register. This can be either
        /// a non-generic, closed-generic or open-generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        [Obsolete("Please use Container." + nameof(Container.Collection) + "." +
            nameof(ContainerCollectionRegistrator.Register) + " instead. " +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        public void RegisterCollection(Type serviceType, params Assembly[] assemblies)
        {
            this.Collection.Register(serviceType, assemblies);
        }

        /// <summary>
        /// Registers all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <paramref name="serviceType"/> 
        /// with a default lifestyle and register them as a collection of <paramref name="serviceType"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// <see cref="TypesToRegisterOptions.IncludeComposites">Composites</see>,
        /// <see cref="TypesToRegisterOptions.IncludeDecorators">decorators</see> and
        /// <see cref="TypesToRegisterOptions.IncludeGenericTypeDefinitions">generic type definitions</see>
        /// will be excluded from registration.
        /// </summary>
        /// <param name="serviceType">The element type of the collections to register. This can be either
        /// a non-generic, closed-generic or open-generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        [Obsolete("Please use Container." + nameof(Container.Collection) + "." +
            nameof(ContainerCollectionRegistrator.Register) + " instead. " +
            "Will be treated as an error from version 5.0. " +
            "Will be removed in version 6.0.",
            error: false)]
        public void RegisterCollection(Type serviceType, IEnumerable<Assembly> assemblies)
        {
            this.Collection.Register(serviceType, assemblies);
        }

        /// <summary>
        /// Returns all concrete non-generic types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <typeparamref name="TService"/>. 
        /// </summary>
        /// <remarks>
        /// Use this method when you need influence the types that are registered using 
        /// <see cref="ContainerCollectionRegistrator.Register(Type, Assembly[])">Container.Collections.Register</see>. 
        /// The <b>Collections.Register</b> overloads that take a collection of <see cref="Assembly"/> 
        /// objects use this method internally to get the list of types that need to be registered. Instead of
        /// calling such overload, you can call an overload that takes a list of <see cref="System.Type"/> objects 
        /// and pass in a filtered result from this <b>GetTypesToRegister</b> method.
        /// <code lang="cs"><![CDATA[
        /// var container = new Container();
        /// 
        /// IEnumerable<Assembly> assemblies = new[] { typeof(ILogger).Assembly };
        /// var types = container.GetTypesToRegister<ILogger>(assemblies)
        ///     .Where(type => type.IsPublic);
        /// 
        /// container.Collections.Register<ILogger>(types);
        /// ]]></code>
        /// This example calls the <b>GetTypesToRegister</b> method to request a list of concrete implementations
        /// of the <b>ILogger</b> interface from the assembly of that interface. After that
        /// all internal types are filtered out. This list is supplied to the
        /// <see cref="ContainerCollectionRegistrator.Register{TService}(IEnumerable{Type})">Collections.Register&lt;TService&gt;(IEnumerable&lt;Type&gt;)</see>
        /// overload to finish the registration.
        /// </remarks>
        /// <typeparam name="TService">The base type or interface to find derived types for.</typeparam>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A collection of types.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments contain a null reference 
        /// (Nothing in VB).</exception>
        /// <returns>A collection of types.</returns>
        public IEnumerable<Type> GetTypesToRegister<TService>(IEnumerable<Assembly> assemblies)
        {
            return this.GetTypesToRegister(typeof(TService), assemblies, new TypesToRegisterOptions());
        }

        /// <summary>
        /// Returns all concrete non-generic types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <typeparamref name="TService"/>. 
        /// </summary>
        /// <remarks>
        /// Use this method when you need influence the types that are registered using
        /// <see cref="ContainerCollectionRegistrator.Register(Type, Assembly[])">Container.Collections.Register</see>. 
        /// The <b>Collections.Register</b> overloads that take a collection of <see cref="Assembly"/> 
        /// objects use this method internally to get the list of types that need to be registered. Instead of
        /// calling such overload, you can call an overload that takes a list of <see cref="System.Type"/> objects 
        /// and pass in a filtered result from this <b>GetTypesToRegister</b> method.
        /// <code lang="cs"><![CDATA[
        /// var container = new Container();
        /// 
        /// var types = container.GetTypesToRegister<ILogger>(
        ///     typeof(ILogger).Assembly,
        ///     typeof(FileLogger).Assembly)
        ///     .Where(type => type.IsPublic);
        /// 
        /// container.Collections.Register<ILogger>(types);
        /// ]]></code>
        /// This example calls the <b>GetTypesToRegister</b> method to request a list of concrete implementations
        /// of the <b>ILogger</b> interface from the assembly of that interface. After that
        /// all internal types are filtered out. This list is supplied to the
        /// <see cref="ContainerCollectionRegistrator.Register{TService}(IEnumerable{TService})">Container.Collections.Register&lt;TService&gt;(IEnumerable&lt;Type&gt;)</see>
        /// overload to finish the registration.
        /// </remarks>
        /// <typeparam name="TService">The base type or interface to find derived types for.</typeparam>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A collection of types.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments contain a null reference 
        /// (Nothing in VB).</exception>
        /// <returns>A collection of types.</returns>
        public IEnumerable<Type> GetTypesToRegister<TService>(params Assembly[] assemblies)
        {
            return this.GetTypesToRegister(typeof(TService), assemblies, new TypesToRegisterOptions());
        }

        /// <summary>
        /// Returns all concrete non-generic types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="serviceType"/>. 
        /// <paramref name="serviceType"/> can be an open-generic type.
        /// </summary>
        /// <remarks>
        /// Use this method when you need influence the types that are registered using 
        /// <see cref="Register(System.Type, IEnumerable{System.Reflection.Assembly})">Register</see> or
        /// <see cref="ContainerCollectionRegistrator.Register(Type, Assembly[])">Collections.Register</see>. 
        /// The <b>Register</b> overloads that take a collection of <see cref="Assembly"/> 
        /// objects use this method internally to get the list of types that need to be registered. Instead of
        /// calling such overload, you can call an overload that takes a list of <see cref="System.Type"/> objects 
        /// and pass in a filtered result from this <b>GetTypesToRegister</b> method.
        /// <code lang="cs"><![CDATA[
        /// var container = new Container();
        /// 
        /// var assemblies = new[] { typeof(ICommandHandler<>).Assembly };
        /// var types = container.GetTypesToRegister(typeof(ICommandHandler<>), assemblies)
        ///     .Where(type => type.IsPublic);
        /// 
        /// container.Register(typeof(ICommandHandler<>), types);
        /// ]]></code>
        /// This example calls the <b>GetTypesToRegister</b> method to request a list of concrete implementations
        /// of the <b>ICommandHandler&lt;T&gt;</b> interface from the assembly of that interface. After that
        /// all internal types are filtered out. This list is supplied to the
        /// <see cref="Container.Register(System.Type,IEnumerable{System.Type})">Register(Type, IEnumerable&lt;Type&gt;)</see>
        /// overload to finish the registration.
        /// </remarks>
        /// <param name="serviceType">The base type or interface to find derived types for. This can be both
        /// a non-generic and open-generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A collection of types.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments contain a null reference 
        /// (Nothing in VB).</exception>
        public IEnumerable<Type> GetTypesToRegister(Type serviceType, params Assembly[] assemblies)
        {
            return this.GetTypesToRegister(serviceType, assemblies, new TypesToRegisterOptions());
        }

        /// <summary>
        /// Returns all concrete non-generic types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="serviceType"/>. 
        /// <paramref name="serviceType"/> can be an open-generic type.
        /// </summary>
        /// <remarks>
        /// Use this method when you need influence the types that are registered using 
        /// <see cref="Register(System.Type, IEnumerable{System.Reflection.Assembly})">Register</see> or
        /// <see cref="ContainerCollectionRegistrator.Register(Type, Assembly[])">Collections.Register</see>. 
        /// The <b>Register</b> overloads that take a collection of <see cref="Assembly"/> 
        /// objects use this method internally to get the list of types that need to be registered. Instead of
        /// calling such overload, you can call an overload that takes a list of <see cref="System.Type"/> objects 
        /// and pass in a filtered result from this <b>GetTypesToRegister</b> method.
        /// <code lang="cs"><![CDATA[
        /// var container = new Container();
        /// 
        /// var assemblies = new[] { typeof(ICommandHandler<>).Assembly };
        /// var types = container.GetTypesToRegister(typeof(ICommandHandler<>), assemblies)
        ///     .Where(type => type.IsPublic);
        /// 
        /// container.Register(typeof(ICommandHandler<>), types);
        /// ]]></code>
        /// This example calls the <b>GetTypesToRegister</b> method to request a list of concrete implementations
        /// of the <b>ICommandHandler&lt;T&gt;</b> interface from the assembly of that interface. After that
        /// all internal types are filtered out. This list is supplied to the
        /// <see cref="Container.Register(System.Type,IEnumerable{System.Type})">Register(Type, IEnumerable&lt;Type&gt;)</see>
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
            return this.GetTypesToRegister(serviceType, assemblies, new TypesToRegisterOptions());
        }

        /// <summary>
        /// Returns all concrete types that are located in the supplied <paramref name="assemblies"/> 
        /// and implement or inherit from the supplied <paramref name="serviceType"/> and match the specified
        /// <paramref name="options."/>. <paramref name="serviceType"/> can be an open-generic type.
        /// </summary>
        /// <remarks>
        /// Use this method when you need influence the types that are registered using 
        /// <see cref="Register(System.Type, IEnumerable{System.Reflection.Assembly})">Register</see>. 
        /// The <b>Register</b> overloads that take a collection of <see cref="Assembly"/> 
        /// objects use this method internally to get the list of types that need to be registered. Instead of
        /// calling  such overload, you can call an overload that takes a list of <see cref="System.Type"/> objects 
        /// and pass  in a filtered result from this <b>GetTypesToRegister</b> method.
        /// <code lang="cs"><![CDATA[
        /// var container = new Container();
        /// 
        /// var assemblies = new[] { typeof(ICommandHandler<>).Assembly };
        /// var options = new TypesToRegisterOptions { IncludeGenericTypeDefinitions: true };
        /// var types = container.GetTypesToRegister(typeof(ICommandHandler<>), assemblies, options)
        ///     .Where(type => type.IsPublic);
        /// 
        /// container.Register(typeof(ICommandHandler<>), types);
        /// ]]></code>
        /// This example calls the <b>GetTypesToRegister</b> method to request a list of concrete implementations
        /// of the <b>ICommandHandler&lt;T&gt;</b> interface from the assembly of that interface. After that
        /// all internal types are filtered out. This list is supplied to the
        /// <see cref="Container.Register(System.Type,IEnumerable{System.Type})">Register(Type, IEnumerable&lt;Type&gt;)</see>
        /// overload to finish the registration.
        /// </remarks>
        /// <param name="serviceType">The base type or interface to find derived types for. This can be both
        /// a non-generic and open-generic type.</param>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <param name="options">The options.</param>
        /// <returns>A collection of types.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments contain a null reference 
        /// (Nothing in VB).</exception>
        public IEnumerable<Type> GetTypesToRegister(
            Type serviceType, IEnumerable<Assembly> assemblies, TypesToRegisterOptions options)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(assemblies, nameof(assemblies));
            Requires.IsNotNull(options, nameof(options));
            Requires.IsNotPartiallyClosed(serviceType, nameof(serviceType));

            return this.GetTypesToRegisterInternal(serviceType, assemblies, options);
        }

        private Type[] GetTypesToRegisterInternal(
            Type serviceType, IEnumerable<Assembly> assemblies, TypesToRegisterOptions options)
        {
            var types =
                from assembly in assemblies.Distinct()
                where !assembly.IsDynamic
                from type in GetTypesFromAssembly(assembly)
                where Types.IsConcreteType(type)
                where options.IncludeGenericTypeDefinitions || !type.IsGenericTypeDefinition()
                where Types.ServiceIsAssignableFromImplementation(serviceType, type)
                let ctor = this.SelectImplementationTypeConstructorOrNull(type)
                where ctor == null || options.IncludeDecorators || !Types.IsDecorator(serviceType, ctor)
                where ctor == null || options.IncludeComposites || !Types.IsComposite(serviceType, ctor)
                select type;

            return types.ToArray();
        }

        private NonGenericTypesToRegisterForOneToOneMappingResults
            GetNonGenericTypesToRegisterForOneToOneMapping(
                Type openGenericServiceType, IEnumerable<Assembly> assemblies)
        {
            var options = new TypesToRegisterOptions { IncludeDecorators = true };

            Type[] typesIncludingDecorators =
                this.GetTypesToRegisterInternal(openGenericServiceType, assemblies, options);

            var partitions =
                typesIncludingDecorators.Partition(type => !this.IsDecorator(openGenericServiceType, type));

            return new NonGenericTypesToRegisterForOneToOneMappingResults(
                implementationTypes: partitions.Item1,
                skippedDecorators: partitions.Item2);
        }

        private bool IsDecorator(Type openGenericServiceType, Type implemenationType)
        {
            var ctor = this.SelectImplementationTypeConstructorOrNull(implemenationType);
            return ctor != null && Types.IsDecorator(openGenericServiceType, ctor);
        }

        private ConstructorInfo? SelectImplementationTypeConstructorOrNull(Type implementationType)
        {
            try
            {
                return this.Options.SelectConstructor(implementationType);
            }
            catch (ActivationException)
            {
                // If the constructor resolution behavior fails, we can't determine the type's constructor.
                // Since this method is used by batch registration, by returning null the type
                // will be included in batch registration and at that point GetConstructor is called again
                // -and will fail again- giving the user the required information.
                return null;
            }
        }

        private static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (Exception ex)
            {
                // Throw a more descriptive message containing the name of the assembly.
                throw new InvalidOperationException(
                    StringResources.UnableToLoadTypesFromAssembly(assembly, ex), ex);
            }
        }

        private void AddSkippedDecorators(Type openGenericServiceType, IEnumerable<Type> nonGenericDecorators)
        {
            if (!this.skippedNonGenericDecorators.ContainsKey(openGenericServiceType))
            {
                this.skippedNonGenericDecorators[openGenericServiceType] = new List<Type>();
            }

            this.skippedNonGenericDecorators[openGenericServiceType].AddRange(nonGenericDecorators);
        }

        private Type[] GetNonGenericDecoratorsThatWereSkippedDuringBatchRegistration(Type serviceType)
        {
            if (serviceType.IsGenericType())
            {
                var typeDef = serviceType.GetGenericTypeDefinition();

                if (this.skippedNonGenericDecorators.ContainsKey(typeDef))
                {
                    return this.skippedNonGenericDecorators[typeDef]
                        .Where(t => serviceType.IsAssignableFrom(t))
                        .ToArray();
                }
            }

            return Helpers.Array<Type>.Empty;
        }

        private sealed class BatchMapping
        {
            private BatchMapping(Type implementationType, IEnumerable<Type> closedServiceTypes)
            {
                this.ImplementationType = implementationType;
                this.ClosedServiceTypes = closedServiceTypes;
            }

            internal Type ImplementationType { get; }
            internal IEnumerable<Type> ClosedServiceTypes { get; }

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
                return new BatchMapping(
                    implementationType: implementationType,
                    closedServiceTypes: implementationType.GetBaseTypesAndInterfacesFor(openServiceType));
            }

            private static void RequiresNoDuplicateRegistrations(BatchMapping[] mappings)
            {
                // Use of 'Count() > 1' instead of 'Skip(1).Any()' is not a performance problem here, and is
                // actually faster in this case, because Enumerable.GroupBy returns an instance that
                // implements ICollection<T>.
#pragma warning disable RCS1083
                var duplicateServiceTypes =
                    from mapping in mappings
                    from closedServiceType in mapping.ClosedServiceTypes
                    group mapping.ImplementationType by closedServiceType into serviceTypeGroup
                    where serviceTypeGroup.Count() > 1
                    select new
                    {
                        service = serviceTypeGroup.Key,
                        implementations = serviceTypeGroup.ToArray()
                    };

                var invalidRegistration = duplicateServiceTypes.FirstOrDefault();

                if (invalidRegistration != null)
                {
                    throw new InvalidOperationException(
                        StringResources.MultipleTypesThatRepresentClosedGenericType(
                            invalidRegistration.service!, invalidRegistration.implementations!));
                }
            }
        }

        private class NonGenericTypesToRegisterForOneToOneMappingResults
        {
            public NonGenericTypesToRegisterForOneToOneMappingResults(
                List<Type> skippedDecorators, List<Type> implementationTypes)
            {
                this.SkippedDecorators = skippedDecorators;
                this.ImplementationTypes = implementationTypes;
            }

            public List<Type> SkippedDecorators { get; }
            public List<Type> ImplementationTypes { get; }
        }
    }
}