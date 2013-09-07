namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // Extension methods for stuff that's missing from Silverlight, but is in .NET
    internal static class MissingMethods
    {
        public static void RemoveAll<T>(this List<T> list, Predicate<T> match)
        {
            var items = list.Where(item => !match(item)).ToArray();

            list.Clear();

            list.AddRange(items);
        }
    }
}