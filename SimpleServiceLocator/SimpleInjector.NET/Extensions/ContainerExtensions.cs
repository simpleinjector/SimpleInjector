#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET.
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

using System;
using System.Linq;

namespace SimpleInjector.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="Container"/> class.
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        /// Injects all public writable properties of the given <paramref name="instance"/> that have a type
        /// that can be resolved by the <paramref name="container"/>.
        /// </summary>
        /// <param name="container">The container that will be used for the injection.</param>
        /// <param name="instance">The instance whos properties will be injected.</param>
        /// <exception cref="ArgumentNullException">Thrown when either the <paramref name="container"/> or
        /// the <paramref name="instance"/> are null references (Nothing in VB).</exception>
        public static void InjectProperties(this Container container, object instance)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            var snapshot = container.PropertyInjectionCache;

            PropertyProducerPair[] pairs;

            if (!snapshot.TryGetValue(instance.GetType(), out pairs))
            {
                pairs = Create(container, instance.GetType());

                container.RegisterPropertyProducerPairs(instance.GetType(), pairs, snapshot);
            }

            if (pairs != null)
            {
                for (int i = 0; i < pairs.Length; i++)
                {
                    pairs[i].InjectProperty(instance);
                }
            }
        }

        // Returns null when the type has no injectable properties.
        private static PropertyProducerPair[] Create(Container container, Type type)
        {
            var pairs = (
                from property in type.GetProperties()
                where property.CanWrite
                where property.GetSetMethod() != null
                where !property.PropertyType.IsValueType
                let producer = container.GetRegistration(property.PropertyType)
                where producer != null
                select new PropertyProducerPair(property, producer))
                .ToArray();

            return pairs.Length == 0 ? null : pairs;
        }
    }
}