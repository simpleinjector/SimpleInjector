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
using System.Linq.Expressions;

namespace SimpleInjector.InstanceProducers
{
    internal sealed class FuncResolutionInstanceProducer<TService> : InstanceProducer where TService : class
    {
        private readonly Func<object> instanceCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FuncResolutionInstanceProducer{TService}"/> class.
        /// </summary>
        /// <param name="instanceCreator">The delegate that knows how to create that type.</param>
        public FuncResolutionInstanceProducer(Func<object> instanceCreator) : base(typeof(TService))
        {
            this.instanceCreator = instanceCreator;
        }

        public TService GetInstanceWithTypeCheck()
        {
            object instance;

            try
            {
                instance = this.instanceCreator();
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    StringResources.DelegateRegisteredUsingResolveUnregisteredTypeThatThrewAnException(
                    typeof(TService)) + " " + ex.Message, ex);
            }

            try
            {
                return (TService)instance;
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    StringResources.DelegateRegisteredUsingResolveUnregisteredTypeReturnedAnUnassignableFrom(
                    typeof(TService), instance.GetType()), ex);
            }
        }

        protected override Expression BuildExpressionCore()
        {
            // When building the expression, we need to check the type of the instance creator. The expression
            // will be used to build up a larger expression and would fail later on with a very unclear
            // exception when the given type is not of TService. 
            return Expression.Call(Expression.Constant(this), this.GetType().GetMethod("GetInstanceWithTypeCheck"));
        }

        protected override string BuildErrorWhileTryingToGetInstanceOfTypeExceptionMessage()
        {
            return StringResources.DelegateRegisteredUsingResolveUnregisteredTypeThatThrewAnException(
                typeof(TService));
        }

        protected override string BuildRegisteredDelegateForTypeReturnedNullExceptionMessage()
        {
            return StringResources.DelegateRegisteredUsingResolveUnregisteredTypeThatReturnedNull(
                typeof(TService));
        }
    }
}