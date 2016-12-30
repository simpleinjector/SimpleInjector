namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
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
            options.PropertySelectionBehavior =
                new AttributePropertyInjectionBehavior(options.PropertySelectionBehavior, typeof(TAttribute));
        }

        internal sealed class AttributePropertyInjectionBehavior : IPropertySelectionBehavior
        {
            private readonly IPropertySelectionBehavior baseBehavior;
            private readonly Type attributeType;

            public AttributePropertyInjectionBehavior(IPropertySelectionBehavior baseBehavior, Type attributeType)
            {
                this.baseBehavior = baseBehavior;
                this.attributeType = attributeType;
            }

            [DebuggerStepThrough]
            public bool SelectProperty(PropertyInfo p) => 
                this.IsPropertyDecoratedWithAttribute(p) || this.baseBehavior.SelectProperty(p);

            [DebuggerStepThrough]
            private bool IsPropertyDecoratedWithAttribute(PropertyInfo property)
            {
                return property.GetCustomAttributes(this.attributeType, true).Any();
            }
        }
    }
}