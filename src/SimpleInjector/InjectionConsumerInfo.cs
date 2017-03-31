#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015-2016 Simple Injector Contributors
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
    using System.ComponentModel;
    using System.Reflection;

    /// <summary>
    /// Contains contextual information about the direct consumer for which the given dependency is injected
    /// into.
    /// </summary>
    public class InjectionConsumerInfo
    {
        internal static readonly InjectionConsumerInfo Root = null;
        
        /// <summary>Initializes a new instance of the <see cref="InjectionConsumerInfo"/> class.</summary>
        /// <param name="parameter">The constructor parameter for the created component.</param>
        public InjectionConsumerInfo(ParameterInfo parameter)
        {
            Requires.IsNotNull(parameter, nameof(parameter));

            this.Target = new InjectionTargetInfo(parameter);
            this.ImplementationType = parameter.Member.DeclaringType;
        }

        /// <summary>Initializes a new instance of the <see cref="InjectionConsumerInfo"/> class.</summary>
        /// <param name="implementationType">The implementation type of the consumer of the component that should be created.</param>
        /// <param name="property">The property for the created component.</param>
        public InjectionConsumerInfo(Type implementationType, PropertyInfo property)
        {
            Requires.IsNotNull(implementationType, nameof(implementationType));
            Requires.IsNotNull(property, nameof(property));

            this.Target = new InjectionTargetInfo(property);
            this.ImplementationType = implementationType;
        }

        /// <summary>Gets the service type of the consumer of the component that should be created.</summary>
        /// <value>The closed generic service type.</value>
        [Obsolete(
            "This property has been removed. Please use ImplementationType instead. " +
            "See https://simpleinjector.org/depr3.",
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
        public Type ImplementationType { get; }

        /// <summary>
        /// Gets the information about the consumer's target in which the dependency is injected. The target
        /// can be either a property or a constructor parameter.
        /// </summary>
        /// <value>The <see cref="InjectionTargetInfo"/> for this context.</value>
        public InjectionTargetInfo Target { get; }

        /// <summary>Returns a string that represents the <see cref="InjectionConsumerInfo"/>.</summary>
        /// <returns>A string.</returns>
        public override string ToString() => 
            "{ ImplementationType: " + this.ImplementationType.ToFriendlyName() + 
            ", Target.Name: '" + this.Target.Name + "' }";
    }
}