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
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace SimpleInjector
{
    /// <summary>Base class for producing instances.</summary>
    [DebuggerDisplay(Helpers.InstanceProviderDebuggerDisplayString)]
    public abstract class InstanceProducer : IInstanceProducer
    {
        private readonly object instanceCreationLock = new object();

        private CyclicDependencyValidator validator;
        private Func<object> instanceCreator;

        /// <summary>Initializes a new instance of the <see cref="InstanceProducer"/> class.</summary>
        /// <param name="serviceType">The type of the service this instance will produce.</param>
        protected InstanceProducer(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            this.ServiceType = serviceType;
            this.validator = new CyclicDependencyValidator(serviceType);
        }

        /// <summary>Gets the service type for which this producer produces instances.</summary>
        /// <value>A <see cref="Type"/> instance.</value>
        public Type ServiceType { get; private set; }

        internal Container Container { get; set; }

        /// <summary>
        /// Builds an expression that expresses the intent to get an instance by the current producer.
        /// </summary>
        /// <returns>An Expression.</returns>
        public Expression BuildExpression()
        {
            return this.BuildExpressionWithCheckForRecursiveCalls();
        }

        /// <summary>Produces an instance.</summary>
        /// <returns>An instance. Will never return null.</returns>
        /// <exception cref="ActivationException">When the instance could not be retrieved or is null.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification =
            "A property is not appropriate, because get instance could possibly be a heavy ")]
        public object GetInstance()
        {
            this.validator.CheckForRecursiveCalls();

            object instance;

            try
            {
                if (this.instanceCreator == null)
                {
                    this.SetInstanceCreator();
                }

                instance = this.instanceCreator();

                this.RemoveValidator();
            }
            catch (Exception ex)
            {
                this.validator.Reset();

                throw this.BuildErrorWhileTryingToGetInstanceOfTypexception(ex);
            }

            if (instance == null)
            {
                throw this.BuildRegisteredDelegateForTypeReturnedNullException();
            }

            return instance;
        }

        internal virtual Func<object> BuildInstanceCreator()
        {
            // Don't do recursive checks. The GetInstance() already does that.
            var expression = this.BuildExpressionWithoutCheckForRecursiveCalls();

            try
            {
                var newInstanceMethod = Expression.Lambda<Func<object>>(expression, new ParameterExpression[0]);

                return newInstanceMethod.Compile();
            }
            catch (Exception ex)
            {
                throw new ActivationException(StringResources.ErrorWhileBuildingDelegateFromExpression(
                    this.ServiceType, expression, ex), ex);
            }
        }

        internal virtual ActivationException BuildErrorWhileTryingToGetInstanceOfTypexception(Exception ex)
        {
            return new ActivationException(
                StringResources.DelegateForTypeThrewAnException(this.ServiceType, ex), ex);
        }

        internal virtual ActivationException BuildRegisteredDelegateForTypeReturnedNullException()
        {
            return new ActivationException(StringResources.DelegateForTypeReturnedNull(this.ServiceType));
        }

        /// <summary>
        /// Builds an expression that expresses the intent to get an instance by the current producer.
        /// </summary>
        /// <returns>An Expression.</returns>
        protected abstract Expression BuildExpressionCore();

        private void SetInstanceCreator()
        {
            // We use a lock to prevent the delegate to be created more than once.
            lock (this.instanceCreationLock)
            {
                if (this.instanceCreator == null)
                {
                    this.instanceCreator = this.BuildInstanceCreator();
                }
            }
        }

        private Expression BuildExpressionWithCheckForRecursiveCalls()
        {
            this.validator.CheckForRecursiveCalls();

            try
            {
                return this.BuildExpressionWithoutCheckForRecursiveCalls();
            }
            finally
            {
                this.validator.Reset();
            }
        }

        private Expression BuildExpressionWithoutCheckForRecursiveCalls()
        {
            return this.BuildExpressionCore();
        }

        // This method will be inlined by the JIT.
        private void RemoveValidator()
        {
            // No recursive calls detected, we can remove the validator to increase performance.
            // We first check for null, because this is faster. Every time we write, the CPU has to send
            // the new value to all the other CPUs. We only nullify the validator while using the GetInstance
            // method, because the BuildExpression will only be called a limited amount of time.
            if (this.validator != null)
            {
                this.validator = null;
            }
        }
    }
}
