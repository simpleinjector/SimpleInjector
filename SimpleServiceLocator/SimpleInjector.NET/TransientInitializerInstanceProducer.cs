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
    /// Allows retrieval of concrete transient instances of type <typeparamref name="TConcrete"/> with will
    /// after creation be initialized by calling the supplied Action delegate..
    /// </summary>
    /// <typeparam name="TConcrete">The concrete type to create.</typeparam>
    internal sealed class TransientInitializerInstanceProducer<TConcrete> 
        : TransientInstanceProducer<TConcrete>
        where TConcrete : class
    {
        private readonly Action<TConcrete> instanceInitializer;

        internal TransientInitializerInstanceProducer(SimpleServiceLocator container,
            Action<TConcrete> instanceInitializer) : base(container)
        {
            this.instanceInitializer = instanceInitializer;
        }

        /// <summary>Builds an expression that expresses the intent to get an instance by the current producer.</summary>
        /// <returns>An Expression.</returns>
        public override Expression BuildExpression()
        {
            // It's not possible to return a Expression that is as heavily optimized as the 
            // TransientInstanceProducer can do, because we the instanceInitializer must be called as well.
            return Expression.Call(Expression.Constant(this), this.GetType().GetMethod("GetInstance"),
                new Expression[0]);           
        }

        /// <summary>
        /// Builds the delegate that allows the creation of instances of type TConcrete.
        /// </summary>
        /// <returns>Returns a new delegate.</returns>
        protected override Func<TConcrete> BuildInstanceCreator()
        {
            var creator = base.BuildInstanceCreator();

            return () =>
            {
                TConcrete instance = creator();

                this.instanceInitializer(instance);

                return instance;
            };
        }
    }   
}