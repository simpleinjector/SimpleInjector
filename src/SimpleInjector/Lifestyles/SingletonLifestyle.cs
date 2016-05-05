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

        protected override int Length => 1000;

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
                instance, Lifestyle.Singleton, container);
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

        protected override Registration CreateRegistrationCore<TService, TImplementation>(Container container) => 
            new SingletonLifestyleRegistration<TService, TImplementation>(this, container);

        protected override Registration CreateRegistrationCore<TService>(Func<TService> instanceCreator,
            Container container)
        {
            return new SingletonFuncLifestyleRegistration<TService>(instanceCreator, this, container);
        }

        private sealed class SingletonInstanceLifestyleRegistration : Registration
        {
            private readonly object originalInstance;
            private readonly Type serviceType;
            private readonly Type implementationType;
            private readonly Lazy<object> initializedInstance;

            internal SingletonInstanceLifestyleRegistration(Type serviceType, Type implementationType, 
                object instance, Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
                this.originalInstance = instance;
                this.serviceType = serviceType;
                this.implementationType = implementationType;

                // Default lazy behavior ensures that the initializer is guaranteed to be called just once.
                this.initializedInstance = new Lazy<object>(this.GetInjectedInterceptedAndInitializedInstance);
            }

            public override Type ImplementationType => this.implementationType;

            public override Expression BuildExpression() => 
                Expression.Constant(this.initializedInstance.Value, this.serviceType);

            private object GetInjectedInterceptedAndInitializedInstance()
            {
                try
                {
                    return this.GetInjectedInterceptedAndInitializedInstanceInternal();
                }
                catch (MemberAccessException ex)
                {
                    throw new ActivationException(
                        StringResources.UnableToResolveTypeDueToSecurityConfiguration(this.serviceType, ex));
                }
            }

            private object GetInjectedInterceptedAndInitializedInstanceInternal()
            {
                Expression expression = Expression.Constant(this.originalInstance, this.serviceType);

                expression = this.WrapWithPropertyInjector(this.serviceType, this.serviceType, expression);
                expression = this.InterceptInstanceCreation(this.serviceType, this.serviceType, expression);
                expression = this.WrapWithInitializer(this.serviceType, this.serviceType, expression);

                var initializer = Expression.Lambda(expression).Compile();

                // This delegate might return a different instance than the originalInstance (caused by a
                // possible interceptor).
                return initializer.DynamicInvoke();
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

            public override Type ImplementationType => typeof(TService);

            protected override Expression BuildTransientExpression() => 
                this.BuildTransientExpression(this.instanceCreator);
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

            public override Type ImplementationType => typeof(TImplementation);

            protected override Expression BuildTransientExpression() => 
                this.BuildTransientExpression<TService, TImplementation>();
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
                // created. Since the same Registration instance can be used by multiple InstanceProducers,
                // we absolutely need this protection.
                this.lazyInstance = new Lazy<TService>(this.CreateInstanceWithNullCheck);
            }

            public override Expression BuildExpression() => 
                Expression.Constant(this.lazyInstance.Value, typeof(TService));

            protected abstract Expression BuildTransientExpression();

            private TService CreateInstanceWithNullCheck()
            {
                var expression = this.BuildTransientExpression();

                Func<TService> func = CompileExpression(expression);

                var instance = func();

                EnsureInstanceIsNotNull(instance);

                IDisposable disposable = instance as IDisposable;

                if (disposable != null)
                {
                    this.Container.RegisterForDisposal(disposable);
                }

                return instance;
            }

            private static Func<TService> CompileExpression(Expression expression)
            {
                try
                {
                    // Don't call BuildTransientDelegate, because that might do optimizations that are simply
                    // not needed, since the delegate will be called just once.
                    return CompilationHelpers.CompileLambda<TService>(expression);
                }
                catch (Exception ex)
                {
                    string message = StringResources.ErrorWhileBuildingDelegateFromExpression(
                        typeof(TService), expression, ex);

                    throw new ActivationException(message, ex);
                }
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