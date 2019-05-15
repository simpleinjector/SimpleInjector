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

    internal sealed class HybridRegistration : Registration
    {
        private readonly Func<bool> test;
        private readonly Registration trueRegistration;
        private readonly Registration falseRegistration;

        public HybridRegistration(
            Type implementationType,
            Func<bool> test,
            Registration trueRegistration,
            Registration falseRegistration,
            Lifestyle lifestyle,
            Container container)
            : base(lifestyle, container)
        {
            this.ImplementationType = implementationType;
            this.test = test;
            this.trueRegistration = trueRegistration;
            this.falseRegistration = falseRegistration;
        }

        public override Type ImplementationType { get; }

        public override Expression BuildExpression()
        {
            Expression trueExpression = this.trueRegistration.BuildExpression();
            Expression falseExpression = this.falseRegistration.BuildExpression();

            // Must be called after BuildExpression has been called.
            this.AddRelationships();

            return Expression.Condition(
                test: Expression.Invoke(Expression.Constant(this.test)),
                ifTrue: Expression.Convert(trueExpression, this.ImplementationType),
                ifFalse: Expression.Convert(falseExpression, this.ImplementationType));
        }

        internal override void SetParameterOverrides(IEnumerable<OverriddenParameter> overrides)
        {
            this.trueRegistration.SetParameterOverrides(overrides);
            this.falseRegistration.SetParameterOverrides(overrides);
        }

        private void AddRelationships()
        {
            var trueRelationships = this.GetRelationshipsThisLifestyle(this.trueRegistration);
            var falseRelationships = this.GetRelationshipsThisLifestyle(this.falseRegistration);

            foreach (var relationship in trueRelationships.Union(falseRelationships))
            {
                this.AddRelationship(relationship);
            }
        }

        private IEnumerable<KnownRelationship> GetRelationshipsThisLifestyle(Registration registration) =>
            from relationship in registration.GetRelationships()
            let mustReplace = object.ReferenceEquals(relationship.Lifestyle, registration.Lifestyle)
            select mustReplace ? this.ReplaceLifestyle(relationship) : relationship;

        private KnownRelationship ReplaceLifestyle(KnownRelationship relationship) =>
            new KnownRelationship(
                relationship.ImplementationType,
                this.Lifestyle,
                relationship.Consumer,
                relationship.Dependency,
                relationship.AdditionalInformation);
    }
}