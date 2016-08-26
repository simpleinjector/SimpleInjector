using System;
using System.Linq;
using System.Reflection;

namespace SimpleInjector
{
    public static class TypeExtensions
    {
        public static Attribute[] GetCustomAttributes(this Type type, Type attributeType, bool inherit) =>
            type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
    }
}