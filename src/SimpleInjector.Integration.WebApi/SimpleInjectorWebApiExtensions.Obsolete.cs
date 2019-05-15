// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.ComponentModel;

    /// <content>Deprecated methods.</content>>
    public static partial class SimpleInjectorWebApiExtensions
    {
        /// <summary>
        /// Registers that a single instance of <typeparamref name="TConcrete"/> will be returned within the
        /// Web API request. When the Web API request ends and
        /// <typeparamref name="TConcrete"/> implements <see cref="IDisposable"/>, the cached instance will be
        /// disposed.
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
        [Obsolete("Please use Register<TConcrete>(Lifestyle.Scoped) instead. " +
            "See: https://simpleinjector.org/webapi. Will be removed in version 5.0.",
            error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterWebApiRequest<TConcrete>(this Container container)
            where TConcrete : class
        {
            throw new NotSupportedException(
                "RegisterWebApiRequest has been deprecated. " +
                "Please use Register<TConcrete>(Lifestyle.Scoped) instead. " +
                "See: https://simpleinjector.org/webapi");
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TImplementation"/> will be returned  will
        /// be returned within the Web API request. When the Web API request ends and
        /// <typeparamref name="TImplementation"/> implements <see cref="IDisposable"/>, the cached instance
        /// will be disposed.
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
        [Obsolete("Please use Register<TService, TImplementation>(Lifestyle.Scoped) instead. " +
            "See: https://simpleinjector.org/webapi. Will be removed in version 5.0.",
            error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterWebApiRequest<TService, TImplementation>(
            this Container container)
            where TImplementation : class, TService
            where TService : class
        {
            throw new NotSupportedException(
                "RegisterWebApiRequest has been deprecated. " +
                "Please use Register<TService, TImplementation>(Lifestyle.Scoped) instead. " +
                "See: https://simpleinjector.org/webapi");
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the lifetime of a Web API request. When the Web API
        /// request ends, and the cached instance implements <see cref="IDisposable"/>, that cached instance
        /// will be disposed.
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
        [Obsolete("Please use Register<TService>(Func<TService>, Lifestyle.Scoped) instead. " +
            "See: https://simpleinjector.org/webapi. Will be removed in version 5.0.",
            error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterWebApiRequest<TService>(this Container container,
            Func<TService> instanceCreator)
            where TService : class
        {
            throw new NotSupportedException(
                "RegisterWebApiRequest has been deprecated. " +
                "Please use Register<TService>(Func<TService>, Lifestyle.Scoped) instead. " +
                "See: https://simpleinjector.org/webapi");
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TConcrete"/> will be returned for
        /// each Web API request. When the Web API request ends, and <typeparamref name="TConcrete"/>
        /// implements <see cref="IDisposable"/>, the cached instance will be disposed.
        /// Scopes can be nested, and each scope gets its own instance.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="disposeWhenScopeEnds">If set to <c>true</c> the cached instance will be
        /// disposed at the end of its lifetime.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an
        /// the <typeparamref name="TConcrete"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TConcrete"/> is a type
        /// that can not be created by the container.</exception>
        [Obsolete("Please use Register<TConcrete>(Lifestyle.Scoped) instead. " +
            "Will be removed in version 5.0.",
            error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterWebApiRequest<TConcrete>(this Container container,
            bool disposeWhenScopeEnds)
            where TConcrete : class, IDisposable
        {
            throw new NotSupportedException(
                "RegisterWebApiRequest has been deprecated. " +
                "Please use Register<TConcrete>(new WebApiRequesstLifestyle(false)) instead " +
                "to suppress disposal.");
        }

        /// <summary>
        /// Registers that a single instance of <typeparamref name="TImplementation"/> will be returned for
        /// the duration of a single Web API request. When the Web API request ends,
        /// <paramref name="disposeWhenScopeEnds"/> is set to <b>true</b>, and the cached instance
        /// implements <see cref="IDisposable"/>, that cached instance will be disposed.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="disposeWhenScopeEnds">If set to <c>true</c> the cached instance will be
        /// disposed at the end of its lifetime.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an
        /// the <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <typeparamref name="TImplementation"/>
        /// type is not a type that can be created by the container.
        /// </exception>
        [Obsolete("Please use Register<TService, TImplementation>(Lifestyle.Scoped) instead. " +
            "Will be removed in version 5.0.",
            error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterWebApiRequest<TService, TImplementation>(
            this Container container, bool disposeWhenScopeEnds)
            where TService : class
            where TImplementation : class, TService, IDisposable
        {
            throw new NotSupportedException(
                "RegisterWebApiRequest has been deprecated. " +
                "Please use Register<TService, TImplementation>(new WebApiRequesstLifestyle(false)) instead " +
                "to suppress disposal.");
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>,
        /// and returned instances are cached during the lifetime of single Web API request. When the Web API
        /// request ends, <paramref name="disposeWhenScopeEnds"/> is set to <b>true</b>, and the cached
        /// instance implements <see cref="IDisposable"/>, that cached instance will be disposed.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <param name="disposeWhenScopeEnds">If set to <c>true</c> the cached instance will be
        /// disposed at the end of its lifetime.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the <paramref name="container"/>, or <paramref name="instanceCreator"/> are
        /// null references.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the
        /// <typeparamref name="TService"/> has already been registered.</exception>
        [Obsolete("RegisterWebApiRequest has been deprecated. " +
            "Please use Register<TService>(Func<TService>, Lifestyle.Scoped) instead. " +
            "Will be removed in version 5.0.",
            error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterWebApiRequest<TService>(
            this Container container, Func<TService> instanceCreator, bool disposeWhenScopeEnds)
            where TService : class
        {
            throw new NotSupportedException(
                "RegisterWebApiRequest has been deprecated. " +
                "Please use Register<TService>(Func<TService>, new WebApiRequesstLifestyle(false)) instead " +
                "to suppress disposal.");
        }
    }
}