namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Reflection;
    using SimpleInjector.Advanced;

    public enum CreationPolicy 
    {
        Transient, 
        Scoped, 
        Singleton 
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, 
        Inherited = false, AllowMultiple = false)]
    public sealed class CreationPolicyAttribute : Attribute
    {
        public CreationPolicyAttribute(CreationPolicy policy)
        {
            this.Policy = policy;
        }

        public CreationPolicy Policy { get; }
    }

    public class AttributeBasedLifestyleSelectionBehavior : ILifestyleSelectionBehavior
    {
        private const CreationPolicy DefaultPolicy = CreationPolicy.Transient;

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
                    return Lifestyle.Scoped;
                default:
                    return Lifestyle.Transient;
            }
        }
    }
}