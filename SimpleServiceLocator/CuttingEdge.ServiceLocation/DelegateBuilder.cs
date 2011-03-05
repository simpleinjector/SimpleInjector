#region Copyright (c) 2010 S. van Deursen
/* The SimpleServiceLocator library is a simple but complete implementation of the CommonServiceLocator 
 * interface.
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.Practices.ServiceLocation;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Builds <see cref="Func{T}"/> delegates that can create a new instance of the supplied Type, where
    /// the supplied container will be used to locate the constructor arguments. The generated code of the
    /// built <see cref="Func{T}"/> might look like this.
    /// <![CDATA[
    ///     Func<object> func = () => return new Samurai(container.GetInstance<IWeapon>());
    /// ]]>
    /// </summary>
    internal static class DelegateBuilder
    {
        internal static Func<TConcrete> Build<TConcrete>(SimpleServiceLocator container)
        {
            var newExpression = BuildExpression<TConcrete>(container);

            var newServiceTypeMethod = Expression.Lambda<Func<TConcrete>>(
                newExpression, new ParameterExpression[0]);

            return newServiceTypeMethod.Compile();
        }

        internal static Expression BuildExpression<TConcrete>(SimpleServiceLocator container)
        {
            Helpers.ThrowActivationExceptionWhenTypeIsNotConstructable(typeof(TConcrete));

            var constructor = typeof(TConcrete).GetConstructors().Single();

            var constructorArgumentCalls =
                from parameter in constructor.GetParameters()
                select BuildParameterExpression<TConcrete>(container, parameter.ParameterType);

            return Expression.New(constructor, constructorArgumentCalls.ToArray());
        }
        
        private static Expression BuildParameterExpression<TConcrete>(SimpleServiceLocator container, 
            Type parameterType)
        {
            var instanceProducer = container.GetInstanceProducerForType(parameterType);

            if (instanceProducer == null)
            {
                throw new ActivationException(
                    StringResources.ParameterTypeMustBeRegistered(typeof(TConcrete), parameterType));
            }

            return instanceProducer.BuildExpression();
        }
    }
}