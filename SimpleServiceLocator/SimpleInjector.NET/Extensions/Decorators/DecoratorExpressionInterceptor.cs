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
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Advanced;
    using SimpleInjector.Analysis;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Hooks into the building process and adds a decorator if needed.
    /// </summary>
    internal abstract class DecoratorExpressionInterceptor
    {
        private readonly DecoratorExpressionInterceptorData data;

        protected DecoratorExpressionInterceptor(DecoratorExpressionInterceptorData data)
        {
            this.data = data;
        }

        internal Container Container
        {
            get { return this.data.Container; }
        }

        internal Lifestyle Lifestyle
        {
            get { return this.data.Singleton ? Lifestyle.Singleton : Lifestyle.Transient; }
        }

        // The service type definition (possibly open generic).
        protected Type ServiceTypeDefinition
        {
            get { return this.data.ServiceType; }
        }

        // The decorator type definition (possibly open generic).
        protected Type DecoratorTypeDefinition
        {
            get { return this.data.DecoratorType; }
        }

        protected Predicate<DecoratorPredicateContext> Predicate
        {
            get { return this.data.Predicate; }
        }

        protected abstract Dictionary<Container, Dictionary<Type, ServiceTypeDecoratorInfo>>
            ThreadStaticServiceTypePredicateCache
        {
            get;
            set;
        }

        protected IConstructorResolutionBehavior ResolutionBehavior
        {
            get { return this.Container.Options.ConstructorResolutionBehavior; }
        }

        private IConstructorInjectionBehavior InjectionBehavior
        {
            get { return this.Container.Options.ConstructorInjectionBehavior; }
        }

        protected bool MustDecorate(Type serviceType, out Type decoratorType)
        {
            decoratorType = null;

            if (this.ServiceTypeDefinition == serviceType)
            {
                decoratorType = this.DecoratorTypeDefinition;

                return true;
            }

            if (!this.ServiceTypeDefinition.IsGenericTypeDefinitionOf(serviceType))
            {
                return false;
            }

            var results = this.BuildClosedGenericImplementation(serviceType);

            if (!results.ClosedServiceTypeSatisfiesAllTypeConstraints)
            {
                return false;
            }

            decoratorType = results.ClosedGenericImplementation;

            return true;
        }

        protected bool SatisfiesPredicate(Type registeredServiceType, Expression expression, Lifestyle lifestyle)
        {
            var context = this.CreatePredicateContext(registeredServiceType, expression, lifestyle);

            return this.SatisfiesPredicate(context);
        }

        protected bool SatisfiesPredicate(ExpressionBuiltEventArgs e)
        {
            var context = this.CreatePredicateContext(e.RegisteredServiceType, e.Expression, e.Lifestyle);

            return this.SatisfiesPredicate(context);
        }

        protected bool SatisfiesPredicate(DecoratorPredicateContext context)
        {
            return this.Predicate == null || this.Predicate(context);
        }

        protected ServiceTypeDecoratorInfo GetServiceTypeInfo(ExpressionBuiltEventArgs e)
        {
            return this.GetServiceTypeInfo(e.Expression, e.RegisteredServiceType, e.Lifestyle);
        }

        protected ServiceTypeDecoratorInfo GetServiceTypeInfo(Expression originalExpression,
            Type registeredServiceType, Lifestyle lifestyle)
        {
            var producer =
                new InstanceProducer(registeredServiceType,
                    new ExpressionRegistration(originalExpression, lifestyle, this.Container));

            return this.GetServiceTypeInfo(originalExpression, registeredServiceType, producer);
        }

        protected ServiceTypeDecoratorInfo GetServiceTypeInfo(Expression originalExpression,
            Type registeredServiceType, InstanceProducer producer)
        {
            Dictionary<Type, ServiceTypeDecoratorInfo> predicateCache = this.GetServiceTypePredicateCache();

            if (!predicateCache.ContainsKey(registeredServiceType))
            {
                Type implementationType =
                    ExtensionHelpers.DetermineImplementationType(originalExpression, registeredServiceType);

                predicateCache[registeredServiceType] =
                    new ServiceTypeDecoratorInfo(registeredServiceType, implementationType, producer);
            }

            return predicateCache[registeredServiceType];
        }

        protected Expression[] BuildParameters(ConstructorInfo constructor, ExpressionBuiltEventArgs e)
        {
            var parameterExpressions =
                from dependencyParameter in constructor.GetParameters()
                select this.BuildExpressionForDependencyParameter(dependencyParameter, e);

            try
            {
                return parameterExpressions.ToArray();
            }
            catch (ActivationException ex)
            {
                // Build a more expressive exception message.
                string message =
                    StringResources.ErrorWhileTryingToGetInstanceOfType(constructor.DeclaringType, ex.Message);

                throw new ActivationException(message, ex);
            }
        }

        protected KnownRelationship[] GetKnownDecoratorRelationships(ConstructorInfo decoratorConstructor,
            Type registeredServiceType, InstanceProducer decoratee)
        {
            var decorateeRelationships =
                this.GetDecorateeRelationships(decoratorConstructor, registeredServiceType, decoratee);

            var normalRelationships = this.GetNormalRelationships(decoratorConstructor, registeredServiceType);

            return normalRelationships.Union(decorateeRelationships).ToArray();
        }

        private IEnumerable<KnownRelationship> GetNormalRelationships(ConstructorInfo constructor,
            Type registeredServiceType)
        {
            return
                from parameter in constructor.GetParameters()
                where !IsExpressionForDecorateeDependency(parameter, registeredServiceType)
                where !IsDecorateeFactoryParameter(parameter, registeredServiceType)
                let parameterProducer = this.Container.GetRegistration(parameter.ParameterType)
                where parameterProducer != null
                select new KnownRelationship(
                    implementationType: constructor.DeclaringType,
                    lifestyle: this.Lifestyle,
                    dependency: parameterProducer);
        }

        private IEnumerable<KnownRelationship> GetDecorateeRelationships(ConstructorInfo constructor,
            Type registeredServiceType, InstanceProducer decoratee)
        {
            return
                from parameter in constructor.GetParameters()
                where IsExpressionForDecorateeDependency(parameter, registeredServiceType)
                select new KnownRelationship(
                    implementationType: constructor.DeclaringType,
                    lifestyle: this.Lifestyle,
                    dependency: decoratee);
        }

        protected static bool IsDecorateeFactoryParameter(ParameterInfo parameter, Type serviceType)
        {
            return parameter.ParameterType.IsGenericType &&
                parameter.ParameterType.GetGenericTypeDefinition() == typeof(Func<>) &&
                parameter.ParameterType == typeof(Func<>).MakeGenericType(serviceType);
        }

        private GenericTypeBuilder.BuildResult BuildClosedGenericImplementation(Type serviceType)
        {
            var builder = new GenericTypeBuilder(serviceType, this.DecoratorTypeDefinition);

            return builder.BuildClosedGenericImplementation();
        }

        private DecoratorPredicateContext CreatePredicateContext(Type registeredServiceType,
            Expression expression, Lifestyle lifestyle)
        {
            var info = this.GetServiceTypeInfo(expression, registeredServiceType, lifestyle);

            return DecoratorPredicateContext.CreateFromInfo(registeredServiceType, expression, info);
        }

        private Dictionary<Type, ServiceTypeDecoratorInfo> GetServiceTypePredicateCache()
        {
            var predicateCache = this.ThreadStaticServiceTypePredicateCache;

            if (predicateCache == null)
            {
                predicateCache = new Dictionary<Container, Dictionary<Type, ServiceTypeDecoratorInfo>>();

                this.ThreadStaticServiceTypePredicateCache = predicateCache;
            }

            if (!predicateCache.ContainsKey(this.Container))
            {
                predicateCache[this.Container] = new Dictionary<Type, ServiceTypeDecoratorInfo>();
            }

            return predicateCache[this.Container];
        }

        private Expression BuildExpressionForDependencyParameter(ParameterInfo parameter,
            ExpressionBuiltEventArgs e)
        {
            return
                BuildExpressionForDecorateeDependencyParameter(parameter, e) ??
                BuildExpressionForDecorateeFactoryDependencyParameter(parameter, e) ??
                this.BuildExpressionForNormalDependencyParameter(parameter);
        }

        // The constructor parameter in which the decorated instance should be injected.
        private static Expression BuildExpressionForDecorateeDependencyParameter(ParameterInfo parameter,
            ExpressionBuiltEventArgs e)
        {
            if (IsExpressionForDecorateeDependency(parameter, e.RegisteredServiceType))
            {
                return e.Expression;
            }

            return null;
        }

        private static bool IsExpressionForDecorateeDependency(ParameterInfo parameter, Type registeredServiceType)
        {
            return parameter.ParameterType == registeredServiceType;
        }

        // The constructor parameter in which the factory for creating decorated instances should be injected.
        private static Expression BuildExpressionForDecorateeFactoryDependencyParameter(
            ParameterInfo parameter, ExpressionBuiltEventArgs e)
        {
            if (IsDecorateeFactoryParameter(parameter, e.RegisteredServiceType))
            {
                var instanceCreator =
                    Expression.Lambda(Expression.Convert(e.Expression, e.RegisteredServiceType)).Compile();

                return Expression.Constant(instanceCreator);
            }

            return null;
        }

        private Expression BuildExpressionForNormalDependencyParameter(ParameterInfo parameter)
        {
            return this.InjectionBehavior.BuildParameterExpression(parameter);
        }
    }
}