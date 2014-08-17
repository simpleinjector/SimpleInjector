namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Reflection;
    using SimpleInjector.Advanced;

    public enum CreationPolicy { Transient, Scoped, Singleton }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CreationPolicyAttribute : Attribute
    {
        public CreationPolicyAttribute(CreationPolicy policy)
        {
            this.Policy = policy;
        }

        public CreationPolicy Policy { get; private set; }
    }

    public class AttributeBasedLifestyleSelectionBehavior : ILifestyleSelectionBehavior
    {
        private const CreationPolicy DefaultPolicy = CreationPolicy.Transient;
        private readonly ScopedLifestyle scopedLifestyle;

        public AttributeBasedLifestyleSelectionBehavior(ScopedLifestyle scopedLifestyle)
        {
            this.scopedLifestyle = scopedLifestyle;
        }

        public Lifestyle SelectLifestyle(Type serviceType, Type implementationType)
        {
            var attribute = implementationType.GetCustomAttribute<CreationPolicyAttribute>()
                ?? serviceType.GetCustomAttribute<CreationPolicyAttribute>();

            var policy = attribute == null ? DefaultPolicy : attribute.Policy;

            switch (policy)
            {
                case CreationPolicy.Singleton:
                    return Lifestyle.Singleton;
                case CreationPolicy.Scoped:
                    return this.scopedLifestyle;
                default:
                    return Lifestyle.Transient;
            }
        }
    }
}