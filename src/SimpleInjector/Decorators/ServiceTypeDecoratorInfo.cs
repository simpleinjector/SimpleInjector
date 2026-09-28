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
    internal sealed class ServiceTypeDecoratorInfo(Type implementationType, InstanceProducer originalProducer)
    {
        private readonly List<DecoratorInfo> appliedDecorators = [];

        internal Type ImplementationType { get; } = implementationType;

        internal InstanceProducer OriginalProducer { get; } = originalProducer;

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

            registration.ReplaceRelationships(decoratorRelationships ?? []);

            var producer = new InstanceProducer(serviceType, registration) { IsDecorated = true };

            this.appliedDecorators.Add(new DecoratorInfo(decoratorType, producer));
        }
    }
}