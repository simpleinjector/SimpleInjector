// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

// This class is placed in the root namespace to allow users to start using these extension methods after
// adding the assembly reference, without find and add the correct namespace.
namespace SimpleInjector
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Extension methods for integrating Simple Injector with WCF services.
    /// </summary>
    public static partial class SimpleInjectorWcfExtensions
    {
        /// <summary>
        /// Registers that a single instance of <typeparamref name="TConcrete"/> will be returned during
        /// the WCF configured lifetime of the WCF service class. When the WCF service class is released by
        /// WCF and <typeparamref name="TConcrete"/> implements <see cref="IDisposable"/>,
        /// the cached instance will be disposed as well.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an
        /// the <typeparamref name="TConcrete"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TConcrete"/> is a type
        /// that can not be created by the container.</exception>
        [Obsolete("Please use Container.Register<TConcrete>(Lifestyle.Scoped) instead. " +
            "See: https://simpleinjector.org/wcf. Will be removed in version 5.0.",
            error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterPerWcfOperation<TConcrete>(this Container container)
            where TConcrete : class
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TImplementation"/> will be returned during
        /// the WCF configured lifetime of the WCF service class. When the WCF service class is released by
        /// WCF and <typeparamref name="TImplementation"/> implements <see cref="IDisposable"/>, the cached
        /// instance will be disposed as well.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an
        /// the <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <typeparamref name="TImplementation"/>
        /// type is not a type that can be created by the container.
        /// </exception>
        [Obsolete("Please use Container.Register<TService, TImplementation>(Lifestyle.Scoped) instead. " +
            "See: https://simpleinjector.org/wcf. Will be removed in version 5.0.",
            error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterPerWcfOperation<TService, TImplementation>(
            this Container container)
            where TService : class
            where TImplementation : class, TService
        {
            throw new NotSupportedException(
                "RegisterPerWcfOperation has been deprecated. " +
                "Please use Register<TService, TImplementation>(Lifestyle.Scoped) instead. " +
                "See: https://simpleinjector.org/wcf");
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the WCF configured lifetime of the WCF service class.
        /// When the WCF service class is released by WCF and the cached instance implements
        /// <see cref="IDisposable"/>, that cached instance will be disposed as well.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the <paramref name="container"/>, or <paramref name="instanceCreator"/> are
        /// null references.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the
        /// <typeparamref name="TService"/> has already been registered.</exception>
        [Obsolete("Please use Container.Register<TService>(Func<TService>, Lifestyle.Scoped) instead. " +
            "See: https://simpleinjector.org/wcf. Will be removed in version 5.0.",
            error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterPerWcfOperation<TService>(this Container container,
            Func<TService> instanceCreator)
            where TService : class
        {
            throw new NotSupportedException(
                "RegisterPerWcfOperation has been deprecated. " +
                "Please use Register<TService>(Func<TService>, Lifestyle.Scoped) instead. " +
                "See: https://simpleinjector.org/wcf");
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the WCF configured lifetime of the WCF service class.
        /// When the WCF service class is released by WCF, <paramref name="disposeWhenRequestEnds"/> is set to
        /// <b>true</b>, and the cached instance implements <see cref="IDisposable"/>, that cached instance
        /// will be disposed as well.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <param name="disposeWhenRequestEnds">If set to <c>true</c> the cached instance will be
        /// disposed at the end of its lifetime.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the <paramref name="container"/>, or <paramref name="instanceCreator"/> are
        /// null references.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the
        /// <typeparamref name="TService"/> has already been registered.</exception>
        [Obsolete("Please use Container.Register<TService>(Func<TService>, Lifestyle.Scoped) instead. " +
            "Will be removed in version 5.0.",
            error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterPerWcfOperation<TService>(
            this Container container, Func<TService> instanceCreator, bool disposeWhenRequestEnds)
            where TService : class
        {
            throw new NotSupportedException("RegisterPerWcfOperation has been deprecated.");
        }

        /// <summary>
        /// Gets the <see cref="Scope"/> for the current WCF request or <b>null</b> when no
        /// <see cref="Scope"/> is currently in scope.
        /// </summary>
        /// <example>
        /// The following example registers a <b>ServiceImpl</b> type as transient (a new instance will be
        /// returned every time) and registers an initializer for that type that will register that instance
        /// for disposal in the <see cref="Scope"/> in which context it is created:
        /// <code lang="cs"><![CDATA[
        /// container.Register<IService, ServiceImpl>();
        /// container.RegisterInitializer<ServiceImpl>(instance =>
        /// {
        ///     container.GetCurrentWcfOperationScope().RegisterForDisposal(instance);
        /// });
        /// ]]></code>
        /// </example>
        /// <param name="container">The container.</param>
        /// <returns>A new <see cref="Scope"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the current <paramref name="container"/>
        /// has both no <b>LifetimeScope</b> registrations.
        /// </exception>
        [Obsolete("Please use Lifestyle.Scoped.GetCurrentScope(Container) instead. " +
            "Will be removed in version 5.0.",
            error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Scope GetCurrentWcfOperationScope(this Container container)
        {
            throw new NotSupportedException(
                "GetCurrentWcfOperationScope has been deprecated. " +
                "Please use Lifestyle.Scoped.GetCurrentScope(Container) instead.");
        }
    }
}