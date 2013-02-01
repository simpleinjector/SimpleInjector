#region Copyright (c) 2013 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2013 S. van Deursen
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

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;

    using SimpleInjector.Diagnostics;

    public class HybridLifestyle : Lifestyle
    {
        public HybridLifestyle(Func<bool> test, Lifestyle trueLifestyle, Lifestyle falseLifestyle)
            : base("Hybrid " + GetName(trueLifestyle) + " / " + GetName(falseLifestyle))
        {
            Requires.IsNotNull(test, "test");
            Requires.IsNotNull(trueLifestyle, "trueLifestyle");
            Requires.IsNotNull(falseLifestyle, "falseLifestyle");

            this.Test = test;
            this.TrueLifestyle = trueLifestyle;
            this.FalseLifestyle = falseLifestyle;
        }

        // TODO: Is this naming right?
        public Func<bool> Test { get; private set; }

        public Lifestyle TrueLifestyle { get; private set; }

        public Lifestyle FalseLifestyle { get; private set; }

        internal override int ComponentLength
        {
            get { return Math.Max(this.TrueLifestyle.ComponentLength, this.FalseLifestyle.ComponentLength); }
        }

        internal override int DependencyLength
        {
            get { return Math.Min(this.TrueLifestyle.DependencyLength, this.FalseLifestyle.DependencyLength); }
        }

        protected override int Length
        {
            get { throw new NotSupportedException("The length property is not supported for this lifestyle."); }
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "See base.CreateRegistration for more info.")]
        public override Registration CreateRegistration<TService, TImplementation>(Container container)
        {
            Requires.IsNotNull(container, "container");

            return new HybridRegistration(typeof(TService), typeof(TImplementation), this.Test,
                this.TrueLifestyle.CreateRegistration<TService, TImplementation>(container),
                this.FalseLifestyle.CreateRegistration<TService, TImplementation>(container),
                this, container);
        }

        public override Registration CreateRegistration<TService>(Func<TService> instanceCreator,
            Container container)
        {
            Requires.IsNotNull(container, "container");

            return new HybridRegistration(typeof(TService), typeof(TService), this.Test,
                this.TrueLifestyle.CreateRegistration<TService>(instanceCreator, container),
                this.FalseLifestyle.CreateRegistration<TService>(instanceCreator, container),
                this, container);
        }

        private string GetName()
        {
            return GetName(this.TrueLifestyle) + " / " + GetName(this.FalseLifestyle);
        }

        private static string GetName(Lifestyle lifestyle)
        {
            var hybrid = lifestyle as HybridLifestyle;

            return hybrid != null ? hybrid.GetName() : (lifestyle != null ? lifestyle.Name : "Null");
        }

        private sealed class HybridRegistration : Registration
        {
            private readonly Type serviceType;
            private readonly Type implementationType;
            private readonly Func<bool> test;
            private readonly Registration trueRegistration;
            private readonly Registration falseRegistration;

            public HybridRegistration(Type serviceType, Type implementationType, Func<bool> test, 
                Registration trueRegistration, Registration falseRegistration, 
                Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
                this.serviceType = serviceType;
                this.implementationType = implementationType;
                this.test = test;
                this.trueRegistration = trueRegistration;
                this.falseRegistration = falseRegistration;
            }

            public override Type ImplementationType 
            {
                get { return this.implementationType; }
            }

            public override Expression BuildExpression()
            {
                Expression trueExpression = this.trueRegistration.BuildExpression();
                Expression falseExpression = this.falseRegistration.BuildExpression();

                // Must be called after BuildExpression has been called.
                this.AddRelationships();

                return Expression.Condition(
                    test: Expression.Invoke(Expression.Constant(this.test)),
                    ifTrue: Expression.Convert(trueExpression, this.serviceType),
                    ifFalse: Expression.Convert(falseExpression, this.serviceType));
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

            private IEnumerable<KnownRelationship> GetRelationshipsThisLifestyle(Registration registration)
            {
                return
                    from relationship in registration.GetRelationships()
                    let mustReplace = object.ReferenceEquals(relationship.Lifestyle, registration.Lifestyle)
                    select mustReplace ? this.ReplaceLifestyle(relationship) : relationship;
            }

            private KnownRelationship ReplaceLifestyle(KnownRelationship relationship)
            {
                return new KnownRelationship(
                    relationship.ImplementationType,
                    this.Lifestyle, 
                    relationship.Dependency);
            }
        }
    }
}