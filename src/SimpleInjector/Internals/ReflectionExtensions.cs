#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2016 Simple Injector Contributors
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

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class ReflectionExtensions
    {
#if NET40 || NET45
        public static bool IsGenericType(this Type type) => type.IsGenericType;
        public static bool IsValueType(this Type type) => type.IsValueType;
        public static bool IsAbstract(this Type type) => type.IsAbstract;
        public static bool ContainsGenericParameters(this Type type) => type.ContainsGenericParameters;
        public static Type BaseType(this Type type) => type.BaseType;
        public static bool IsPrimitive(this Type type) => type.IsPrimitive;
        public static bool IsGenericTypeDefinition(this Type type) => type.IsGenericTypeDefinition;
        public static bool IsNestedPublic(this Type type) => type.IsNestedPublic;
        public static bool IsPublic(this Type type) => type.IsPublic;
        public static bool IsClass(this Type type) => type.IsClass;
        public static bool IsInterface(this Type type) => type.IsInterface;
        public static bool IsGenericParameter(this Type type) => type.IsGenericParameter;
        public static GenericParameterAttributes GetGenericParameterAttributes(this Type type) => type.GenericParameterAttributes;
        public static IEnumerable<PropertyInfo> GetRuntimeProperties(this Type type) =>
            type.GetProperties(BindingFlags.FlattenHierarchy |
                BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.NonPublic | BindingFlags.Public);
        public static Assembly GetAssembly(this Type type) => type.Assembly;
        public static Guid GetGuid(this Type type) => type.GUID;
#endif        
#if NETSTANDARD1_0 || NETSTANDARD1_3
        public static MethodInfo GetSetMethod(this PropertyInfo property, bool nonPublic = true) =>
            nonPublic || property.SetMethod?.IsPublic == true ? property.SetMethod : null;

        public static MethodInfo GetGetMethod(this PropertyInfo property, bool nonPublic = true) =>
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
        public static Type BaseType(this Type type) => type.GetTypeInfo().BaseType;
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

        public static ConstructorInfo[] GetConstructors(this Type type) =>
            type.GetTypeInfo().DeclaredConstructors.Where(ctor => !ctor.IsStatic && ctor.IsPublic).ToArray();

        public static ConstructorInfo GetConstructor(this Type type, Type[] types) => (
            from constructor in type.GetTypeInfo().DeclaredConstructors
            where types.SequenceEqual(constructor.GetParameters().Select(p => p.ParameterType))
            select constructor)
            .FirstOrDefault();
#endif
    }
}
