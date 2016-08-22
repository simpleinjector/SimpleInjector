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
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extension methods for reflection methods that are missing in .NETStandard 1.1.
    /// </summary>
    internal static class ReflectionExtensions
    {
        public static Type[] GetGenericArguments(this TypeInfo type) => type.GenericTypeArguments;

        public static MethodInfo GetSetMethod(this PropertyInfo property, bool nonPublic = true) =>
            nonPublic || property.SetMethod?.IsPublic == true
                ? property.SetMethod
                : null;

        public static MethodInfo GetGetMethod(this PropertyInfo property, bool nonPublic = true) =>
            nonPublic || property.GetMethod?.IsPublic == true
                ? property.GetMethod
                : null;

        public static ConstructorInfo[] GetConstructors(this TypeInfo type) =>
            type.DeclaredConstructors.Where(IsPublicInstance).ToArray();

        public static ConstructorInfo GetConstructor(this TypeInfo type, Type[] types) => (
            from constructor in type.DeclaredConstructors
            where types.SequenceEqual(constructor.GetParameters().Select(p => p.ParameterType))
            select constructor)
            .FirstOrDefault();

        public static MethodInfo GetMethod(this TypeInfo type, string name) =>
            type.DeclaredMethods.Single(m => m.Name == name);

        public static Type[] GetTypes(this Assembly assembly) => 
            assembly.DefinedTypes.Select(i => i.AsType()).ToArray();

        public static MemberInfo[] GetMember(this TypeInfo type, string name) =>
            type.DeclaredMembers.Where(m => m.Name == name).ToArray();

        public static Type[] GetInterfaces(this TypeInfo type) => type.ImplementedInterfaces.ToArray();


        private static Func<ConstructorInfo, bool> IsPublicInstance = ctor => !ctor.IsStatic && ctor.IsPublic;
    }
}