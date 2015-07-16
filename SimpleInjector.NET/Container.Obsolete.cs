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

namespace SimpleInjector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

#if !PUBLISH
    /// <summary>This is where we putt all obsolete stuff that we can't just remove right now, but will
    /// in the future.</summary>
#endif
    public partial class Container
    {
        /// <summary>Initializes a new instance of the <see cref="Container"/> class.</summary>
        /// <param name="options">The container options.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="options"/> is a null
        /// reference.</exception>
        /// <exception cref="ArgumentException">Thrown when supplied <paramref name="options"/> is an instance
        /// that already is supplied to another <see cref="Container"/> instance. Every container must get
        /// its own <see cref="ContainerOptions"/> instance.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "options",
            Justification = "We can't remove the 'options' parameter. That would break the API.")]
        [Obsolete(
            "This method is not supported anymore. Please use Container.Options to configure the container.",
            error: true)]
        public Container(ContainerOptions options)
        {
            throw new InvalidOperationException(
                "This method is not supported anymore. Please use Container.Options to configure the container.");
        }

        /// <summary>
        /// Registers a single concrete instance that will be constructed using constructor injection and will
        /// be returned when this instance is requested by type <typeparamref name="TConcrete"/>. 
        /// This <typeparamref name="TConcrete"/> must be thread-safe when working in a multi-threaded 
        /// environment.
        /// If <typeparamref name="TConcrete"/> implements <see cref="IDisposable"/>, a created instance will
        /// get disposed when <see cref="Container.Dispose()">Container.Dispose</see> gets called.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when 
        /// <typeparamref name="TConcrete"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TConcrete"/> is a type
        /// that can not be created by the container.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "This method has been removed. Please call RegisterSingleton<TConcrete>() instead.",
            error: true)]
        public void RegisterSingle<TConcrete>() where TConcrete : class
        {
            // Forward the call. This allows external NuGet packages that depend on this method to keep working.
            this.Register<TConcrete, TConcrete>(Lifestyle.Singleton, "TConcrete", "TConcrete");
        }

        /// <summary>
        /// Registers that the same a single instance of type <typeparamref name="TImplementation"/> will be 
        /// returned every time an <typeparamref name="TService"/> type is requested. If 
        /// <typeparamref name="TService"/> and <typeparamref name="TImplementation"/>  represent the same 
        /// type, the type is registered by itself. <typeparamref name="TImplementation"/> must be thread-safe 
        /// when working in a multi-threaded environment.
        /// If <typeparamref name="TImplementation"/> implements <see cref="IDisposable"/>, a created instance will
        /// get disposed when <see cref="Container.Dispose()">Container.Dispose</see> gets called.
        /// </summary>
        /// <typeparam name="TService">
        /// The interface or base type that can be used to retrieve the instances.
        /// </typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the 
        /// <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <typeparamref name="TImplementation"/> 
        /// type is not a type that can be created by the container.
        /// </exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "This method has been removed. " +
            "Please call RegisterSingleton<TService, TImplementation>() instead.",
            error: true)]
        public void RegisterSingle<TService, TImplementation>()
            where TImplementation : class, TService
            where TService : class
        {
            // Forward the call. This allows external NuGet packages that depend on this method to keep working.
            this.RegisterSingleton<TService, TImplementation>();
        }

        /// <summary>
        /// Registers the specified delegate that allows constructing a single instance of 
        /// <typeparamref name="TService"/>. This delegate will be called at most once during the lifetime of 
        /// the application. The returned instance must be thread-safe when working in a multi-threaded 
        /// environment.
        /// If the instance returned from <paramref name="instanceCreator"/> implements <see cref="IDisposable"/>, 
        /// the created instance will get disposed when <see cref="Container.Dispose()">Container.Dispose</see> 
        /// gets called.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="instanceCreator">The delegate that allows building or creating this single
        /// instance.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when a 
        /// <paramref name="instanceCreator"/> for <typeparamref name="TService"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instanceCreator"/> is a 
        /// null reference.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "This method has been removed. " +
            "Please call RegisterSingleton<TService>(instanceCreator) instead.",
            error: true)]
        public void RegisterSingle<TService>(Func<TService> instanceCreator) where TService : class
        {
            // Forward the call. This allows external NuGet packages that depend on this method to keep working.
            this.RegisterSingleton<TService>(instanceCreator);
        }

        /// <summary>
        /// Registers that the same instance of type <paramref name="implementation"/> will be returned every 
        /// time an instance of type <paramref name="serviceType"/> type is requested. If 
        /// <paramref name="serviceType"/> and <paramref name="implementation"/> represent the same type, the 
        /// type is registered by itself. <paramref name="implementation"/> must be thread-safe when working 
        /// in a multi-threaded environment.
        /// </summary>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="implementation">The actual type that will be returned when requested.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="serviceType"/> or 
        /// <paramref name="implementation"/> are null references (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="implementation"/> is
        /// no sub type from <paramref name="serviceType"/>, or when one of them represents an open generic
        /// type.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <paramref name="serviceType"/> has already been registered.
        /// </exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "This method has been removed. " +
            "Please call RegisterSingleton(serviceType, implementation) instead.",
            error: true)]
        public void RegisterSingle(Type serviceType, Type implementation)
        {
            // Forward the call. This allows external NuGet packages that depend on this method to keep working.
            this.Register(serviceType, implementation, Lifestyle.Singleton, "serviceType", "implementation");
        }

        /// <summary>
        /// Registers the specified delegate that allows constructing a single <paramref name="serviceType"/> 
        /// instance. The container will call this delegate at most once during the lifetime of the application.
        /// </summary>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instanceCreator">The delegate that will be used for creating that single instance.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> represents an open
        /// generic type.</exception>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="serviceType"/> or 
        /// <paramref name="instanceCreator"/> are null references (Nothing in
        /// VB).</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <paramref name="serviceType"/> has already been registered.
        /// </exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "This method has been removed. " +
            "Please call RegisterSingleton(serviceType, instanceCreator) instead.",
            error: true)]
        public void RegisterSingle(Type serviceType, Func<object> instanceCreator)
        {
            // Forward the call. This allows external NuGet packages that depend on this method to keep working.
            this.RegisterSingleton(serviceType, instanceCreator);
        }

        /// <summary>This method has been removed.</summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instance.</typeparam>
        /// <param name="instance">The instance to register.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been removed. Please call RegisterSingleton<TService>(TService) instead.", error: true)]
        public void RegisterSingle<TService>(TService instance) where TService : class
        {
            this.RegisterSingleton<TService>(instance);
        }

        /// <summary>This method has been removed.</summary>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instance">The instance to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="serviceType"/> or 
        /// <paramref name="instance"/> are null references (Nothing in VB).</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been removed. Please call RegisterSingleton(Type, object) instead.", error: true)]
        public void RegisterSingle(Type serviceType, object instance)
        {
            this.RegisterSingleton(serviceType, instance);
        }

        /// <summary>This method has been removed.</summary>
        /// <param name="instance">The instance whose properties will be injected.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "Making this method static would break the API.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "Container.InjectProperties has been deprecated. Please read https://simpleinjector.org/depr1 " +
            "on why and how what to do instead.",
            error: true)]
        public void InjectProperties(object instance)
        {
            Requires.IsNotNull(instance, "instance");

            throw new InvalidOperationException(
                "Container.InjectProperties has been deprecated. Please read https://simpleinjector.org/depr1 " +
                "on why and how what to do instead.");
        }

        /// <summary>
        /// This method is obsolete. Use <see cref="RegisterCollection{TService}(IEnumerable{TService})"/> instead.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="collection">The collection to register.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "This method has been renamed to RegisterCollection. " +
            "Please call RegisterCollection<TService>(IEnumerable<TService>) instead.",
            error: true)]
        public void RegisterAll<TService>(IEnumerable<TService> collection) where TService : class
        {
            // Forward the call. This allows external NuGet packages that depend on this method to keep working.
            this.RegisterCollection<TService>(collection);
        }
        
        /// <summary>
        /// This method is obsolete. Use <see cref="RegisterCollection{TService}(TService[])"/> instead.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="singletons">The collection to register.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "This method has been renamed to RegisterCollection. " +
            "Please call RegisterCollection<TService>(TService[]) instead.",
            error: true)]
        public void RegisterAll<TService>(params TService[] singletons) where TService : class
        {
            // Forward the call. This allows external NuGet packages that depend on this method to keep working.
            this.RegisterCollection<TService>(singletons);
        }

        /// <summary>
        /// This method is obsolete. Use <see cref="RegisterCollection{TService}(IEnumerable{Type})"/> instead.
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "This method has been renamed to RegisterCollection. " +
            "Please call RegisterCollection<TService>(IEnumerable<Type>) instead.",
            error: true)]
        public void RegisterAll<TService>(params Type[] serviceTypes) where TService : class
        {
            // Forward the call. This allows external NuGet packages that depend on this method to keep working.
            this.RegisterCollection<TService>(serviceTypes);
        }

        /// <summary>
        /// This method is obsolete. Use <see cref="RegisterCollection{TService}(IEnumerable{Type})"/> instead.
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "This method has been renamed to RegisterCollection. " +
            "Please call RegisterCollection<TService>(IEnumerable<Type>) instead.",
            error: true)]
        public void RegisterAll<TService>(IEnumerable<Type> serviceTypes) where TService : class
        {
            // Forward the call. This allows external NuGet packages that depend on this method to keep working.
            this.RegisterCollection<TService>(serviceTypes);
        }

        /// <summary>
        /// This method is obsolete. Use <see cref="RegisterCollection(Type, IEnumerable{Type})"/> instead.
        /// </summary>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "This method has been renamed to RegisterCollection. " +
            "Please call RegisterCollection(Type, IEnumerable<Type>) instead.",
            error: true)]
        public void RegisterAll(Type serviceType, IEnumerable<Type> serviceTypes)
        {
            // Forward the call. This allows external NuGet packages that depend on this method to keep working.
            this.RegisterCollection(serviceType, serviceTypes);
        }

        /// <summary>
        /// This method is obsolete. Use <see cref="RegisterCollection(Type, IEnumerable{Registration})"/> instead.
        /// </summary>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "This method has been renamed to RegisterCollection. " +
            "Please call RegisterCollection(Type, IEnumerable<Registration>) instead.",
            error: true)]
        public void RegisterAll(Type serviceType, IEnumerable<Registration> registrations)
        {
            // Forward the call. This allows external NuGet packages that depend on this method to keep working.
            this.RegisterCollection(serviceType, registrations);
        }

        /// <summary>
        /// This method is obsolete. Use <see cref="RegisterCollection(Type, IEnumerable{Registration})"/> instead.
        /// </summary>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="registrations">The collection of <see cref="Registration"/> objects whose instances
        /// will be requested from the container.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "This method has been renamed to RegisterCollection. " +
            "Please call RegisterCollection(Type, IEnumerable<Registration>) instead.",
            error: true)]
        public void RegisterAll(Type serviceType, params Registration[] registrations)
        {
            // Forward the call. This allows external NuGet packages that depend on this method to keep working.
            this.RegisterCollection(serviceType, registrations);
        }

        /// <summary>
        /// This method is obsolete. Use <see cref="RegisterCollection(Type, IEnumerable)"/> instead.
        /// </summary>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="collection">The collection of items to register.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(
            "This method has been renamed to RegisterCollection. " +
            "Please call RegisterCollection(Type, IEnumerable) instead.",
            error: true)]
        public void RegisterAll(Type serviceType, IEnumerable collection)
        {
            // Forward the call. This allows external NuGet packages that depend on this method to keep working.
            this.RegisterCollection(serviceType, collection);
        }
    }
}