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

namespace SimpleInjector.Extensions.LifetimeScoping
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    // This class will be registered as singleton within a container, allowing each container (if the
    // application -for some reason- has multiple containers) to have it's own set of lifetime scopes, without
    // influencing other scopes from other containers.
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "A LifetimeScopeManager instance is stored as singleton in a Container instance, " +
            "but the container itself does not implement IDisposable, and will never dispose any instances " +
            "it contains. Letting LifetimeScopeManager therefore does not help and when the application " +
            "creates multiple Containers (that go out of scope before the AppDomain is stopped), we must " +
            "rely on the garbage collector calling the Finalize method of the ThreadLocal<T>.")]
    internal sealed class LifetimeScopeManager
    {
        // Here we use .NET 4.0 ThreadLocal instead of the [ThreadStatic] attribute, to allow each container
        // to have it's own set of scopes.
        private readonly ThreadLocal<LifetimeScope> threadLocalScopes = new ThreadLocal<LifetimeScope>();

        internal LifetimeScopeManager()
        {
        }

        internal LifetimeScope CurrentScope => this.GetCurrentScopeWithAutoCleanup();

        internal LifetimeScope BeginLifetimeScope() => 
            this.threadLocalScopes.Value = new LifetimeScope(this, parentScope: this.CurrentScope);

        internal void RemoveLifetimeScope(LifetimeScope scope)
        {
            // If the scope is not the current scope or one of its ancestors, this means that either one of
            // the scope's parents have already been disposed, or the scope is disposed on a completely
            // unrelated thread. In both cases we shouldn't change the CurrentScope, since doing this,
            // since would cause an invalid scope to be registered as the current scope (this scope will
            // either be disposed or does not belong to the current execution context).
            if (this.IsScopeInLocalChain(scope))
            {
                this.threadLocalScopes.Value = scope.ParentScope;
            }
        }

        // Determines whether this instance is the currently registered lifetime scope or an ancestor of it.
        private bool IsScopeInLocalChain(LifetimeScope scope)
        {
            var currentScope = this.threadLocalScopes.Value;

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

        private LifetimeScope GetCurrentScopeWithAutoCleanup()
        {
            var scope = this.threadLocalScopes.Value;

            // When the current scope is disposed, make the parent scope the current.
            while (scope != null && scope.Disposed)
            {
                this.threadLocalScopes.Value = scope = scope.ParentScope;
            }

            return scope;
        }
    }
}