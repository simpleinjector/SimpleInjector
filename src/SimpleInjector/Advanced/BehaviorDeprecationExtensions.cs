namespace SimpleInjector.Advanced
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>Deprecation extensions.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class BehaviorDeprecationExtensions
    {
        private const string DependencyInjectionBehaviorObsoleteMessage =
            "This interface method has been removed. Please call " +
            nameof(IDependencyInjectionBehavior.GetInstanceProducer) + " instead. " +
            "In case you need to change the expression while need to return an InstanceProducer, use the " +
            "InstanceProducer." + nameof(InstanceProducer.FromExpression) + " method to wrap the " +
            "expression.";

        private const string PropertySelectionBehaviorObsoleteMessage =
            "This interface method has been removed. Please call SelectProperty(PropertyInfo) instead.";

        /// <summary>
        /// This interface method has been removed. Please call GetInstanceProducerFor instead.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        /// <param name="consumer">The consumer.</param>
        /// <returns>Throws an exception.</returns>
        [Obsolete(DependencyInjectionBehaviorObsoleteMessage, error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Expression BuildExpression(this IDependencyInjectionBehavior behavior,
            InjectionConsumerInfo consumer)
        {
            throw new NotSupportedException(DependencyInjectionBehaviorObsoleteMessage);
        }

        /// <summary>
        /// This interface method has been removed. Please call SelectProperty(PropertyInfo) instead.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        /// <param name="serviceType">Type of the abstraction that is requested.</param>
        /// <param name="propertyInfo">The property to check.</param>
        /// <returns>True when the property should be injected.</returns>
        [Obsolete(PropertySelectionBehaviorObsoleteMessage, error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool SelectProperty(this IPropertySelectionBehavior behavior,
            Type serviceType, PropertyInfo propertyInfo)
        {
            throw new NotSupportedException(PropertySelectionBehaviorObsoleteMessage);
        }
    }
}
