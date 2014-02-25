#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014 Simple Injector Contributors
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
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    // This class will be registered as singleton within a container, allowing each container (if the
    // application -for some reason- has multiple containers) to have it's own set of execution context scopes, 
    // without influencing other scopes from other containers.
    [SecuritySafeCritical]
    internal sealed class ExecutionContextScopeManager
    {
        private readonly string key = Guid.NewGuid().ToString("N").Substring(0, 12);

        internal ExecutionContextScope CurrentScope
        {
            get 
            {
                var wrapper = (ExecutionContextScopeWrapper)CallContext.LogicalGetData(this.key);

                return wrapper != null ? wrapper.Scope : null;
            }

            private set 
            {
                var wrapper = value == null ? null : new ExecutionContextScopeWrapper(value);

                CallContext.LogicalSetData(this.key, wrapper);
            }
        }

        internal ExecutionContextScope BeginExecutionContextScope()
        {
            var parentScope = this.CurrentScope;
            var scope = new ExecutionContextScope(this, parentScope);
            this.CurrentScope = scope;
            return scope;
        }

        internal void EndExecutionContextScope(ExecutionContextScope scope)
        {
            // If the scope is not the current scope or one of its ancestors, this means that either one of
            // the scope's parents have already been disposed, or the scope is disposed on a completely
            // unrelated thread. In both cases we shouldn't change the CurrentScope, since doing this,
            // since would cause an invalid scope to be registered as the current scope (this scope will
            // either be disposed or does not belong to the current execution context).
            if (scope.IsCurrentScopeOrAncestor)
            {
                this.CurrentScope = scope.ParentScope;
            }
        }

        [Serializable]
        internal sealed class ExecutionContextScopeWrapper : MarshalByRefObject
        {
            [NonSerializedAttribute]
            internal readonly ExecutionContextScope Scope;

            internal ExecutionContextScopeWrapper(ExecutionContextScope scope)
            {
                this.Scope = scope;
            }
        }
    }
}