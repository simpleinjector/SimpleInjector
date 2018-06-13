using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleInjector.Internals
{
    internal static class CollectionInitializer
    {
        private static readonly Dictionary<Type, MethodInfo> Initializers;

        public static Collection<T> ToCollection<T>(IEnumerable<T> list)
        {
            return new Collection<T>(new List<T>(list));
        }

        static CollectionInitializer()
        {
            Initializers = new Dictionary<Type, MethodInfo>();
            Initializers.Add(typeof(List<>), typeof(Enumerable).GetMethod("ToList"));
            Initializers.Add(typeof(Collection<>), typeof(CollectionInitializer).GetMethod("ToCollection"));
        }
     
        public static MethodInfo GetInitializer(Type type)
        {
            MethodInfo result;
            Initializers.TryGetValue(type, out result);
            return result;
        }
    }
}
