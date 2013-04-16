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

namespace SimpleInjector.Extensions.Decorators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using SimpleInjector.Advanced;

    // This class allows decorating collections of services with elements that are created by the container. 
    // Collections are registered using the following methods:
    // -RegisterAll<TService>(params TService[] singletons)
    // -RegisterAll(this Container container, Type serviceType, IEnumerable<Type> serviceTypes)
    // -RegisterAll<TService>(this Container container, IEnumerable<Type> serviceTypes)
    // -RegisterAll<TService>(this Container container, params Type[] serviceTypes)
    internal sealed class ContainerControlledServicesDecoratorInterceptor : DecoratorExpressionInterceptor
    {
        private readonly Dictionary<Type, IDecoratedEnumerable> decoratableEnumerablesCache;
        private readonly ExpressionBuiltEventArgs e;
        private readonly Type registeredServiceType;
        private readonly ConstructorInfo decoratorConstructor;
        private readonly Type decoratorType;

        public ContainerControlledServicesDecoratorInterceptor(DecoratorExpressionInterceptorData data,
            Dictionary<Type, IDecoratedEnumerable> decoratableEnumerablesCache,
            ExpressionBuiltEventArgs e, Type registeredServiceType, Type decoratorType)
            : base(data)
        {
            this.decoratableEnumerablesCache = decoratableEnumerablesCache;
            this.e = e;
            this.registeredServiceType = registeredServiceType;

            this.decoratorConstructor = data.Container.Options.ConstructorResolutionBehavior
                .GetConstructor(this.registeredServiceType, decoratorType);

            // The actual decorator could be different. TODO: must... write... test... for... this.
            this.decoratorType = this.decoratorConstructor.DeclaringType;
        }
        
        protected override Dictionary<Type, ServiceTypeDecoratorInfo> ThreadStaticServiceTypePredicateCache
        {
            get { throw new NotSupportedException(); }
        }
        
        internal void ApplyDecorator()
        {
            IEnumerable<KnownRelationship> foundRelationships;

            this.e.Expression = this.BuildDecoratorExpression(out foundRelationships);

            // Adding known relationships allows the configuration to be analysed for errors.
            this.e.KnownRelationships.AddRange(foundRelationships);
        }
        
        private Expression BuildDecoratorExpression(out IEnumerable<KnownRelationship> foundRelationships)
        {
            var decoratableCollection = ((ConstantExpression)this.e.Expression).Value;

            IDecoratedEnumerable decoratables = DecoratorHelpers.ConvertToDecoratableEnumerable(
                this.registeredServiceType, this.Container, decoratableCollection);

            return this.BuildDecoratorExpression(decoratables, out foundRelationships);
        }
        
        private Expression BuildDecoratorExpression(IDecoratedEnumerable collection, 
            out IEnumerable<KnownRelationship> foundRelationships)
        {
            foundRelationships = Enumerable.Empty<KnownRelationship>();

            // When we're dealing with a Singleton decorator, we must cache the list, because other theads
            // could create another 'singleton' instance of this decorator. But since this list is at this 
            // point currently just a list of Expressions (returned as constant), we can always cache this 
            // list, even if the decorator is transient. This cache is an instance list, since it is specific 
            // to the current decorator registration.
            lock (this.decoratableEnumerablesCache)
            {
                IDecoratedEnumerable decoratedCollection;

                if (!this.decoratableEnumerablesCache.TryGetValue(this.registeredServiceType, 
                    out decoratedCollection))
                {
                    decoratedCollection = 
                        this.BuildDecoratableEnumerable(collection, out foundRelationships);

                    this.decoratableEnumerablesCache[this.registeredServiceType] = decoratedCollection;
                }

                return Expression.Constant(decoratedCollection);
            }
        }

        private IDecoratedEnumerable BuildDecoratableEnumerable(IDecoratedEnumerable originalDecoratables,
            out IEnumerable<KnownRelationship> foundRelationships)
        {
            var contexts = (
                from context in originalDecoratables.GetDecoratorPredicateContexts()
                let predicateIsSatisfied = this.SatisfiesPredicate(context)
                select new
                {
                    IsDecorated = predicateIsSatisfied,
                    OriginalContext = context,
                    Context = predicateIsSatisfied ? this.DecorateContext(context) : context,
                })
                .ToArray();

            foundRelationships = (
                from context in contexts
                where context.IsDecorated
                let dependency = context.OriginalContext.Registration
                let decoratorRegistration = context.Context.Registration.Registration
                from relationship in this.GetKnownDecoratorRelationships(decoratorRegistration,
                    this.decoratorConstructor, this.registeredServiceType, dependency)
                select relationship)
                .ToArray();

            var allContexts = contexts.Select(c => c.Context).ToArray();

            return DecoratorHelpers.CreateDecoratedEnumerable(this.registeredServiceType, this.Container, 
                allContexts);
        }

        private DecoratorPredicateContext DecorateContext(DecoratorPredicateContext predicateContext)
        {
            // CreateRegistration must only be called once per decorated item in the collection, but this is
            // guaranteed by BuildDecoratorExpression, which simply locks the decoration of the complete
            // collection.
            var registration = this.CreateRegistration(this.registeredServiceType, this.decoratorConstructor, 
                predicateContext.Expression);

            var producer = new InstanceProducer(this.registeredServiceType, registration);

            var decoratedExpression = registration.BuildExpression();

            return predicateContext.Decorate(this.decoratorType, decoratedExpression, producer);
        }
    }
}