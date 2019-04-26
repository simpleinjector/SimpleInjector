#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector.Advanced;

    internal interface IContainerControlledCollection : IEnumerable
    {
        bool AllProducersVerified { get; }

        KnownRelationship[] GetRelationships();

        void Append(ContainerControlledItem item);

        void Clear();

        void VerifyCreatingProducers();
    }

    internal static class ContainerControlledCollectionExtensions
    {
        internal static void AppendAll(
            this IContainerControlledCollection collection, IEnumerable<ContainerControlledItem> items)
        {
            foreach (ContainerControlledItem item in items)
            {
                collection.Append(item);
            }
        }

        internal static void AppendAll(
            this IContainerControlledCollection collection, IEnumerable<Registration> registrations)
        {
            collection.AppendAll(registrations.Select(ContainerControlledItem.CreateFromRegistration));
        }

        internal static void AppendAll(
            this IContainerControlledCollection collection, IEnumerable<Type> types)
        {
            collection.AppendAll(types.Select(ContainerControlledItem.CreateFromType));
        }
    }
}