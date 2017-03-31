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

namespace SimpleInjector.Decorators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Internals;
    using SimpleInjector.Lifestyles;

    // This class allows decorating collections of services with elements that are created out of the control
    // of the container. Collections are registered using the following methods:
    // -RegisterCollection<TService>(IEnumerable<TService> uncontrolledCollection)
    // -Register<TService>(TService) (where TService is a IEnumerable<T>)
    // -RegisterCollection(Type serviceType, IEnumerable uncontrolledCollection).
    internal sealed class ContainerUncontrolledServicesDecoratorInterceptor : DecoratorExpressionInterceptor
    {
        private static readonly object ContainerItemsKeyAndLock = new object();

        private readonly Dictionary<InstanceProducer, IEnumerable> singletonDecoratedCollectionsCache;
        private readonly ExpressionBuiltEventArgs e;
        private readonly Type registeredServiceType;

        private ConstructorInfo decoratorConstructor;
        private Type decoratorType;

        public ContainerUncontrolledServicesDecoratorInterceptor(DecoratorExpressionInterceptorData data,
            Dictionary<InstanceProducer, IEnumerable> singletonDecoratedCollectionsCache,
            ExpressionBuiltEventArgs e, Type registeredServiceType)
            : base(data)
        {
            this.singletonDecoratedCollectionsCache = singletonDecoratedCollectionsCache;
            this.e = e;
            this.registeredServiceType = registeredServiceType;
        }

        protected override Dictionary<InstanceProducer, ServiceTypeDecoratorInfo> ThreadStaticServiceTypePredicateCache
        {
            get { return this.GetThreadStaticServiceTypePredicateCacheByKey(ContainerItemsKeyAndLock); }
        }

        internal bool SatisfiesPredicate()
        {
            // We don't have an expression at this point, since the instances are not created by the container.
            // Therefore we fake an expression so it can still be passed on to the predicate the user might
            // have defined.
            var expression = Expression.Constant(null, this.registeredServiceType);

            var registration = new ExpressionRegistration(expression, this.registeredServiceType,
                Lifestyle.Unknown, this.Container);

            registration.ReplaceRelationships(this.e.InstanceProducer.GetRelationships());

            this.Context = this.CreatePredicateContext(this.e.InstanceProducer, registration,
                this.registeredServiceType, expression);

            return this.SatisfiesPredicate(this.Context);
        }

        internal void SetDecorator(Type decorator)
        {
            this.decoratorConstructor = this.Container.Options.SelectConstructor(decorator);

            if (object.ReferenceEquals(this.Lifestyle, this.Container.SelectionBasedLifestyle))
            {
                this.Lifestyle = this.Container.Options.SelectLifestyle(decorator);
            }

            // The actual decorator could be different. TODO: must... write... test... for... this.
            this.decoratorType = this.decoratorConstructor.DeclaringType;
        }

        internal void ApplyDecorator()
        {
            var registration = new ExpressionRegistration(this.e.Expression, this.registeredServiceType,
                Lifestyle.Unknown, this.Container);

            registration.ReplaceRelationships(this.e.InstanceProducer.GetRelationships());

            var serviceTypeInfo = this.GetServiceTypeInfo(this.e.Expression, this.e.InstanceProducer,
                registration, this.registeredServiceType);

            Registration decoratorRegistration;

            var decoratedExpression = this.BuildDecoratorExpression(out decoratorRegistration);

            this.e.Expression = decoratedExpression;

            // Add the decorator to the list of applied decorator. This way users can use this
            // information in the predicate of the next decorator they add.
            serviceTypeInfo.AddAppliedDecorator(this.registeredServiceType, this.decoratorType, 
                this.Container, this.Lifestyle, decoratedExpression);

            this.e.KnownRelationships.AddRange(decoratorRegistration.GetRelationships());
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
            Justification = "This is not a performance critical path.")]
        private Expression BuildDecoratorExpression(out Registration decoratorRegistration)
        {
            this.ThrowWhenDecoratorNeedsAFunc();
            this.ThrownWhenLifestyleIsNotSupported();

            ParameterExpression parameter = Expression.Parameter(this.registeredServiceType, "decoratee");

            decoratorRegistration = this.CreateRegistrationForUncontrolledCollection(parameter);

            Expression parameterizedDecoratorExpression = decoratorRegistration.BuildExpression();

            // TODO: Optimize for performance by using a dynamic assembly where possible.
            Delegate wrapInstanceWithDecorator =
                this.BuildDecoratorWrapper(parameter, parameterizedDecoratorExpression)
                .Compile();

            Expression originalEnumerableExpression = this.e.Expression;

            if (originalEnumerableExpression is ConstantExpression)
            {
                var collection = ((ConstantExpression)originalEnumerableExpression).Value as IEnumerable;

                return this.BuildDecoratorEnumerableExpressionForConstantEnumerable(wrapInstanceWithDecorator,
                    collection);
            }
            else
            {
                return this.BuildDecoratorEnumerableExpressionForNonConstantExpression(
                    wrapInstanceWithDecorator, originalEnumerableExpression);
            }
        }

        private Registration CreateRegistrationForUncontrolledCollection(Expression decorateeExpression)
        {
            var overriddenParameters = this.CreateOverriddenParameters(decorateeExpression);

            // Create the decorator as transient. Caching is applied later on.
            return Lifestyle.Transient.CreateDecoratorRegistration(
                this.decoratorConstructor.DeclaringType, this.Container, overriddenParameters);
        }

        private OverriddenParameter[] CreateOverriddenParameters(Expression decorateeExpression)
        {
            ParameterInfo decorateeParameter =
                GetDecorateeParameter(this.registeredServiceType, this.decoratorConstructor);

            decorateeExpression =
                this.GetExpressionForDecorateeDependencyParameterOrNull(
                    decorateeParameter, this.registeredServiceType, decorateeExpression);

            var currentProducer = this.GetServiceTypeInfo(this.e).GetCurrentInstanceProducer();

            var decorateeOverriddenParameter =
                new OverriddenParameter(decorateeParameter, decorateeExpression, currentProducer);

            IEnumerable<OverriddenParameter> predicateContextOverriddenParameters =
                this.CreateOverriddenDecoratorContextParameters(currentProducer);

            var overriddenParameters = (new[] { decorateeOverriddenParameter })
                .Concat(predicateContextOverriddenParameters);

            return overriddenParameters.ToArray();
        }

        private IEnumerable<OverriddenParameter> CreateOverriddenDecoratorContextParameters(
            InstanceProducer currentProducer)
        {
            return
                from parameter in this.decoratorConstructor.GetParameters()
                where parameter.ParameterType == typeof(DecoratorContext)
                let contextExpression = Expression.Constant(new DecoratorContext(this.Context))
                select new OverriddenParameter(parameter, contextExpression, currentProducer);
        }

        // Creates an expression that calls a Func<T, T> delegate that takes in the service and returns
        // that instance, wrapped with the decorator.
        private LambdaExpression BuildDecoratorWrapper(ParameterExpression parameter,
            Expression decoratorExpression)
        {
            Type funcType =
                typeof(Func<,>).MakeGenericType(this.registeredServiceType, this.registeredServiceType);

            return Expression.Lambda(funcType, decoratorExpression, parameter);
        }

        private Expression BuildDecoratorEnumerableExpressionForConstantEnumerable(
            Delegate wrapInstanceWithDecoratorDelegate, IEnumerable collection)
        {
            // Build the query: from item in collection select wrapInstanceWithDecorator(item);
            IEnumerable decoratedCollection =
                collection.Select(this.registeredServiceType, wrapInstanceWithDecoratorDelegate);

            // Passing the enumerable type is needed when running in the Silverlight sandbox.
            Type enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(this.registeredServiceType);

            if (this.Lifestyle == Lifestyle.Singleton)
            {
                Func<IEnumerable> collectionCreator = () =>
                {
                    Array array = ToArray(this.registeredServiceType, decoratedCollection);
                    return DecoratorHelpers.MakeReadOnly(this.registeredServiceType, array);
                };

                IEnumerable singleton = this.GetSingletonDecoratedCollection(collectionCreator);

                return Expression.Constant(singleton, enumerableServiceType);
            }

            return Expression.Constant(decoratedCollection, enumerableServiceType);
        }

        private Expression BuildDecoratorEnumerableExpressionForNonConstantExpression(
            Delegate wrapInstanceWithDecorator, Expression expression)
        {
            // Build the query: from item in expression select wrapInstanceWithDecorator(item);
            var callExpression =
                DecoratorHelpers.Select(expression, this.registeredServiceType, wrapInstanceWithDecorator);

            if (this.Lifestyle == Lifestyle.Singleton)
            {
                Type enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(this.registeredServiceType);

                Func<IEnumerable> collectionCreator = () =>
                {
                    Type funcType = typeof(Func<>).MakeGenericType(enumerableServiceType);
                    Delegate lambda = Expression.Lambda(funcType, callExpression).Compile();
                    var decoratedCollection = (IEnumerable)lambda.DynamicInvoke();
                    Array array = ToArray(this.registeredServiceType, decoratedCollection);
                    return DecoratorHelpers.MakeReadOnly(this.registeredServiceType, array);
                };

                IEnumerable singleton = this.GetSingletonDecoratedCollection(collectionCreator);

                // Passing the enumerable type is needed when running in a (Silverlight) sandbox.
                return Expression.Constant(singleton, enumerableServiceType);
            }

            return callExpression;
        }

        private void ThrowWhenDecoratorNeedsAFunc()
        {
            bool needsADecorateeFactory = this.DecoratorNeedsADecorateeFactory();

            if (needsADecorateeFactory)
            {
                string message = StringResources.CantGenerateFuncForDecorator(this.registeredServiceType,
                    this.DecoratorTypeDefinition);

                throw new ActivationException(message);
            }
        }

        private bool DecoratorNeedsADecorateeFactory() => (
            from parameter in this.decoratorConstructor.GetParameters()
            where IsDecorateeFactoryDependencyParameter(parameter, this.registeredServiceType)
            select parameter)
            .Any();

        private void ThrownWhenLifestyleIsNotSupported()
        {
            // Because the user registered an IEnumerable<TService>, this collection can be dynamic in nature,
            // and the number of elements could change on each enumeration. It's impossible to detect if a
            // returned element is supposed to be a new element and should get its own new decorator, or if
            // it is supposed to be an existing element, for which an already cached decorator can be used.
            // In fact we can't really cache elements as Singleton, but since this was already supported in
            // the past, we don't want to introduce (yet another) breaking change.
            if (this.Lifestyle != Lifestyle.Transient && this.Lifestyle != Lifestyle.Singleton)
            {
                throw new NotSupportedException(
                    StringResources.CanNotDecorateContainerUncontrolledCollectionWithThisLifestyle(
                        this.DecoratorTypeDefinition, this.Lifestyle, this.registeredServiceType));
            }
        }

        private IEnumerable GetSingletonDecoratedCollection(Func<IEnumerable> collectionCreator)
        {
            lock (this.singletonDecoratedCollectionsCache)
            {
                IEnumerable collection;

                if (!this.singletonDecoratedCollectionsCache.TryGetValue(this.e.InstanceProducer,
                    out collection))
                {
                    collection = collectionCreator();

                    this.singletonDecoratedCollectionsCache[this.e.InstanceProducer] = collection;
                }

                return collection;
            }
        }

        private static Array ToArray(Type elementType, IEnumerable source)
        {
            object[] collection = source.Cast<object>().ToArray();
            Array array = Array.CreateInstance(elementType, collection.Length);
            Array.Copy(collection, array, collection.Length);

            return array;
        }
    }
}