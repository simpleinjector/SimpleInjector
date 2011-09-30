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
        public static void AllowToResolveArraysAndLists(this Container container)
        {
            container.ResolveUnregisteredType += (sender, e) =>
            {
                if (e.UnregisteredServiceType.IsArray)
                {
                    RegisterArrayResolver(e, container, e.UnregisteredServiceType.GetElementType());
                }
                else if (e.UnregisteredServiceType.IsGenericType &&
                    e.UnregisteredServiceType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    RegisterArrayResolver(e, container, e.UnregisteredServiceType.GetGenericArguments()[0]);
                }
            };
        }

        private static void RegisterArrayResolver(UnregisteredTypeEventArgs e, Container container, 
            Type elementType)
        {
            var producer = container.GetRegistration(typeof(IEnumerable<>).MakeGenericType(elementType));
            var enumerableExpression = producer.BuildExpression();
            var arrayMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(elementType);
            var arrayExpression = Expression.Call(arrayMethod, enumerableExpression);
            e.Register(arrayExpression);
        }
    }
}