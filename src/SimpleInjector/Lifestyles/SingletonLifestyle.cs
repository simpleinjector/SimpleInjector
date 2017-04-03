#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using SimpleInjector.Internals;

    internal sealed class SingletonLifestyle : Lifestyle
    {
        internal SingletonLifestyle() : base("Singleton")
        {
        }

        public override int Length => 1000;

        // This method seems somewhat redundant, since the exact same can be achieved by calling
        // Lifetime.Singleton.CreateRegistration(serviceType, () => instance, container). Calling that method
        // however, will trigger the ExpressionBuilding event later on where the supplied Expression is an
        // InvocationExpression calling that internally created Func<TService>. By having this extra method
        // (and the extra SingletonInstanceLifestyleRegistration class), we can ensure that the
        // ExpressionBuilding event is called with a ConstantExpression, which is much more intuitive to
        // anyone handling that event.
        internal static Registration CreateSingleInstanceRegistration(Type serviceType, object instance,
            Container container, Type implementationType = null)
        {
            Requires.IsNotNull(instance, nameof(instance));

            return new SingletonInstanceLifestyleRegistration(serviceType, implementationType ?? serviceType,
                instance, container);
        }

        internal static InstanceProducer CreateUncontrolledCollectionProducer(Type itemType, 
            IEnumerable collection, Container container)
        {
            return new InstanceProducer(
                typeof(IEnumerable<>).MakeGenericType(itemType),
                CreateUncontrolledCollectionRegistration(itemType, collection, container));
        }

        internal static Registration CreateUncontrolledCollectionRegistration(Type itemType, 
            IEnumerable collection, Container container)
        {
            Type enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);

            var registration = CreateSingleInstanceRegistration(enumerableType, collection, container);

            registration.IsCollection = true;

            return registration;
        }

        internal static bool IsSingletonInstanceRegistration(Registration registration) => 
            registration is SingletonInstanceLifestyleRegistration;

        protected internal override Registration CreateRegistrationCore<TConcrete>(Container container) => 
            new SingletonLifestyleRegistration<TConcrete>(container);

        protected internal override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container)
        {
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            return new SingletonLifestyleRegistration<TService>(container, instanceCreator);
        }

        private sealed class SingletonInstanceLifestyleRegistration : Registration
        {
            private readonly object locker = new object();

            private object instance;
            private bool initialized;

            internal SingletonInstanceLifestyleRegistration(Type serviceType, Type implementationType, 
                object instance, Container container)
                : base(Lifestyle.Singleton, container)
            {
                this.instance = instance;
                this.ServiceType = serviceType;
                this.ImplementationType = implementationType;
            }

            public Type ServiceType { get; }
            public override Type ImplementationType { get; }

            public override Expression BuildExpression()
            {
                return Expression.Constant(this.GetInitializedInstance(), this.ImplementationType);
            }

            private object GetInitializedInstance()
            {
                if (!this.initialized)
                {
                    lock (this.locker)
                    {
                        if (!this.initialized)
                        {
                            this.instance = this.GetInjectedInterceptedAndInitializedInstance();

                            this.initialized = true;
                        }
                    }
                }

                return this.instance;
            }

            private object GetInjectedInterceptedAndInitializedInstance()
            {
                try
                {
                    return this.GetInjectedInterceptedAndInitializedInstanceInternal();
                }
                catch (MemberAccessException ex)
                {
                    throw new ActivationException(
                        StringResources.UnableToResolveTypeDueToSecurityConfiguration(this.ImplementationType, ex));
                }
            }

            private object GetInjectedInterceptedAndInitializedInstanceInternal()
            {
                Expression expression = Expression.Constant(this.instance, this.ImplementationType);

                // NOTE: We pass on producer.ServiceType as the implementation type for the following three
                // methods. This will the initialization to be only done based on information of the service
                // type; not on that of the implementation. Although now the initialization could be 
                // incomplete, this behavior is consistent with the initialization of 
                // Register<TService>(Func<TService>, Lifestyle), which doesn't have the proper static type
                // information available to use the implementation type.
                // TODO: This behavior should be reconsidered, because now it is incompatible with
                // Register<TService, TImplementation>(Lifestyle). So the question is, do we consider
                // RegisterSingleton<TService>(TService) to be similar to Register<TService>(Func<TService>)
                // or to Register<TService, TImplementation>()? See: #353.
                expression = this.WrapWithPropertyInjector(this.ServiceType, expression);
                expression = this.InterceptInstanceCreation(this.ServiceType, expression);
                expression = this.WrapWithInitializer(this.ServiceType, expression);

                // Optimization: We don't need to compile a delegate in case all we have is a constant.
                if (expression is ConstantExpression)
                {
                    return ((ConstantExpression)expression).Value;
                }

                Delegate initializer = Expression.Lambda(expression).Compile();

                // This delegate might return a different instance than the originalInstance (caused by a
                // possible interceptor).
                return initializer.DynamicInvoke();
            }
        }

        private sealed class SingletonLifestyleRegistration<TImplementation> : Registration 
            where TImplementation : class
        {
            private readonly object locker = new object();
            private readonly Func<TImplementation> instanceCreator;

            private object interceptedInstance;

            public SingletonLifestyleRegistration(Container container, Func<TImplementation> instanceCreator = null)
                : base(Lifestyle.Singleton, container)
            {
                this.instanceCreator = instanceCreator;
            }

            public override Type ImplementationType => typeof(TImplementation);

            public override Expression BuildExpression() => 
                Expression.Constant(this.GetInterceptedInstance(), this.ImplementationType);

            private object GetInterceptedInstance()
            {
                // Even though the InstanceProducer takes a lock before calling Registration.BuildExpression
                // we need to take a lock here, because multiple InstanceProducer instances could reference
                // the same Registration and call this code in parallel.
                if (this.interceptedInstance == null)
                {
                    lock (this.locker)
                    {
                        if (this.interceptedInstance == null)
                        {
                            this.interceptedInstance = this.CreateInstanceWithNullCheck();

                            var disposable = this.interceptedInstance as IDisposable;

                            if (disposable != null)
                            {
                                this.Container.RegisterForDisposal(disposable);
                            }
                        }
                    }
                }

                return this.interceptedInstance;
            }

            private TImplementation CreateInstanceWithNullCheck()
            {
                Expression expression =
                    this.instanceCreator == null
                        ? this.BuildTransientExpression()
                        : this.BuildTransientExpression(this.instanceCreator);

                Func<TImplementation> func = CompileExpression(expression);

                TImplementation instance = func();

                EnsureInstanceIsNotNull(instance);

                return instance;
            }

            private static Func<TImplementation> CompileExpression(Expression expression)
            {
                try
                {
                    // Don't call BuildTransientDelegate, because that might do optimizations that are simply
                    // not needed, since the delegate will be called just once.
                    return CompilationHelpers.CompileLambda<TImplementation>(expression);
                }
                catch (Exception ex)
                {
                    string message = StringResources.ErrorWhileBuildingDelegateFromExpression(
                        typeof(TImplementation), expression, ex);

                    throw new ActivationException(message, ex);
                }
            }
            
            private static void EnsureInstanceIsNotNull(object instance)
            {
                if (instance == null)
                {
                    throw new ActivationException(
                        StringResources.DelegateForTypeReturnedNull(typeof(TImplementation)));
                }
            }
        }
    }
}