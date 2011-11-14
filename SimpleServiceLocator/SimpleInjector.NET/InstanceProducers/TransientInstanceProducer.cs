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

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleInjector.InstanceProducers
{
    internal class TransientInstanceProducer<TService, TImplementation> : InstanceProducer
        where TImplementation : class, TService
        where TService : class
    {
        public TransientInstanceProducer() : base(typeof(TService))
        {
        }

        internal override ActivationException BuildErrorWhileTryingToGetInstanceOfTypexception(Exception ex)
        {
            return new ActivationException(
                StringResources.ErrorWhileTryingToGetInstanceOfType(this.ServiceType, ex), ex);
        }

        protected override Expression BuildExpressionCore()
        {
            NewExpression newExpression = this.BuildNewExpression();

            Action<TImplementation> instanceInitializer = this.Container.GetInitializerFor<TImplementation>();

            if (instanceInitializer != null)
            {
                // It's not possible to return a Expression that is as heavily optimized as the newExpression
                // simply is, because the instance initializer must be called as well.
                return BuildExpressionWithInstanceInitializer(newExpression, instanceInitializer);
            }

            return newExpression;
        }

        private static Expression BuildExpressionWithInstanceInitializer(Expression newExpression,
            Action<TImplementation> instanceInitializer)
        {
            var instanceCreator =
                Expression.Lambda<Func<TImplementation>>(newExpression, new ParameterExpression[0]).Compile();

            Func<TImplementation> instanceCreatorWithInitializer = () =>
            {
                TImplementation instance = instanceCreator();

                instanceInitializer(instance);

                return instance;
            };

            var wrapper = new InstanceCreatorWrapper<TImplementation>(instanceCreatorWithInitializer);

            return wrapper.GetInvocationExpression();
        }

        private NewExpression BuildNewExpression()
        {
            Helpers.ThrowActivationExceptionWhenTypeIsNotConstructable(typeof(TImplementation));

            var constructor = typeof(TImplementation).GetConstructors().Single();

            var constructorArgumentCalls =
                from parameter in constructor.GetParameters()
                select this.BuildParameterExpression(parameter.ParameterType);

            return Expression.New(constructor, constructorArgumentCalls.ToArray());
        }

        private Expression BuildParameterExpression(Type parameterType)
        {
            var instanceProducer = this.Container.GetRegistration(parameterType);

            if (instanceProducer != null)
            {
                return instanceProducer.BuildExpression();
            }

            throw new ActivationException(
                StringResources.ParameterTypeMustBeRegistered(typeof(TImplementation), parameterType));
        }
    }
}