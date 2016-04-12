namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    internal static class TestHelpers
    {
        internal static string ToFriendlyNamesText(this IEnumerable<Type> types) => 
            string.Join(", ", types.Select(type => type.ToFriendlyName()));

        internal static string ToFriendlyName(this Type type)
        {
            if (type == null)
            {
                return "null";
            }

            string name = type.Name;

            if (type.IsNested && !type.IsGenericParameter)
            {
                name = type.DeclaringType.ToFriendlyName() + "+" + type.Name;
            }

            var genericArguments = GetGenericArguments(type);

            if (genericArguments.Length == 0)
            {
                return name;
            }

            name = name.Substring(0, name.IndexOf('`'));

            var argumentNames = genericArguments.Select(argument => argument.ToFriendlyName()).ToArray();

            return name + "<" + string.Join(", ", argumentNames) + ">";
        }
        
        private static Type[] GetGenericArguments(Type type)
        {
            if (!type.Name.Contains('`'))
            {
                return Type.EmptyTypes;
            }

            int numberOfGenericArguments = Convert.ToInt32(type.Name.Substring(type.Name.IndexOf('`') + 1),
                 CultureInfo.InvariantCulture);

            var argumentOfTypeAndOuterType = type.GetGenericArguments();

            return argumentOfTypeAndOuterType
                .Skip(argumentOfTypeAndOuterType.Length - numberOfGenericArguments)
                .ToArray();
        }
    }
}