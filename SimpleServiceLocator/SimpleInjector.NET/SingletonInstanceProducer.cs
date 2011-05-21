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
    /// <summary>Wraps an instance and returns that single instance every time.</summary>
    /// <typeparam name="T">The type, what else.</typeparam>
    internal sealed class SingletonInstanceProducer<T> : IInstanceProducer where T : class
    {
        // Storing a key is not needed, because the Validate method will never throw.
        private readonly T instance;

        /// <summary>Initializes a new instance of the <see cref="SingletonInstanceProducer{T}"/> class.</summary>
        /// <param name="instance">The single instance.</param>
        public SingletonInstanceProducer(T instance)
        {
            this.instance = instance;
        }

        /// <summary>Gets the <see cref="Type"/> for which this producer produces instances.</summary>
        Type IInstanceProducer.ServiceType
        {
            get { return typeof(T); }
        }

        /// <summary>Produces an instance.</summary>
        /// <returns>An instance. Will never return null.</returns>
        object IInstanceProducer.GetInstance()
        {
            return this.instance;
        }

        /// <summary>Builds an expression that expresses the intent to get an instance by the current producer.</summary>
        /// <returns>An Expression.</returns>
        Expression IInstanceProducer.BuildExpression()
        {
            return Expression.Constant(this.instance);
        }
    }
}