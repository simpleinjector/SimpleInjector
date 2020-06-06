// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Decorators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

    // A list of all decorators applied to a given service type.
    internal sealed class ServiceTypeDecoratorInfo
    {
        private readonly List<DecoratorInfo> appliedDecorators = new List<DecoratorInfo>();

        internal ServiceTypeDecoratorInfo(Type implementationType, InstanceProducer originalProducer)
        {
            this.ImplementationType = implementationType;
            this.OriginalProducer = originalProducer;
        }

        internal Type ImplementationType { get; }

        internal InstanceProducer OriginalProducer { get; }

        internal IEnumerable<DecoratorInfo> AppliedDecorators => this.appliedDecorators;

        internal InstanceProducer GetCurrentInstanceProducer() =>
            this.AppliedDecorators.Any()
                ? this.AppliedDecorators.Last().DecoratorProducer
                : this.OriginalProducer;

        internal void AddAppliedDecorator(
            Type serviceType,
            Type decoratorType,
            Container container,
            Lifestyle lifestyle,
            Expression decoratedExpression,
            IEnumerable<KnownRelationship>? decoratorRelationships = null)
        {
            var registration = new ExpressionRegistration(
                decoratedExpression, decoratorType, lifestyle, container);

            registration.ReplaceRelationships(decoratorRelationships ?? Enumerable.Empty<KnownRelationship>());

            var producer = new InstanceProducer(serviceType, registration);
            producer.IsDecorated = true;

            this.appliedDecorators.Add(new DecoratorInfo(decoratorType, producer));
        }
    }
}