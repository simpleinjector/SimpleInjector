namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    using SimpleInjector.Advanced;

    public static class ImplicitPropertyInjectionExtensions
    {
        [DebuggerStepThrough]
        public static void AutowirePropertiesImplicitly(this ContainerOptions options)
        {
            options.PropertySelectionBehavior =
                new ImplicitPropertyInjectionBehavior(options.PropertySelectionBehavior, options);
        }

        internal sealed class ImplicitPropertyInjectionBehavior : IPropertySelectionBehavior
        {
            private readonly IPropertySelectionBehavior baseBehavior;
            private readonly ContainerOptions options;

            internal ImplicitPropertyInjectionBehavior(IPropertySelectionBehavior baseBehavior,
                ContainerOptions options)
            {
                this.baseBehavior = baseBehavior;
                this.options = options;
            }

            [DebuggerStepThrough]
            public bool SelectProperty(Type serviceType, PropertyInfo property)
            {
                return this.IsImplicitInjectableProperty(property) || 
                    this.baseBehavior.SelectProperty(serviceType, property);
            }

            [DebuggerStepThrough]
            private bool IsImplicitInjectableProperty(PropertyInfo property)
            {
                return IsInjectableProperty(property) && this.IsAvailableService(property.PropertyType);
            }

            [DebuggerStepThrough]
            private static bool IsInjectableProperty(PropertyInfo property)
            {
                MethodInfo setMethod = property.GetSetMethod(nonPublic: false);

                return setMethod != null && !setMethod.IsStatic && property.CanWrite;
            }

            [DebuggerStepThrough]
            private bool IsAvailableService(Type serviceType)
            {
                return this.options.Container.GetRegistration(serviceType) != null;
            }
        }
    }
}