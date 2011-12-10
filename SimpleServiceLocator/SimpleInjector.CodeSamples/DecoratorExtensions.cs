namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=DecoratorExtensions
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public static class DecoratorExtensions
    {
        public static void RegisterOpenGenericDecorator(this Container container,
            Type openGenericType, Type openGenericDecorator)
        {
            container.ExpressionBuilt += (sender, e) =>
            {
                var serviceType = e.RegisteredServiceType;

                if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == openGenericType)
                {
                    var closedGenericDecorator =
                        openGenericDecorator.MakeGenericType(serviceType.GetGenericArguments());

                    var ctor = closedGenericDecorator.GetConstructors().Single();

                    e.Expression = Expression.New(ctor,
                        from parameter in ctor.GetParameters()
                        let type = parameter.ParameterType
                        select type == serviceType ? e.Expression :
                            container.GetRegistration(type, true).BuildExpression());
                }
            };
        }
    }
}