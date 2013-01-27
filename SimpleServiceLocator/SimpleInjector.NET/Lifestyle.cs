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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Lifestyles;

    [DebuggerDisplay("{Name,nq}")]
    public abstract class Lifestyle
    {
        /// <summary>
        /// A new instance of the specified service type is created every time it is requested or injected.
        /// </summary>
        public static readonly Lifestyle Transient = new TransientLifestyle();

        /// <summary>
        /// Ensures that only one instance of the specified service type is created within the context of the
        /// given container instance.
        /// </summary>
        public static readonly Lifestyle Singleton = new SingletonLifestyle();

        internal static readonly Lifestyle Unknown = new UnknownLifestyle();

        private static readonly MethodInfo OpenCreateRegistrationTServiceTImplementationMethod =
            GetMethod(lifestyle => lifestyle.CreateRegistration<object, object>(null));
        private static readonly MethodInfo OpenCreateRegistrationTServiceFuncMethod =
            GetMethod(lifestyle => lifestyle.CreateRegistration<object>(null, null));

        private readonly string name;

        protected Lifestyle(string name)
        {
            Requires.IsNotNullOrEmpty(name, "name");

            this.name = name;
        }

        public string Name 
        { 
            get { return this.name; } 
        }

        internal virtual int ComponentLength 
        {
            get { return this.Length; }
        }

        internal virtual int DependencyLength 
        {
            get { return this.Length; }
        }

        protected abstract int Length
        {
            get;
        }

        // TODO: Make virtual to make implementing a lifestyle easier.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Supplying the generic type arguments is needed, since internal types can not " + 
                            "be created using the non-generic overloads in a sandbox.")]
        public abstract Registration CreateRegistration<TService, TImplementation>(Container container)
            where TImplementation : class, TService
            where TService : class;

        public abstract Registration CreateRegistration<TService>(Func<TService> instanceCreator, 
            Container container)
            where TService : class;

        public Registration CreateRegistration(Type serviceType, Type implementationType,
            Container container)
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
                return (Registration)
                    closedCreateRegistrationMethod.Invoke(this, new object[] { container });
            }
            catch (MemberAccessException ex)
            {
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                throw new ArgumentException(
                    StringResources.UnableToResolveTypeDueToSecurityConfiguration(implementationType, ex),
#if !SILVERLIGHT
                    "implementationType", 
#endif
                    ex);
            }
        }

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
                // This happens when the user tries to resolve an internal type inside a (Silverlight) sandbox.
                throw new ArgumentException(
                    StringResources.UnableToResolveTypeDueToSecurityConfiguration(serviceType, ex),
#if !SILVERLIGHT
                    "serviceType",
#endif
                    ex);
            }
        }

        internal void OnRegistration(Container container)
        {
            this.OnRegistration(new LifestyleRegistrationEventArgs(container));
        }

        protected virtual void OnRegistration(LifestyleRegistrationEventArgs e)
        {
        }

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

        private static MethodInfo GetMethod(Expression<Action<Lifestyle>> methodCall)
        {
            var body = methodCall.Body as MethodCallExpression;
            return body.Method.GetGenericMethodDefinition();
        }
    }
}