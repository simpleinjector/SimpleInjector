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
    /// <summary>
    /// Interface for the transient instance producer.
    /// </summary>
    internal interface ITransientInstanceProducer
    {
        Container Container { set; }

        /// <summary>Gets or sets the <see cref="Type"/> for which this producer produces instances.</summary>
        /// <value>A <see cref="Type"/> instance.</value>
        Type ServiceType { get; set; }
    }

    /// <summary>
    /// Allows retrieval of concrete transient instances of type <typeparamref name="TConcrete"/>, where the
    /// producer postpones the creation of the instanceCreator delegate till the first time an instance is
    /// requested. This ensures the compiled delegate to be as efficient as possible, because all dependencies
    /// will be registered by that time.
    /// </summary>
    /// <typeparam name="TConcrete">The concrete type to create.</typeparam>
    internal class TransientInstanceProducer<TConcrete> : IInstanceProducer, ITransientInstanceProducer
        where TConcrete : class
    {
        private Container container;

        private Func<TConcrete> instanceCreator;
        private CyclicDependencyValidator validator = new CyclicDependencyValidator(typeof(TConcrete));

        /// <summary>Initializes a new instance of the <see cref="TransientInstanceProducer{TConcrete}"/> class.</summary>
        public TransientInstanceProducer()
        {
            this.ServiceType = typeof(TConcrete);
        }

        /// <summary>Gets the <see cref="Type"/> for which this producer produces instances.</summary>
        Type IInstanceProducer.ServiceType
        {
            get { return this.ServiceType; }
        }

        /// <summary>Sets the container.</summary>
        Container ITransientInstanceProducer.Container 
        { 
            set { this.container = value; } 
        }

        /// <summary>Gets or sets the <see cref="Type"/> for which this producer produces instances.</summary>
        public Type ServiceType { get; set; }

        /// <summary>Builds an expression that expresses the intent to get an instance by the current producer.</summary>
        /// <returns>An Expression.</returns>
        public Expression BuildExpression()
        {
            this.validator.Prevent();

            try
            {
                bool hasInitializer = this.container.GetInitializerFor<TConcrete>() != null;

                if (!hasInitializer)
                {
                    // For this producer we can return an Expression that just directly news up the instance,
                    // without the need to call the GetInstance method. It can't get any faster than this. 
                    // Downside of this approach is that in the case of a failure, there is less error 
                    // information available (see the catch in the GetInstance method).
                    return DelegateBuilder.BuildExpression(this.container, typeof(TConcrete));
                }
                else
                {
                    // It's not possible to return a Expression that is as heavily optimized as above, because
                    // the instance initializer must be called as well.
                    return Expression.Call(Expression.Constant(this), this.GetType().GetMethod("GetInstance"),
                        new Expression[0]);
                }
            }
            finally
            {
                this.validator.Reset();
            }
        }

        /// <summary>Produces an instance.</summary>
        /// <returns>An instance.</returns>
        object IInstanceProducer.GetInstance()
        {
            return this.GetInstance();
        }

        /// <summary>Produces an instance.</summary>
        /// <returns>An instance.</returns>
        public TConcrete GetInstance()
        {
            this.validator.Prevent();

            if (this.instanceCreator == null)
            {
                this.SetInstanceCreator();
            }

            try
            {
                // instanceCreator can never return null, because it is generated by us.
                var instance = this.instanceCreator();

                this.RemoveValidator();

                return instance;
            }
            catch (Exception ex)
            {
                this.validator.Reset();

                throw new ActivationException(
                    StringResources.ErrorWhileTryingToGetInstanceOfType(typeof(TConcrete), ex), ex);
            }
        }

        /// <summary>Returns a string that represents the current instance.</summary>
        /// <returns>A string that represents the current instance.</returns>
        public override string ToString()
        {
            return this.GetDescription();
        }

        internal static TransientInstanceProducer<TConcrete> Create(Container container, Type serviceType)
        {
            return new TransientInstanceProducer<TConcrete>()
            {
                container = container,
                ServiceType = serviceType
            };
        }

        private void SetInstanceCreator()
        {
            // We use a lock to prevent the delegate to be created more than once. Not strictly needed, but
            // it won't harm us either.
            // We can take a lock on this, because instances of this type are never publicly exposed.
            lock (this)
            {
                if (this.instanceCreator == null)
                {
                    this.instanceCreator = this.BuildInstanceCreator();
                }
            }
        }

        private Func<TConcrete> BuildInstanceCreator()
        {
            Action<TConcrete> instanceInitializer = this.container.GetInitializerFor<TConcrete>();

            if (instanceInitializer == null)
            {
                return DelegateBuilder.Build<TConcrete>(this.container);
            }
            else
            {
                return this.BuildInstanceCreatorWithInitializer(instanceInitializer);
            }            
        }

        private Func<TConcrete> BuildInstanceCreatorWithInitializer(Action<TConcrete> instanceInitializer)
        {
            var instanceCreator = DelegateBuilder.Build<TConcrete>(this.container);

            return () =>
            {
                TConcrete instance = instanceCreator();

                instanceInitializer(instance);

                return instance;
            };
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