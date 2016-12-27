namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    using SimpleInjector.Advanced;

    public static class ImplicitPropertyInjectionExtensions
    {
        [DebuggerStepThrough]
        public static void AutoWirePropertiesImplicitly(this ContainerOptions options)
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
            public bool SelectProperty(PropertyInfo p) => 
                this.IsImplicitInjectable(p) || this.core.SelectProperty(p);

            [DebuggerStepThrough]
            private bool IsImplicitInjectable(PropertyInfo p) =>
                IsInjectableProperty(p) && this.IsAvailableService(p.PropertyType);

            [DebuggerStepThrough]
            private static bool IsInjectableProperty(PropertyInfo property)
            {
                MethodInfo setMethod = property.GetSetMethod(nonPublic: false);

                return setMethod != null && !setMethod.IsStatic && property.CanWrite;
            }

            [DebuggerStepThrough]
            private bool IsAvailableService(Type type) => 
                this.options.Container.GetRegistration(type) != null;
        }
    }
}