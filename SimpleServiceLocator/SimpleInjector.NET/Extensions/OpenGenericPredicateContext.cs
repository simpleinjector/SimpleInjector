#region Copyright (c) 2013
#endregion

namespace SimpleInjector.Extensions
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// An instance of this type will be supplied to the <see cref="Predicate{T}"/>
    /// delegate that is that is supplied to the 
    /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric(Container,Type,Type,Lifestyle,Predicate{OpenGenericPredicateContext})">RegisterOpenGeneric</see>
    /// overload that takes this delegate. This type contains information about the open generic service that is about
    /// to be created and it allows the user to examine the given instance to decide whether this implementation should
    /// be created or not.
    /// </summary>
    /// <remarks>
    /// Please see the 
    /// <see cref="OpenGenericRegistrationExtensions.RegisterOpenGeneric(Container,Type,Type,Lifestyle,Predicate{OpenGenericPredicateContext})">RegisterOpenGeneric</see>
    /// method for more information.
    /// </remarks>
    [DebuggerDisplay("OpenGenericPredicateContext (ServiceType = {Helpers.ToFriendlyName(ServiceType),nq}, " +
        "ImplementationType = {Helpers.ToFriendlyName(ImplementationType),nq}), " +
        "Handled = {Helpers.ToFriendlyName(Handled),nq})")]
    public sealed class OpenGenericPredicateContext
    {
        internal OpenGenericPredicateContext(Type serviceType, Type implementationType,
            bool handled)
        {
            this.ServiceType = serviceType;
            this.ImplementationType = implementationType;
            this.Handled = handled;
        }

        /// <summary>
        /// Gets the closed generic service type that is to be created.
        /// </summary>
        /// <value>The closed generic service type.</value>
        public Type ServiceType { get; private set; }

        /// <summary>
        /// Gets the closed generic implementation type that will be created by the container.
        /// </summary>
        /// <value>The implementation type.</value>
        public Type ImplementationType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the event represented by this instance has been handled. 
        /// </summary>
        /// <value>The indication whether the event has been handled.</value>
        public bool Handled { get; private set; }
    }
}
