// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Reflection;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Contextual information the a dependency and its direct consumer for which the dependency is injected
    /// into. The consumer's type is given by the <see cref="ImplementationType"/> property, where the
    /// <see cref="Target"/> property gives access to the consumer's target element (property or constructor
    /// argument) in which the dependency will be injected, and the dependency's type information.
    /// </summary>
    public class InjectionConsumerInfo : ApiObject, IEquatable<InjectionConsumerInfo?>
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

        internal bool IsRoot => object.ReferenceEquals(this, Root);

        /// <inheritdoc />
        public override int GetHashCode() => Helpers.Hash(this.implementationType, this.target);

        /// <inheritdoc />
        public override bool Equals(object obj) => this.Equals(obj as InjectionConsumerInfo);

        /// <inheritdoc />
        public bool Equals(InjectionConsumerInfo? other) =>
            other != null
            && this.implementationType.Equals(other.implementationType)
            && this.target.Equals(other.target);

        /// <inheritdoc />
        public override string ToString() =>
            "{ ImplementationType: " + this.implementationType.ToFriendlyName() +
            ", Target.Name: '" + this.target.Name + "' }";
    }
}