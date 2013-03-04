#region Copyright (c) 2013 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2013 S. van Deursen
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
    using System.Linq.Expressions;
    using System.Reflection;

    using SimpleInjector.Interception;

    /// <summary>Extension methods for doing interception.</summary>
    public static class InterceptorExtensions
    {
        /// <summary>
        /// Starts a fluent registration for intercepting the given <typeparamref name="TService"/> interface.
        /// A method on the returned object should be made to make the actual registration.
        /// </summary>
        /// <typeparam name="TService">The interface to be intercepted.</typeparam>
        /// <param name="container">The container instance.</param>
        /// <returns>An object that allows to proceed with the fluent registration.</returns>
        public static InterceptionRegistrator Intercept<TService>(this Container container) 
            where TService : class
        {
            Requires.IsNotNull(container, "container");
            Requires.IsInterface(typeof(TService), "TService");

            return new InterceptionRegistrator(container, type => type == typeof(TService));
        }

        /// <summary>
        /// Starts a fluent registration for intercepting the given <paramref name="serviceType"/> interface.
        /// A method on the returned object should be made to make the actual registration.
        /// </summary>
        /// <param name="container">The container instance.</param>
        /// <param name="serviceType">The Type object of the interface to be intercepted.</param>
        /// <returns>An object that allows to proceed with the fluent registration.</returns>
        public static InterceptionRegistrator Intercept(this Container container, Type serviceType)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsInterface(serviceType, "serviceType");

            if (serviceType.IsGenericTypeDefinition)
            {
                return new InterceptionRegistrator(container, type =>
                    type.IsGenericType && type.GetGenericTypeDefinition() == serviceType);
            }
            
            return new InterceptionRegistrator(container, type => type == serviceType);
        }

        /// <summary>
        /// Starts a fluent registration for intercepting the the interface types that are specified by the
        /// given <paramref name="serviceTypeSelector"/> predicate.
        /// A method on the returned object should be made to make the actual registration.
        /// </summary>
        /// <param name="container">The container instance.</param>
        /// <param name="serviceTypeSelector">The predicate that selects the given types to intercept.</param>
        /// <returns>An object that allows to proceed with the fluent registration.</returns>
        public static InterceptionRegistrator Intercept(this Container container, 
            Predicate<Type> serviceTypeSelector)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceTypeSelector, "serviceTypeSelector");

            return new InterceptionRegistrator(container, serviceTypeSelector);
        }

        /// <summary>
        /// Sets the interceptor that will be used to intercept the given service. The interceptor will
        /// have the transient lifestyle.
        /// </summary>
        /// <typeparam name="TInterceptor">An <see cref="IInterceptor"/>.</typeparam>
        /// <param name="registration">The fluent registration class.</param>
        public static void With<TInterceptor>(this InterceptionRegistrator registration)
            where TInterceptor : class, IInterceptor
        {
            With<TInterceptor>(registration, Lifestyle.Transient);
        }

        /// <summary>
        /// Sets the interceptor with the given <paramref name="lifestyle"/> that will be used to intercept 
        /// the given service.
        /// </summary>
        /// <typeparam name="TInterceptor">An <see cref="IInterceptor"/>.</typeparam>
        /// <param name="registration">The fluent registration class.</param>
        /// <param name="lifestyle">The lifestyle of the interceptor.</param>
        public static void With<TInterceptor>(this InterceptionRegistrator registration, Lifestyle lifestyle)
            where TInterceptor : class, IInterceptor
        {
            Requires.IsNotNull(registration, "registration");
            Requires.IsConstructable(typeof(TInterceptor), registration.Container, "TInterceptor");

            var interceptWith = new InterceptionHelper(registration.Container)
            {
                InterceptorType = typeof(TInterceptor),
                TransientInterceptor =
                    Lifestyle.Transient.CreateRegistration<TInterceptor, TInterceptor>(registration.Container),
                Lifestyle = lifestyle,
                Predicate = registration.ServiceTypeSelector,
            };

            registration.Container.ExpressionBuilt += interceptWith.OnExpressionBuilt;
        }

        /// <summary>
        /// Sets a single interceptor instance that will be used to intercept the given service.
        /// </summary>
        /// <param name="registration">The fluent registration class.</param>
        /// <param name="interceptor">The interceptor.</param>
        public static void With(this InterceptionRegistrator registration, IInterceptor interceptor)
        {
            With(registration, interceptor, Lifestyle.Transient);
        }

        /// <summary>
        /// Sets a single interceptor instance that will be used to intercept the given service.
        /// </summary>
        /// <param name="registration">The fluent registration class.</param>
        /// <param name="interceptor">The interceptor.</param>
        /// <param name="lifestyle">The lifestyle of the given proxy that hold both the interceptor as the
        /// interceptee.</param>
        public static void With(this InterceptionRegistrator registration, IInterceptor interceptor,
            Lifestyle lifestyle)
        {
            Requires.IsNotNull(registration, "registration");
            Requires.IsNotNull(interceptor, "interceptor");
            Requires.IsNotNull(lifestyle, "lifestyle");

            var interceptWith = new InterceptionHelper(registration.Container)
            {
                InterceptorType = interceptor.GetType(),
                TransientInterceptor = Lifestyle.Singleton.CreateRegistration<IInterceptor>(
                    () => interceptor, registration.Container),
                Lifestyle = lifestyle,
                Predicate = registration.ServiceTypeSelector,
            };

            registration.Container.ExpressionBuilt += interceptWith.OnExpressionBuilt;
        }

        private static Func<object, IInterceptor, object> BuildProxyCreator(Type interfaceToProxy)
        {
            return (decorated, interceptor) =>
            {
                var proxy = new InterceptorProxy(interfaceToProxy, decorated, interceptor);

                return proxy.GetTransparentProxy();
            };
        }

        private class InterceptionHelper
        {
            public InterceptionHelper(Container container)
            {
                this.Container = container;
            }

            internal Container Container { get; private set; }

            internal Type InterceptorType { get; set; }

            internal Registration TransientInterceptor { get; set; }

            internal Lifestyle Lifestyle { get; set; }

            internal Predicate<Type> Predicate { get; set; }
                        
            public void OnExpressionBuilt(object sender, ExpressionBuiltEventArgs e)
            {
                if (this.Predicate(e.RegisteredServiceType))
                {
                    ThrowIfServiceTypeIsNotAnInterface(e);

                    var constructor = this.GetInterceptorConstructor();

                    e.Expression = this.ApplyInterceptorUsingProxy(e);

                    e.Lifestyle = this.Lifestyle;

                    this.AddKnownDecoratorRelationships(e);
                }
            }

            private static void ThrowIfServiceTypeIsNotAnInterface(ExpressionBuiltEventArgs e)
            {
                // NOTE: We can only handle interfaces, because System.Runtime.Remoting.Proxies.RealProxy 
                // only supports interfaces. But besides that, users should program to interfaces not to
                // base types and, it is easy to make mistakes when using base types, since you have to make
                // all members virtual.
                if (!e.RegisteredServiceType.IsInterface)
                {
                    throw new NotSupportedException("Can't intercept type " +
                        e.RegisteredServiceType.Name + " because it is not an interface. " +
                        "Only interception of interfaces is supported.");
                }
            }

            private ConstructorInfo GetInterceptorConstructor()
            {
                return this.Container.Options.ConstructorResolutionBehavior.GetConstructor(
                    this.InterceptorType, this.InterceptorType);
            }
                     
            private Expression ApplyInterceptorUsingProxy(ExpressionBuiltEventArgs e)
            {
                Expression interceptee = e.Expression;
                Expression interceptor = this.TransientInterceptor.BuildExpression();

                Func<object, IInterceptor, object> proxyCreator = BuildProxyCreator(e.RegisteredServiceType);

                // create: () => proxyCreator(interceptee, interceptor)
                var expression = Expression.Invoke(Expression.Constant(proxyCreator), interceptee, interceptor);

                Func<object> instanceCreator = Expression.Lambda<Func<object>>(expression).Compile();

                return this.Lifestyle.CreateRegistration(e.RegisteredServiceType, instanceCreator, this.Container)
                    .BuildExpression();
            }
            
            private void AddKnownDecoratorRelationships(ExpressionBuiltEventArgs e)
            {
                // TODO: Add the relationship between the interceptor and the interceptee.
                // e.KnownRelationships.Add(new KnownRelationship(
                // this.InterceptorType, 
                // this.Lifestyle,
                // // TODO: We need to store the
                // null));
                e.KnownRelationships.AddRange(this.TransientInterceptor.GetRelationships());
            }
        }
    }
}