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

namespace SimpleInjector.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using SimpleInjector.Extensions.Decorators;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// An instance of this type will be supplied to the <see cref="Predicate{T}"/>
    /// delegate that is that is supplied to the 
    /// <see cref="DecoratorExtensions.RegisterDecorator(Container, Type, Type, Predicate{DecoratorPredicateContext})">RegisterDecorator</see>
    /// overload that takes this delegate. This type contains information about the decoration that is about
    /// to be applied and it allows users to examine the given instance to see whether the decorator should
    /// be applied or not.
    /// </summary>
    /// <remarks>
    /// Please see the 
    /// <see cref="DecoratorExtensions.RegisterDecorator(Container, Type, Type, Predicate{DecoratorPredicateContext})">RegisterDecorator</see>
    /// method for more information.
    /// </remarks>
    [DebuggerDisplay("DecoratorPredicateContext (ServiceType = {Helpers.ToFriendlyName(ServiceType),nq}, " +
        "ImplementationType = {Helpers.ToFriendlyName(ImplementationType),nq})")]
    public sealed class DecoratorPredicateContext
    {
        internal static readonly ReadOnlyCollection<Type> NoDecorators =
            new ReadOnlyCollection<Type>(Type.EmptyTypes);

        internal DecoratorPredicateContext(Type serviceType, Type implementationType,
            ReadOnlyCollection<Type> appliedDecorators, Expression expression, InstanceProducer producer)
        {
            this.ServiceType = serviceType;
            this.ImplementationType = implementationType;
            this.AppliedDecorators = appliedDecorators;
            this.Expression = expression;
            this.InstanceProducer = producer;
        }

        /// <summary>
        /// Gets the closed generic service type for which the decorator is about to be applied. The original
        /// service type will be returned, even if other decorators have already been applied to this type.
        /// </summary>
        /// <value>The closed generic service type.</value>
        public Type ServiceType { get; private set; }

        /// <summary>
        /// Gets the type of the implementation that is created by the container and for which the decorator
        /// is about to be applied. The original implementation type will be returned, even if other decorators
        /// have already been applied to this type. Please not that the implementation type can not always be
        /// determined. In that case the closed generic service type will be returned.
        /// </summary>
        /// <value>The implementation type.</value>
        public Type ImplementationType { get; private set; }

        /// <summary>
        /// Gets the list of the types of decorators that have already been applied to this instance.
        /// </summary>
        /// <value>The applied decorators.</value>
        public ReadOnlyCollection<Type> AppliedDecorators { get; private set; }

        /// <summary>
        /// Gets the current <see cref="Expression"/> object that describes the intention to create a new
        /// instance with its currently applied decorators.
        /// </summary>
        /// <value>The current expression that is about to be decorated.</value>
        public Expression Expression { get; private set; }

        internal InstanceProducer InstanceProducer { get; private set; }

        internal static DecoratorPredicateContext CreateFromExpression(Container container, Type serviceType,
            Type implementationType, Expression expression, InstanceProducer producer = null)
        {
            var lifestyle = ExtensionHelpers.DetermineLifestyle(expression);
            var registration = new ExpressionRegistration(expression, implementationType, lifestyle, container);

            // This producer will never be part of the container, but can still be used for analysis.
            producer = producer ?? new InstanceProducer(serviceType, registration);

            return new DecoratorPredicateContext(serviceType, implementationType,
                DecoratorPredicateContext.NoDecorators, expression, producer);
        }

        internal static DecoratorPredicateContext CreateFromInfo(Type serviceType, Expression expression,
            ServiceTypeDecoratorInfo info)
        {
            var appliedDecorators = info.AppliedDecorators.Select(d => d.DecoratorType).ToList().AsReadOnly();

            return new DecoratorPredicateContext(serviceType, info.ImplementationType, appliedDecorators,
                expression, info.GetCurrentInstanceProducer());
        }
    }
}