#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2018 Simple Injector Contributors
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

namespace SimpleInjector.Lifestyles
{
    using System;

    /// <summary>
    /// This lifestyle can be used to implement ambient context-less scoping in Simple Injector. This lifestyle
    /// can be set as DefaultScopedLifestyle and later used via Lifestyle.Scoped to register scoped instances,
    /// while instances are resolved via Scope.GetInstance.
    /// </summary>
    internal sealed class FlowingScopedLifestyle : ScopedLifestyle
    {
        public FlowingScopedLifestyle() : base("Scoped")
        {
        }

        protected internal override Func<Scope> CreateCurrentScopeProvider(Container container)
        {
            // Notify the container that we're using the thread-resolve scope.
            container.UseCurrentThreadResolveScope();

            return () => container.GetVerificationOrResolveScopeForCurrentThread();
        }

        // This method gets called by ScopedLifestyle.GetCurrentScopeInternal, and it already calls
        // GetVerificationOrResolveScopeForCurrentThread() and falls back to this method if there is not
        // scope. So there's nothing left to do here.
        protected override Scope GetCurrentScopeCore(Container container) => null;
    }
}