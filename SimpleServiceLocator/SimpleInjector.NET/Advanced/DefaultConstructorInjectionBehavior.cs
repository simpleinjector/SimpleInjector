#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2010 S. van Deursen
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

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;

    [DebuggerDisplay("{GetType().Name}")]
    internal sealed class DefaultConstructorInjectionBehavior : IConstructorInjectionBehavior
    {
        // By supplying a delegate for the retrieval of the container, the ContainerOptions can create and 
        // return a DefaultConstructorInjectionBehavior before this ContainerOptions instance has been added
        // to a container
        private readonly Func<Container> getContainer;

        internal DefaultConstructorInjectionBehavior(Func<Container> getContainer)
        {
            this.getContainer = getContainer;
        }

        public Expression BuildParameterExpression(ParameterInfo parameter)
        {
            Requires.IsNotNull(parameter, "parameter");

            Container container = this.getContainer();

            InstanceProducer producer =
                container == null ? null : container.GetRegistrationEvenIfInvalid(parameter.ParameterType);

            if (producer != null)
            {
                // When the instance producer is invalid, this call will fail with an expressive exception.
                return producer.BuildExpression();
            }

            throw new ActivationException(StringResources.ParameterTypeMustBeRegistered(
                parameter.Member.DeclaringType, parameter));
        }
    }
}