namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using SimpleInjector.Advanced;

    public interface IConstructorSelector
    {
        ConstructorInfo GetConstructor(Type type);
    }

    public sealed class ConstructorSelector : IConstructorSelector
    {
        public static readonly IConstructorSelector MostParameters =
            new ConstructorSelector(type => type.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length).First());

        public static readonly IConstructorSelector LeastParameters =
            new ConstructorSelector(type => type.GetConstructors()
                .OrderBy(c => c.GetParameters().Length).First());

        private readonly Func<Type, ConstructorInfo> selector;

        public ConstructorSelector(Func<Type, ConstructorInfo> selector)
        {
            this.selector = selector;
        }

        public ConstructorInfo GetConstructor(Type type)
        {
            return this.selector(type);
        }
    }

    public static class ConstructorRegistrationExtensions
    {
        public static ConstructorSelectorConvention RegisterConstructorSelectorConvention(
            this Container container)
        {
            var convention = new ConstructorSelectorConvention(container, 
                container.Options.ConstructorResolutionBehavior);

            container.Options.ConstructorResolutionBehavior = convention;

            return convention;
        }
    }

    public sealed class ConstructorSelectorConvention : IConstructorResolutionBehavior
    {
        private readonly Container container;
        private readonly IConstructorResolutionBehavior baseBehavior;
        private readonly Dictionary<Type, ConstructorInfo> constructors;

        public ConstructorSelectorConvention(Container container,
            IConstructorResolutionBehavior baseBehavior)
        {
            this.container = container;
            this.baseBehavior = baseBehavior;
            this.constructors = new Dictionary<Type, ConstructorInfo>();
        }

        ConstructorInfo IConstructorResolutionBehavior.GetConstructor(Type implementationType)
        {
            ConstructorInfo constructor;

            if (this.constructors.TryGetValue(implementationType, out constructor))
            {
                return constructor;
            }

            return this.baseBehavior.GetConstructor(implementationType);
        }

        public void Register<TConcrete>(IConstructorSelector selector)
            where TConcrete : class
        {
            this.RegisterExplicitConstructor<TConcrete>(selector);

            this.container.Register<TConcrete, TConcrete>();
        }

        public void Register<TService, TImplementation>(IConstructorSelector selector)
            where TService : class
            where TImplementation : class, TService
        {
            this.RegisterExplicitConstructor<TImplementation>(selector);

            this.container.Register<TService, TImplementation>();
        }

        public void Register<TService, TImplementation>(IConstructorSelector selector, Lifestyle lifestyle)
            where TService : class
            where TImplementation : class, TService
        {
            this.RegisterExplicitConstructor<TImplementation>(selector);

            this.container.Register<TService, TImplementation>(lifestyle);
        }

        private void RegisterExplicitConstructor<TImplementation>(IConstructorSelector selector)
        {
            this.constructors[typeof(TImplementation)] = selector.GetConstructor(typeof(TImplementation));
        }
    }
}