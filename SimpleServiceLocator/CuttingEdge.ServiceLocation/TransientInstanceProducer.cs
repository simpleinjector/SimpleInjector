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
    /// Allows retrieval of concrete transient instances of type <typeparamref name="TConcrete"/>, where the
    /// producer postpones the creation of the instanceCreator delegate till the first time an instance is
    /// requested. This ensures the compiled delegate to be as efficient as possible, because all dependencies
    /// will be registered by that time.
    /// </summary>
    /// <typeparam name="TConcrete">The concrete type to create.</typeparam>
    internal sealed class TransientInstanceProducer<TConcrete> : IInstanceProducer
    {
        private readonly SimpleServiceLocator container;

        private readonly Action<TConcrete> instanceInitializer;
        private Func<TConcrete> instanceCreator;

        /// <summary>Initializes a new instance of the <see cref="TransientInstanceProducer{TConcrete}"/> class.</summary>
        /// <param name="container">The parent container.</param>
        public TransientInstanceProducer(SimpleServiceLocator container)
        {
            this.container = container;
        }

        internal TransientInstanceProducer(SimpleServiceLocator container,
            Action<TConcrete> instanceInitializer)
            : this(container)
        {
            this.instanceInitializer = instanceInitializer;
        }

        /// <summary>Builds an expression that expresses the intent to get an instance by the current producer.</summary>
        /// <returns>An Expression.</returns>
        Expression IInstanceProducer.BuildExpression()
        {
            // Create an expression that directly calls the this.GetInstance() method.
            // We could further optimize it by directly calling the Func<T> instanceCreator, but this will
            // make us loose some error checking.
            return Expression.Call(Expression.Constant(this),
                typeof(TransientInstanceProducer<TConcrete>).GetMethod("GetInstance"),
                new Expression[0]);
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
            if (this.instanceCreator == null)
            {
                this.CreateInstanceCreator();
            }

            try
            {
                // instanceCreator can never return null.
                return this.instanceCreator();
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    StringResources.ErrorWhileTryingToGetInstanceOfType(typeof(TConcrete), ex));
            }
        }

        private void CreateInstanceCreator()
        {
            // We use a lock to prevent the delegate to be created more than once. Not strictly needed, but
            // it won't harm us either.
            // We can take a lock on this, because instances of this type are never publicly exposed.
            lock (this)
            {
                if (this.instanceCreator == null)
                {
                    var snapshot = this.container.Registrations;

                    // Adding the snapshot will improve the quality of the created delegate.
                    var instanceProducer = DelegateBuilder.Build<TConcrete>(snapshot, this.container);

                    if (this.instanceInitializer != null)
                    {
                        this.instanceCreator = () =>
                        {
                            TConcrete instance = instanceProducer();

                            this.instanceInitializer(instance);

                            return instance;
                        };
                    }
                    else
                    {
                        this.instanceCreator = instanceProducer;
                    }
                }
            }
        }
    }
}