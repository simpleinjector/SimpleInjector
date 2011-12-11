namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public interface IConstructorSelector
    {
        ConstructorInfo GetConstructor(Type type);
    }

    public sealed class ConstructorSelector : IConstructorSelector
    {
        public static readonly IConstructorSelector MostParameters =
            new ConstructorSelector(type => type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First());

        public static readonly IConstructorSelector LeastParameters =
            new ConstructorSelector(type => type.GetConstructors().OrderBy(c => c.GetParameters().Length).First());

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
        public static void Register<TService, TImplementation>(this Container container,
            IConstructorSelector selector)
            where TService : class
        {
            Func<TService> fakeInstanceCreator = () => null;

            container.Register<TService>(fakeInstanceCreator);

            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(TService))
                {
                    Verify(e.Expression, fakeInstanceCreator);

                    var ctor = selector.GetConstructor(typeof(TImplementation));

                    e.Expression = Expression.New(ctor,
                        from parameter in ctor.GetParameters()
                        select container.GetRegistration(parameter.ParameterType, true).BuildExpression());
                }
            };
        }

        public static void RegisterSingle<TService, TImplementation>(this Container container,
            IConstructorSelector selector)
            where TService : class
        {
            Register<TService, TImplementation>(container, selector);

            container.ExpressionBuilt += (sender, e) =>
            {
                if (e.RegisteredServiceType == typeof(TService))
                {
                    var instanceCreator = Expression.Lambda<Func<TService>>(e.Expression).Compile();

                    e.Expression = Expression.Constant(instanceCreator(), typeof(TService));
                }
            };
        }

        private static void Verify<TService>(Expression expression, Func<TService> fakeInstanceCreator)
        {
            var invocation = expression as InvocationExpression;

            if (invocation != null)
            {
                var constant = invocation.Expression as ConstantExpression;

                if (constant != null && object.ReferenceEquals(constant.Value, fakeInstanceCreator))
                {
                    return;
                }
            }

            throw new ActivationException(string.Format("The {0} was registered with an " +
                "IConstructorSelector, but its Expression was changed, which indicates there was another " +
                "delegate hooked to the ExpressionBuilt event that altered the registration for this type. " +
                "Make sure that delegate fires after this one.", typeof(TService)));
        }
    }
}