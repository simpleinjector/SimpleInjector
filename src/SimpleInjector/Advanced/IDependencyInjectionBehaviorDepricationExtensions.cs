namespace SimpleInjector.Advanced
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    /// <summary>Deprecation extensions.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IDependencyInjectionBehaviorDepricationExtensions
    {
        private const string ObsoleteMessage =
            "This interface method has been removed. Please call GetInstanceProducerFor instead. " +
            "In case you need to change the expression while need to return an InstanceProducer, use the " +
            "InstanceProducer." + nameof(InstanceProducer.FromExpression) + " method to wrap the " +
            "expression.";

        /// <summary>
        /// This interface method has been removed. Please call GetInstanceProducerFor instead.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        /// <param name="consumer">The consumer.</param>
        /// <returns>Throws an exception.</returns>
        [Obsolete(ObsoleteMessage, error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Expression BuildExpression(this IDependencyInjectionBehavior behavior, 
            InjectionConsumerInfo consumer)
        {
            throw new NotSupportedException(ObsoleteMessage);
        }
    }
}
