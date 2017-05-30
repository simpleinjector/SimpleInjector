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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq.Expressions;

    using SimpleInjector.Advanced;

    /// <summary>
    /// Provides data for and interaction with the 
    /// <see cref="Container.ExpressionBuilt">ExpressionBuilt</see> event of 
    /// the <see cref="Container"/>. An observer can change the 
    /// <see cref="Expression"/> property to change the component that is currently 
    /// being built. 
    /// </summary>
    [DebuggerDisplay(nameof(ExpressionBuiltEventArgs) + " ({" + nameof(DebuggerDisplay) + "), nq})")]
    public class ExpressionBuiltEventArgs : EventArgs
    {
        private Expression expression;
        private Lifestyle lifestyle;

        /// <summary>Initializes a new instance of the <see cref="ExpressionBuiltEventArgs"/> class.</summary>
        /// <param name="registeredServiceType">Type of the registered service.</param>
        /// <param name="expression">The registered expression.</param>
        public ExpressionBuiltEventArgs(Type registeredServiceType, Expression expression)
        {
            this.RegisteredServiceType = registeredServiceType;

            this.expression = expression;
        }

        /// <summary>Gets the registered service type that is currently requested.</summary>
        /// <value>The registered service type that is currently requested.</value>
        [DebuggerDisplay("{" + TypesExtensions.FriendlyName + "(" + nameof(RegisteredServiceType) + "), nq}")]
        public Type RegisteredServiceType { get; }

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

        /// <summary>Gets or sets the current lifestyle of the registration.</summary>
        /// <value>The original lifestyle of the registration.</value>
        public Lifestyle Lifestyle
        {
            get
            {
                return this.lifestyle;
            }

            set
            {
                Requires.IsNotNull(value, nameof(value));

                this.lifestyle = value;
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

        // For now we keep this property internal. We can open it up when there is a valid use case for doing
        // so. Currently only the decorator subsystem needs to be able to change the registration.
        internal Registration ReplacedRegistration { get; set; }

        internal InstanceProducer InstanceProducer { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "This method is called by the debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebuggerDisplay => string.Format(
            CultureInfo.InvariantCulture,
            "{0}: {1}, {2}: {3}",
            nameof(this.RegisteredServiceType),
            this.RegisteredServiceType.ToFriendlyName(),
            nameof(this.RegisteredServiceType),
            this.RegisteredServiceType.ToFriendlyName());
    }
}