// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using SimpleInjector.Advanced;
    using SimpleInjector.Internals;

    internal sealed class HybridRegistration(
        Type implementationType,
        Func<bool> test,
        Registration trueRegistration,
        Registration falseRegistration,
        Lifestyle lifestyle,
        Container container)
        : Registration(lifestyle, container, implementationType)
    {
        public override Expression BuildExpression()
        {
            Expression trueExpression = trueRegistration.BuildExpression();
            Expression falseExpression = falseRegistration.BuildExpression();

            // Must be called after BuildExpression has been called.
            this.AddRelationships();

            return Expression.Condition(
                test: Expression.Invoke(Expression.Constant(test)),
                ifTrue: Expression.Convert(trueExpression, this.ImplementationType),
                ifFalse: Expression.Convert(falseExpression, this.ImplementationType));
        }

        internal override void SetParameterOverrides(IEnumerable<OverriddenParameter> overrides)
        {
            trueRegistration.SetParameterOverrides(overrides);
            falseRegistration.SetParameterOverrides(overrides);
        }

        private void AddRelationships()
        {
            var trueRelationships = this.GetRelationshipsThisLifestyle(trueRegistration);
            var falseRelationships = this.GetRelationshipsThisLifestyle(falseRegistration);

            foreach (var relationship in trueRelationships.Union(falseRelationships))
            {
                this.AddRelationship(relationship);
            }
        }

        private IEnumerable<KnownRelationship> GetRelationshipsThisLifestyle(Registration registration) =>
            from relationship in registration.GetRelationships()
            let mustReplace = object.ReferenceEquals(relationship.Lifestyle, registration.Lifestyle)
            select mustReplace ? relationship.ReplaceLifestyle(this.Lifestyle) : relationship;
    }
}