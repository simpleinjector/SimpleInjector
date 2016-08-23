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
    using System.Reflection;

    /// <summary>
    /// Extension methods to adapt Simple Injector to the .NET and PCL. This class consists of extension
    /// methods that map to properties with the same name. The Simple Injector library for .NETStandard
    /// contains these extension methods as well, but are differently maps.
    /// </summary>
    internal static partial class NetReflectionExtensions
    {
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
    }
}