// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using SimpleInjector.Advanced;

    /// <summary>
    /// An instance of this type can be injected into constructors of decorator classes that are registered
    /// using <see cref="Container.RegisterDecorator(Type, Type)">RegisterDecorator</see>. This type contains
    /// contextual information about the applied decoration and it allows users to examine the given instance
    /// to make runtime decisions.
    /// </summary>
    [DebuggerDisplay(nameof(DecoratorContext) + " ({" + nameof(DecoratorContext.DebuggerDisplay) + ", nq})")]
    public sealed class DecoratorContext : ApiObject
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
        public Type ServiceType => this.context.ServiceType;

        /// <summary>
        /// Gets the type of the implementation that is created by the container and for which the decorator
        /// is about to be applied. The original implementation type will be returned, even if other decorators
        /// have already been applied to this type. Please note that the implementation type can not always be
        /// determined. In that case the closed generic service type will be returned.
        /// </summary>
        /// <value>The implementation type.</value>
        public Type ImplementationType => this.context.ImplementationType;

        /// <summary>
        /// Gets the list of the types of decorators that have already been applied to this instance.
        /// </summary>
        /// <value>The applied decorators.</value>
        public ReadOnlyCollection<Type> AppliedDecorators => this.context.AppliedDecorators;

        /// <summary>
        /// Gets the current <see cref="Expression"/> object that describes the intention to create a new
        /// instance with its currently applied decorators.
        /// </summary>
        /// <value>The current expression that is about to be decorated.</value>
        public Expression Expression => this.context.Expression;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebuggerDisplay => this.context.DebuggerDisplay;
    }
}
