namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    internal static class TestHelpers
    {
        internal static string ToFriendlyNamesText(this IEnumerable<Type> types) =>
            string.Join(", ", types.Select(TypesExtensions.ToFriendlyName));

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