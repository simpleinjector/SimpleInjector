#region Copyright (c) 2012 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2012 S. van Deursen
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
    using System.Diagnostics;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides data for and interaction with the 
    /// <see cref="Container.ExpressionBuilding">ExpressionBuilding</see> event of 
    /// the <see cref="Container"/>. An observer can change the 
    /// <see cref="ExpressionBuildingEventArgs.Expression"/> property to change the registered type.
    /// </summary>
    [DebuggerDisplay("ExpressionBuildingEventArgs (RegisteredServiceType: {SimpleInjector.Helpers.ToFriendlyName(RegisteredServiceType),nq}, Expression: {Expression})")]
    public class ExpressionBuildingEventArgs : EventArgs
    {
        private Expression expression;

        internal ExpressionBuildingEventArgs(Type registeredServiceType, Type knownImplementationType, 
            Expression expression)
        {
            this.RegisteredServiceType = registeredServiceType;
            this.KnownImplementationType = knownImplementationType;

            this.expression = expression;
        }

        /// <summary>Gets the registered service type that is currently requested.</summary>
        /// <value>The registered service type that is currently requested.</value>
        public Type RegisteredServiceType { get; private set; }

        /// <summary>
        /// Gets the type that is known to be returned by the 
        /// <see cref="ExpressionBuildingEventArgs.Expression">Expression</see> (most often the implementation
        /// type used in the <b>Register</b> call). This type will be a derivative of
        /// <see cref="ExpressionBuildingEventArgs.RegisteredServiceType">RegisteredServiceType</see> (or
        /// or <b>RegisteredServiceType</b> itself). If the <b>Expression</b> is changed, the new expression 
        /// must also return an instance of type <b>KnownImplementationType</b> or a sub type. 
        /// This information must be described in the new Expression.
        /// </summary>
        public Type KnownImplementationType { get; private set; }

        /// <summary>Gets or sets the currently registered <see cref="Expression"/>.</summary>
        /// <value>The current registration.</value>
        /// <exception cref="ArgumentNullException">Thrown when the supplied value is a null reference.</exception>
        public Expression Expression
        {
            get
            {
                return this.expression;
            }

            set
            {
                Requires.IsNotNull(value, "value");

                this.expression = value;
            }
        }
    }
}