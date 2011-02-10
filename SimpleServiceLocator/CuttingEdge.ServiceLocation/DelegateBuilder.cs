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
    internal sealed class DelegateBuilder
    {
        private readonly Type serviceType;
        private readonly Dictionary<Type, IInstanceProducer> registrations;
        private readonly SimpleServiceLocator container;

        private DelegateBuilder(Type serviceType, Dictionary<Type, IInstanceProducer> registrations,
            SimpleServiceLocator container)
        {
            this.serviceType = serviceType;
            this.registrations = registrations;
            this.container = container;
        }

        internal static Func<T> Build<T>(Dictionary<Type, IInstanceProducer> registrations,
            SimpleServiceLocator serviceLocator)
        {
            var builder = new DelegateBuilder(typeof(T), registrations, serviceLocator);
            return builder.Build<T>();
        }

        private Func<TConcrete> Build<TConcrete>()
        {
            Helpers.ThrowActivationExceptionWhenTypeIsNotConstructable(this.serviceType);

            var constructor = this.serviceType.GetConstructors().First();

            Expression[] constructorArgumentCalls = this.BuildGetInstanceCallsForConstructor(constructor);

            var newServiceTypeMethod = Expression.Lambda<Func<TConcrete>>(
                Expression.New(constructor, constructorArgumentCalls), new ParameterExpression[0]);

            return newServiceTypeMethod.Compile();
        }

        private Expression[] BuildGetInstanceCallsForConstructor(ConstructorInfo constructor)
        {
            List<Expression> parameterExpressions = new List<Expression>();

            foreach (var parameter in constructor.GetParameters())
            {
                var parameterExpression = this.BuildParameterExpression(parameter.ParameterType);

                parameterExpressions.Add(parameterExpression);
            }

            return parameterExpressions.ToArray();
        }

        private Expression BuildParameterExpression(Type parameterType)
        {
            var instanceProducer =
                this.container.GetInstanceProducerForType(parameterType, this.registrations);

            if (instanceProducer == null)
            {
                throw new ActivationException(
                    StringResources.ParameterTypeMustBeRegistered(this.serviceType, parameterType));
            }

            return instanceProducer.BuildExpression();
        }
    }
}