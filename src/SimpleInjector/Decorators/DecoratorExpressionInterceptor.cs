// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Decorators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Internals;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Hooks into the building process and adds a decorator if needed.
    /// </summary>
    internal abstract class DecoratorExpressionInterceptor
    {
        private static readonly MethodInfo ResolveWithinThreadResolveScopeMethod =
            typeof(DecoratorExpressionInterceptor).GetMethod(nameof(ResolveWithinThreadResolveScope));

        private readonly DecoratorExpressionInterceptorData data;

        protected DecoratorExpressionInterceptor(DecoratorExpressionInterceptorData data)
        {
            this.data = data;

            this.Lifestyle = data.Lifestyle;
        }

        // Must be set after construction.
        internal DecoratorPredicateContext? Context { get; set; }

        protected Container Container => this.data.Container;

        protected Lifestyle Lifestyle { get; set; }

        // The decorator type definition (possibly open generic).
        protected Type? DecoratorTypeDefinition => this.data.DecoratorType;

        protected Predicate<DecoratorPredicateContext>? Predicate => this.data.Predicate;

        // NOTE: This method must be public for it to be callable through reflection when running in a sandbox.
        public static TService ResolveWithinThreadResolveScope<TService>(
            Scope scope, Func<TService> instanceCreator, Container container)
        {
            if (!object.ReferenceEquals(container, scope.Container))
            {
                if (scope.Container is null)
                {
                    throw new InvalidOperationException(
                        StringResources.ScopeSuppliedToScopedDecorateeFactoryMustHaveAContainer<TService>());
                }
                else
                {
                    throw new InvalidOperationException(
                        StringResources.ScopeSuppliedToScopedDecorateeFactoryMustBeForSameContainer<TService>());
                }
            }

            Scope? originalScope = container.CurrentThreadResolveScope;

            try
            {
                container.CurrentThreadResolveScope = scope;
                return instanceCreator();
            }
            finally
            {
                container.CurrentThreadResolveScope = originalScope;
            }
        }

        protected bool SatisfiesPredicate(DecoratorPredicateContext context) =>
            this.Predicate == null || this.Predicate(context);

        protected ServiceTypeDecoratorInfo GetServiceTypeInfo(
            ExpressionBuiltEventArgs e,
            Expression? originalExpression = null,
            Registration? originalRegistration = null,
            Type? registeredServiceType = null)
        {
            originalExpression = originalExpression ?? e.Expression;
            originalRegistration = originalRegistration ?? e.ReplacedRegistration;
            registeredServiceType = registeredServiceType ?? e.RegisteredServiceType;

            // registeredProducer.ServiceType and registeredServiceType are different when called by
            // container uncontrolled decorator. producer.ServiceType will be IEnumerable<T> and
            // registeredServiceType will be T.
            if (e.DecoratorInfo == null)
            {
                Type implementationType =
                    DecoratorHelpers.DetermineImplementationType(originalExpression, e.InstanceProducer);

                // The InstanceProducer created here is used to do correct diagnostics. We can't return the
                // registeredProducer here, since the lifestyle of the original producer can change after
                // the ExpressionBuilt event has ran, which means that this would invalidate the diagnostic
                // results.
                var producer = new InstanceProducer(
                    registeredServiceType, originalRegistration, registerExternalProducer: false);

                e.DecoratorInfo = new ServiceTypeDecoratorInfo(implementationType, producer);
            }

            return e.DecoratorInfo;
        }

        protected Registration CreateRegistration(
            Type serviceType,
            ConstructorInfo decoratorConstructor,
            Expression decorateeExpression,
            InstanceProducer realProducer,
            ServiceTypeDecoratorInfo info)
        {
            var overriddenParameters = this.CreateOverriddenParameters(
                serviceType, decoratorConstructor, decorateeExpression, realProducer, info);

            return this.Lifestyle.CreateDecoratorRegistration(
                decoratorConstructor.DeclaringType, this.Container, overriddenParameters);
        }

        protected DecoratorPredicateContext CreatePredicateContext(ExpressionBuiltEventArgs e)
        {
            ServiceTypeDecoratorInfo info = this.GetServiceTypeInfo(e);

            return this.CreatePredicateContext(e.RegisteredServiceType, e.Expression, info);
        }

        protected DecoratorPredicateContext CreatePredicateContext(
            Type registeredServiceType, Expression expression, ServiceTypeDecoratorInfo info)
        {
            // NOTE: registeredServiceType can be different from registeredProducer.ServiceType.
            // This is the case for container-uncontrolled collections where producer.ServiceType is the
            // IEnumerable<T> and registeredServiceType is T.
            return DecoratorPredicateContext.CreateFromInfo(registeredServiceType, expression, info);
        }

        protected Expression? GetExpressionForDecorateeDependencyParameterOrNull(
            ParameterInfo param, Type serviceType, Expression expr)
        {
            return
                BuildExpressionForDecorateeDependencyParameter(param, serviceType, expr)
                ?? this.BuildExpressionForDecorateeFactoryDependencyParameter(param, serviceType, expr)
                ?? this.BuildExpressionForScopedDecorateeFactoryDependencyParameter(param, serviceType, expr);
        }

        protected static ParameterInfo GetDecorateeParameter(
            Type serviceType, ConstructorInfo decoratorConstructor)
        {
            // Although we partly check for duplicate arguments during registration phase, we must do it here
            // as well, because some registrations are allowed while not all closed-generic implementations
            // can be resolved.
            var parameters = (
                from parameter in decoratorConstructor.GetParameters()
                where DecoratorHelpers.IsDecorateeParameter(parameter, serviceType)
                select parameter)
                .ToArray();

            if (parameters.Length > 1)
            {
                throw new ActivationException(
                    StringResources.TypeDependsOnItself(decoratorConstructor.DeclaringType));
            }

            return parameters.Single();
        }

        protected InstanceProducer CreateDecorateeFactoryProducer(ParameterInfo parameter)
        {
            // We create a dummy expression with a null value. Much easier than passing on the real delegate.
            // We won't miss it, since the created InstanceProducer is just a dummy for purposes of analysis.
            var dummyExpression = Expression.Constant(null, parameter.ParameterType);

            var registration = new ExpressionRegistration(dummyExpression, this.Container);

            return new InstanceProducer(parameter.ParameterType, registration);
        }

        private static void AddVerifierForDecorateeFactoryDependency(
            Expression decorateeExpression, InstanceProducer producer)
        {
            // Func<T> dependencies for the decoratee must be explicitly added to the InstanceProducer as
            // verifier. This allows those dependencies to be verified when calling Container.Verify().
            Action<Scope> verifier = GetVerifierFromDecorateeExpression(decorateeExpression);

            producer.AddVerifier(verifier);
        }

        private OverriddenParameter[] CreateOverriddenParameters(
            Type serviceType,
            ConstructorInfo decoratorConstructor,
            Expression decorateeExpression,
            InstanceProducer realProducer,
            ServiceTypeDecoratorInfo info)
        {
            ParameterInfo decorateeParameter = GetDecorateeParameter(serviceType, decoratorConstructor);

            decorateeExpression = this.GetExpressionForDecorateeDependencyParameterOrNull(
                decorateeParameter, serviceType, decorateeExpression)!;

            var currentProducer = info.GetCurrentInstanceProducer();

            if (DecoratorHelpers.IsDecorateeFactoryDependencyType(decorateeParameter.ParameterType, serviceType))
            {
                // Adding a verifier makes sure the graph for the decoratee gets created,
                // which allows testing whether constructing the graph fails.
                AddVerifierForDecorateeFactoryDependency(decorateeExpression, realProducer);

                // By adding the decoratee producer, we allow that object graph to be diagnosed.
                realProducer.AddProducerToVerify(currentProducer);

                currentProducer = this.CreateDecorateeFactoryProducer(decorateeParameter);
            }

            var decorateeOverriddenParameter =
                new OverriddenParameter(decorateeParameter, decorateeExpression, currentProducer);

            IEnumerable<OverriddenParameter> predicateContextOverriddenParameters =
                this.CreateOverriddenDecoratorContextParameters(decoratorConstructor, currentProducer);

            var overriddenParameters = (new[] { decorateeOverriddenParameter })
                .Concat(predicateContextOverriddenParameters);

            return overriddenParameters.ToArray();
        }

        private IEnumerable<OverriddenParameter> CreateOverriddenDecoratorContextParameters(
            ConstructorInfo decoratorConstructor, InstanceProducer currentProducer)
        {
            return
                from parameter in decoratorConstructor.GetParameters()
                where parameter.ParameterType == typeof(DecoratorContext)
                let contextExpression = Expression.Constant(new DecoratorContext(this.Context!))
                select new OverriddenParameter(parameter, contextExpression, currentProducer);
        }

        private static Action<Scope> GetVerifierFromDecorateeExpression(Expression decorateeExpression)
        {
            var value = ((ConstantExpression)decorateeExpression).Value;

            if (value is Func<object> instanceCreator)
            {
                return _ => instanceCreator();
            }
            else
            {
                var scopedInstanceCreator = (Func<Scope, object>)value;

                return scope => scopedInstanceCreator(scope);
            }
        }

        // The constructor parameter in which the decorated instance should be injected.
        private static Expression? BuildExpressionForDecorateeDependencyParameter(
            ParameterInfo parameter, Type serviceType, Expression expression)
        {
            return IsDecorateeDependencyParameter(parameter, serviceType) ? expression : null;
        }

        private static bool IsDecorateeDependencyParameter(ParameterInfo parameter, Type registeredServiceType)
        {
            return parameter.ParameterType == registeredServiceType;
        }

        // The constructor parameter in which the factory for creating decorated instances should be injected.
        private Expression? BuildExpressionForDecorateeFactoryDependencyParameter(
            ParameterInfo param, Type serviceType, Expression expr)
        {
            if (DecoratorHelpers.IsScopelessDecorateeFactoryDependencyType(param.ParameterType, serviceType))
            {
                // We can't call CompilationHelpers.CompileExpression here, because it has a generic type and
                // we don't know the type at runtime here. We need to do some refactoring to CompilationHelpers
                // to get that working.
                expr = CompilationHelpers.OptimizeScopedRegistrationsInObjectGraph(this.Container, expr);

                var instanceCreator =
                    Expression.Lambda(Expression.Convert(expr, serviceType)).Compile();

                return Expression.Constant(instanceCreator);
            }

            return null;
        }

        // The constructor parameter in which the factory for creating decorated instances should be injected.
        private Expression? BuildExpressionForScopedDecorateeFactoryDependencyParameter(
            ParameterInfo param, Type serviceType, Expression expr)
        {
            if (DecoratorHelpers.IsScopeDecorateeFactoryDependencyParameter(param.ParameterType, serviceType))
            {
                expr = CompilationHelpers.OptimizeScopedRegistrationsInObjectGraph(this.Container, expr);

                var instanceCreator =
                    Expression.Lambda(Expression.Convert(expr, serviceType)).Compile();

                var scopeParameter = Expression.Parameter(typeof(Scope), "scope");

                // Build a Func<Scope, ServiceType>:
                // scope => ResolveWithinThreadResolveScope<TService>(scope, instanceCreator)
                var scopedInstanceCreator = Expression.Lambda(
                    Expression.Call(
                        ResolveWithinThreadResolveScopeMethod.MakeGenericMethod(serviceType),
                        scopeParameter,
                        Expression.Constant(instanceCreator),
                        Expression.Constant(this.Container)),
                    scopeParameter)
                    .Compile();

                return Expression.Constant(scopedInstanceCreator);
            }

            return null;
        }
    }
}