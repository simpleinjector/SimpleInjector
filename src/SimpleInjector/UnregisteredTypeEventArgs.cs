#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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
    using System.Reflection;

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
        internal UnregisteredTypeEventArgs(Type unregisteredServiceType)
        {
            this.UnregisteredServiceType = unregisteredServiceType;
        }

        /// <summary>Gets the unregistered service type that is currently requested.</summary>
        /// <value>The unregistered service type that is currently requested.</value>
        public Type UnregisteredServiceType { get; }

        /// <summary>
        /// Gets a value indicating whether the event represented by this instance has been handled. 
        /// This property will return <b>true</b> when <see cref="Register(Func{object})"/> has been called on
        /// this instance.
        /// </summary>
        /// <value>The indication whether the event has been handled.</value>
        public bool Handled => this.Expression != null || this.Registration != null;

        internal Expression Expression { get; private set; }

        internal Registration Registration { get; private set; }

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
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            this.RequiresNotHandled();

            this.Expression =
                Expression.Call(
                    typeof(UnregisteredTypeEventArgsCallHelper).GetMethod("GetInstance")
                        .MakeGenericMethod(this.UnregisteredServiceType),
                    Expression.Constant(instanceCreator));
        }

        /// <summary>
        /// Registers an <see cref="Expression"/> that describes the creation of instances of the type
        /// expressed by the <see cref="UnregisteredServiceType"/> for this and future requests. The delegate
        /// will be cached and future requests will directly use that expression or the compiled delegate.
        /// </summary>
        /// <remarks>
        /// NOTE: If possible, use the <see cref="Register(Registration)">Register(Registration)</see> overload,
        /// since this allows the analysis services to determine any configuration errors on the lifestyle of
        /// the registration.
        /// </remarks>
        /// <param name="expression">The expression that describes the creation of instances of the type
        /// expressed by the <see cref="UnregisteredServiceType"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="expression"/> is a
        /// null reference.</exception>
        /// <exception cref="ActivationException">Thrown when multiple observers that have registered to
        /// the <see cref="Container.ResolveUnregisteredType">ResolveUnregisteredType</see> event
        /// called this method for the same type.</exception>
        public void Register(Expression expression)
        {
            Requires.IsNotNull(expression, nameof(expression));
            Requires.ServiceIsAssignableFromExpression(this.UnregisteredServiceType, expression, 
                nameof(expression));

            Requires.ServiceIsAssignableFromImplementation(this.UnregisteredServiceType, expression.Type,
                nameof(expression));

            this.RequiresNotHandled();

            this.Expression = expression;
        }

        /// <summary>
        /// Registers a <see cref="Registration"/> that describes the creation of instances of the type
        /// expressed by the <see cref="UnregisteredServiceType"/> for this and future requests. The 
        /// registration will be cached and future requests will directly call unon that registration, the
        /// expression that it generates or the delegate that gets compiled from that expression.
        /// </summary>
        /// <param name="registration">The registration that describes the creation of instances according to
        /// the registration's lifestyle of the type expressed by the <see cref="UnregisteredServiceType"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="registration"/> is a
        /// null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="registration"/> is a
        /// not exactly of type <see cref="Func{T}"/> where T equals the <see cref="UnregisteredServiceType"/>.
        /// </exception>
        /// <exception cref="ActivationException">Thrown when multiple observers that have registered to
        /// the <see cref="Container.ResolveUnregisteredType">ResolveUnregisteredType</see> event
        /// called this method for the same type.</exception>
        public void Register(Registration registration)
        {
            Requires.IsNotNull(registration, nameof(registration));
            Requires.ServiceIsAssignableFromRegistration(this.UnregisteredServiceType, registration, 
                nameof(registration));

            this.RequiresNotHandled();

            this.Registration = registration;
        }

        private void RequiresNotHandled()
        {
            if (this.Handled)
            {
                throw new ActivationException(
                    StringResources.MultipleObserversRegisteredTheSameTypeToResolveUnregisteredType(
                        this.UnregisteredServiceType));
            }
        }

        internal static class UnregisteredTypeEventArgsCallHelper
        {
            // This method must be public.
            public static TService GetInstance<TService>(Func<object> instanceCreator)
            {
                object instance;

                try
                {
                    instance = instanceCreator();
                }
                catch (Exception ex)
                {
                    throw new ActivationException(
                        StringResources.UnregisteredTypeEventArgsRegisterDelegateThrewAnException(
                            typeof(TService), ex), ex);
                }

                try
                {
                    return (TService)instance;
                }
                catch (InvalidCastException ex)
                {
                    throw new InvalidCastException(
                        StringResources.UnregisteredTypeEventArgsRegisterDelegateReturnedUncastableInstance(
                            typeof(TService), ex), ex);
                }
            }
        }
    }
}