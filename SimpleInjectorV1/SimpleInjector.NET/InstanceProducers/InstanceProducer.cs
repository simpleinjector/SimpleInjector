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

namespace SimpleInjector.InstanceProducers
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    /// <summary>Base class for producing instances.</summary>
    [DebuggerDisplay(Helpers.InstanceProviderDebuggerDisplayString)]
    internal abstract class InstanceProducer : IInstanceProducer
    {
        private readonly object instanceCreationLock = new object();

        private CyclicDependencyValidator validator;
        private Func<object> instanceCreator;
        private Expression expression;
        private bool? isValid = true;

        /// <summary>Initializes a new instance of the <see cref="InstanceProducer"/> class.</summary>
        /// <param name="serviceType">The type of the service this instance will produce.</param>
        protected InstanceProducer(Type serviceType)
        {
            this.ServiceType = serviceType;
            this.validator = new CyclicDependencyValidator(serviceType);
        }

        /// <summary>Gets the service type for which this producer produces instances.</summary>
        /// <value>A <see cref="Type"/> instance.</value>
        public Type ServiceType { get; private set; }

        internal Container Container { get; set; }

        internal bool IsResolvedThroughUnregisteredTypeResolution
        {
            set { this.isValid = value ? null : (bool?)true; }
        }

        // Will only return false when the type is a concrete unregistered type that was automatically added
        // by the container, while the expression can not be generated.
        // Types that are registered upfront are always considered to be valid, while unregistered types must
        // be validated. The reason for this is that we must prevent the container to throw an exception when
        // GetRegistration() is called for an unregistered (concrete) type that can not be resolved.
        internal bool IsValid
        {
            get
            {
                if (this.isValid == null)
                {
                    this.isValid = this.CanBuildExpression();
                }

                return this.isValid.Value;
            }
        }

        /// <summary>
        /// Builds an expression that expresses the intent to get an instance by the current producer.
        /// </summary>
        /// <returns>An Expression.</returns>
        public Expression BuildExpression()
        {
            this.validator.CheckForRecursiveCalls();

            try
            {
                this.expression = this.GetExpression();

                this.RemoveValidator();

                return this.expression;
            }
            catch (Exception ex)
            {
                this.validator.Reset();

                this.ThrowErrorWhileTryingToGetInstanceOfType(ex);

                throw;
            }
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
                    this.instanceCreator = this.BuildInstanceCreator();
                }

                instance = this.instanceCreator();

                this.RemoveValidator();
            }
            catch (Exception ex)
            {
                this.validator.Reset();

                this.ThrowErrorWhileTryingToGetInstanceOfType(ex);

                throw;
            }

            if (instance == null)
            {
                throw new ActivationException(this.BuildRegisteredDelegateForTypeReturnedNullExceptionMessage());
            }

            return instance;
        }

        /// <summary>
        /// Builds an expression that expresses the intent to get an instance by the current producer.
        /// </summary>
        /// <returns>An Expression.</returns>
        protected abstract Expression BuildExpressionCore();

        protected virtual string BuildErrorWhileTryingToGetInstanceOfTypeExceptionMessage()
        {
            return StringResources.DelegateForTypeThrewAnException(this.ServiceType);
        }

        protected virtual string BuildRegisteredDelegateForTypeReturnedNullExceptionMessage()
        {
            return StringResources.DelegateForTypeReturnedNull(this.ServiceType);
        }

        protected virtual string BuildErrorWhileBuildingDelegateFromExpressionExceptionMessage(
            Expression expression, Exception exception)
        {
            return StringResources.ErrorWhileBuildingDelegateFromExpression(this.ServiceType, expression, 
                exception);
        }

        private Func<object> BuildInstanceCreator()
        {
            // Don't do recursive checks. The GetInstance() already does that.
            var expression = this.GetExpression();

            try
            {
                var newInstanceMethod = Expression.Lambda<Func<object>>(expression, new ParameterExpression[0]);

                return newInstanceMethod.Compile();
            }
            catch (Exception ex)
            {
                string message = this.BuildErrorWhileBuildingDelegateFromExpressionExceptionMessage(
                    expression, ex);

                throw new ActivationException(message, ex);
            }
        }

        private Expression GetExpression()
        {
            // Prevent the Expression from being built more than once on this InstanceProducer. Note that this
            // still means that the expression can be created multiple times for a single service type, because
            // the container does not guarantee that a single InstanceProducer is created, just as the
            // ResolveUnregisteredType event can be called multiple times for a single service type.
            if (this.expression == null)
            {
                lock (this.instanceCreationLock)
                {
                    if (this.expression == null)
                    {
                        this.expression = this.BuildExpressionWithInterception();
                    }
                }
            }

            return this.expression;
        }

        private Expression BuildExpressionWithInterception()
        {
            var expression = this.BuildExpressionCore();

            var e = new ExpressionBuiltEventArgs(this.ServiceType, expression);

            this.Container.OnExpressionBuilt(e);

            return e.Expression;
        }

        private void ThrowErrorWhileTryingToGetInstanceOfType(Exception innerException)
        {
            string exceptionMessage = this.BuildErrorWhileTryingToGetInstanceOfTypeExceptionMessage();

            // Prevent wrapping duplicate exceptions.
            if (!innerException.Message.StartsWith(exceptionMessage, StringComparison.OrdinalIgnoreCase))
            {
                throw new ActivationException(exceptionMessage + " " + innerException.Message, innerException);
            }
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

        private bool CanBuildExpression()
        {
            try
            {
                // Test if the instance can be made.
                this.BuildExpression();

                return true;
            }
            catch (ActivationException)
            {
                return false;
            }
        }
    }
}