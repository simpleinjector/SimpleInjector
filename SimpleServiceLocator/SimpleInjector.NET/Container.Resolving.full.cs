namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

#if !PUBLISH
    /// <summary>Methods for resolving instances.</summary>
#endif
    public partial class Container
    {
#if NET45
        partial void TryBuildCollectionInstanceProducerForReadOnly(Type serviceType, 
            ref InstanceProducer producer)
        {
            Type serviceTypeDefinition = serviceType.GetGenericTypeDefinition();

            if (serviceTypeDefinition == typeof(IReadOnlyList<>) ||
                serviceTypeDefinition == typeof(IReadOnlyCollection<>))
            {
                Type elementType = serviceType.GetGenericArguments()[0];

                var collection = this.GetAllInstances(elementType) as IContainerControlledCollection;

                if (collection != null)
                {
                    var registration = SingletonLifestyle.CreateSingleRegistration(serviceType, collection, this);

                    producer = new InstanceProducer(serviceType, registration);

                    if (!((IEnumerable<object>)collection).Any())
                    {
                        producer.IsContainerAutoRegistered = true;
                    }
                }
            }
        }
#endif
    }
}