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

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;

    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Instances returned from the container can be cached. The <see cref="Container"/> contains several
    /// overloads of the <b>Register</b> method that take an <b>Lifestyle</b> as argument to define how
    /// returned instances should be cached. The core library contains two lifestyles out of the box. By
    /// supplying <see cref="Lifestyle.Transient">Lifestyle.Transient</see>, the registered instance is not
    /// cached; a new instance is returned every time it is requested or injected. By supplying
    /// <see cref="Lifestyle.Singleton">Lifestyle.Singleton</see> instances can be cached indefinately; only
    /// a single instance of the registered component will be returned by that container instance.
    /// </summary>
    /// <remarks>
    /// This type is abstract and can be overridden to implement a custom lifestyle.
    /// </remarks>
    [DebuggerDisplay("{Name,nq}")]
    public abstract class Lifestyle
    {
        /// <summary>
        /// The lifestyle instance that doesn't cache instances. A new instance of the specified
        /// component is created every time the registered service it is requested or injected.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "It's not mutable.")]
        public static readonly Lifestyle Transient = new TransientLifestyle();

        /// <summary>
        /// The lifestyle that caches components during the lifetime of the <see cref="Container"/> instance
        /// and guarantees that only a single instance of that component is created for that instance. Since
        /// general use is to create a single <b>Container</b> instance for the lifetime of the application /
        /// AppDomain, this would mean that only a single instance of that component would exist during the
        /// lifetime of the application. In a multi-threaded applications, implementations registered using 
        /// this lifestyle must be thread-safe.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", 
            Justification = "It's not mutable.")]
        public static readonly Lifestyle Singleton = new SingletonLifestyle();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal static readonly Lifestyle Unknown = new UnknownLifestyle();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly MethodInfo OpenCreateRegistrationTServiceTImplementationMethod =
            GetMethod(lifestyle => lifestyle.CreateRegistration<object, object>(null));

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly MethodInfo OpenCreateRegistrationTServiceFuncMethod =
            GetMethod(lifestyle => lifestyle.CreateRegistration<object>(null, null));

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string name;

        /// <summary>Initializes a new instance of the <see cref="Lifestyle"/> class.</summary>
        /// <param name="name">The user friendly name of this lifestyle.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null (Nothing in VB) 
        /// or an empty string.</exception>
        protected Lifestyle(string name)
        {
            Requires.IsNotNullOrEmpty(name, "name");

            this.name = name;
        }

        /// <summary>Gets the user friendly name of this lifestyle.</summary>
        /// <value>The user friendly name of this lifestyle.</value>
        public string Name 
        { 
            get { return this.name; } 
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal virtual int ComponentLength 
        {
            get { return this.Length; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal virtual int DependencyLength 
        {
            get { return this.Length; }
        }

        /// <summary>
        /// Gets the length of the lifestyle. Implementers must implement this property. The diagnostic
        /// services use this value to compare lifestyles with each other to determine lifestyle 
        /// misconfigurations.
        /// </summary>
        /// <value>The <see cref="Int32"/> representing the length of this lifestyle.</value>
        protected abstract int Length
        {
            get;
        }

        /// <summary>
        /// The hybrid lifestyle allows mixing two lifestyles in a single registration. Based on the supplied
        /// <paramref name="test"/> delegate the hybrid lifestyle will redirect the creation of the instance
        /// to the correct lifestyle. The result of the test delegate will not be cached; it is invoked each
        /// time an instance is requested or injected. By nesting hybrid lifestyles, any number of lifestyles 
        /// can be mixed.
        /// </summary>
        /// <param name="test">The <see cref="Func{TResult}"/> delegate that determines which lifestyle should
        /// be used. The <paramref name="trueLifestyle"/> will be used if <b>true</b> is returned; the
        /// <paramref name="falseLifestyle"/> otherwise.</param>
        /// <param name="trueLifestyle">The lifestyle to use when <paramref name="test"/> returns <b>true</b>.</param>
        /// <param name="falseLifestyle">The lifestyle to use when <paramref name="test"/> returns <b>false</b>.</param>
        /// <returns>A new hybrid lifestyle that wraps the supplied lifestyles.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one of the supplied arguments is a null
        /// reference (Nothing in VB).</exception>
        /// <example>
        /// The following example shows the creation of a <b>HybridLifestyle</b> that mixes an 
        /// <b>WebRequestLifestyle</b> and <b>LifetimeScopeLifestyle</b>:
        /// <code lang="cs"><![CDATA[
        /// // NOTE: WebRequestLifestyle is located in SimpleInjector.Integration.Web.dll.
        /// // NOTE: LifetimeScopeLifestyle is located in SimpleInjector.Extensions.LifetimeScoping.dll.
        /// var mixedScopeLifestyle = Lifestyle.Hybrid(
        ///     () => HttpContext.Current != null,
        ///     new WebRequestLifestyle(),
        ///     new LifetimeScopeLifestyle());
        /// 
        /// // The created lifestyle can be reused for many registrations.
        /// container.Register<IUserRepository, SqlUserRepository>(mixedScopeLifestyle);
        /// container.Register<ICustomerRepository, SqlCustomerRepository>(mixedScopeLifestyle);
        /// ]]></code>
        /// Hybrid lifestyles can be nested:
        /// <code lang="cs"><![CDATA[
        /// var mixedLifetimeTransientLifestyle = Lifestyle.Hybrid(
        ///     () => container.GetCurrentLifetimeScope() != null,
        ///     new LifetimeScopeLifestyle(),
        ///     Lifestyle.Transient);
        /// 
        /// var mixedScopeLifestyle = Lifestyle.Hybrid(
        ///     () => HttpContext.Current != null,
        ///     new WebRequestLifestyle(),
        ///     mixedLifetimeTransientLifestyle);
        /// ]]></code>
        /// The <b>mixedScopeLifestyle</b> now mixed three lifestyles: Web Request, Lifetime Scope and 
        /// Transient.
        /// </example>
        public static Lifestyle Hybrid(Func<bool> test, Lifestyle trueLifestyle, Lifestyle falseLifestyle)
        {
            Requires.IsNotNull(test, "test");
            Requires.IsNotNull(trueLifestyle, "trueLifestyle");
            Requires.IsNotNull(falseLifestyle, "falseLifestyle");

            return new HybridLifestyle(test, trueLifestyle, falseLifestyle);
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of the
        /// specified <typeparamref name="TImplementation"/> with the caching as specified by this lifestyle.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="container">The <see cref="Container"/> instance for which a 
        /// <see cref="Registration"/> must be created.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/> is a null
        /// reference (Nothing in VB).</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Supplying the generic type arguments is needed, since internal types can not " +
                            "be created using the non-generic overloads in a sandbox.")]
        public Registration CreateRegistration<TService, TImplementation>(Container container)
            where TImplementation : class, TService
            where TService : class
        {
            Requires.IsNotNull(container, "container");

            return this.CreateRegistrationCore<TService, TImplementation>(container);
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of the
        /// specified <typeparamref name="TService"/> using the supplied <paramref name="instanceCreator"/> 
        /// with the caching as specified by this lifestyle.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <param name="instanceCreator">A delegate that will create a new instance of 
        /// <typeparamref name="TService"/> every time it is called.</param>
        /// <param name="container">The <see cref="Container"/> instance for which a 
        /// <see cref="Registration"/> must be created.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="instanceCreator"/> or
        /// <paramref name="container"/> are null references (Nothing in VB).</exception>
        public Registration CreateRegistration<TService>(Func<TService> instanceCreator,
            Container container)
            where TService : class
        {
            Requires.IsNotNull(instanceCreator, "instanceCreator");
            Requires.IsNotNull(container, "container");

            return this.CreateRegistrationCore<TService>(instanceCreator, container);
        }

        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of the
        /// specified <paramref name="implementationType"/> with the caching as specified by this lifestyle.
        /// This method might fail when run in a partial trust sandbox when <paramref name="implementationType"/>
        /// is an internal type.
        /// </summary>
        /// <param name="serviceType">The interface or base type that can be used to retrieve the instances.</param>
        /// <param name="implementationType">The concrete type that will be registered.</param>
        /// <param name="container">The <see cref="Container"/> instance for which a 
        /// <see cref="Registration"/> must be created.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when on of the supplied arguments is a null 
        /// reference (Nothing in VB).</exception>
        public Registration CreateRegistration(Type serviceType, Type implementationType, Container container)
        {
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(implementationType, "implementationType");
            Requires.IsNotNull(container, "container");

            Requires.IsReferenceType(serviceType, "serviceType");
            Requires.IsReferenceType(implementationType, "implementationType");

            Requires.ServiceIsAssignableFromImplementation(serviceType, implementationType, 
                "implementationType");

            var closedCreateRegistrationMethod = OpenCreateRegistrationTServiceTImplementationMethod
                .MakeGenericMethod(serviceType, implementationType);

            try
            {
                return (Registration)closedCreateRegistrationMethod.Invoke(this, new object[] { container });
            }
            catch (MemberAccessException ex)
            {
                throw BuildUnableToResolveTypeDueToSecurityConfigException(implementationType, ex, 
                    "implementationType");
            }
        }
       
        /// <summary>
        /// Creates a new <see cref="Registration"/> instance defining the creation of the
        /// specified <paramref name="serviceType"/>  using the supplied <paramref name="instanceCreator"/> 
        /// with the caching as specified by this lifestyle.
        /// </summary>
        /// <param name="serviceType">The interface or base type that can be used to retrieve the instances.</param>
        /// <param name="instanceCreator">The delegate that will be responsible for creating new instances.</param>
        /// <param name="container">The <see cref="Container"/> instance for which a 
        /// <see cref="Registration"/> must be created.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when on of the supplied arguments is a null 
        /// reference (Nothing in VB).</exception>
        public Registration CreateRegistration(Type serviceType, Func<object> instanceCreator,
            Container container)
        {
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(instanceCreator, "instanceCreator");
            Requires.IsNotNull(container, "container");

            Requires.IsReferenceType(serviceType, "serviceType");

            var closedCreateRegistrationMethod = OpenCreateRegistrationTServiceFuncMethod
                .MakeGenericMethod(serviceType);

            try
            {
                // Build the following delegate: () => (ServiceType)instanceCreator();
                var typeSafeInstanceCreator = ConvertDelegateToTypeSafeDelegate(serviceType, instanceCreator);
                
                return (Registration)closedCreateRegistrationMethod.Invoke(this, 
                    new object[] { typeSafeInstanceCreator, container });
            }
            catch (MemberAccessException ex)
            {
                throw BuildUnableToResolveTypeDueToSecurityConfigException(serviceType, ex, "serviceType");
            }
        }

        internal Registration CreateRegistration(Type serviceType, Type implementationType,
            Container container, IEnumerable<Tuple<ParameterInfo, Expression>> overriddenParameters)
        {
            var registration = this.CreateRegistration(serviceType, implementationType, container);

            registration.SetParameterOverrides(overriddenParameters);

            return registration;
        }
        
        /// <summary>
        /// When overridden in a derived class, 
        /// creates a new <see cref="Registration"/> instance defining the creation of the
        /// specified <typeparamref name="TImplementation"/> with the caching as specified by this lifestyle.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="container">The <see cref="Container"/> instance for which a 
        /// <see cref="Registration"/> must be created.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <remarks>
        /// If you are implementing your own lifestyle, override this method to implement the code necessary 
        /// to create and return a new <see cref="Registration"/>. Note that you should <b>always</b> create
        /// a new <see cref="Registration"/> instance. They should never be cached.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Supplying the generic type arguments is needed, since internal types can not " +
                            "be created using the non-generic overloads in a sandbox.")]
        protected abstract Registration CreateRegistrationCore<TService, TImplementation>(Container container)
            where TImplementation : class, TService
            where TService : class;

        /// <summary>
        /// When overridden in a derived class, 
        /// creates a new <see cref="Registration"/> instance defining the creation of the
        /// specified <typeparamref name="TService"/> using the supplied <paramref name="instanceCreator"/> 
        /// with the caching as specified by this lifestyle.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.</typeparam>
        /// <param name="instanceCreator">A delegate that will create a new instance of 
        /// <typeparamref name="TService"/> every time it is called.</param>
        /// <param name="container">The <see cref="Container"/> instance for which a 
        /// <see cref="Registration"/> must be created.</param>
        /// <returns>A new <see cref="Registration"/> instance.</returns>
        /// <remarks>
        /// If you are implementing your own lifestyle, override this method to implement the code necessary 
        /// to create and return a new <see cref="Registration"/>. Note that you should <b>always</b> create
        /// a new <see cref="Registration"/> instance. They should never be cached.
        /// </remarks>
        protected abstract Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container)
            where TService : class;

        private static object ConvertDelegateToTypeSafeDelegate(Type serviceType, Func<object> instanceCreator)
        {
            // Build the following delegate: () => (ServiceType)instanceCreator();
            var invocationExpression =
                Expression.Invoke(Expression.Constant(instanceCreator), new Expression[0]);

            var convertExpression = Expression.Convert(invocationExpression, serviceType);

            var parameters = new ParameterExpression[0];

            // This might throw an MemberAccessException when serviceType is internal while we're running in
            // a Silverlight sandbox.
            return Expression.Lambda(convertExpression, parameters).Compile();
        }

        private static ArgumentException BuildUnableToResolveTypeDueToSecurityConfigException(
            Type type, MemberAccessException innerException, string paramName)
        {
            // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
            return new ArgumentException(
                StringResources.UnableToResolveTypeDueToSecurityConfiguration(type, innerException),
#if !SILVERLIGHT
                paramName,
#endif
                innerException);
        }

        private static MethodInfo GetMethod(Expression<Action<Lifestyle>> methodCall)
        {
            var body = methodCall.Body as MethodCallExpression;
            return body.Method.GetGenericMethodDefinition();
        }
    }
}