namespace SimpleInjector.Interception
{
    using System;

    /// <summary>
    /// Helper for doing fluent registration of interceptors.
    /// </summary>
    public class InterceptionRegistrator
    {
        internal InterceptionRegistrator(Container container, Predicate<Type> serviceTypeSelector)
        {
            this.Container = container;
            this.ServiceTypeSelector = serviceTypeSelector;
        }

        internal Container Container { get; private set; }

        internal Predicate<Type> ServiceTypeSelector { get; private set; }
    }
}