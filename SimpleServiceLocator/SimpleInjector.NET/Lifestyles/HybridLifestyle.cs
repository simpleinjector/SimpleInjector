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
    using System.Linq.Expressions;

    public class HybridLifestyle : Lifestyle
    {
        private readonly Func<bool> test;
        private readonly Lifestyle trueLifestyle;
        private readonly Lifestyle falseLifestyle;

        public HybridLifestyle(Func<bool> test, Lifestyle trueLifestyle, Lifestyle falseLifestyle)
        {
            Requires.IsNotNull(test, "test");
            Requires.IsNotNull(trueLifestyle, "ifTrue");
            Requires.IsNotNull(falseLifestyle, "ifFalse");

            this.test = test;
            this.trueLifestyle = trueLifestyle;
            this.falseLifestyle = falseLifestyle;
        }

        public override LifestyleRegistration CreateRegistration<TService, TImplementation>(
            Container container)
        {
            Requires.IsNotNull(container, "container");

            return new HybridLifestyleRegistration(typeof(TService), this.test,
                this.trueLifestyle.CreateRegistration<TService, TImplementation>(container),
                this.falseLifestyle.CreateRegistration<TService, TImplementation>(container),
                container);
        }

        public override LifestyleRegistration CreateRegistration<TService>(Func<TService> instanceCreator,
            Container container)
        {
            Requires.IsNotNull(container, "container");

            return new HybridLifestyleRegistration(typeof(TService), this.test,
                this.trueLifestyle.CreateRegistration<TService>(instanceCreator, container),
                this.falseLifestyle.CreateRegistration<TService>(instanceCreator, container),
                container);
        }

        private sealed class HybridLifestyleRegistration : LifestyleRegistration
        {
            private readonly Type serviceType;
            private readonly Func<bool> test;
            private readonly LifestyleRegistration trueLifestyle;
            private readonly LifestyleRegistration falseLifestyle;

            public HybridLifestyleRegistration(Type serviceType, Func<bool> test, 
                LifestyleRegistration trueLifestyle, LifestyleRegistration falseLifestyle, Container container)
                : base(container)
            {
                this.serviceType = serviceType;
                this.test = test;
                this.trueLifestyle = trueLifestyle;
                this.falseLifestyle = falseLifestyle;
            }

            public override Expression BuildExpression()
            {               
                return Expression.Condition(
                    test: Expression.Invoke(Expression.Constant(this.test)),
                    ifTrue: Expression.Convert(this.trueLifestyle.BuildExpression(), this.serviceType),
                    ifFalse: Expression.Convert(this.falseLifestyle.BuildExpression(), this.serviceType));
            }
        }
    }
}