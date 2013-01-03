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
    
    internal sealed class SingletonLifestyle : Lifestyle
    {
        public override LifestyleRegistration CreateRegistration<TService, TImplementation>(
            Container container)
        {
            Requires.IsNotNull(container, "container");

            return new SingletonLifestyleRegistration<TService, TImplementation>(container);
        }

        public override LifestyleRegistration CreateRegistration<TService>(
            Func<TService> instanceCreator, Container container)
        {
            Requires.IsNotNull(instanceCreator, "instanceCreator");
            Requires.IsNotNull(container, "container");

            return new SingletonFuncLifestyleRegistration<TService>(instanceCreator, container);
        }

        internal static LifestyleRegistration CreateRegistration(Type serviceType, object instance, 
            Container container)
        {
            return new SingletonLifestyleRegistration(serviceType, instance, container);
        }

        private static Expression BuildConstantExpression<TService>(Func<TService> getInstance)
        {
            var instance = getInstance();

            if (instance == null)
            {
                throw new ActivationException(StringResources.DelegateForTypeReturnedNull(typeof(TService)));
            }

            return Expression.Constant(instance, typeof(TService));
        }

        private sealed class SingletonLifestyleRegistration : LifestyleRegistration
        {
            private readonly Type serviceType;
            private readonly object instance;

            internal SingletonLifestyleRegistration(Type serviceType, object instance, Container container)
                : base(container)
            {
                this.serviceType = serviceType;
                this.instance = instance;
            }

            public override Expression BuildExpression()
            {
                // The lock in InstanceProducer.BuildExpression ensures this expression is built just once and
                // therefore only runs the initializer once.
                this.RunInitializer();

                var constantExpression = Expression.Constant(this.instance, this.serviceType);

                return this.InterceptExpression(this.serviceType, constantExpression);
            }

            private void RunInitializer()
            {
                Action<object> initializer = this.Container.GetInitializer(this.serviceType);

                if (initializer != null)
                {
                    initializer(this.instance);
                }
            }
        }

        private sealed class SingletonFuncLifestyleRegistration<TService> : LifestyleRegistration
            where TService : class
        {
            private readonly Func<TService> instanceCreator;

            internal SingletonFuncLifestyleRegistration(Func<TService> instanceCreator, Container container)
                : base(container)
            {
                this.instanceCreator = instanceCreator;
            }

            public override Expression BuildExpression()
            {
                var getInstance = this.BuildTransientDelegate<TService>(this.instanceCreator);

                return SingletonLifestyle.BuildConstantExpression<TService>(getInstance);
            }
        }

        private class SingletonLifestyleRegistration<TService, TImplementation> : LifestyleRegistration
            where TImplementation : class, TService
            where TService : class
        {
            public SingletonLifestyleRegistration(Container container) : base(container)
            {
            }

            public override Expression BuildExpression()
            {
                var getInstance = this.BuildTransientDelegate<TService, TImplementation>();

                return SingletonLifestyle.BuildConstantExpression<TService>(getInstance);
            }
        }
    }
}