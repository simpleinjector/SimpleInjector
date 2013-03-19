namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=ImplicitPropertyInjectionExtensions
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
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

            private bool IsImplicitInjectableProperty(PropertyInfo property)
            {
                MethodInfo setMethod = property.GetSetMethod(nonPublic: false);

                return setMethod != null && !setMethod.IsStatic && property.CanWrite && 
                    this.options.Container.GetRegistration(property.PropertyType, throwOnFailure: false) != null;
            }
        }
    }
}