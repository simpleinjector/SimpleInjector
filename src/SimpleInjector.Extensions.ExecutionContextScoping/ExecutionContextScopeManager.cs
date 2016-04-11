#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014-2015 Simple Injector Contributors
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

namespace SimpleInjector.Extensions.ExecutionContextScoping
{
    using System.Security;

    // This class will be registered as singleton within a container, allowing each container (if the
    // application -for some reason- has multiple containers) to have it's own set of execution context scopes, 
    // without influencing other scopes from other containers.
    [SecuritySafeCritical]
    internal sealed partial class ExecutionContextScopeManager
    {
        internal ExecutionContextScope CurrentScope => this.GetCurrentScopeWithAutoCleanup();

        internal ExecutionContextScope BeginExecutionContextScope() =>
            this.CurrentScopeInternal = new ExecutionContextScope(this, this.GetCurrentScopeWithAutoCleanup());

        internal void EndExecutionContextScope(ExecutionContextScope scope)
        {
            // If the scope is not the current scope or one of its ancestors, this means that either one of
            // the scope's parents have already been disposed, or the scope is disposed on a completely
            // unrelated thread. In both cases we shouldn't change the CurrentScope, since doing this,
            // since would cause an invalid scope to be registered as the current scope (this scope will
            // either be disposed or does not belong to the current execution context).
            if (this.IsScopeInLocalChain(scope))
            {
                this.CurrentScopeInternal = scope.ParentScope;
            }
        }

        // Determines whether this instance is the currently registered execution context scope or an ancestor 
        // of it.
        private bool IsScopeInLocalChain(ExecutionContextScope scope)
        {
            var currentScope = this.CurrentScopeInternal;

            while (currentScope != null)
            {
                if (object.ReferenceEquals(scope, currentScope))
                {
                    return true;
                }

                currentScope = currentScope.ParentScope;
            }

            return false;
        }

        private ExecutionContextScope GetCurrentScopeWithAutoCleanup()
        {
            var scope = this.CurrentScopeInternal;

            // When the current scope is disposed, make the parent scope the current.
            while (scope != null && scope.Disposed)
            {
                this.CurrentScopeInternal = scope = scope.ParentScope;
            }

            return scope;
        }
    }
}