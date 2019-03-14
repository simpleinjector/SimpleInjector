#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2018-2019 Simple Injector Contributors
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using SimpleInjector.Internals;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Contains methods for registering and creating collections in the <see cref="SimpleInjector.Container"/>.
    /// </summary>
    public sealed class ContainerCollectionRegistrator
    {
        internal ContainerCollectionRegistrator(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            this.Container = container;
        }

        /// <summary>
        /// Gets the <see cref="SimpleInjector.Container"/> for this instance.
        /// </summary>
        /// <value>The <see cref="SimpleInjector.Container"/> for this instance.</value>
        public Container Container { get; }

        /// <summary>
        /// Creates a collection of 
        /// all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <typeparamref name="TService"/>
        /// with a default lifestyle and register them as a collection of <typeparamref name="TService"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// </summary>
        /// <typeparam name="TService">The element type of the collections to register.</typeparam>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        /// <returns>A collection that acts as stream, and calls back into the container to resolve instances
        /// every time the collection is enumerated.</returns>
        public IList<TService> Create<TService>(params Assembly[] assemblies) where TService : class
        {
            return this.Create<TService>((IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Creates a collection of 
        /// all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <typeparamref name="TService"/>
        /// with a default lifestyle and register them as a collection of <typeparamref name="TService"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// </summary>
        /// <typeparam name="TService">The element type of the collections to register.</typeparam>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        /// <returns>A collection that acts as stream, and calls back into the container to resolve instances
        /// every time the collection is enumerated.</returns>
        public IList<TService> Create<TService>(IEnumerable<Assembly> assemblies) where TService : class
        {
            Requires.IsNotNull(assemblies, nameof(assemblies));

            var compositesExcluded = new TypesToRegisterOptions { IncludeComposites = false };
            var types = this.Container.GetTypesToRegister(typeof(TService), assemblies, compositesExcluded);

            return this.Create<TService>(types);
        }

        /// <summary>
        /// Creates a collection of <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated.
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A collection that acts as stream, and calls back into the container to resolve instances
        /// every time the collection is enumerated.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceTypes"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <typeparamref name="TService"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        public IList<TService> Create<TService>(params Type[] serviceTypes) where TService : class
        {
            return this.Create<TService>((IEnumerable<Type>)serviceTypes);
        }

        /// <summary>
        /// Creates a collection of <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated.
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A collection that acts as stream, and calls back into the container to resolve instances
        /// every time the collection is enumerated.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceTypes"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <typeparamref name="TService"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        public IList<TService> Create<TService>(IEnumerable<Type> serviceTypes) where TService : class
        {
            return this.CreateInternal<TService>(serviceTypes);
        }

        /// <summary>
        /// Creates a collection of <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A collection that acts as stream, and calls back into the container to resolve instances
        /// every time the collection is enumerated.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="registrations"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="registrations"/> contains a null
        /// (Nothing in VB) element or when <typeparamref name="TService"/> is not assignable from any of the
        /// types supplied by the given <paramref name="registrations"/> instances.
        /// </exception>
        public IList<TService> Create<TService>(params Registration[] registrations) where TService : class
        {
            return this.Create<TService>((IEnumerable<Registration>)registrations);
        }

        /// <summary>
        /// Creates a collection of <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A collection that acts as stream, and calls back into the container to resolve instances
        /// every time the collection is enumerated.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="registrations"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="registrations"/> contains a null
        /// (Nothing in VB) element or when <typeparamref name="TService"/> is not assignable from any of the
        /// types supplied by the given <paramref name="registrations"/> instances.
        /// </exception>
        public IList<TService> Create<TService>(IEnumerable<Registration> registrations)
            where TService : class
        {
            return this.CreateInternal<TService>(registrations);
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of a collection of 
        /// all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <typeparamref name="TService"/>
        /// with a default lifestyle and register them as a collection of <typeparamref name="TService"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// The collection's instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        public Registration CreateRegistration<TService>(params Assembly[] assemblies) where TService : class
        {
            return this.CreateRegistration<TService>((IEnumerable<Assembly>)assemblies);
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of a collection of 
        /// all concrete, non-generic types (both public and internal) that are defined in the given
        /// set of <paramref name="assemblies"/> and that implement the given <typeparamref name="TService"/>
        /// with a default lifestyle and register them as a collection of <typeparamref name="TService"/>.
        /// Unless overridden using a custom 
        /// <see cref="ContainerOptions.LifestyleSelectionBehavior">LifestyleSelectionBehavior</see>, the
        /// default lifestyle is <see cref="Lifestyle.Transient">Transient</see>.
        /// The collection's instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="assemblies">A list of assemblies that will be searched.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments contain a null
        /// reference (Nothing in VB).</exception>
        public Registration CreateRegistration<TService>(IEnumerable<Assembly> assemblies)
            where TService : class
        {
            Requires.IsNotNull(assemblies, nameof(assemblies));

            var compositesExcluded = new TypesToRegisterOptions { IncludeComposites = false };
            var types = this.Container.GetTypesToRegister(typeof(TService), assemblies, compositesExcluded);

            return this.CreateRegistration<TService>(types);
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of a collection of 
        /// <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceTypes"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <typeparamref name="TService"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        public Registration CreateRegistration<TService>(params Type[] serviceTypes) where TService : class
        {
            return this.CreateRegistration<TService>((IEnumerable<Type>)serviceTypes);
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of a collection of 
        /// <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceTypes"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <typeparamref name="TService"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        public Registration CreateRegistration<TService>(IEnumerable<Type> serviceTypes)
            where TService : class
        {
            // This might seem backwards, but CreateInternal<T> also creates a Producer/Registration pair to
            // ensure the collection is correctly verified, and we can make use of this existing pair.
            return this.CreateInternal<TService>(serviceTypes).ParentProducer.Registration;
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of a collection of 
        /// <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="registrations"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="registrations"/> contains a null
        /// (Nothing in VB) element or when <typeparamref name="TService"/> is not assignable from any of the
        /// types supplied by the given <paramref name="registrations"/> instances.
        /// </exception>
        public Registration CreateRegistration<TService>(params Registration[] registrations)
            where TService : class
        {
            return this.CreateRegistration<TService>((IEnumerable<Registration>)registrations);
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of a collection of 
        /// <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the returned collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="registrations"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="registrations"/> contains a null
        /// (Nothing in VB) element or when <typeparamref name="TService"/> is not assignable from any of the
        /// types supplied by the given <paramref name="registrations"/> instances.
        /// </exception>
        public Registration CreateRegistration<TService>(IEnumerable<Registration> registrations)
            where TService : class
        {
            return this.CreateInternal<TService>(registrations).ParentProducer.Registration;
        }

        /// <summary>
        /// Allows appending new registrations to existing registrations made using one of the
        /// <b>Collections.Register</b> overloads.
        /// </summary>
        /// <param name="serviceType">The service type of the collection.</param>
        /// <param name="registration">The registration to append.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceType"/> is not a
        /// reference type, is open generic, or ambiguous.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the container is locked.</exception>
        /// <exception cref="NotSupportedException">Thrown when the method is called for a registration
        /// that is made with one of the <b>Collections.Register</b> overloads that accepts a dynamic collection
        /// (an <b>IEnumerable</b> or <b>IEnumerable&lt;TService&gt;</b>).</exception>
        [Obsolete("Please use Container." + nameof(SimpleInjector.Container.Collection) + "." +
            nameof(ContainerCollectionRegistrator.Append) + " instead.", error: false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public void AppendTo(Type serviceType, Registration registration)
        {
            this.Append(serviceType, registration);
        }

        /// <summary>
        /// Allows appending new registrations to existing registrations made using one of the
        /// <b>Collections.Register</b> overloads.
        /// </summary>
        /// <param name="serviceType">The service type of the collection.</param>
        /// <param name="implementationType">The implementation type to append.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceType"/> is not a
        /// reference type, or ambiguous.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the container is locked.</exception>
        /// <exception cref="NotSupportedException">Thrown when the method is called for a registration
        /// that is made with one of the <b>Collections.Register</b> overloads that accepts a dynamic collection
        /// (an <b>IEnumerable</b> or <b>IEnumerable&lt;TService&gt;</b>).</exception>
        [Obsolete("Please use Container." + nameof(SimpleInjector.Container.Collection) + "." +
            nameof(ContainerCollectionRegistrator.Append) + " instead.", error: false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public void AppendTo(Type serviceType, Type implementationType)
        {
            this.Append(serviceType, implementationType);
        }

        /// <summary>
        /// Appends a new <paramref name="registration"/> to existing registrations made using one of the
        /// <see cref="Register(Type, IEnumerable{Type})">Container.Collections.Register</see>
        /// overloads.
        /// </summary>
        /// <param name="serviceType">The service type of the collection.</param>
        /// <param name="registration">The registration to append.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceType"/> is not a
        /// reference type, is open generic, or ambiguous.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the container is locked.</exception>
        /// <exception cref="NotSupportedException">Thrown when the method is called for a registration
        /// that is made with one of the <b>Collections.Register</b> overloads that accepts a dynamic collection
        /// (an <b>IEnumerable</b> or <b>IEnumerable&lt;TService&gt;</b>).</exception>
        public void Append(Type serviceType, Registration registration)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(registration, nameof(registration));

            Requires.IsReferenceType(serviceType, nameof(serviceType));
            Requires.IsNotAnAmbiguousType(serviceType, nameof(serviceType));

            Requires.IsRegistrationForThisContainer(this.Container, registration, nameof(registration));
            Requires.ServiceOrItsGenericTypeDefinitionIsAssignableFromImplementation(serviceType,
                registration.ImplementationType, nameof(registration));

            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(serviceType, new[] { registration },
                nameof(registration));

            this.AppendToCollectionInternal(serviceType, registration);
        }

        /// <summary>
        /// Appends a new registration of <typeparamref name="TImplementation"/> to existing registrations 
        /// made for a collection of <typeparamref name="TService"/> elements using one of the 
        /// <see cref="Register(Type, IEnumerable{Type})">Container.Collections.Register</see>
        /// overloads.
        /// </summary>
        /// <typeparam name="TService">The element type of the collections to register.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be appended as registration to the
        /// collection.</typeparam>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TService"/>is ambiguous.
        /// </exception>
        public void Append<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));

            this.AppendToCollectionInternal(typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// Appends a new registration of <typeparamref name="TImplementation"/> to existing registrations 
        /// made for a collection of <typeparamref name="TService"/> elements using one of the 
        /// <see cref="Register(Type, IEnumerable{Type})">Container.Collections.Register</see>
        /// overloads with the given <paramref name="lifestyle"/>.
        /// </summary>
        /// <typeparam name="TService">The element type of the collections to register.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be appended as registration to the
        /// collection.</typeparam>
        /// <param name="lifestyle">The lifestyle that specifies how the returned instance will be cached.</param>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TService"/>is ambiguous.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="lifestyle"/> is a null reference.
        /// </exception>
        public void Append<TService, TImplementation>(Lifestyle lifestyle)
            where TImplementation : class, TService
            where TService : class
        {
            Requires.IsNotNull(lifestyle, nameof(lifestyle));
            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));

            this.AppendToCollectionInternal(typeof(TService),
                lifestyle.CreateRegistration<TImplementation>(this.Container));
        }

        /// <summary>
        /// Appends a new registration to existing registrations made for a collection of 
        /// <paramref name="serviceType"/> elements using one of the 
        /// <see cref="Register(Type, IEnumerable{Type})">Container.Collections.Register</see>
        /// overloads.
        /// </summary>
        /// <param name="serviceType">The service type of the collection.</param>
        /// <param name="implementationType">The implementation type to append.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceType"/> is not a
        /// reference type, or ambiguous.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the container is locked.</exception>
        /// <exception cref="NotSupportedException">Thrown when the method is called for a registration
        /// that is made with one of the <b>Collections.Register</b> overloads that accepts a dynamic collection
        /// (an <b>IEnumerable</b> or <b>IEnumerable&lt;TService&gt;</b>).</exception>
        public void Append(Type serviceType, Type implementationType)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(implementationType, nameof(implementationType));

            Requires.IsReferenceType(serviceType, nameof(serviceType));
            Requires.IsNotAnAmbiguousType(serviceType, nameof(serviceType));

            Requires.ServiceOrItsGenericTypeDefinitionIsAssignableFromImplementation(serviceType,
                implementationType, nameof(implementationType));

            Requires.OpenGenericTypeDoesNotContainUnresolvableTypeArguments(
                serviceType, implementationType, nameof(implementationType));

            this.AppendToCollectionInternal(serviceType, implementationType);
        }

        /// <summary>
        /// Appends the specified delegate <paramref name="instanceCreator"/> to existing registrations
        /// made for a collection of <typeparamref name="TService"/> elements using one of the 
        /// <see cref="Register(Type, IEnumerable{Type})">Container.Collections.Register</see>
        /// overloads.
        /// </summary>
        /// <typeparam name="TService">The element type of the collections to register.</typeparam>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <param name="lifestyle">The lifestyle that specifies how the returned instance will be cached.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TService"/> is not a
        /// reference type, or ambiguous.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the container is locked.</exception>
        /// <exception cref="NotSupportedException">Thrown when the method is called for a registration
        /// that is made with one of the <b>Collections.Register</b> overloads that accepts a dynamic collection
        /// (an <b>IEnumerable</b> or <b>IEnumerable&lt;TService&gt;</b>).</exception>
        public void Append<TService>(Func<TService> instanceCreator, Lifestyle lifestyle)
            where TService : class
        {
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));
            Requires.IsNotNull(lifestyle, nameof(lifestyle));
            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));

            this.AppendToCollectionInternal(typeof(TService),
                lifestyle.CreateRegistration(instanceCreator, this.Container));
        }

        /// <summary>
        /// Appends a single instance to existing registrations made for a collection of 
        /// <typeparamref name="TService"/> elements using one of the 
        /// <see cref="Register(Type, IEnumerable{Type})">Container.Collections.Register</see>
        /// overloads. This <paramref name="instance"/> must be thread-safe when working in a multi-threaded
        /// environment.
        /// <b>NOTE:</b> Do note that instances supplied by this method <b>NEVER</b> get disposed by the
        /// container, since the instance is assumed to outlive this container instance. If disposing is
        /// required, use 
        /// <see cref="Container.RegisterSingleton{TService}(Func{TService})">RegisterSingleton&lt;TService&gt;(Func&lt;TService&gt;)</see>.
        /// </summary>
        /// <typeparam name="TService">The element type of the collections to register.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TService"/> is ambiguous.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is a null reference.
        /// </exception>
        public void AppendInstance<TService>(TService instance) where TService : class
        {
            Requires.IsNotNull(instance, nameof(instance));
            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));

            this.AppendToCollectionInternal(typeof(TService),
                SingletonLifestyle.CreateSingleInstanceRegistration(typeof(TService), instance, this.Container));
        }

        /// <summary>
        /// Appends a single instance to existing registrations made for a collection of 
        /// <paramref name="serviceType"/> elements using one of the 
        /// <see cref="Register(Type, IEnumerable{Type})">Container.Collections.Register</see>
        /// overloads. This <paramref name="instance"/> must be thread-safe when working in a multi-threaded
        /// environment.
        /// <b>NOTE:</b> Do note that instances supplied by this method <b>NEVER</b> get disposed by the
        /// container, since the instance is assumed to outlive this container instance. If disposing is
        /// required, use 
        /// <see cref="Container.RegisterSingleton{TService}(Func{TService})">RegisterSingleton&lt;TService&gt;(Func&lt;TService&gt;)</see>.
        /// </summary>
        /// <param name="serviceType">TThe element type of the collections to register.</param>
        /// <param name="instance">The instance to register.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceType"/> is ambiguous, or
        /// <paramref name="instance"/> does not implement <paramref name="serviceType"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="serviceType"/> or 
        /// <paramref name="instance"/> are null references.
        /// </exception>
        public void AppendInstance(Type serviceType, object instance)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(instance, nameof(instance));
            Requires.IsReferenceType(serviceType, nameof(serviceType));
            Requires.IsNotAnAmbiguousType(serviceType, nameof(serviceType));

            Requires.ServiceOrItsGenericTypeDefinitionIsAssignableFromImplementation(
                serviceType, instance.GetType(), nameof(instance));

            // We need to build a registration using the actual instance type because serviceType
            // is allowed to be a generic type definition.
            this.AppendToCollectionInternal(
                serviceType,
                SingletonLifestyle.CreateSingleInstanceRegistration(
                    instance.GetType(), instance, this.Container));
        }

        /// <summary>
        /// Registers a dynamic (container uncontrolled) collection of elements of type 
        /// <typeparamref name="TService"/>. A call to <see cref="Container.GetAllInstances{T}"/> will return the 
        /// <paramref name="containerUncontrolledCollection"/> itself, and updates to the collection will be 
        /// reflected in the result. If updates are allowed, make sure the collection can be iterated safely 
        /// if you're running a multi-threaded application.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="containerUncontrolledCollection">The container-uncontrolled collection to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when a <paramref name="containerUncontrolledCollection"/>
        /// for <typeparamref name="TService"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="containerUncontrolledCollection"/> is a null
        /// reference.</exception>
        public void Register<TService>(IEnumerable<TService> containerUncontrolledCollection)
            where TService : class
        {
            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));
            Requires.IsNotNull(containerUncontrolledCollection, nameof(containerUncontrolledCollection));

            this.RegisterContainerUncontrolledCollection(typeof(TService), containerUncontrolledCollection);
        }

        /// <summary>
        /// Registers a collection of singleton elements of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="singletons">The collection to register.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when a <paramref name="singletons"/>
        /// for <typeparamref name="TService"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="singletons"/> is a null
        /// reference.</exception>
        /// <exception cref="ArgumentException">Thrown when one of the elements of <paramref name="singletons"/>
        /// is a null reference.</exception>
        public void Register<TService>(params TService[] singletons) where TService : class
        {
            Requires.IsNotNull(singletons, nameof(singletons));
            Requires.DoesNotContainNullValues(singletons, nameof(singletons));

            if (typeof(TService) == typeof(Type) && singletons.Any())
            {
                throw new ArgumentException(
                    StringResources.CollectionsRegisterCalledWithTypeAsTService(singletons.Cast<Type>()),
                    nameof(TService));
            }

            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));

            var singletonRegistrations =
                from singleton in singletons
                select SingletonLifestyle.CreateSingleInstanceRegistration(
                    typeof(TService),
                    singleton,
                    this.Container,
                    singleton.GetType());

            this.Register(typeof(TService), singletonRegistrations);
        }

        /// <summary>
        /// Registers a collection of <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the resolved collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceTypes"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <typeparamref name="TService"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        public void Register<TService>(params Type[] serviceTypes) where TService : class
        {
            this.Register(typeof(TService), serviceTypes);
        }

        /// <summary>
        /// Registers a collection of <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the resolved collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceTypes"/> is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <typeparamref name="TService"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        public void Register<TService>(IEnumerable<Type> serviceTypes) where TService : class
        {
            this.Register(typeof(TService), serviceTypes);
        }

        /// <summary>
        /// Registers a collection of <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the resolved collection of <typeparamref name="TService"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="registrations"/> contains a null
        /// (Nothing in VB) element or when <typeparamref name="TService"/> is not assignable from any of the
        /// service types supplied by the given <paramref name="registrations"/> instances.
        /// </exception>
        public void Register<TService>(IEnumerable<Registration> registrations)
            where TService : class
        {
            this.Register(typeof(TService), registrations);
        }

        /// <summary>
        /// Registers a collection of <paramref name="serviceTypes"/>, whose instances will be resolved lazily
        /// each time the resolved collection of <paramref name="serviceType"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, a generic type definition, or the <paramref name="serviceType"/> is
        /// not assignable from one of the given <paramref name="serviceTypes"/> elements.
        /// </exception>
        public void Register(Type serviceType, IEnumerable<Type> serviceTypes)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(serviceTypes, nameof(serviceTypes));

            // Make a copy for correctness and performance.
            serviceTypes = serviceTypes.ToArray();

            Requires.DoesNotContainNullValues(serviceTypes, nameof(serviceTypes));
            Requires.ServiceIsAssignableFromImplementations(serviceType, serviceTypes, nameof(serviceTypes),
                typeCanBeServiceType: true);
            Requires.DoesNotContainOpenGenericTypesWhenServiceTypeIsNotGeneric(serviceType, serviceTypes,
                nameof(serviceTypes));
            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(serviceType, serviceTypes,
                nameof(serviceTypes));

            this.RegisterCollectionInternal(serviceType, serviceTypes);
        }

        /// <summary>
        /// Registers a collection of <paramref name="registrations"/>, whose instances will be resolved lazily
        /// each time the resolved collection of <paramref name="serviceType"/> is enumerated. 
        /// The underlying collection is a stream that will return individual instances based on their 
        /// specific registered lifestyle, for each call to <see cref="IEnumerator{T}.Current"/>. 
        /// The order in which the types appear in the collection is the exact same order that the items were 
        /// supplied to this method, i.e the resolved collection is deterministic.   
        /// </summary>
        /// <param name="serviceType">The base type or interface for elements in the collection. This can be
        /// an a non-generic type, closed generic type or generic type definition.</param>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null 
        /// reference (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="registrations"/> contains a null
        /// (Nothing in VB) element or when <paramref name="serviceType"/> is not assignable from any of the
        /// service types supplied by the given <paramref name="registrations"/> instances.
        /// </exception>
        public void Register(Type serviceType, IEnumerable<Registration> registrations)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(registrations, nameof(registrations));

            // Make a copy for performance and correctness.
            registrations = registrations.ToArray();

            Requires.DoesNotContainNullValues(registrations, nameof(registrations));
            Requires.AreRegistrationsForThisContainer(this.Container, registrations, nameof(registrations));
            Requires.ServiceIsAssignableFromImplementations(serviceType, registrations, nameof(registrations),
                typeCanBeServiceType: true);
            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(serviceType, registrations,
                nameof(registrations));

            this.RegisterCollectionInternal(serviceType, registrations);
        }

        /// <summary>
        /// Registers a dynamic (container uncontrolled) collection of elements of type 
        /// <paramref name="serviceType"/>. A call to <see cref="Container.GetAllInstances{T}"/> will return the 
        /// <paramref name="containerUncontrolledCollection"/> itself, and updates to the collection will be 
        /// reflected in the result. If updates are allowed, make sure the collection can be iterated safely 
        /// if you're running a multi-threaded application.
        /// </summary>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="containerUncontrolledCollection">The collection of items to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null 
        /// reference (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> represents an
        /// open generic type.</exception>
        public void Register(Type serviceType, IEnumerable containerUncontrolledCollection)
        {
            Requires.IsNotNull(serviceType, nameof(serviceType));
            Requires.IsNotNull(containerUncontrolledCollection, nameof(containerUncontrolledCollection));
            Requires.IsNotOpenGenericType(serviceType, nameof(serviceType));
            Requires.IsNotAnAmbiguousType(serviceType, nameof(serviceType));

            try
            {
                this.RegisterContainerUncontrolledCollection(serviceType,
                    containerUncontrolledCollection.Cast<object>());
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                throw new ArgumentException(
                    StringResources.UnableToResolveTypeDueToSecurityConfiguration(serviceType, ex) +
                    Environment.NewLine + "paramName: " + nameof(serviceType), ex);
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
        public void Register<TService>(params Assembly[] assemblies) where TService : class
        {
            this.Register(typeof(TService), assemblies);
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
        public void Register<TService>(IEnumerable<Assembly> assemblies) where TService : class
        {
            this.Register(typeof(TService), assemblies);
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
        public void Register(Type serviceType, params Assembly[] assemblies)
        {
            this.Register(serviceType, (IEnumerable<Assembly>)assemblies);
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
        public void Register(Type serviceType, IEnumerable<Assembly> assemblies)
        {
            var compositesExcluded = new TypesToRegisterOptions { IncludeComposites = false };
            var types = this.Container.GetTypesToRegister(serviceType, assemblies, compositesExcluded);
            this.Register(serviceType, types);
        }

        private ContainerControlledCollection<TService> CreateInternal<TService>(
            IEnumerable<Type> serviceTypes)
            where TService : class
        {
            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));
            Requires.IsNotNull(serviceTypes, nameof(serviceTypes));

            // Make a copy for correctness and performance.
            serviceTypes = serviceTypes.ToArray();

            Requires.DoesNotContainNullValues(serviceTypes, nameof(serviceTypes));
            Requires.ServiceIsAssignableFromImplementations(typeof(TService), serviceTypes, nameof(serviceTypes),
                typeCanBeServiceType: true);
            Requires.DoesNotContainOpenGenericTypesWhenServiceTypeIsNotGeneric(typeof(TService), serviceTypes,
                nameof(serviceTypes));
            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(typeof(TService), serviceTypes,
                nameof(serviceTypes));

            var collection = new ContainerControlledCollection<TService>(this.Container);

            collection.AppendAll(serviceTypes);

            this.RegisterForVerification(collection);

            return collection;
        }

        private ContainerControlledCollection<TService> CreateInternal<TService>(
            IEnumerable<Registration> registrations)
            where TService : class
        {
            Requires.IsNotAnAmbiguousType(typeof(TService), nameof(TService));
            Requires.IsNotNull(registrations, nameof(registrations));

            Requires.DoesNotContainNullValues(registrations, nameof(registrations));
            Requires.AreRegistrationsForThisContainer(this.Container, registrations, nameof(registrations));
            Requires.ServiceIsAssignableFromImplementations(typeof(TService), registrations, nameof(registrations),
                typeCanBeServiceType: true);
            Requires.OpenGenericTypesDoNotContainUnresolvableTypeArguments(typeof(TService), registrations,
                nameof(registrations));

            var collection = new ContainerControlledCollection<TService>(this.Container);

            collection.AppendAll(registrations);

            this.RegisterForVerification(collection);

            return collection;
        }

        private void RegisterForVerification<TService>(ContainerControlledCollection<TService> collection)
        {
            // By creating a Producer, Simple Injector will automatically register it as 'external producer',
            // which allows it to be verified. To prevent memory leaks however, this external producer is
            // linked using a WeakReference to allow it to be GCed. To prevent this from happening, while
            // the application keeps referencing the collection, we let the collection reference the producer.
            collection.ParentProducer = collection.CreateInstanceProducer(this.Container);
        }

        // This method is internal to prevent the main API of the framework from being 'polluted'. The
        // Collections.Append method enabled public exposure.
        private void AppendToCollectionInternal(Type itemType, Registration registration)
        {
            this.RegisterCollectionInternal(itemType,
                new[] { ContainerControlledItem.CreateFromRegistration(registration) },
                appending: true);
        }

        private void AppendToCollectionInternal(Type itemType, Type implementationType)
        {
            // NOTE: The supplied serviceTypes can be opened, partially-closed, closed, non-generic or even
            // abstract.
            this.RegisterCollectionInternal(itemType,
                new[] { ContainerControlledItem.CreateFromType(implementationType) },
                appending: true);
        }

        private void RegisterCollectionInternal(Type itemType, IEnumerable<Registration> registrations)
        {
            var controlledItems = registrations.Select(ContainerControlledItem.CreateFromRegistration).ToArray();

            this.RegisterCollectionInternal(itemType, controlledItems);
        }

        private void RegisterCollectionInternal(Type itemType, IEnumerable<Type> serviceTypes)
        {
            // NOTE: The supplied serviceTypes can be opened, partially-closed, closed, non-generic or even
            // abstract.
            var controlledItems = serviceTypes.Select(ContainerControlledItem.CreateFromType).ToArray();
            this.RegisterCollectionInternal(itemType, controlledItems);
        }

        private void RegisterCollectionInternal(Type itemType, ContainerControlledItem[] controlledItems,
            bool appending = false)
        {
            this.Container.ThrowWhenContainerIsLockedOrDisposed();

            this.RegisterGenericContainerControlledCollection(itemType, controlledItems, appending);
        }

        private void RegisterGenericContainerControlledCollection(Type itemType,
            ContainerControlledItem[] controlledItems, bool appending)
        {
            CollectionResolver resolver = this.GetContainerControlledResolver(itemType);

            resolver.AddControlledRegistrations(itemType, controlledItems, append: appending);
        }

        private void RegisterGenericContainerUncontrolledCollection(Type itemType, IEnumerable collection)
        {
            var resolver = this.Container.GetContainerUncontrolledResolver(itemType);

            var producer = SingletonLifestyle.CreateUncontrolledCollectionProducer(
                itemType, collection, this.Container);

            resolver.RegisterUncontrolledCollection(itemType, producer);
        }

        private void RegisterContainerUncontrolledCollection<T>(Type itemType,
            IEnumerable<T> containerUncontrolledCollection)
        {
            IEnumerable readOnlyCollection = containerUncontrolledCollection.MakeReadOnly();
            IEnumerable castedCollection = Helpers.CastCollection(readOnlyCollection, itemType);

            this.RegisterGenericContainerUncontrolledCollection(itemType, castedCollection);
        }

        private CollectionResolver GetContainerControlledResolver(Type itemType)
        {
            return this.Container.GetCollectionResolver(itemType, containerControlled: true);
        }
    }
}