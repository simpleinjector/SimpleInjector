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
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Helper methods for the extensions.
    /// </summary>
    internal static class ExtensionHelpers
    {
        // This method name does not describe what it does, but since the C# compiler will create a iterator
        // type named after this method, it allows us to return a type that has a nice name that will show up
        // during debugging.
        public static IEnumerable<T> ReadOnlyCollection<T>(T[] collection)
        {
            for (int index = 0; index < collection.Length; index++)
            {
                yield return collection[index];
            }
        }

        internal static IEnumerable MakeReadOnly(Type elementType, Array collection)
        {
            var readOnlyCollection = typeof(ExtensionHelpers).GetMethod("ReadOnlyCollection")
                .MakeGenericMethod(elementType)
                .Invoke(null, new object[] { collection });

            return (IEnumerable)readOnlyCollection;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
            Justification = "I don't care about the extra casts. This is not a performance critical part.")]
        internal static Type DetermineImplementationType(Expression expression, Type registeredServiceType)
        {
            if (expression is ConstantExpression)
            {
                var constantExpression = (ConstantExpression)expression;

                object singleton = constantExpression.Value;
                return singleton == null ? constantExpression.Type : singleton.GetType();
            }

            if (expression is NewExpression)
            {
                // Transient without initializers.
                return ((NewExpression)expression).Constructor.DeclaringType;
            }

            var invocation = expression as InvocationExpression;

            if (invocation != null && invocation.Expression is ConstantExpression &&
                invocation.Arguments.Count == 1 && invocation.Arguments[0] is NewExpression)
            {
                // Transient with initializers.
                return ((NewExpression)invocation.Arguments[0]).Constructor.DeclaringType;
            }

            // Implementation type can not be determined.
            return registeredServiceType;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
            Justification = "I don't care about the extra casts. This is not a performance critical part.")]
        internal static Lifestyle DetermineLifestyle(Expression expression)
        {
            if (IsSingletonExpression(expression))
            {
                return Lifestyle.Singleton;
            }

            if (IsTransientExpression(expression))
            {
                return Lifestyle.Transient;
            }

            // Implementation type can not be determined.
            return Lifestyle.Unknown;
        }

        internal static MethodInfo GetGenericMethod(Expression<Action> methodCall)
        {
            var body = methodCall.Body as MethodCallExpression;

            return body.Method.GetGenericMethodDefinition();
        }

        internal static MethodInfo GetGenericMethod(Expression<Action<Container>> methodCall)
        {
            var body = methodCall.Body as MethodCallExpression;

            return body.Method.GetGenericMethodDefinition();
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

        internal static IEnumerable<Type> GetTypeBaseTypesAndInterfacesFor(this Type type, Type serviceType)
        {
            return GetGenericImplementationsOf(type.GetTypeBaseTypesAndInterfaces(), serviceType);
        }

        internal static bool IsConcreteType(Type type)
        {
            return !type.IsAbstract && !type.ContainsGenericParameters;
        }

        internal static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly,
            AccessibilityOption accessibility)
        {
            try
            {
                if (accessibility == AccessibilityOption.AllTypes)
                {
                    return assembly.GetTypes();
                }
                else
                {
                    return assembly.GetExportedTypes();
                }
            }
            catch (NotSupportedException)
            {
                // A type load exception would typically happen on an Anonymously Hosted DynamicMethods 
                // Assembly and it would be safe to skip this exception.
                return Enumerable.Empty<Type>();
            }
        }

        internal static bool IsGenericTypeDefinitionOf(this Type genericTypeDefinition,
            Type typeToCheck)
        {
            if (!typeToCheck.IsGenericType)
            {
                return false;
            }

            if (typeToCheck.GetGenericTypeDefinition() != genericTypeDefinition)
            {
                return false;
            }

            return true;
        }

        internal static bool IsGenericArgument(this Type type)
        {
            return type.IsGenericParameter || type.GetGenericArguments().Any(arg => arg.IsGenericArgument());
        }

        internal static IEnumerable<Type> GetBaseTypesAndInterfaces(this Type type)
        {
            return type.GetInterfaces().Concat(type.GetBaseTypes());
        }
        
        internal static void AddRange<T>(this Collection<T> collection, IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                collection.Add(item);
            }
        }

        internal static IEnumerable<T> Union<T>(this IEnumerable<T> source, T element)
        {
            return source.Union(Enumerable.Repeat(element, 1));
        }

        internal static IEnumerable<Type> GetTypeBaseTypesAndInterfaces(this Type type)
        {
            var thisType = new[] { type };
            return thisType.Concat(type.GetBaseTypesAndInterfaces());
        }

        private static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            Type baseType = type.BaseType ?? (type != typeof(object) ? typeof(object) : null);

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
        
        private static bool IsSingletonExpression(Expression expression)
        {
            return expression is ConstantExpression;
        }

        private static bool IsTransientExpression(Expression expression)
        {
            if (expression is NewExpression)
            {
                // Transient without initializers.
                return true;
            }

            var invocation = expression as InvocationExpression;

            if (invocation != null && invocation.Expression is ConstantExpression &&
                invocation.Arguments.Count == 1 && invocation.Arguments[0] is NewExpression)
            {
                // Transient with initializers.
                return true;
            }

            return false;
        }
    }
}