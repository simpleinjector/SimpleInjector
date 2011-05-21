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
    /// <summary>Ensures that the wrapped delegate will only be executed once.</summary>
    /// <typeparam name="T">The interface or base type that can be used to retrieve instances.</typeparam>
    [DebuggerDisplay(Helpers.InstanceProviderDebuggerDisplayString)]
    internal sealed class FuncSingletonInstanceProducer<T> : IInstanceProducer where T : class
    {
        private Func<T> instanceCreator;
        private bool instanceCreated;
        private T instance;
        private CyclicDependencyValidator validator = new CyclicDependencyValidator(typeof(T));

        internal FuncSingletonInstanceProducer(Func<T> instanceCreator)
        {
            this.instanceCreator = instanceCreator;
        }

        /// <summary>Gets the <see cref="Type"/> for which this producer produces instances.</summary>
        Type IInstanceProducer.ServiceType
        {
            get { return typeof(T); }
        }

        /// <summary>Produces an instance.</summary>
        /// <returns>An instance.</returns>
        public object GetInstance()
        {
            // We use a lock to prevent the delegate to be called more than once during the lifetime of
            // the application. We use a double checked lock to prevent the lock statement from being 
            // called again after the instance was created.
            if (!this.instanceCreated)
            {
                // We can take a lock on this, because instances of this type are never publicly exposed.
                lock (this)
                {
                    if (!this.instanceCreated)
                    {
                        this.instance = this.GetInstanceFromCreatorWithRecursiveCheck();

                        this.instanceCreated = true;
                    }
                }
            }

            return this.instance;
        }

        /// <summary>Builds an expression that expresses the intent to get an instance by the current producer.</summary>
        /// <returns>An Expression.</returns>
        Expression IInstanceProducer.BuildExpression()
        {
            return Expression.Constant(this.GetInstance());
        }

        private T GetInstanceFromCreatorWithRecursiveCheck()
        {
            this.validator.Prevent();

            T instance = this.GetInstanceFromCreator();

            // Remove the reference to the validator; it is not needed anymore.
            this.validator = null;

            return instance;
        }

        private T GetInstanceFromCreator()
        {
            T instance;

            try
            {
                instance = this.instanceCreator();
            }
            catch (ActivationException)
            {
                // This extra catch statement prevents ActivationExceptions from being wrapped in a new
                // ActivationException. This FuncSingletonInstanceProducer is used as wrapper around
                // TransientInstanceProducer instances that can throw ActivationException on their own.
                // Wrapping these again in a ActivationException would obfuscate the real error.
                throw;
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    StringResources.DelegateForTypeThrewAnException(typeof(T), ex), ex);
            }

            if (instance == null)
            {
                throw new ActivationException(StringResources.DelegateForTypeReturnedNull(typeof(T)));
            }

            // Remove the reference to the delegate; it is not needed anymore.
            this.instanceCreator = null;

            return instance;
        }
    }
}