#region Copyright (c) 2012 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2012 S. van Deursen
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

namespace SimpleInjector.Extensions.LifetimeScoping
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    // This class will be registered as singleton within a container, allowing each container (if the
    // application -for some reason- has multiple containers) to have it's own set of lifetime scopes, without
    // influencing other scopes from other containers.
    // By making this class abstract, we ensure that an instance is registered explicitly, otherwise the
    // container will return a transient object when EnableLifetimeScoping is not called, which will fail 
    // horribly.
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
        private readonly ThreadLocal<Stack<LifetimeScope>> threadLocalScopes =
            new ThreadLocal<Stack<LifetimeScope>>();

        internal LifetimeScopeManager()
        {
        }

        internal LifetimeScope CurrentScope
        {
            get
            {
                Stack<LifetimeScope> scopes = this.threadLocalScopes.Value;

                return scopes != null && scopes.Count > 0 ? scopes.Peek() : null;
            }
        }

        internal LifetimeScope BeginLifetimeScope()
        {
            Stack<LifetimeScope> scopes = this.threadLocalScopes.Value;

            if (scopes == null)
            {
                this.threadLocalScopes.Value = scopes = new Stack<LifetimeScope>();
            }

            var scope = new LifetimeScope(this);

            scopes.Push(scope);

            return scope;
        }

        internal void EndLifetimeScope(LifetimeScope scope)
        {
            Stack<LifetimeScope> scopes = this.threadLocalScopes.Value;

            if (scopes != null && scopes.Contains(scope))
            {
                // Remove the scope and any non-disposed inner scopes as well.
                while (scopes.Count > 0 && scopes.Pop() != scope)
                {
                }
            }
            else
            {
                // We will come here when the LifetimeScope gets disposed on a different thread than were it
                // was created.
            }
        }
    }
}