#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2015 Simple Injector Contributors
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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// An instance of this type will be supplied to the <see cref="Predicate{T}"/>
    /// delegate that is that is supplied to the 
    /// <see cref="Container.RegisterConditional(System.Type, System.Type, Lifestyle, Predicate{PredicateContext})">RegisterConditional</see>
    /// overload that takes this delegate. This type contains information about the open generic service that 
    /// is about to be created and it allows the user to examine the given instance to decide whether this 
    /// implementation should be created or not.
    /// </summary>
    /// <remarks>
    /// Please see the 
    /// <see cref="Container.RegisterConditional(System.Type, System.Type, Lifestyle, Predicate{PredicateContext})">Register</see>
    /// method for more information.
    /// </remarks>
    [DebuggerDisplay(nameof(PredicateContext) + " ({" + nameof(DebuggerDisplay) + ", nq})")]
    public sealed class PredicateContext
    {
        private readonly Func<Type> implementationTypeProvider;
        private Type implementationType;

        internal PredicateContext(InstanceProducer producer, InjectionConsumerInfo consumer, bool handled)
            : this(producer.ServiceType, producer.Registration.ImplementationType, consumer, handled)
        {
        }

        internal PredicateContext(Type serviceType, Type implementationType, InjectionConsumerInfo consumer,
            bool handled)
        {
            this.ServiceType = serviceType;
            this.implementationType = implementationType;
            this.Consumer = consumer;
            this.Handled = handled;
        }

        internal PredicateContext(Type serviceType, Func<Type> implementationTypeProvider,
            InjectionConsumerInfo consumer, bool handled)
        {
            this.ServiceType = serviceType;
            this.implementationTypeProvider = implementationTypeProvider;
            this.Consumer = consumer;
            this.Handled = handled;
        }

        /// <summary>Gets the closed generic service type that is to be created.</summary>
        /// <value>The closed generic service type.</value>
        public Type ServiceType { get; }

        /// <summary>Gets the closed generic implementation type that will be created by the container.</summary>
        /// <value>The implementation type.</value>
        public Type ImplementationType
        {
            get
            {
                if (this.implementationType == null)
                {
                    this.implementationType = this.implementationTypeProvider();
                }

                return this.implementationType;
            }
        }

        /// <summary>Gets a value indicating whether a previous <b>Register</b> registration has already
        /// been applied for the given <see cref="ServiceType"/>.</summary>
        /// <value>The indication whether the event has been handled.</value>
        public bool Handled { get; }

        /// <summary>
        /// Gets the contextual information of the consuming component that directly depends on the resolved
        /// service. This property will return null in case the service is resolved directly from the container.
        /// </summary>
        /// <value>The <see cref="InjectionConsumerInfo"/> or null.</value>
        public InjectionConsumerInfo Consumer { get; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "This method is called by the debugger.")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebuggerDisplay => string.Format(CultureInfo.InvariantCulture,
            "{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}",
            nameof(this.ServiceType), this.ServiceType.ToFriendlyName(),
            nameof(this.ImplementationType), this.ImplementationType.ToFriendlyName(),
            nameof(this.Handled), this.Handled,
            nameof(this.Consumer), this.Consumer);
    }
}