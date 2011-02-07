using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Practices.ServiceLocation;

namespace CuttingEdge.ServiceLocation
{
    /// <summary>
    /// The Simple Service Locator container. Create an instance of this type for registration of dependencies.
    /// </summary>
    public partial class SimpleServiceLocator
    {
        internal static readonly StringComparer StringComparer = StringComparer.Ordinal;

        private readonly Dictionary<Type, IKeyedInstanceProducer> keyedInstanceProducers =
            new Dictionary<Type, IKeyedInstanceProducer>();

        private readonly object locker = new object();

        private Dictionary<Type, IInstanceProducer> registrations = new Dictionary<Type, IInstanceProducer>();

        // This dictionary is only used for validation. After validation is gets erased.
        private Dictionary<Type, IEnumerable> collectionsToValidate = new Dictionary<Type, IEnumerable>();

        private bool locked;

        private EventHandler<UnregisteredTypeEventArgs> resolveUnregisteredType;

        /// <summary>Initializes a new instance of the <see cref="SimpleServiceLocator"/> class.</summary>
        public SimpleServiceLocator()
        {
        }

        internal Dictionary<Type, IInstanceProducer> Registrations
        {
            get { return this.registrations; }
        }
    }
}