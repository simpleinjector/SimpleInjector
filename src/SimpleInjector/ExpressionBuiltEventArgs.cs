// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

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
    [DebuggerDisplay(nameof(ExpressionBuiltEventArgs) + " ({" + nameof(ExpressionBuiltEventArgs.DebuggerDisplay) + ", nq})")]
    public class ExpressionBuiltEventArgs : EventArgs
    {
        private Expression expression;
        private Lifestyle lifestyle;

        internal ExpressionBuiltEventArgs(
            Type registeredServiceType,
            Expression expression,
            InstanceProducer producer,
            Registration replacedRegistration,
            Collection<KnownRelationship> knownRelationships)
        {
            this.RegisteredServiceType = registeredServiceType;
            this.expression = expression;
            this.InstanceProducer = producer;
            this.lifestyle = producer.Lifestyle;
            this.ReplacedRegistration = replacedRegistration;
            this.KnownRelationships = knownRelationships;
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

        // By storing the ServiceTypeDecoratorInfo as part of the ExpressionBuiltEventArgs instance, we allow
        // all applied decorators on a single InstanceProducer to reuse this info object, which allows them to,
        // among other things, to construct DecoratorPredicateContext objects.
        // It seems a bit ugly to let ExpressionBuiltEventArgs reference the decorator sub system, but the
        // (more decoupled) alternative would be to expose a Items Dictionary that can be used to add arbitrary
        // items, such as an ServiceTypeDecoratorInfo. Although great, we don't need that flexibility, and the
        // creation of a new Dictionary object for every InstanceProducer that gets a one or multiple decorators
        // applied can cause quite a lot of memory overhead (an empty Dictionary takes roughly 60 bytes of
        // memory in a 32bit process).
        internal Decorators.ServiceTypeDecoratorInfo? DecoratorInfo { get; set; }

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