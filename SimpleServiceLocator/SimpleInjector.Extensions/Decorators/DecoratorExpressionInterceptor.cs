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

        protected Container Container
        {
            get { return this.data.Container; }
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

        protected bool Singleton
        {
            get { return this.data.Singleton; }
        }

        protected abstract Dictionary<Container, Dictionary<Type, ServiceTypeDecoratorInfo>>
            ThreadStaticServiceTypePredicateCache
        {
            get;
            set;
        }

        protected IConstructorResolutionBehavior ResolutionBehavior
        {
            get { return this.Container.GetConstructorResolutionBehavior(); }
        }

        private IConstructorInjectionBehavior InjectionBehavior
        {
            get { return this.Container.GetConstructorInjectionBehavior(); }
        }

        protected bool MustDecorate(Type serviceType, out Type decoratorType)
        {
            decoratorType = null;

            if (this.ServiceTypeDefinition == serviceType)
            {
                decoratorType = this.DecoratorTypeDefinition;

                return true;
            }

            if (this.ServiceTypeDefinition.IsGenericTypeDefinitionOf(serviceType))
            {
                var results = this.BuildClosedGenericImplementation(serviceType);

                if (!results.ClosedServiceTypeSatisfiesAllTypeConstraints)
                {
                    return false;
                }

                decoratorType = results.ClosedGenericImplementation;

                return true;
            }

            return false;
        }

        protected bool SatisfiesPredicate(ExpressionBuiltEventArgs e)
        {
            return this.SatisfiesPredicate(this.CreatePredicateContext(e));
        }

        protected bool SatisfiesPredicate(DecoratorPredicateContext context)
        {
            return this.Predicate == null || this.Predicate(context);
        }

        protected ServiceTypeDecoratorInfo GetServiceTypeInfo(ExpressionBuiltEventArgs e)
        {
            return this.GetServiceTypeInfo(e.Expression, e.RegisteredServiceType);
        }

        protected ServiceTypeDecoratorInfo GetServiceTypeInfo(Expression expression, Type registeredServiceType)
        {
            Dictionary<Type, ServiceTypeDecoratorInfo> predicateCache = this.GetServiceTypePredicateCache();

            if (!predicateCache.ContainsKey(registeredServiceType))
            {
                Type implementationType = Helpers.DetermineImplementationType(expression, registeredServiceType);

                predicateCache[registeredServiceType] = new ServiceTypeDecoratorInfo(implementationType);
            }

            return predicateCache[registeredServiceType];
        }

        protected Expression[] BuildParameters(Type decoratorType, ExpressionBuiltEventArgs e)
        {
            ConstructorInfo constructor = 
                this.ResolutionBehavior.GetConstructor(e.RegisteredServiceType, decoratorType);

            var dependencyParameters =
                from dependencyParameter in constructor.GetParameters()
                select this.BuildExpressionForDependencyParameter(dependencyParameter, e);

            try
            {
                return dependencyParameters.ToArray();
            }
            catch (ActivationException ex)
            {
                // Build a more expressive exception message.
                string message =
                    StringResources.ErrorWhileTryingToGetInstanceOfType(decoratorType, ex.Message);

                throw new ActivationException(message, ex);
            }
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

        private DecoratorPredicateContext CreatePredicateContext(ExpressionBuiltEventArgs e)
        {
            var info = this.GetServiceTypeInfo(e);

            return new DecoratorPredicateContext
            {
                ServiceType = e.RegisteredServiceType,
                ImplementationType = info.ImplementationType,
                AppliedDecorators = info.AppliedDecorators.ToList().AsReadOnly(),
                Expression = e.Expression,
            };
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
            if (parameter.ParameterType == e.RegisteredServiceType)
            {
                return e.Expression;
            }

            return null;
        }

        // The constructor parameter in which the factory for creating decorated instances should be injected.
        private static Expression BuildExpressionForDecorateeFactoryDependencyParameter(
            ParameterInfo parameter, ExpressionBuiltEventArgs e)
        {
            bool isDecoratorFactoryParameter = IsDecorateeFactoryParameter(parameter, e.RegisteredServiceType);

            if (isDecoratorFactoryParameter)
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