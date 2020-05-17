// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Linq;
    using System.Reflection;

    internal static class ReflectionExtensions
    {
        public static MethodInfo? GetSetMethod(this PropertyInfo property, bool nonPublic = true) =>
            nonPublic || property.SetMethod?.IsPublic == true ? property.SetMethod : null;

        public static MethodInfo? GetGetMethod(this PropertyInfo property, bool nonPublic = true) =>
            nonPublic || property.GetMethod?.IsPublic == true ? property.GetMethod : null;

        public static Type[] GetGenericArguments(this Type type) => type.GetTypeInfo().IsGenericTypeDefinition
            ? type.GetTypeInfo().GenericTypeParameters
            : type.GetTypeInfo().GenericTypeArguments;

        public static MethodInfo GetMethod(this Type type, string name) => type.GetTypeInfo().DeclaredMethods.Single(m => m.Name == name);
        public static Type[] GetTypes(this Assembly assembly) => assembly.DefinedTypes.Select(i => i.AsType()).ToArray();
        public static MemberInfo[] GetMember(this Type type, string name) => type.GetTypeInfo().DeclaredMembers.Where(m => m.Name == name).ToArray();
        public static Type[] GetInterfaces(this Type type) => type.GetTypeInfo().ImplementedInterfaces.ToArray();
        public static bool IsGenericType(this Type type) => type.GetTypeInfo().IsGenericType;
        public static bool IsValueType(this Type type) => type.GetTypeInfo().IsValueType;
        public static bool IsAbstract(this Type type) => type.GetTypeInfo().IsAbstract;
        public static bool IsAssignableFrom(this Type type, Type other) => type.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
        public static bool ContainsGenericParameters(this Type type) => type.GetTypeInfo().ContainsGenericParameters;
        public static Type? BaseType(this Type type) => type.GetTypeInfo().BaseType;
        public static bool IsGenericTypeDefinition(this Type type) => type.GetTypeInfo().IsGenericTypeDefinition;
        public static bool IsPrimitive(this Type type) => type.GetTypeInfo().IsPrimitive;
        public static bool IsNestedPublic(this Type type) => type.GetTypeInfo().IsNestedPublic;
        public static bool IsPublic(this Type type) => type.GetTypeInfo().IsPublic;
        public static Type[] GetGenericParameterConstraints(this Type type) => type.GetTypeInfo().GetGenericParameterConstraints();
        public static bool IsClass(this Type type) => type.GetTypeInfo().IsClass;
        public static bool IsInterface(this Type type) => type.GetTypeInfo().IsInterface;
        public static bool IsGenericParameter(this Type type) => type.GetTypeInfo().IsGenericParameter;
        public static GenericParameterAttributes GetGenericParameterAttributes(this Type type) => type.GetTypeInfo().GenericParameterAttributes;
        public static Assembly GetAssembly(this Type type) => type.GetTypeInfo().Assembly;
        public static PropertyInfo[] GetProperties(this Type type) => type.GetTypeInfo().DeclaredProperties.ToArray();
        public static Guid GetGuid(this Type type) => type.GetTypeInfo().GUID;

        public static ConstructorInfo[] GetConstructors(this Type type, bool nonPublic) =>
            type.GetTypeInfo().DeclaredConstructors
            .Where(ctor => !ctor.IsStatic && (nonPublic || ctor.IsPublic)).ToArray();

        public static ConstructorInfo[] GetConstructors(this Type type) =>
            type.GetConstructors(nonPublic: false);

        public static ConstructorInfo? GetConstructor(this Type type, Type[] types) => (
            from constructor in type.GetConstructors()
            where types.SequenceEqual(constructor.GetParameters().Select(p => p.ParameterType))
            select constructor)
            .FirstOrDefault();
    }
}