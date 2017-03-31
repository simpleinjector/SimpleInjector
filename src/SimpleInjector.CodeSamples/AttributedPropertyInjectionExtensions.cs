namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    using SimpleInjector.Advanced;

    public static class AttributedPropertyInjectionExtensions
    {
        [DebuggerStepThrough]
        public static void AutoWirePropertiesWithAttribute<TAttribute>(this ContainerOptions options)
            where TAttribute : Attribute
        {
            options.PropertySelectionBehavior = new AttributePropertyInjectionBehavior<TAttribute>(
                options.PropertySelectionBehavior);
        }

        internal sealed class AttributePropertyInjectionBehavior<TAttribute> : IPropertySelectionBehavior
            where TAttribute : Attribute
        {
            private readonly IPropertySelectionBehavior baseBehavior;

            public AttributePropertyInjectionBehavior(IPropertySelectionBehavior baseBehavior)
            {
                this.baseBehavior = baseBehavior;
            }

            public bool SelectProperty(Type t, PropertyInfo p) => 
                this.IsPropertyDecoratedWithAttribute(p) || this.baseBehavior.SelectProperty(t, p);

            private bool IsPropertyDecoratedWithAttribute(PropertyInfo property) =>
                property.GetCustomAttributes(typeof(TAttribute), true).Any();
        }
    }
}