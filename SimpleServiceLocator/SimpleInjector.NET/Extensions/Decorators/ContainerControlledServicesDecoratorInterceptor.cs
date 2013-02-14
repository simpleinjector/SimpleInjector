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
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Lifestyles;

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

        protected override Dictionary<Type, ServiceTypeDecoratorInfo> ThreadStaticServiceTypePredicateCache
        {
            get { throw new NotSupportedException(); }
        }

        internal void Decorate(object sender, ExpressionBuiltEventArgs e)
        {
            if (typeof(IEnumerable<>).IsGenericTypeDefinitionOf(e.RegisteredServiceType))
            {
                if (DecoratorHelpers.IsDecoratableEnumerableExpression(e.Expression))
                {
                    var serviceType = e.RegisteredServiceType.GetGenericArguments()[0];

                    Type decoratorType;

                    // We don't check the predicate here; that is done for each element individually.
                    if (this.MustDecorate(serviceType, out decoratorType))
                    {
                        this.ApplyDecorator(serviceType, decoratorType, e);
                    }
                }
            }
        }

        private void ApplyDecorator(Type serviceType, Type decoratorType, ExpressionBuiltEventArgs e)
        {
            var decoratorConstructor = this.ResolutionBehavior.GetConstructor(serviceType, decoratorType);

            IEnumerable<KnownRelationship> foundRelationships;

            e.Expression =
                this.BuildDecoratorExpression(e, serviceType, decoratorConstructor, out foundRelationships);

            // Adding known relationships allows the configuration to be analysed for errors.
            e.KnownRelationships.AddRange(foundRelationships);
        }

        private Expression BuildDecoratorExpression(ExpressionBuiltEventArgs e, Type serviceType,
            ConstructorInfo decoratorConstructor, out IEnumerable<KnownRelationship> foundRelationships)
        {
            var decoratableCollection = ((ConstantExpression)e.Expression).Value;

            IDecoratableEnumerable decoratables = DecoratorHelpers.ConvertToDecoratableEnumerable(
                serviceType, this.Container, decoratableCollection);

            return this.BuildDecoratorExpression(serviceType, decoratorConstructor, decoratables,
                out foundRelationships);
        }

        private Expression BuildDecoratorExpression(Type serviceType, ConstructorInfo decoratorConstructor,
            IDecoratableEnumerable collection, out IEnumerable<KnownRelationship> foundRelationships)
        {
            foundRelationships = Enumerable.Empty<KnownRelationship>();

            // When we're dealing with a Singleton decorator, we must cache the list, because other theads
            // could create another 'singleton' instance of this decorator. But since this list is at this 
            // point currently just a list of Expressions (returned as constant), we can always cache this 
            // list, even if the decorator is transient. This cache is an instance list, since it is specific 
            // to the current decorator registration.
            lock (this.decoratableEnumerables)
            {
                IDecoratableEnumerable decoratedCollection;

                if (!this.decoratableEnumerables.TryGetValue(serviceType, out decoratedCollection))
                {
                    decoratedCollection = this.BuildDecoratableEnumerable(serviceType, decoratorConstructor,
                        collection, out foundRelationships);

                    this.decoratableEnumerables[serviceType] = decoratedCollection;
                }

                return Expression.Constant(decoratedCollection);
            }
        }

        private IDecoratableEnumerable BuildDecoratableEnumerable(Type serviceType,
            ConstructorInfo decoratorCtor, IDecoratableEnumerable originalDecoratables,
            out IEnumerable<KnownRelationship> foundRelationships)
        {
            var contexts = (
                from context in originalDecoratables.GetDecoratorPredicateContexts()
                let predicateIsSatisfied = this.SatisfiesPredicate(context)
                select new
                {
                    IsDecorated = predicateIsSatisfied,
                    OriginalContext = context,
                    Context = predicateIsSatisfied ? 
                        this.DecorateContext(context, serviceType, decoratorCtor) : context,
                })
                .ToArray();

            foundRelationships = (
                from context in contexts
                where context.IsDecorated
                let dependency = context.OriginalContext.Registration
                from relationship in this.GetKnownDecoratorRelationships(decoratorCtor, serviceType, dependency)
                select relationship)
                .ToArray();

            var allContexts = contexts.Select(c => c.Context).ToArray();

            return DecoratorHelpers.CreateDecoratableEnumerable(serviceType, allContexts);
        }

        private DecoratorPredicateContext DecorateContext(DecoratorPredicateContext predicateContext,
            Type serviceType, ConstructorInfo decoratorConstructor)
        {
            Type decoratorType = decoratorConstructor.DeclaringType;

            // CreateRegistration must only be called once per decorated item in the collection, but this is
            // guaranteed by BuildDecoratorExpression, which simply locks the decoration of the complete
            // collection.
            var registration = 
                this.CreateRegistration(serviceType, decoratorConstructor, predicateContext.Expression);

            var producer = new InstanceProducer(serviceType, registration);

            var decoratedExpression = registration.BuildExpression();

            return predicateContext.Decorate(decoratorType, decoratedExpression, producer);
        }
    }
}