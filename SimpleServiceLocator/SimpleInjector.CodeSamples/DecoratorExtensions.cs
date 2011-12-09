namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public static class DecoratorExtensions
    {
        public static void RegisterOpenGenericDecorator(this Container container,
            Type openGenericType, Type openGenericDecorator)
        {
            if (openGenericType.GetGenericArguments().Length != openGenericDecorator.GetGenericArguments().Length)
            {
                throw new ArgumentException("The generic arguments of openGenericDecorator and " + 
                    "openGenericType do not match", "openGenericDecorator");
            }

            if (openGenericDecorator.GetConstructors().Length != 1)
            {
                throw new ArgumentException("The type should have exactly one public constructor.", 
                    "openGenericDecorator");
            }

            if (openGenericDecorator.GetConstructors().Single().GetParameters()
                .Any(p => p.ParameterType == openGenericType))
            {
                throw new ArgumentException(string.Format("The type is not a decorator, because {0} is not " +
                    "a constructor argument.", openGenericType), "openGenericDecorator");
            }

            container.ExpressionBuilt += (sender, e) =>
            {
                var serviceType = e.RegisteredServiceType;

                if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == openGenericType)
                {
                    var closedGenericDecorator =
                        openGenericDecorator.MakeGenericType(serviceType.GetGenericArguments());

                    var ctor = closedGenericDecorator.GetConstructors().Single();

                    var arguments = 
                        from parameter in ctor.GetParameters()
                        let type = parameter.ParameterType
                        select type == serviceType ? e.Expression : 
                            container.GetRegistration(type, true).BuildExpression();

                    e.Expression = Expression.New(ctor, arguments.ToArray());
                }
            };
        }
    }
}