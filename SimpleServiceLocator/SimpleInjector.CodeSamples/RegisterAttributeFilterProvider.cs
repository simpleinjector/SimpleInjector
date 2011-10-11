namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using SimpleInjector;

    /// <summary>
    /// Extension methods for MVC3.
    /// </summary>
    public static class MVC3Extensions
    {
        public static void RegisterAsMvcDependencyResolver(this Container container)
        {
            DependencyResolver.SetResolver(new SimpleResolver { Container = container });
        }

        /// <summary>Registers a <see cref="IFilterProvider"/>. Use this method in conjunction with the
        /// <see cref="RegisterAsMvcDependencyResolver"/> method.</summary>
        /// <param name="container">The container.</param>
        public static void RegisterAttributeFilterProvider(this Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            var providers = FilterProviders.Providers.OfType<FilterAttributeFilterProvider>().ToArray();

            foreach (var provider in providers)
            {
                FilterProviders.Providers.Remove(provider);
            }

            container.RegisterSingle<IFilterProvider, SimpleInjectorFilterAttributeFilterProvider>();
        }

        private sealed class SimpleResolver : IDependencyResolver
        {
            public Container Container { get; set; }

            public object GetService(Type serviceType)
            {
                return ((IServiceProvider)this.Container).GetService(serviceType);
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                return this.Container.GetAllInstances(serviceType);
            }
        }

        private sealed class SimpleInjectorFilterAttributeFilterProvider : FilterAttributeFilterProvider
        {
            private readonly Container container;

            public SimpleInjectorFilterAttributeFilterProvider(Container container)
                : base(false)
            {
                this.container = container;
            }

            public override IEnumerable<Filter> GetFilters(ControllerContext controllerContext, 
                ActionDescriptor actionDescriptor)
            {
                var filters = base.GetFilters(controllerContext, actionDescriptor).ToArray();

                foreach (var filter in filters)
                {
                    this.container.InjectProperties(filter.Instance);
                }

                return filters;
            }
        }
    }
}