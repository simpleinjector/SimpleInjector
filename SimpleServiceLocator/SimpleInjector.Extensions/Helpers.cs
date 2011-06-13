using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleInjector.Extensions
{
    /// <summary>
    /// Helper methods for the extensions.
    /// </summary>
    internal static class Helpers
    {
        internal static MethodInfo GetGenericMethod(Expression<Action<Container>> methodCall)
        {
            var body = methodCall.Body as MethodCallExpression;

            return body.Method.GetGenericMethodDefinition();
        }

        internal static bool ServiceIsAssignableFromImplementation(Type service, Type implementation)
        {
            return implementation.GetBaseTypesAndInterfaces(service).Any();
        }

        // Example: when implementation implements IComparable<int> and IComparable<double>, the method will
        // return typeof(IComparable<int>) and typeof(IComparable<double>) when serviceType is
        // typeof(IComparable<>).
        internal static IEnumerable<Type> GetBaseTypesAndInterfaces(this Type type, Type serviceType)
        {
            return
                from parent in type.GetBaseTypesAndInterfaces()
                where parent == serviceType || 
                    (parent.IsGenericType && parent.GetGenericTypeDefinition() == serviceType)
                select parent;
        }

        internal static bool IsConcreteType(Type type)
        {
            return !type.IsAbstract && !type.IsGenericTypeDefinition;
        }

        internal static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly,
            AccessibilityOption accessibility)
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

        private static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            Type baseType = type.BaseType;

            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }
    }
}