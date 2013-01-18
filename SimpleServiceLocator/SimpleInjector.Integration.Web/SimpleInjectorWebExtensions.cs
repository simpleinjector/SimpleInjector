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

            container.Register<TConcrete, TConcrete>(WebRequestLifestyle.Dispose);
        }

        /// <summary>
        /// Registers that one instance of <typeparamref name="TImplementation"/> will be returned for every 
        /// web request every time a <typeparamref name="TService"/> is requested and ensures that -if 
        /// <typeparamref name="TImplementation"/> implements <see cref="IDisposable"/>- this instance 
        /// will get disposed on the end of the web request.
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

            container.Register<TService, TImplementation>(WebRequestLifestyle.Dispose);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>
        /// and the returned instance will be reused for the duration of a single web request and ensures that,
        /// if the returned instance implements <see cref="IDisposable"/>, that instance will get
        /// disposed on the end of the web request.
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
            RegisterPerWebRequest(container, instanceCreator, disposeInstanceWhenWebRequestEnds: true);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>
        /// and the returned instance will be reused for the duration of a single web request and ensures that,
        /// if the returned instance implements <see cref="IDisposable"/>, and
        /// <paramref name="disposeInstanceWhenWebRequestEnds"/> is set to <b>true</b>, that instance will get
        /// disposed on the end of the web request.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <param name="disposeInstanceWhenWebRequestEnds">If set to <c>true</c>, the instance will get disposed
        /// when it implements <see cref="IDisposable"/> at the end of the web request.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the
        /// <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="container"/> or <paramref name="instanceCreator"/> are null
        /// references.</exception>
        public static void RegisterPerWebRequest<TService>(this Container container,
            Func<TService> instanceCreator, bool disposeInstanceWhenWebRequestEnds) where TService : class
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            container.Register<TService>(instanceCreator, 
                new WebRequestLifestyle(disposeInstanceWhenWebRequestEnds));
        }

        /// <summary>
        /// Registers the supplied <paramref name="disposable"/> for disposal when the current web request
        /// ends.
        /// </summary>
        /// <example>
        /// The following example registers a <b>DisposableServiceImpl</b> type as transient (a new instance 
        /// will be returned every time) and registers an initializer for that type that will ensure that
        /// that instance will be disposed when the web request ends:
        /// <code lang="cs"><![CDATA[
        /// container.Register<IService, ServiceImpl>();
        /// container.RegisterInitializer<ServiceImpl>(SimpleInjectorWebExtensions.RegisterForDisposal);
        /// ]]></code>
        /// </example>
        /// <param name="disposable">The disposable.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the given <paramref name="disposable"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the <see cref="HttpContext.Current"/>
        /// returns null.</exception>
        public static void RegisterForDisposal(IDisposable disposable)
        {
            if (disposable == null)
            {
                throw new ArgumentNullException("disposable");
            }

            var context = HttpContext.Current;

            if (context == null)
            {
                throw new InvalidOperationException(
                    "This method can only be called in the context of a web request.");
            }

            RegisterDelegateForEndWebRequest(context, () => disposable.Dispose());
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
    }
}