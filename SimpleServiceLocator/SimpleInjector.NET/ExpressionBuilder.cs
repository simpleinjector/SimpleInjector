#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET.
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

using System;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleInjector
{
    /// <summary>
    /// Builds <see cref="Expression"/> objects that can create a new instance of the supplied Type, where
    /// the supplied container will be used to locate the constructor arguments. The generated code of the
    /// built <see cref="Func{T}"/> might look like this.
    /// <![CDATA[
    ///     Func<object> func = () => return new RealUserService(container.GetInstance<IUserRepository>());
    /// ]]>
    /// </summary>
    internal class ExpressionBuilder
    {
        private readonly Container container;

        internal ExpressionBuilder(Container container)
        {
            this.container = container;
        }

        internal Expression BuildExpression(Type concreteType)
        {
            Helpers.ThrowActivationExceptionWhenTypeIsNotConstructable(concreteType);

            var constructor = concreteType.GetConstructors().Single();

            var constructorArgumentCalls =
                from parameter in constructor.GetParameters()
                select this.BuildParameterExpression(concreteType, parameter.ParameterType);

            return Expression.New(constructor, constructorArgumentCalls.ToArray());
        }

        private Expression BuildParameterExpression(Type concreteType, Type parameterType)
        {
            var instanceProducer = this.container.GetRegistration(parameterType);

            if (instanceProducer == null)
            {
                throw new ActivationException(
                    StringResources.ParameterTypeMustBeRegistered(concreteType, parameterType));
            }

            return instanceProducer.BuildExpression();
        }
    }
}