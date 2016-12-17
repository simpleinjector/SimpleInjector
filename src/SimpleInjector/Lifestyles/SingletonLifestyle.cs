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

            return new SingletonInstanceLifestyleRegistration(implementationType ?? serviceType, 
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

        protected override Registration CreateRegistrationCore<TConcrete>(Container container) => 
            new SingletonLifestyleRegistration<TConcrete>(container);

        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container)
        {
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            return new SingletonLifestyleRegistration<TService>(container, instanceCreator);
        }

        private sealed class SingletonInstanceLifestyleRegistration : Registration
        {
            private readonly object locker = new object();
            private readonly object originalInstance;

            private object interceptedInstance;

            internal SingletonInstanceLifestyleRegistration(Type implementationType, 
                object instance, Container container)
                : base(Lifestyle.Singleton, container)
            {
                this.originalInstance = instance;
                this.ImplementationType = implementationType;
            }

            public override Type ImplementationType { get; }

            public override Expression BuildExpression(InstanceProducer producer)
            {
                // NOTE: The ConstantExpression should define the service type, not the implementation type, 
                // because this implementation type might be internal, and this can cause problems in partial 
                // trust.
                return Expression.Constant(
                    this.GetInterceptedInstance(producer),
                    producer.ServiceType);
            }

            private object GetInterceptedInstance(InstanceProducer producer)
            {
                if (this.interceptedInstance == null)
                {
                    lock (this.locker)
                    {
                        if (this.interceptedInstance == null)
                        {
                            // TODO: It's wrong to use the InstanceProducer to build up the instance, because there could be
                            // multiple InstanceProducers for this instance.
                            this.interceptedInstance =
                                this.GetInjectedInterceptedAndInitializedInstance(producer);
                        }
                    }
                }

                return this.interceptedInstance;
            }

            private object GetInjectedInterceptedAndInitializedInstance(InstanceProducer producer)
            {
                try
                {
                    return this.GetInjectedInterceptedAndInitializedInstanceInternal(producer);
                }
                catch (MemberAccessException ex)
                {
                    throw new ActivationException(
                        StringResources.UnableToResolveTypeDueToSecurityConfiguration(this.ImplementationType, ex));
                }
            }

            private object GetInjectedInterceptedAndInitializedInstanceInternal(InstanceProducer producer)
            {
                Expression expression = Expression.Constant(this.originalInstance, producer.ServiceType);

                expression = this.WrapWithPropertyInjector(producer.ServiceType, this.ImplementationType, expression);
                expression = this.InterceptInstanceCreation(producer.ServiceType, this.ImplementationType, expression);
                expression = this.WrapWithInitializer(producer, producer.ServiceType, this.ImplementationType, expression);

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

            public override Expression BuildExpression(InstanceProducer producer)
            {
                return Expression.Constant(
                    this.GetInterceptedInstance(producer),
                    typeof(TImplementation));
            }

            private object GetInterceptedInstance(InstanceProducer producer)
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
                            this.interceptedInstance = this.CreateInstanceWithNullCheck(producer);
                        }
                    }
                }

                return this.interceptedInstance;
            }

            private TImplementation CreateInstanceWithNullCheck(InstanceProducer producer)
            {
                Expression expression =
                    this.instanceCreator == null
                        ? this.BuildTransientExpression(producer)
                        : this.BuildTransientExpression(producer, this.instanceCreator);

                Func<TImplementation> func = CompileExpression(expression);

                TImplementation instance = func();

                EnsureInstanceIsNotNull(instance);

                var disposable = instance as IDisposable;

                if (disposable != null)
                {
                    this.Container.RegisterForDisposal(disposable);
                }

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