#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2014 Simple Injector Contributors
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

namespace SimpleInjector.Advanced.Internal
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This is an internal type. Only depend on this type when you want to be absolutely sure a future 
    /// version of the framework will break your code.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes",
        Justification = "This struct is not intended for public use.")]
    public struct LazyScope
    {
        private readonly Container container;
        private Func<Scope> scopeFactory;
        private Scope value;

        /// <summary>Initializes a new instance of the <see cref="LazyScope"/> struct.</summary>
        /// <param name="scopeFactory">The scope factory.</param>
        /// <param name="container">The container.</param>
        public LazyScope(Func<Scope> scopeFactory, Container container)
        {
            this.scopeFactory = scopeFactory;
            this.container = container;
            this.value = null;
        }

        /// <summary>Gets the lazily initialized Scope of the current LazyScope instance.</summary>
        /// <value>The current Scope or null.</value>
        public Scope Value
        {
            get
            {
                if (this.scopeFactory != null)
                {
                    this.value = container.GetVerificationScopeForCurrentThread() ?? this.scopeFactory.Invoke();
                    this.scopeFactory = null;
                }

                return this.value;
            }
        }
    }
}