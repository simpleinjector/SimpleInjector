namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class TestHelpers
    {
        public static string ToFriendlyNamesText(this IEnumerable<Type> types) => 
            types.Select(type => type.ToFriendlyName()).ToCommaSeparatedText();

        public static Attribute[] GetCustomAttributes(this Type type, Type attributeType, bool inherit) =>
            type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).Cast<Attribute>().ToArray();

        public static Type MakePartialOpenGenericType(this Type type, Type firstArgument = null,
            Type secondArgument = null)
        {
            var arguments = type.GetGenericArguments();

            if (firstArgument != null)
            {
                arguments[0] = firstArgument;
            }

            if (secondArgument != null)
            {
                arguments[1] = secondArgument;
            }

            return type.MakeGenericType(arguments);
        }
    }
}