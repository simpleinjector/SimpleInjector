namespace SimpleInjector.Tests.Unit
{
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions
    {
        public static T Second<T>(this IEnumerable<T> collection) => collection.Skip(1).First();
    }
}