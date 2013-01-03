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

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    
    public abstract class Lifestyle
    {
        public static readonly Lifestyle Transient = new TransientLifestyle();
        public static readonly Lifestyle Singleton = new SingletonLifestyle();

        private static readonly MethodInfo OpenCreateRegistrationTServiceTImplementationMethod =
            GetMethod(lifestyle => lifestyle.CreateRegistration<object, object>(null));

        private static readonly MethodInfo OpenCreateRegistrationTServiceMethod =
            GetMethod(lifestyle => lifestyle.CreateRegistration<object>(null, null));

        protected Lifestyle()
        {
        }

        // TODO: Make virtual to make implementing a lifestyle easier.
        public abstract LifestyleRegistration CreateRegistration<TService, TImplementation>(
            Container container)
            where TImplementation : class, TService
            where TService : class;

        public abstract LifestyleRegistration CreateRegistration<TService>(Func<TService> instanceCreator, 
            Container container)
            where TService : class;

        public LifestyleRegistration CreateRegistration(Type serviceType, Type implementationType,
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
                return (LifestyleRegistration)
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

        internal void OnRegistration(Container container)
        {
            this.OnRegistration(new LifestyleRegistrationEventArgs(container));
        }

        protected virtual void OnRegistration(LifestyleRegistrationEventArgs e)
        {
        }

        private static MethodInfo GetMethod(Expression<Action<Lifestyle>> methodCall)
        {
            var body = methodCall.Body as MethodCallExpression;
            return body.Method.GetGenericMethodDefinition();
        }
    }
}