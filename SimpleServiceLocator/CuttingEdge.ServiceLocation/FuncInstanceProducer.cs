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
using System.Linq.Expressions;

using Microsoft.Practices.ServiceLocation;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Allows producing instances based on a supplied Func{T} delegate.
    /// </summary>
    /// <typeparam name="T">Type service type.</typeparam>
    internal sealed class FuncInstanceProducer<T> : IInstanceProducer
    {
        // The key of the producer if it is part of a IKeyedInstanceProducer, or null if not.
        private readonly Func<T> instanceCreator;

        internal FuncInstanceProducer(Func<T> instanceCreator)
        {
            this.instanceCreator = instanceCreator;
        }

        /// <summary>Produces an instance.</summary>
        /// <returns>An instance.</returns>
        object IInstanceProducer.GetInstance()
        {
            return this.GetInstance();
        }

        /// <summary>Builds an expression that expresses the intent to get an instance by the current producer.</summary>
        /// <returns>An Expression.</returns>
        Expression IInstanceProducer.BuildExpression()
        {
            // Create an expression that directly calls the this.GetInstance() method.
            // We could further optimize it by directly calling the Func<T> instanceCreator, but this will
            // make us loose some error checking.
            return Expression.Call(Expression.Constant(this),
                typeof(FuncInstanceProducer<T>).GetMethod("GetInstance"), new Expression[0]);
        }

        /// <summary>Produces an instance.</summary>
        /// <returns>An instance.</returns>
        public T GetInstance()
        {
            T instance;

            try
            {
                instance = this.instanceCreator();
            }
            catch (Exception ex)
            {
                // TODO: This catch might cost us some performance. Find out if it does.
                throw new ActivationException(
                    StringResources.DelegateForTypeThrewAnException(typeof(T), ex));
            }

            if (instance == null)
            {
                throw new ActivationException(
                    StringResources.RegisteredDelegateForTypeReturnedNull(typeof(T)));
            }

            return instance;
        }
    }
}