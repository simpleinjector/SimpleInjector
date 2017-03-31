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

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal sealed class ParameterDictionary<TValue> : Dictionary<ParameterInfo, TValue>
    {
        public ParameterDictionary(IEnumerable<TValue> collection, Func<TValue, ParameterInfo> keySelector)
            : this()
        {
            foreach (TValue item in collection)
            {
                this.Add(keySelector(item), item);
            }
        }

        public ParameterDictionary() : base(ParameterInfoComparer.Instance)
        {
        }

        private sealed class ParameterInfoComparer : IEqualityComparer<ParameterInfo>
        {
            public static readonly ParameterInfoComparer Instance = new ParameterInfoComparer();

            // We compare ParameterInfo by its Name, Type and its member, since there is no guarantee that
            // there will be only one ParameterInfo instance per 'physical' parameter in the CLR.
            // This caused an actual bug (see #323) in Simple Injector.
            public bool Equals(ParameterInfo x, ParameterInfo y) =>
                x.Name == y.Name
                && x.ParameterType == y.ParameterType
                && Equals(x.Member, y.Member);

            // Note that it is valid for a ParameterInfo.Name to be null in the CLR (and Castle Dynamic proxy
            // actually spits out types with null parameter names), so we have to guard against this. Since
            // it is valid for all parameter names of the same member to be null, we have to have use the
            // ParameterType as well to compare two ParameterInfo objects.
            public int GetHashCode(ParameterInfo obj) =>
                (obj?.Name ?? string.Empty).GetHashCode() 
                ^ obj.ParameterType.GetHashCode()
                ^ GetHashCode(obj.Member);

            // Although there is a lock around the creation of MemberInfo's in the System.Type for the
            // full framework version, there is no guarantee that this holds for all versions (and I
            // believe .NET Native actually lacks this lock) and there is no guarantee given by MSDN
            // that there will be only one MemberInfo per member. That's why we have to compare by Name
            // and DeclaringType. Fortunately this guarantee exists for System.Type.
            private static bool Equals(MemberInfo x, MemberInfo y) =>
                x.DeclaringType == y.DeclaringType && x.Name == y.Name;

            private static int GetHashCode(MemberInfo obj) =>
                obj.DeclaringType.GetHashCode() ^ obj.Name.GetHashCode();
        }
    }
}