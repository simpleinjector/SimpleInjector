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
            private readonly IDependencyInjectionBehavior injectionBehavior;

            internal ImplicitPropertyInjectionBehavior(IPropertySelectionBehavior core,
                ContainerOptions options)
            {
                this.core = core;
                this.injectionBehavior = options.DependencyInjectionBehavior;
            }

            public bool SelectProperty(Type t, PropertyInfo p) => 
                this.IsImplicitInjectable(t, p) || this.core.SelectProperty(t, p);

            private bool IsImplicitInjectable(Type t, PropertyInfo p) =>
                IsInjectableProperty(p) && this.CanBeResolved(t, p);

            private static bool IsInjectableProperty(PropertyInfo property) =>
                property.CanWrite && property.GetSetMethod(nonPublic: false)?.IsStatic == false;

            private bool CanBeResolved(Type t, PropertyInfo property) =>
                this.GetProducer(new InjectionConsumerInfo(t, property)) != null;

            private InstanceProducer GetProducer(InjectionConsumerInfo info) =>
                this.injectionBehavior.GetInstanceProducer(info, false);
        }
    }
}