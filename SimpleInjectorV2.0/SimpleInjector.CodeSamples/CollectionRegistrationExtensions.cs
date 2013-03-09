namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=CollectionRegistrationExtensions
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using SimpleInjector;

    public static class CollectionRegistrationExtensions
    {
        public static void AllowToResolveArraysAndLists(
            this Container container)
        {
            container.ResolveUnregisteredType += (sender, e) =>
            {
                var serviceType = e.UnregisteredServiceType;

                if (serviceType.IsArray)
                {
                    RegisterArrayResolver(e, container, 
                        serviceType.GetElementType());
                }
                else if (serviceType.IsGenericType &&
                    serviceType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    RegisterArrayResolver(e, container, 
                        serviceType.GetGenericArguments()[0]);
                }
            };
        }

        private static void RegisterArrayResolver(UnregisteredTypeEventArgs e, 
            Container container, Type elementType)
        {
            var producer = container.GetRegistration(typeof(IEnumerable<>)
                .MakeGenericType(elementType));
            var enumerableExpression = producer.BuildExpression();
            var arrayMethod = typeof(Enumerable).GetMethod("ToArray")
                .MakeGenericMethod(elementType);
            var arrayExpression = 
                Expression.Call(arrayMethod, enumerableExpression);

            e.Register(arrayExpression);
        }
    }
}