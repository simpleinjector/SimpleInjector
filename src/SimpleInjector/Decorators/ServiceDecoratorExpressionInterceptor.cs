#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2019 Simple Injector Contributors
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using SimpleInjector.Advanced;

    internal sealed class ServiceDecoratorExpressionInterceptor : DecoratorExpressionInterceptor
    {
        private readonly Dictionary<InstanceProducer, Registration> registrations;
        private readonly ExpressionBuiltEventArgs e;
        private readonly Type registeredServiceType;

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
            var decoratorConstructor = this.Container.Options.SelectConstructor(closedDecoratorType);

            if (object.ReferenceEquals(this.Lifestyle, this.Container.SelectionBasedLifestyle))
            {
                this.Lifestyle = this.Container.Options.SelectLifestyle(closedDecoratorType);
            }

            // By creating the decorator using a Lifestyle Registration the decorator can be completely
            // incorporated into the pipeline. This means that the ExpressionBuilding can be applied,
            // properties can be injected, and it can be wrapped with an initializer.
            Registration decoratorRegistration = this.CreateRegistrationForDecorator(decoratorConstructor);

            this.ReplaceOriginalExpression(decoratorRegistration);

            this.AddAppliedDecoratorToPredicateContext(
                decoratorRegistration.GetRelationships(), decoratorConstructor);
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

        private Registration CreateRegistrationForDecorator(ConstructorInfo decoratorConstructor)
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
                        decoratorConstructor,
                        this.e.Expression,
                        this.e.InstanceProducer,
                        this.GetServiceTypeInfo(this.e));

                    this.registrations[this.e.InstanceProducer] = registration;
                }
            }

            return registration;
        }

        private void AddAppliedDecoratorToPredicateContext(
            IEnumerable<KnownRelationship> decoratorRelationships, ConstructorInfo decoratorConstructor)
        {
            var info = this.GetServiceTypeInfo(this.e);

            // Add the decorator to the list of applied decorators. This way users can use this information in 
            // the predicate of the next decorator they add.
            info.AddAppliedDecorator(
                this.e.RegisteredServiceType,
                decoratorConstructor.DeclaringType,
                this.Container,
                this.Lifestyle,
                this.e.Expression,
                decoratorRelationships);
        }
    }
}