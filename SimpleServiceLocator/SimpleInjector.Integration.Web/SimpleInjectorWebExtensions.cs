#region Copyright (c) 2012 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2012 S. van Deursen
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

// This class is placed in the root namespace to allow users to start using these extension methods after
// adding the assembly reference, without find and add the correct namespace.
namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Web;

    using SimpleInjector.Integration.Web;

    /// <summary>
    /// Extension methods for integrating Simple Injector with ASP.NET web applications.
    /// </summary>
    public static class SimpleInjectorWebExtensions
    {
        /// <summary>
        /// Registers that one instance of <typeparamref name="TConcrete"/> will be returned for every web
        /// request and ensures that -if <typeparamref name="TConcrete"/> implements 
        /// <see cref="IDisposable"/>- this instance will get disposed on the end of the web request. 
        /// When no web request is available, a new instance will be returned each time (transient) and those 
        /// instances will <b>not</b> get disposed.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TConcrete"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TConcrete"/> is a type
        /// that can not be created by the container.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> is a null
        /// reference.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A design without a generic T would be unpractical, because the other " +
            "overloads also take a generic T.")]
        public static void RegisterPerWebRequest<TConcrete>(this Container container)
            where TConcrete : class
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            // Register the type as transient. This prevents it from being registered twice and allows us to 
            // hook onto the ExpressionBuilt event, and allows us to use the error checking of the container.
            // The container will validate directly whether the TConcrete is a constructable type.
            container.Register<TConcrete>();

            // By registering an ExpressionBuilt event we use the ability of the container to create an
            // Expression tree that auto-wires that instance, since calling GetInstance<TConcrete> would cause
            // an stack overflow exception.
            ReplaceRegistrationWithPerWebRequestBehavior<TConcrete>(container);
        }

        /// <summary>
        /// Registers that one instance of <typeparamref name="TImplementation"/> will be returned for every 
        /// web request every time a <typeparamref name="TService"/> is requested and ensures that -if 
        /// <typeparamref name="TImplementation"/> implements <see cref="IDisposable"/>- this instance 
        /// will get disposed on the end of the web request. When no web request is available, a new instance 
        /// will be returned each time (transient) and those instances will <b>not</b> get disposed.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.
        /// </typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <typeparamref name="TImplementation"/> 
        /// type is not a type that can be created by the container.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> is a null
        /// reference.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A design without a generic T would be unpractical, because we will lose " +
            "compile-time support.")]
        public static void RegisterPerWebRequest<TService, TImplementation>(this Container container)
            where TService : class
            where TImplementation : class, TService
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            container.Register<TService, TImplementation>();

            ReplaceRegistrationWithPerWebRequestBehavior<TService>(container);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>
        /// and the returned instance will be reused for the duration of a single web request and ensures that,
        /// if the returned instance implements <see cref="IDisposable"/>, that instance will get
        /// disposed on the end of the web request. When no web request is available, the specified delegate
        /// will be called each time and those instances will not get disposed.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the
        /// <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="container"/> or <paramref name="instanceCreator"/> are null
        /// references.</exception>
        public static void RegisterPerWebRequest<TService>(this Container container,
            Func<TService> instanceCreator) where TService : class
        {
            RegisterPerWebRequest(container, instanceCreator, disposeWhenWebRequestEnds: true);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>
        /// and the returned instance will be reused for the duration of a single web request and ensures that,
        /// if the returned instance implements <see cref="IDisposable"/>, and
        /// <paramref name="disposeWhenWebRequestEnds"/> is set to <b>true</b>, that instance will get
        /// disposed on the end of the web request. When no web request is available, the specified delegate
        /// will be called each time and those instances will not get disposed.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <param name="disposeWhenWebRequestEnds">If set to <c>true</c>, the instance will get disposed
        /// when it implements <see cref="IDisposable"/> at the end of the web request.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the
        /// <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="container"/> or <paramref name="instanceCreator"/> are null
        /// references.</exception>
        public static void RegisterPerWebRequest<TService>(this Container container,
            Func<TService> instanceCreator, bool disposeWhenWebRequestEnds) where TService : class
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            container.Register<TService>(instanceCreator);

            ReplaceRegistrationWithPerWebRequestBehavior<TService>(container, disposeWhenWebRequestEnds);
        }

        internal static void RegisterDelegateForEndWebRequest(HttpContext context, Action webRequestEnds)
        {
            var key = typeof(SimpleInjectorWebExtensions);

            var actions = (List<Action>)context.Items[key];

            if (actions == null)
            {
                context.Items[key] = actions = new List<Action>();
            }

            actions.Add(webRequestEnds);
        }

        internal static void ExecuteAllRegisteredEndWebRequestDelegates()
        {
            var context = HttpContext.Current;

            var key = typeof(SimpleInjectorWebExtensions);

            var actions = (List<Action>)context.Items[key];

            if (actions != null)
            {
                actions.ForEach(action => action());
                context.Items[key] = null;
            }
        }

        private static void ReplaceRegistrationWithPerWebRequestBehavior<TService>(
            Container container, bool disposeWhenRequestEnds = true)
            where TService : class
        {
            // In case of calling RegisterPerWebRequest<TConcrete>, the e.Expression contains a 
            // "new TConcrete(...)" call, where that TConcrete is auto-wired by the container. There is no 
            // other way than using ExpressionBuilt to get this auto-wired instance, since calling the
            // GetInstance<TConcrete> would cause a recursive call and causes a stack overflow. Although using 
            // ExpressionBuilt is not needed for RegisterPerWebRequest<TService, TImpl> and
            // RegisterPerWebRequest<TService>(Func<TService>), we use this same method, since this keeps the
            // behavior of these overloads in sync.
            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(TService))
                {
                    // Extract a Func<T> delegate for creating the transient TConcrete.
                    var transientInstanceCreator = Expression.Lambda<Func<TService>>(
                        e.Expression, new ParameterExpression[0]).Compile();

                    var instanceCreator = new PerWebRequestInstanceCreator<TService>(transientInstanceCreator,
                        disposeWhenRequestEnds);

                    // Swap the original expression so that the lifetime becomes a per-web-request.
                    e.Expression = Expression.Call(Expression.Constant(instanceCreator),
                        instanceCreator.GetType().GetMethod("GetInstance"));
                }
            };
        }
    }
}