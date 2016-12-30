#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2016 Simple Injector Contributors
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

        public HybridRegistration(Type implementationType, Func<bool> test,
            Registration trueRegistration, Registration falseRegistration,
            Lifestyle lifestyle, Container container)
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

        internal override void SetParameterOverrides(IEnumerable<OverriddenParameter> overriddenParameters)
        {
            this.trueRegistration.SetParameterOverrides(overriddenParameters);
            this.falseRegistration.SetParameterOverrides(overriddenParameters);
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
                relationship.Dependency);
    }
}