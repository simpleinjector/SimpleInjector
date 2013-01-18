#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2010 S. van Deursen
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

namespace SimpleInjector.Extensions.Decorators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Lifestyles;

    // This class allows decorating collections of services with elements that are created out of the control
    // of the container. Collections are registered using the following methods:
    // -RegisterAll<TService>(IEnumerable<TService> collection)
    // -Register<TService>(TService) (where TService is a IEnumerable<T>)
    // -RegisterAll(this Container container, Type serviceType, IEnumerable collection).
    internal sealed class ContainerUncontrolledServicesDecoratorInterceptor : DecoratorExpressionInterceptor
    {
        // NOTE: We have a memory leak here, in the situation when many containers are newed up.
        // Store a ServiceTypeDecoratorInfo object per closed service type. This list must be shared across
        // all decorators that get applied within the same container. We need a dictionary per thread, since 
        // the ExpressionBuilt event can get raised by multiple threads at the same time (especially for types 
        // resolved using unregistered type resolution) and using the same dictionary could lead to duplicate 
        // entries in the ServiceTypeDecoratorInfo.AppliedDecorators list.
        [ThreadStatic]
        private static Dictionary<Container, Dictionary<Type, ServiceTypeDecoratorInfo>>
            threadStaticServiceTypePredicateCache;

        private readonly Dictionary<Type, IEnumerable> singletonDecoratedCollections =
            new Dictionary<Type, IEnumerable>();

        internal ContainerUncontrolledServicesDecoratorInterceptor(DecoratorExpressionInterceptorData data)
            : base(data)
        {
        }

        protected override Dictionary<Container, Dictionary<Type, ServiceTypeDecoratorInfo>>
            ThreadStaticServiceTypePredicateCache
        {
            get { return threadStaticServiceTypePredicateCache; }
            set { threadStaticServiceTypePredicateCache = value; }
        }

        internal void Decorate(object sender, ExpressionBuiltEventArgs e)
        {
            if (typeof(IEnumerable<>).IsGenericTypeDefinitionOf(e.RegisteredServiceType))
            {
                if (!DecoratorHelpers.IsDecoratableEnumerableExpression(e.Expression))
                {
                    var serviceType = e.RegisteredServiceType.GetGenericArguments()[0];

                    Type decoratorType;

                    if (this.MustDecorate(serviceType, out decoratorType) &&
                        this.SatisfiesPredicate(serviceType))
                    {
                        this.ApplyDecorator(e, serviceType, decoratorType);
                    }
                }
            }
        }

        private void ApplyDecorator(ExpressionBuiltEventArgs e, Type serviceType, Type decoratorType)
        {
            ConstructorInfo decoratorConstructor = 
                this.ResolutionBehavior.GetConstructor(serviceType, decoratorType);

            var serviceInfo = this.GetServiceTypeInfo(e.Expression, serviceType, UnknownLifestyle.Instance);

            var decoratedExpression = 
                this.BuildDecoratorExpression(serviceType, decoratorConstructor, e.Expression);

            var relationships = this.GetKnownDecoratorRelationships(decoratorConstructor, serviceType, 
                serviceInfo.GetCurrentInstanceProducer());
            
            e.Expression = decoratedExpression;

            // Add the decorator to the list of applied decorator. This way users can use this
            // information in the predicate of the next decorator they add.
            serviceInfo.AddAppliedDecorator(decoratorType, this, decoratedExpression);

            e.KnownRelationships.AddRange(relationships);
        }

        private bool SatisfiesPredicate(Type serviceType)
        {
            // We don't have an expression at this point, since the instances are not created by the container.
            // Therefore we fake an expression so it can still be passed on to the predicate the user might
            // have defined.
            var expression = Expression.Constant(null, serviceType);

            return this.SatisfiesPredicate(serviceType, expression, UnknownLifestyle.Instance);
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
            Justification = "This is not a performance critical path.")]
        private Expression BuildDecoratorExpression(Type serviceType, ConstructorInfo decoratorConstructor, 
            Expression originalExpression)
        {
            this.ThrowWhenDecoratorNeedsAFunc(serviceType, decoratorConstructor);

            ParameterExpression parameter = Expression.Parameter(serviceType, "service");

            var parameters = 
                this.BuildParameters(decoratorConstructor, new ExpressionBuiltEventArgs(serviceType, parameter));

            Delegate wrapInstanceWithDecorator =
                BuildDecoratorWrapper(serviceType, decoratorConstructor, parameter, parameters)
                .Compile();

            if (originalExpression is ConstantExpression)
            {
                return this.BuildDecoratorEnumerableExpressionForConstantEnumerable(serviceType,
                    wrapInstanceWithDecorator, ((ConstantExpression)originalExpression).Value as IEnumerable);
            }
            else
            {
                return this.BuildDecoratorEnumerableExpressionForNonConstantExpression(serviceType,
                    wrapInstanceWithDecorator, originalExpression);
            }
        }

        private void ThrowWhenDecoratorNeedsAFunc(Type serviceType, ConstructorInfo decoratorConstructor)
        {
            bool needsADecorateeFactory =
                DecoratorNeedsADecorateeFactory(serviceType, decoratorConstructor);

            if (needsADecorateeFactory)
            {
                string message =
                    StringResources.CantGenerateFuncForDecorator(serviceType, this.DecoratorTypeDefinition);

                throw new ActivationException(message);
            }
        }

        private static bool DecoratorNeedsADecorateeFactory(Type serviceType,
            ConstructorInfo decoratorConstructor)
        {
            return (
                from parameter in decoratorConstructor.GetParameters()
                where IsDecorateeFactoryParameter(parameter, serviceType)
                select parameter)
                .Any();
        }

        // Creates an expression that calls a Func<T, T> delegate that takes in the service and returns
        // that instance, wrapped with the decorator.
        private static LambdaExpression BuildDecoratorWrapper(Type serviceType,
            ConstructorInfo decoratorConstructor, ParameterExpression parameter, Expression[] parameters)
        {
            Type funcType = typeof(Func<,>).MakeGenericType(serviceType, serviceType);

            return Expression.Lambda(funcType, Expression.New(decoratorConstructor, parameters),
                new ParameterExpression[] { parameter });
        }

        private Expression BuildDecoratorEnumerableExpressionForConstantEnumerable(Type serviceType,
            Delegate wrapInstanceWithDecorator, IEnumerable collection)
        {
            IEnumerable decoratedCollection = collection.Select(serviceType, wrapInstanceWithDecorator);

            // Passing the enumerable type is needed when running in the Silverlight sandbox.
            Type enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);

            if (this.Lifestyle == Lifestyle.Singleton)
            {
                Func<IEnumerable> collectionCreator = () =>
                {
                    Array array = ToArray(serviceType, decoratedCollection);
                    return Helpers.MakeReadOnly(serviceType, array);
                };

                IEnumerable singleton = this.GetSingletonDecoratedCollection(serviceType, collectionCreator);

                return Expression.Constant(singleton, enumerableServiceType);
            }

            return Expression.Constant(decoratedCollection, enumerableServiceType);
        }

        private Expression BuildDecoratorEnumerableExpressionForNonConstantExpression(Type serviceType,
            Delegate wrapInstanceWithDecorator, Expression expression)
        {
            var callExpression = DecoratorHelpers.Select(expression, serviceType, wrapInstanceWithDecorator);

            if (this.Lifestyle == Lifestyle.Singleton)
            {
                Type enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);

                Func<IEnumerable> collectionCreator = () =>
                {
                    Type funcType = typeof(Func<>).MakeGenericType(enumerableServiceType);
                    Delegate lambda = Expression.Lambda(funcType, callExpression).Compile();
                    var decoratedCollection = (IEnumerable)lambda.DynamicInvoke();
                    Array array = ToArray(serviceType, decoratedCollection);
                    return Helpers.MakeReadOnly(serviceType, array);
                };

                IEnumerable singleton = this.GetSingletonDecoratedCollection(serviceType, collectionCreator);

                // Passing the enumerable type is needed when running in the Silverlight sandbox.
                return Expression.Constant(singleton, enumerableServiceType);
            }

            return callExpression;
        }

        private IEnumerable GetSingletonDecoratedCollection(Type serviceType,
            Func<IEnumerable> collectionCreator)
        {
            lock (this.singletonDecoratedCollections)
            {
                IEnumerable collection;

                if (!this.singletonDecoratedCollections.TryGetValue(serviceType, out collection))
                {
                    this.singletonDecoratedCollections[serviceType] = collection = collectionCreator();
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