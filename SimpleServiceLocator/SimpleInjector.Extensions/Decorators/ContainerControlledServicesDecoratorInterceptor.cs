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
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    // This class allows decorating collections of services with elements that are created by the container. 
    // Collections are registered using the following methods:
    // -RegisterAll<TService>(params TService[] singletons)
    // -RegisterAll(this Container container, Type serviceType, IEnumerable<Type> serviceTypes)
    // -RegisterAll<TService>(this Container container, IEnumerable<Type> serviceTypes)
    // -RegisterAll<TService>(this Container container, params Type[] serviceTypes)
    internal sealed class ContainerControlledServicesDecoratorInterceptor : DecoratorExpressionInterceptor
    {
        private readonly Dictionary<Type, IDecoratableEnumerable> decoratableEnumerables =
            new Dictionary<Type, IDecoratableEnumerable>();

        internal ContainerControlledServicesDecoratorInterceptor(DecoratorExpressionInterceptorData data)
            : base(data)
        {
        }

        protected override Dictionary<Container, Dictionary<Type, ServiceTypeDecoratorInfo>>
            ThreadStaticServiceTypePredicateCache
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        internal void Decorate(object sender, ExpressionBuiltEventArgs e)
        {
            if (typeof(IEnumerable<>).IsGenericTypeDefinitionOf(e.RegisteredServiceType))
            {
                var serviceType = e.RegisteredServiceType.GetGenericArguments()[0];

                Type decoratorType;

                if (DecoratorHelpers.IsDecoratableEnumerableExpression(e.Expression))
                {
                    // We don't check the predicate here; that is done for each element individually.
                    if (this.MustDecorate(serviceType, out decoratorType))
                    {
                        e.Expression = this.BuildDecoratorExpression(e, serviceType, decoratorType);
                    }
                }
            }
        }

        private Expression BuildDecoratorExpression(ExpressionBuiltEventArgs e, Type serviceType, 
            Type decoratorType)
        {
            var decoratableCollection = ((ConstantExpression)e.Expression).Value;

            IDecoratableEnumerable decoratables =
                DecoratorHelpers.ConvertToDecoratableEnumerable(serviceType, decoratableCollection);

            return this.BuildDecoratorExpression(serviceType, decoratorType, decoratables);
        }

        private Expression BuildDecoratorExpression(Type serviceType, Type decoratorType,
            IDecoratableEnumerable collection)
        {
            // When we're dealing with a Singleton decorator, we must cache the list, because other theads
            // could create another 'singleton' instance of this decorator. But since this list is at this 
            // currently just a list of Expressions (returned as constant), we can always cache this list, 
            // even if the decorator is transient. This cache is an instance list, since it is specific to the 
            // current decorator registration.
            lock (this.decoratableEnumerables)
            {
                IDecoratableEnumerable decoratedCollection;

                if (!this.decoratableEnumerables.TryGetValue(serviceType, out decoratedCollection))
                {
                    decoratedCollection = 
                        this.BuildDecoratableEnumerable(serviceType, decoratorType, collection);

                    this.decoratableEnumerables[serviceType] = decoratedCollection;
                }

                return Expression.Constant(decoratedCollection);
            }
        }

        private IDecoratableEnumerable BuildDecoratableEnumerable(Type serviceType, Type decoratorType,
            IDecoratableEnumerable originalDecoratables)
        {
            var decoratorConstructor = this.ResolutionBehavior.GetConstructor(serviceType, decoratorType);

            IEnumerable<DecoratorPredicateContext> predicateContexts =
                from context in originalDecoratables.GetDecoratorPredicateContexts()
                let decoratedContext = this.DecorateContext(context, serviceType, decoratorConstructor)
                select this.SatisfiesPredicate(context) ? decoratedContext : context;

            return DecoratorHelpers.CreateDecoratableEnumerable(serviceType, predicateContexts.ToArray());
        }

        private DecoratorPredicateContext DecorateContext(DecoratorPredicateContext predicateContext,
            Type serviceType, ConstructorInfo decoratorConstructor)
        {
            Type decoratorType = decoratorConstructor.DeclaringType;

            var e = new ExpressionBuiltEventArgs(serviceType, predicateContext.Expression);

            var parameters = this.BuildParameters(decoratorType, e);

            var appliedDecorators = predicateContext.AppliedDecorators.ToList();

            appliedDecorators.Add(decoratorType);

            return new DecoratorPredicateContext
            {
                ServiceType = predicateContext.ServiceType,
                ImplementationType = predicateContext.ImplementationType,
                Expression = this.BuildDecoratorExpression(decoratorConstructor, parameters),
                AppliedDecorators = appliedDecorators.AsReadOnly()
            };
        }

        private Expression BuildDecoratorExpression(ConstructorInfo decoratorConstructor,
            Expression[] parameters)
        {
            Expression expression =
                DecoratorHelpers.BuildDecoratorExpression(this.Container, decoratorConstructor, parameters);

            if (this.Singleton)
            {
                // This method is called inside the lock of the 
                // BuildDecoratorEnumerableExpressionBasedOnDecoratableEnumerable method, which ensures that
                // the instance is created just once.
                return expression.ToConstant();
            }

            return expression;
        }
    }
}