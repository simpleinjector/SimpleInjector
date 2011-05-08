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

using System;
using System.Collections;
using System.Collections.Generic;

namespace SimpleInjector
{
    /// <summary>
    /// The container. Create an instance of this type for registration of dependencies.
    /// </summary>
    public partial class Container
    {
        private readonly object locker = new object();

        private readonly List<InstanceInitializer> instanceInitializers = new List<InstanceInitializer>();

        private Dictionary<Type, IInstanceProducer> registrations = new Dictionary<Type, IInstanceProducer>(40);
        private Dictionary<Type, bool> unavailableTypes = new Dictionary<Type, bool>();

        // This dictionary is only used for validation. After validation is gets erased.
        private Dictionary<Type, IEnumerable> collectionsToValidate = new Dictionary<Type, IEnumerable>();

        private bool locked;

        private EventHandler<UnregisteredTypeEventArgs> resolveUnregisteredType;

        /// <summary>Initializes a new instance of the <see cref="Container"/> class.</summary>
        public Container()
        {
        }

        internal Dictionary<Type, IInstanceProducer> Registrations
        {
            get { return this.registrations; }
        }

        /// <summary>Wrapper for instance initializer Action delegates.</summary>
        private sealed class InstanceInitializer
        {
            internal Type ServiceType { get; set; }

            internal object Action { get; set; }
        }
    }
}