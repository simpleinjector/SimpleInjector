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
using System.Linq.Expressions;

namespace SimpleInjector
{
    /// <summary>
    /// Allows retrieval of instances of type <typeparamref name="T"/> that are resolved using unregistered
    /// type resolution.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    internal sealed class ResolutionInstanceProducer<T> : IInstanceProducer where T : class
    {
        private readonly Func<object> instanceCreator;
        private CyclicDependencyValidator validator = new CyclicDependencyValidator(typeof(T));

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolutionInstanceProducer{T}"/> class.
        /// </summary>
        /// <param name="instanceCreator">The delegate that knows how to create that type.</param>
        public ResolutionInstanceProducer(Func<object> instanceCreator)
        {
            this.instanceCreator = instanceCreator;
        }

        /// <summary>Builds an expression that expresses the intent to get an instance by the current producer.</summary>
        /// <returns>An Expression.</returns>
        Expression IInstanceProducer.BuildExpression()
        {
            // Create an expression that directly calls the this.GetInstance() method.
            // We could further optimize it by directly calling the Func<T> instanceCreator, but this will
            // make us loose some error checking.
            return Expression.Call(Expression.Constant(this),
                typeof(ResolutionInstanceProducer<T>).GetMethod("GetInstance"), new Expression[0]);
        }

        /// <summary>Produces an instance.</summary>
        /// <returns>An instance.</returns>
        object IInstanceProducer.GetInstance()
        {
            return this.GetInstance();
        }

        /// <summary>Produces an instance.</summary>
        /// <returns>An instance.</returns>
        public T GetInstance()
        {
            object instance;

            this.validator.Prevent();

            try
            {
                instance = this.instanceCreator();

                this.RemoveValidator();
            }
            catch (Exception ex)
            {
                this.validator.Reset();

                throw new ActivationException(StringResources
                    .HandlerReturnedADelegateThatThrewAnException(typeof(T), ex.Message), ex);
            }

            if (instance == null)
            {
                throw new ActivationException(
                    StringResources.HandlerReturnedADelegateThatReturnedNull(typeof(T)));
            }

            try
            {
                return (T)instance;
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    StringResources.HandlerReturnedDelegateThatReturnedAnUnassignableFrom(typeof(T),
                    instance.GetType()), ex);
            }
        }

        // This method will be inlined by the JIT.
        private void RemoveValidator()
        {
            // No recursive calls detected, we can remove the recursion validator to increase performance.
            // We first check for null, because this is faster. Every time we write, the CPU has to send
            // the new value to all the other CPUs.
            if (this.validator != null)
            {
                this.validator = null;
            }
        }
    }
}