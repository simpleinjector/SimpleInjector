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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    
    public sealed class TransientLifestyle : Lifestyle
    {
        internal TransientLifestyle() : base("Transient")
        {
        }

        protected override int Length
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "See base.CreateRegistration for more info.")]
        public override Registration CreateRegistration<TService, TImplementation>(Container container)
        {
            Requires.IsNotNull(container, "container");

            return new TransientLifestyleRegistration<TService, TImplementation>(this, container);
        }

        public override Registration CreateRegistration<TService>(Func<TService> instanceCreator, 
            Container container)
        {
            Requires.IsNotNull(instanceCreator, "instanceCreator");
            Requires.IsNotNull(container, "container");

            return new TransientLifestyleRegistration<TService>(this, container, instanceCreator);
        }

        private sealed class TransientLifestyleRegistration<TService> : Registration
            where TService : class
        {
            private readonly Func<TService> instanceCreator;

            public TransientLifestyleRegistration(Lifestyle lifestyle, Container container, 
                Func<TService> instanceCreator)
                : base(lifestyle, container)
            {
                this.instanceCreator = instanceCreator;
            }

            public override Type ImplementationType
            {
                get { return typeof(TService); }
            }

            public override Expression BuildExpression()
            {
                return this.BuildTransientExpression<TService>(this.instanceCreator);
            }
        }

        private class TransientLifestyleRegistration<TService, TImplementation> : Registration
            where TImplementation : class, TService
            where TService : class
        {
            internal TransientLifestyleRegistration(Lifestyle lifestyle, Container container)
                : base(lifestyle, container)
            {
            }

            public override Type ImplementationType
            {
                get { return typeof(TImplementation); }
            }

            public override Expression BuildExpression()
            {
                return this.BuildTransientExpression<TService, TImplementation>();
            }
        }
    }
}