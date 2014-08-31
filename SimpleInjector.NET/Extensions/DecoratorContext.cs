#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014 Simple Injector Contributors
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

namespace SimpleInjector.Extensions
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq.Expressions;

    /// <summary>
    /// An instance of this type can be injected into constructors of decorator classes that are registered
    /// using <see cref="DecoratorExtensions.RegisterDecorator">RegisterDecorator</see>. This type contains 
    /// contextual information about the applied decoration and it allows users to examine the given instance 
    /// to make runtime decisions.
    /// </summary>
    [DebuggerDisplay("DecoratorContext ({DebuggerDisplay,nq})")]
    public sealed class DecoratorContext
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly DecoratorPredicateContext context;

        internal DecoratorContext(DecoratorPredicateContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets the closed generic service type for which the decorator is about to be applied. The original
        /// service type will be returned, even if other decorators have already been applied to this type.
        /// </summary>
        /// <value>The closed generic service type.</value>
        public Type ServiceType 
        {
            get { return this.context.ServiceType; }
        }

        /// <summary>
        /// Gets the type of the implementation that is created by the container and for which the decorator
        /// is about to be applied. The original implementation type will be returned, even if other decorators
        /// have already been applied to this type. Please not that the implementation type can not always be
        /// determined. In that case the closed generic service type will be returned.
        /// </summary>
        /// <value>The implementation type.</value>
        public Type ImplementationType
        {
            get { return this.context.ImplementationType; }
        }

        /// <summary>
        /// Gets the list of the types of decorators that have already been applied to this instance.
        /// </summary>
        /// <value>The applied decorators.</value>
        public ReadOnlyCollection<Type> AppliedDecorators
        {
            get { return this.context.AppliedDecorators; }
        }

        /// <summary>
        /// Gets the current <see cref="Expression"/> object that describes the intention to create a new
        /// instance with its currently applied decorators.
        /// </summary>
        /// <value>The current expression that is about to be decorated.</value>
        public Expression Expression
        {
            get { return this.context.Expression; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "This method is called by the debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "ServiceType = {0}, ImplementationType = {1}",
                    this.ServiceType.ToFriendlyName(),
                    this.ImplementationType.ToFriendlyName());
            }
        }
    }
}