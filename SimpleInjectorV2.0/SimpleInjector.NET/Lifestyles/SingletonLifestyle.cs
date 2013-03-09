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
    using System.Threading;

    internal sealed class SingletonLifestyle : Lifestyle
    {
        internal SingletonLifestyle() : base("Singleton")
        {
        }

        protected override int Length
        {
            get { return 1000; }
        }

        // This method seems somewhat redundant, since the exact same can be achieved by calling
        // Lifetime.Singleton.CreateRegistration(serviceType, () => instance, container). Calling that method
        // however, will trigger the ExpressionBuilding event later on where the supplied Expression is an
        // InvocationExpression calling that internally created Func<TService>. By having this extra method
        // (and the extra SingletonInstanceLifestyleRegistration class), we can ensure that the
        // ExpressionBuilding event is called with a ConstantExpression, which is much more intuitive to
        // anyone handling that event.
        internal static Registration CreateSingleRegistration(Type serviceType, object instance, 
            Container container)
        {
            Requires.IsNotNull(instance, "instance");

            return new SingletonInstanceLifestyleRegistration(serviceType, instance, Lifestyle.Singleton, 
                container);
        }

        protected override Registration CreateRegistrationCore<TService, TImplementation>(
            Container container)
        {
            return new SingletonLifestyleRegistration<TService, TImplementation>(this, container);
        }

        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container)
        {
            return new SingletonFuncLifestyleRegistration<TService>(instanceCreator, this, container);
        }

        private sealed class SingletonInstanceLifestyleRegistration : Registration
        {
            private readonly object locker = new object();
            private readonly Type serviceType;
            private readonly object instance;
            private bool initializerRan;

            internal SingletonInstanceLifestyleRegistration(Type serviceType, object instance, 
                Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
                this.serviceType = serviceType;
                this.instance = instance;
            }

            public override Type ImplementationType
            {
                get { return this.instance.GetType(); }
            }

            public override Expression BuildExpression()
            {
                this.EnsureInitializerHasRun();

                var constantExpression = Expression.Constant(this.instance, this.serviceType);

                return this.InterceptInstanceCreation(this.serviceType, this.serviceType, constantExpression);
            }

            private void EnsureInitializerHasRun()
            {
                // Since the instance is supplied from the outside, we have to run the initializer ourself.
                // The base class can't do this for us.
                if (!this.initializerRan)
                {
                    // Even though the InstanceProducer takes a lock before calling Registration.BuildExpression
                    // we want to be very sure that this instance will never be initialized more than once,
                    // because of the possible side effects that this might cause in user code.
                    lock (this.locker)
                    {
                        if (!this.initializerRan)
                        {
                            this.RunInitializer();

                            this.initializerRan = true;
                        }
                    }
                }
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
        
        private sealed class SingletonFuncLifestyleRegistration<TService> 
            : SingletonLifestyleRegistrationBase<TService>
            where TService : class
        {
            private Func<TService> instanceCreator;

            internal SingletonFuncLifestyleRegistration(Func<TService> instanceCreator, Lifestyle lifestyle,
                Container container)
                : base(lifestyle, container)
            {
                this.instanceCreator = instanceCreator;
            }

            public override Type ImplementationType
            {
                get { return typeof(TService); }
            }

            protected override TService CreateInstance()
            {
                return this.BuildTransientDelegate<TService>(this.instanceCreator)();
            }
        }

        private class SingletonLifestyleRegistration<TService, TImplementation>
            : SingletonLifestyleRegistrationBase<TService>
            where TImplementation : class, TService
            where TService : class
        {
            public SingletonLifestyleRegistration(Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
            }

            public override Type ImplementationType
            {
                get { return typeof(TImplementation); }
            }

            protected override TService CreateInstance()
            {
                return this.BuildTransientDelegate<TService, TImplementation>()();
            }
        }

        private abstract class SingletonLifestyleRegistrationBase<TService> : Registration 
            where TService : class
        {
            private readonly Lazy<TService> lazyInstance;

            protected SingletonLifestyleRegistrationBase(Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
                // Even though the InstanceProducer takes a lock before calling Registration.BuildExpression
                // we want to be very sure that there will never be more than one instance of a singleton
                // created. Since the same Registration instance can be used by multipl InstanceProducers,
                // we absolutely need this protection.
                this.lazyInstance = new Lazy<TService>(this.CreateInstanceWithNullCheck, 
                    LazyThreadSafetyMode.ExecutionAndPublication);
            }

            public override Expression BuildExpression()
            {
                return Expression.Constant(this.lazyInstance.Value, typeof(TService));
            }

            protected abstract TService CreateInstance();

            private TService CreateInstanceWithNullCheck()
            {
                var instance = this.CreateInstance();

                EnsureInstanceIsNotNull(instance);

                return instance;
            }

            private static void EnsureInstanceIsNotNull(object instance)
            {
                if (instance == null)
                {
                    throw new ActivationException(
                        StringResources.DelegateForTypeReturnedNull(typeof(TService)));
                }
            }
        }
    }
}