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

namespace SimpleInjector
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides data for and interaction with the 
    /// <see cref="Container.ResolveUnregisteredType">ResolveUnregisteredType</see> event of 
    /// the <see cref="Container"/>. An observer can check the 
    /// <see cref="UnregisteredServiceType"/> to see whether the unregistered type can be handled. The
    /// <see cref="Register(Func{object})"/> method can be called to register a <see cref="Func{T}"/> delegate 
    /// that allows creation of instances of the unregistered for this and future requests.
    /// </summary>
    public class UnregisteredTypeEventArgs : EventArgs
    {
        /// <summary>Initializes a new instance of the UnregisteredTypeEventArgs class.</summary>
        /// <param name="unregisteredServiceType">The unregistered service type.</param>
        public UnregisteredTypeEventArgs(Type unregisteredServiceType)
        {
            this.UnregisteredServiceType = unregisteredServiceType;
        }

        /// <summary>Gets the unregistered service type that is currently requested.</summary>
        /// <value>The unregistered service type that is currently requested.</value>
        public Type UnregisteredServiceType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the event represented by this instance has been handled. 
        /// This property will return <b>true</b> when <see cref="Register(Func{object})"/> has been called on
        /// this instance.
        /// </summary>
        /// <value>The indication whether the event has been handled.</value>
        public bool Handled
        {
            get { return this.Expression != null; }
        }
        
        internal Expression Expression { get; private set; }

        /// <summary>
        /// Registers a <see cref="Func{T}"/> delegate that allows creation of instances of the type
        /// expressed by the <see cref="UnregisteredServiceType"/> for this and future requests. The delegate
        /// will be caches and future requests will directly call that delegate.
        /// </summary>
        /// <param name="instanceCreator">The delegate that allows creation of instances of the type
        /// expressed by the <see cref="UnregisteredServiceType"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="instanceCreator"/> is a
        /// null reference.</exception>
        /// <exception cref="ActivationException">Thrown when multiple observers that have registered to
        /// the <see cref="Container.ResolveUnregisteredType">ResolveUnregisteredType</see> event
        /// called this method for the same type.</exception>
        public void Register(Func<object> instanceCreator)
        {
            if (instanceCreator == null)
            {
                throw new ArgumentNullException("instanceCreator");
            }

            if (this.Handled)
            {
                throw new ActivationException(
                    StringResources.MultipleObserversRegisteredTheSameTypeToResolveUnregisteredType(
                        this.UnregisteredServiceType));
            }

            this.Expression = 
                Expression.Convert(
                    Expression.Invoke(
                        Expression.Constant(instanceCreator), new Expression[0]),
                    this.UnregisteredServiceType);
        }

        /// <summary>
        /// Registers an <see cref="Expression"/> that describes the creation of instances of the type
        /// expressed by the <see cref="UnregisteredServiceType"/> for this and future requests. The delegate
        /// will be caches and future requests will directly call that expression.
        /// </summary>
        /// <param name="expression">The expression that describes the creation of instances of the type
        /// expressed by the <see cref="UnregisteredServiceType"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="expression"/> is a
        /// null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="expression"/> is a
        /// not exactly of type <see cref="Func{T}"/> where T equals the <see cref="UnregisteredServiceType"/>.
        /// </exception>
        /// <exception cref="ActivationException">Thrown when multiple observers that have registered to
        /// the <see cref="Container.ResolveUnregisteredType">ResolveUnregisteredType</see> event
        /// called this method for the same type.</exception>
        public void Register(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (this.Handled)
            {
                throw new ActivationException(StringResources.MultipleObserversRegisteredTheSameTypeToResolveUnregisteredType(
                    this.UnregisteredServiceType));
            }

            this.Expression = expression;
        }
    }
}