// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Contains contextual information about the direct consumer for which the given dependency is injected
    /// into.
    /// </summary>
    public class InjectionConsumerInfo : ApiObject, IEquatable<InjectionConsumerInfo>
    {
        // Bogus values for implementationType and property. They will never be used, but can't be null.
        internal static readonly InjectionConsumerInfo Root =
            new InjectionConsumerInfo(
                implementationType: typeof(object),
                property: typeof(string).GetProperties()[0]);

        private readonly Type implementationType;
        private readonly InjectionTargetInfo target;

        /// <summary>Initializes a new instance of the <see cref="InjectionConsumerInfo"/> class.</summary>
        /// <param name="parameter">The constructor parameter for the created component.</param>
        public InjectionConsumerInfo(ParameterInfo parameter)
        {
            Requires.IsNotNull(parameter, nameof(parameter));

            this.target = new InjectionTargetInfo(parameter);
            this.implementationType = parameter.Member.DeclaringType;
        }

        /// <summary>Initializes a new instance of the <see cref="InjectionConsumerInfo"/> class.</summary>
        /// <param name="implementationType">The implementation type of the consumer of the component that should be created.</param>
        /// <param name="property">The property for the created component.</param>
        public InjectionConsumerInfo(Type implementationType, PropertyInfo property)
        {
            Requires.IsNotNull(implementationType, nameof(implementationType));
            Requires.IsNotNull(property, nameof(property));

            this.target = new InjectionTargetInfo(property);
            this.implementationType = implementationType;
        }

        /// <summary>Gets the service type of the consumer of the component that should be created.</summary>
        /// <value>The closed generic service type.</value>
        [Obsolete(
            "Please use ImplementationType instead. See https://simpleinjector.org/depr3. " +
            "Will be removed in version 5.0.",
            error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Type ServiceType
        {
            get
            {
                throw new NotSupportedException(
                    "This property has been removed. Please use ImplementationType instead. " +
                    "See https://simpleinjector.org/depr3.");
            }
        }

        /// <summary>Gets the implementation type of the consumer of the component that should be created.</summary>
        /// <value>The implementation type.</value>
        public Type ImplementationType
        {
            get
            {
#if DEBUG
                // Check to make sure that this property is never used internally on the Root instance.
                if (this.IsRoot)
                {
                    throw new InvalidOperationException("Can't be called on Root.");
                }
#endif

                return this.implementationType;
            }
        }

        /// <summary>
        /// Gets the information about the consumer's target in which the dependency is injected. The target
        /// can be either a property or a constructor parameter.
        /// </summary>
        /// <value>The <see cref="InjectionTargetInfo"/> for this context.</value>
        public InjectionTargetInfo Target
        {
            get
            {
#if DEBUG
                // Check to make sure that this property is never used internally on the Root instance.
                if (this.IsRoot)
                {
                    throw new InvalidOperationException("Can't be called on Root.");
                }
#endif

                return this.target;
            }
        }

        internal bool IsRoot => object.ReferenceEquals(this, InjectionConsumerInfo.Root);

        /// <inheritdoc />
        public override int GetHashCode() =>
            this.implementationType.GetHashCode() ^ this.target.GetHashCode();

        /// <inheritdoc />
        public override bool Equals(object obj) => this.Equals(obj as InjectionConsumerInfo);

        /// <inheritdoc />
        public bool Equals(InjectionConsumerInfo other) =>
            other != null
            && this.implementationType.Equals(other.implementationType)
            && this.target.Equals(other.target);

        /// <inheritdoc />
        public override string ToString() =>
            "{ ImplementationType: " + this.implementationType.ToFriendlyName() +
            ", Target.Name: '" + this.target.Name + "' }";
    }
}