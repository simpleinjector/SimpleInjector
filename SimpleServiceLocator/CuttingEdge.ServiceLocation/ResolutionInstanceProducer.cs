using System;
using System.Linq.Expressions;

using Microsoft.Practices.ServiceLocation;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// Allows retrieval of instances of type <typeparamref name="T"/> that are resolved using unregistered
    /// type resolution.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    internal sealed class ResolutionInstanceProducer<T> : IInstanceProducer
    {
        private readonly Type serviceType;
        private readonly Func<object> instanceCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolutionInstanceProducer{T}"/> class.
        /// </summary>
        /// <param name="serviceType">The type of object to create.</param>
        /// <param name="instanceCreator">The delegate that knows how to create that type.</param>
        public ResolutionInstanceProducer(Type serviceType, Func<object> instanceCreator)
        {
            this.serviceType = serviceType;
            this.instanceCreator = instanceCreator;
        }

        /// <summary>Builds an expression that expresses the intent to get an instance by the current producer.</summary>
        /// <returns>An Expression.</returns>
        Expression IInstanceProducer.BuildExpression()
        {
            // Create an expression that directly calls the this.GetInstance() method.
            // We could further optimize it by directly calling the Func<T> instanceCreator, but this will
            // make us loose some error checking.
            return Expression.Call(Expression.Constant(this),
                typeof(ResolutionInstanceProducer<T>).GetMethod("GetInstance"), new Expression[0]);
        }

        /// <summary>Produces an instance.</summary>
        /// <returns>An instance.</returns>
        object IInstanceProducer.GetInstance()
        {
            return this.GetInstance();
        }

        /// <summary>Produces an instance.</summary>
        /// <returns>An instance.</returns>
        public T GetInstance()
        {
            object instance;

            try
            {
                instance = this.instanceCreator();
            }
            catch (Exception ex)
            {
                throw new ActivationException(StringResources
                    .HandlerReturnedADelegateThatThrewAnException(this.serviceType, ex.Message), ex);
            }

            if (instance == null)
            {
                throw new ActivationException(
                    StringResources.HandlerReturnedADelegateThatReturnedNull(this.serviceType));
            }

            try
            {
                return (T)instance;
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    StringResources.HandlerReturnedDelegateThatReturnedAnUnassignableFrom(this.serviceType,
                    instance.GetType()), ex);
            }
        }
    }
}