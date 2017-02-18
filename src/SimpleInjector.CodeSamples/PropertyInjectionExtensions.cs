namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;

    using SimpleInjector.Advanced;

    public static class PropertyInjectionExtensions
    {
        private static readonly object RegistrationsKey = new object();

        [DebuggerStepThrough]
        public static void EnablePropertyAutoWiring(this ContainerOptions options)
        {
            if (options.Container.GetItem(RegistrationsKey) != null)
            {
                throw new InvalidOperationException("EnablePropertyAutoWiring should be called just once.");
            }

            var registrations = new PropertyRegistrations(options.PropertySelectionBehavior);

            options.PropertySelectionBehavior = registrations;

            options.Container.SetItem(RegistrationsKey, registrations);
        }

        [DebuggerStepThrough]
        public static void AutoWireProperty<TImplementation>(this Container container,
            Expression<Func<TImplementation, object>> propertySelector)
        {
            var selectedProperty = (PropertyInfo)((MemberExpression)propertySelector.Body).Member;

            container.GetPropertyRegistrations().AddPropertySelector(property => property == selectedProperty);
        }

        [DebuggerStepThrough]
        public static void AutoWireProperties(this ContainerOptions options,
            Predicate<PropertyInfo> propertyFilter)
        {
            options.Container.GetPropertyRegistrations().AddPropertySelector(propertyFilter);
        }

        [DebuggerStepThrough]
        private static PropertyRegistrations GetPropertyRegistrations(this Container container)
        {
            if (container.IsLocked())
            {
                throw new InvalidOperationException(
                    "New registrations can't be made after the container was locked.");
            }

            var registrations = (PropertyRegistrations)container.GetItem(RegistrationsKey);

            if (registrations == null)
            {
                throw new InvalidOperationException(
                    "Please call container.Options.EnablePropertyAutoWiring() first.");
            }

            return registrations;
        }

        private sealed class PropertyRegistrations : IPropertySelectionBehavior
        {
            private readonly List<Predicate<PropertyInfo>> propertySelectors = new List<Predicate<PropertyInfo>>();
            private readonly IPropertySelectionBehavior baseBehavior;

            internal PropertyRegistrations(IPropertySelectionBehavior baseBehavior)
            {
                this.baseBehavior = baseBehavior;
            }

            bool IPropertySelectionBehavior.SelectProperty(Type t, PropertyInfo p) => 
                this.IsPropertyRegisteredForAutowiring(p) || this.baseBehavior.SelectProperty(t, p);

            public void AddPropertySelector(Predicate<PropertyInfo> selector) => 
                this.propertySelectors.Add(selector);

            private bool IsPropertyRegisteredForAutowiring(PropertyInfo property) => 
                this.propertySelectors.Exists(selector => selector(property));
        }
    }
}