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
            options.PropertySelectionBehavior = new ImplicitPropertyInjectionBehavior(
                options.PropertySelectionBehavior, options);
        }

        internal sealed class ImplicitPropertyInjectionBehavior 
            : IPropertySelectionBehavior
        {
            private readonly IPropertySelectionBehavior core;
            private readonly ContainerOptions options;

            internal ImplicitPropertyInjectionBehavior(IPropertySelectionBehavior core,
                ContainerOptions options)
            {
                this.core = core;
                this.options = options;
            }

            [DebuggerStepThrough]
            public bool SelectProperty(Type type, PropertyInfo property)
            {
                return this.IsImplicitInjectable(property) || 
                    this.core.SelectProperty(type, property);
            }

            [DebuggerStepThrough]
            private bool IsImplicitInjectable(PropertyInfo property)
            {
                return IsInjectableProperty(property) && 
                    this.IsAvailableService(property.PropertyType);
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