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
        private static readonly MethodInfo GenericGetInstanceMethodDefinition =
            typeof(IServiceLocator).GetMethod("GetInstance", Type.EmptyTypes);

        private readonly Type serviceType;
        private readonly Dictionary<Type, Func<object>> registrations;
        private readonly IServiceLocator container;

        private DelegateBuilder(Type serviceType, Dictionary<Type, Func<object>> registrations,
            IServiceLocator container)
        {
            this.serviceType = serviceType;
            this.registrations = registrations;
            this.container = container;
        }

        internal static Func<object> Build(Type serviceType, IServiceLocator serviceLocator)
        {
            var builder = new DelegateBuilder(serviceType, null, serviceLocator);
            return builder.Build();
        }

        internal static Func<TConcrete> Build<TConcrete>(IServiceLocator serviceLocator)
        {
            var builder = new DelegateBuilder(typeof(TConcrete), null, serviceLocator);
            return builder.Build<TConcrete>();
        }

        internal static Func<object> Build(Type serviceType, Dictionary<Type, Func<object>> registrations,
            IServiceLocator serviceLocator)
        {
            var builder = new DelegateBuilder(serviceType, registrations, serviceLocator);
            return builder.Build();
        }

        private Func<object> Build()
        {
            return this.Build<object>();
        }

        private Func<TConcrete> Build<TConcrete>()
        {
            var constructor = this.GetPublicConstructor();

            Expression[] constructorArgumentCalls = this.BuildGetInstanceCallsForConstructor(constructor);

            var newServiceTypeMethod = Expression.Lambda<Func<TConcrete>>(
                Expression.New(constructor, constructorArgumentCalls), new ParameterExpression[0]);

            return newServiceTypeMethod.Compile();
        }

        private ConstructorInfo GetPublicConstructor()
        {
            var constructors = this.serviceType.GetConstructors();

            if (constructors.Length != 1)
            {
                throw new ActivationException(StringResources.TypeMustHaveASinglePublicConstructor(
                    this.serviceType, constructors.Length));
            }

            return constructors[0];
        }

        private Expression[] BuildGetInstanceCallsForConstructor(ConstructorInfo constructor)
        {
            List<Expression> getInstanceCalls = new List<Expression>();

            foreach (var parameter in constructor.GetParameters())
            {
                var getInstanceCall = this.BuildGetInstanceCallForType(parameter.ParameterType);

                getInstanceCalls.Add(getInstanceCall);
            }

            return getInstanceCalls.ToArray();
        }

        private Expression BuildGetInstanceCallForType(Type parameterType)
        {
            this.ValidateParameterType(parameterType);

            var getInstanceMethod = MakeGenericGetInstanceMethod(parameterType);

            // Build the call "serviceLocator.GetInstance<[ParameterType]>()"
            return Expression.Call(Expression.Constant(this.container), getInstanceMethod,
                new Expression[0]);
        }

        private void ValidateParameterType(Type parameterType)
        {
            if (this.registrations == null)
            {
                // Only validate when the registrations collection is supplied. When it is missing, this means
                // that the delegate is constructed in the configuration phase, and registrations may be done
                // in random order. Therefore, we simply assume that all registrations are valid.
                return;
            }

            if (this.registrations.ContainsKey(parameterType))
            {
                // The registrations contains the type: we are valid.
                return;
            }

            if (SimpleServiceLocator.IsConcreteType(parameterType))
            {
                // The type to construct is not registered, but is a concrete type. Concrete types can be
                // created. We pospone validation untill the moment that the delegate for this concrete type
                // is generated.
                return;
            }

            throw new ActivationException(
                StringResources.ParameterTypeMustBeRegistered(this.serviceType, parameterType));
        }

        private static MethodInfo MakeGenericGetInstanceMethod(Type parameterType)
        {
            return GenericGetInstanceMethodDefinition.MakeGenericMethod(parameterType);
        }
    }
}