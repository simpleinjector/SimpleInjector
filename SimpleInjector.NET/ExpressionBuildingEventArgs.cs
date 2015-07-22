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
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Provides data for and interaction with the 
    /// <see cref="Container.ExpressionBuilding">ExpressionBuilding</see> event of 
    /// the <see cref="Container"/>. An observer can change the 
    /// <see cref="Expression"/> property to change the component that is 
    /// currently being built.
    /// </summary>
    [DebuggerDisplay("ExpressionBuildingEventArgs (RegisteredServiceType: {SimpleInjector.Helpers.ToFriendlyName(RegisteredServiceType),nq}, Expression: {Expression})")]
    public class ExpressionBuildingEventArgs : EventArgs
    {
        private Expression expression;

        internal ExpressionBuildingEventArgs(Type registeredServiceType, Type knownImplementationType, 
            Expression expression, Lifestyle lifestyle)
        {
            this.RegisteredServiceType = registeredServiceType;
            this.KnownImplementationType = knownImplementationType;
            this.Lifestyle = lifestyle;

            this.expression = expression;
        }

        /// <summary>Gets the registered service type that is currently requested.</summary>
        /// <value>The registered service type that is currently requested.</value>
        public Type RegisteredServiceType { get; private set; }

        /// <summary>
        /// Gets the type that is known to be returned by the 
        /// <see cref="Expression">Expression</see> (most often the implementation
        /// type used in the <b>Register</b> call). This type will be a derivative of
        /// <see cref="RegisteredServiceType">RegisteredServiceType</see> (or
        /// or <b>RegisteredServiceType</b> itself). If the <b>Expression</b> is changed, the new expression 
        /// must also return an instance of type <b>KnownImplementationType</b> or a sub type. 
        /// This information must be described in the new Expression.
        /// </summary>
        /// <value>A <see cref="Type"/>.</value>
        public Type KnownImplementationType { get; }

        /// <summary>Gets the lifestyle for the component that is currently being built.</summary>
        /// <value>The <see cref="Lifestyle"/>.</value>
        public Lifestyle Lifestyle { get; }

        /// <summary>Gets or sets the currently registered 
        /// <see cref="System.Linq.Expressions.Expression">Expression</see>.</summary>
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
                Requires.IsNotNull(value, nameof(value));

                this.expression = value;
            }
        }
        
        /// <summary>
        /// Gets the collection of currently known relationships. This information is used by the Diagnostics 
        /// Debug View. Change the contents of this collection to represent the changes made to the
        /// <see cref="Expression">Expression</see> property (if any). This allows
        /// the Diagnostics Debug View to analyze those new relationships as well.
        /// </summary>
        /// <value>The collection of <see cref="KnownRelationship"/> instances.</value>
        public Collection<KnownRelationship> KnownRelationships { get; internal set; }
    }
}