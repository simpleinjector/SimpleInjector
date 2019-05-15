// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Decorators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using SimpleInjector.Advanced;

    internal sealed class ServiceDecoratorExpressionInterceptor : DecoratorExpressionInterceptor
    {
        private readonly Dictionary<InstanceProducer, Registration> registrations;
        private readonly ExpressionBuiltEventArgs e;
        private readonly Type registeredServiceType;
        private ConstructorInfo decoratorConstructor;

        public ServiceDecoratorExpressionInterceptor(
            DecoratorExpressionInterceptorData data,
            Dictionary<InstanceProducer, Registration> registrations,
            ExpressionBuiltEventArgs e)
            : base(data)
        {
            this.registrations = registrations;
            this.e = e;
            this.registeredServiceType = e.RegisteredServiceType;
        }

        internal bool SatisfiesPredicate()
        {
            this.Context = this.CreatePredicateContext(this.e);

            return this.SatisfiesPredicate(this.Context);
        }

        internal void ApplyDecorator(Type closedDecoratorType)
        {
            this.decoratorConstructor = this.Container.Options.SelectConstructor(closedDecoratorType);

            if (object.ReferenceEquals(this.Lifestyle, this.Container.SelectionBasedLifestyle))
            {
                this.Lifestyle = this.Container.Options.SelectLifestyle(closedDecoratorType);
            }

            // By creating the decorator using a Lifestyle Registration the decorator can be completely
            // incorporated into the pipeline. This means that the ExpressionBuilding can be applied,
            // properties can be injected, and it can be wrapped with an initializer.
            Registration decoratorRegistration = this.CreateRegistrationForDecorator();

            this.ReplaceOriginalExpression(decoratorRegistration);

            this.AddAppliedDecoratorToPredicateContext(decoratorRegistration.GetRelationships());
        }

        private void ReplaceOriginalExpression(Registration decoratorRegistration)
        {
            this.e.Expression = decoratorRegistration.BuildExpression();

            this.e.ReplacedRegistration = decoratorRegistration;

            this.e.InstanceProducer.IsDecorated = true;

            // Must be called after calling BuildExpression, because otherwise we won't have any relationships
            this.MarkDecorateeFactoryRelationshipAsInstanceCreationDelegate(
                decoratorRegistration.GetRelationships());
        }

        private void MarkDecorateeFactoryRelationshipAsInstanceCreationDelegate(
            KnownRelationship[] relationships)
        {
            foreach (Registration dependency in this.GetDecorateeFactoryDependencies(relationships))
            {
                // Mark the dependency of the decoratee factory
                dependency.WrapsInstanceCreationDelegate = true;
            }
        }

        private IEnumerable<Registration> GetDecorateeFactoryDependencies(KnownRelationship[] relationships) =>
            from relationship in relationships
            where DecoratorHelpers.IsScopelessDecorateeFactoryDependencyType(
                relationship.Dependency.ServiceType, this.e.RegisteredServiceType)
            select relationship.Dependency.Registration;

        private Registration CreateRegistrationForDecorator()
        {
            Registration registration;

            // Ensure that the registration for the decorator is created only once to prevent the possibility
            // of multiple instances being created when dealing lifestyles that cache an instance within the
            // Registration instance itself (such as the Singleton lifestyle does).
            lock (this.registrations)
            {
                if (!this.registrations.TryGetValue(this.e.InstanceProducer, out registration))
                {
                    registration = this.CreateRegistration(
                        this.registeredServiceType,
                        this.decoratorConstructor,
                        this.e.Expression,
                        this.e.InstanceProducer,
                        this.GetServiceTypeInfo(this.e));

                    this.registrations[this.e.InstanceProducer] = registration;
                }
            }

            return registration;
        }

        private void AddAppliedDecoratorToPredicateContext(
            IEnumerable<KnownRelationship> decoratorRelationships)
        {
            var info = this.GetServiceTypeInfo(this.e);

            // Add the decorator to the list of applied decorators. This way users can use this information in
            // the predicate of the next decorator they add.
            info.AddAppliedDecorator(
                this.e.RegisteredServiceType,
                this.decoratorConstructor.DeclaringType,
                this.Container,
                this.Lifestyle,
                this.e.Expression,
                decoratorRelationships);
        }
    }
}