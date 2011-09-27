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

namespace SimpleInjector
{
    [DebuggerDisplay(Helpers.InstanceProviderDebuggerDisplayString)]
    internal class ExpressionResolutionInstanceProducer<T> : IInstanceProducer where T : class
    {
        private readonly object instanceCreationLock = new object();
        private readonly Expression expression;

        private CyclicDependencyValidator validator = new CyclicDependencyValidator(typeof(T));
        private Func<T> instanceCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionResolutionInstanceProducer{T}"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public ExpressionResolutionInstanceProducer(Expression expression)
        {
            this.expression = expression;
        }

        /// <summary>Gets the <see cref="Type"/> for which this producer produces instances.</summary>
        Type IInstanceProducer.ServiceType
        {
            get { return typeof(T); }
        }

        /// <summary>Builds an expression that expresses the intent to get an instance by the current producer.</summary>
        /// <returns>An Expression.</returns>
        public Expression BuildExpression()
        {
            return this.expression;
        }

        /// <summary>Produces an instance.</summary>
        /// <returns>An instance.</returns>
        object IInstanceProducer.GetInstance()
        {
            this.validator.CheckForRecursiveCalls();

            if (this.instanceCreator == null)
            {
                this.SetInstanceCreator();
            }

            T instance;

            try
            {
                instance = this.instanceCreator();

                this.RemoveValidator();
            }
            catch (Exception ex)
            {
                this.validator.Reset();

                throw new ActivationException(
                    StringResources.ErrorWhileTryingToGetInstanceOfType(typeof(T), ex), ex);
            }

            if (instance == null)
            {
                throw new ActivationException(
                    StringResources.HandlerReturnedADelegateThatReturnedNull(typeof(T)));
            }

            return instance;
        }

        private void SetInstanceCreator()
        {
            // We use a lock to prevent the delegate to be created more than once. Not strictly needed, but
            // it won't harm us either.
            lock (this.instanceCreationLock)
            {
                if (this.instanceCreator == null)
                {
                    this.instanceCreator = this.BuildInstanceCreator();
                }
            }
        }

        private Func<T> BuildInstanceCreator()
        {
            try
            {
                return Expression.Lambda<Func<T>>(this.expression, new ParameterExpression[0]).Compile();
            }
            catch (Exception ex)
            {
                throw new ActivationException(StringResources.ErrorWhileBuildingDelegateFromExpression(
                    typeof(T), this.expression, ex), ex);
            }
        }

        // This method will be inlined by the JIT.
        private void RemoveValidator()
        {
            if (this.validator != null)
            {
                this.validator = null;
            }
        }
    }
}