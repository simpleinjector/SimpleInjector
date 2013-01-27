#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2010 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Helper methods for the extensions.
    /// </summary>
    internal static class Helpers
    {
        internal static string ToFriendlyName(this Type type)
        {
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

        internal static bool ServiceIsAssignableFromImplementation(Type service, Type implementation)
        {
            return implementation.GetBaseTypesAndInterfacesFor(service).Any();
        }

        // Example: when implementation implements IComparable<int> and IComparable<double>, the method will
        // return typeof(IComparable<int>) and typeof(IComparable<double>) when serviceType is
        // typeof(IComparable<>).
        internal static IEnumerable<Type> GetBaseTypesAndInterfacesFor(this Type type, Type serviceType)
        {
            return GetGenericImplementationsOf(type.GetBaseTypesAndInterfaces(), serviceType);
        }

        internal static IEnumerable<Type> GetBaseTypesAndInterfaces(this Type type)
        {
            return type.GetInterfaces().Concat(type.GetBaseTypes());
        }

        private static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            Type baseType = type.BaseType;

            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }

        private static IEnumerable<Type> GetGenericImplementationsOf(IEnumerable<Type> types, Type serviceType)
        {
            return
                from type in types
                where type == serviceType ||
                    (type.IsGenericType && type.GetGenericTypeDefinition() == serviceType)
                select type;
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