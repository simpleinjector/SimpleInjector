namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    // Extension methods for stuff that's missing from PCL, but is in the full .NET framework.
    internal static class MissingMethods
    {
        public static void RemoveAll<T>(this List<T> list, Predicate<T> match)
        {
            var items = list.Where(item => !match(item)).ToArray();

            list.Clear();

            list.AddRange(items);
        }

        public static ReadOnlyCollection<T> AsReadOnly<T>(this List<T> list)
        {
            return new ReadOnlyCollection<T>(list);
        }
    }
}