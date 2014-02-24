namespace SimpleInjector.Integration.WebApi
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    internal sealed class SimpleInjectorActionDescriptorFilterProvider : ActionDescriptorFilterProvider, 
        IFilterProvider
    {
        private readonly ConcurrentDictionary<Type, Registration> registrations =
            new ConcurrentDictionary<Type, Registration>();

        private readonly Func<Type, Registration> registrationFactory;

        internal SimpleInjectorActionDescriptorFilterProvider(Container container)
        {
            this.registrationFactory =
                concreteType => Lifestyle.Transient.CreateRegistration(concreteType, container);
        }

        public new IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, 
            HttpActionDescriptor actionDescriptor)
        {
            FilterInfo[] filters = this.GetFilterInfos(configuration, actionDescriptor);

            foreach (var filter in filters)
            {
                IFilter instance = filter.Instance;

                Registration registration =
                    this.registrations.GetOrAdd(instance.GetType(), this.registrationFactory);

                registration.InitializeInstance(instance);
            }

            return filters;
        }

        private FilterInfo[] GetFilterInfos(HttpConfiguration configuration,
            HttpActionDescriptor actionDescriptor)
        {
            IEnumerable<FilterInfo> filters = base.GetFilters(configuration, actionDescriptor);

            return (filters as FilterInfo[]) ?? filters.ToArray();
        }
    }
}
