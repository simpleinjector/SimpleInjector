#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2015 Simple Injector Contributors
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

namespace SimpleInjector.Advanced
{
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;

    [DebuggerDisplay("{GetType().Name,nq}")]
    internal sealed class DefaultConstructorInjectionBehavior : IConstructorInjectionBehavior
    {
        private readonly Container container;

        internal DefaultConstructorInjectionBehavior(Container container)
        {
            this.container = container;
        }

        public Expression BuildParameterExpression(ParameterInfo parameter)
        {
            Requires.IsNotNull(parameter, "parameter");

            InstanceProducer producer = this.GetInstanceProducerFor(parameter);
            
            // When the instance producer is invalid, this call will fail with an expressive exception.
            return producer.BuildExpression();
        }

        public void Verify(ParameterInfo parameter)
        {
            Requires.IsNotNull(parameter, "parameter");

            if (parameter.ParameterType.IsValueType || parameter.ParameterType == typeof(string))
            {
                string exceptionMessage = StringResources.ConstructorMustNotContainInvalidParameter(
                    (ConstructorInfo)parameter.Member, parameter);

                throw new ActivationException(exceptionMessage);
            }
        }

        private InstanceProducer GetInstanceProducerFor(ParameterInfo parameter)
        {
            InstanceProducer producer = this.container.GetRegistrationEvenIfInvalid(parameter.ParameterType);
            
            if (producer == null)
            {
                this.container.Options.ConstructorInjectionBehavior.Verify(parameter);

                throw new ActivationException(StringResources.ParameterTypeMustBeRegistered(
                    parameter.Member.DeclaringType, parameter));
            }

            return producer;
        }
    }
}