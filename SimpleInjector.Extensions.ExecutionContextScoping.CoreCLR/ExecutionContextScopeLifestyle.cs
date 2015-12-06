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
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Defines a lifestyle that caches instances during the lifetime of an explicitly defined scope using the
    /// <see cref="SimpleInjectorExecutionContextScopeExtensions.BeginExecutionContextScope(Container)">BeginExecutionContextScope</see>
    /// method. An execution context scope flows with the logical execution context. Scopes can be nested and
    /// nested scopes will get their own instance. Instances created by this lifestyle can be disposed when 
    /// the created scope gets disposed. 
    /// </summary>
    /// <example>
    /// The following example shows the usage of the <b>ExecutionContextScopeLifestyle</b> class:
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    /// 
    /// container.Register<IUnitOfWork, EntityFrameworkUnitOfWork>(new ExecutionContextScopeLifestyle());
    /// 
    /// using (container.BeginExecutionContextScope())
    /// {
    ///     var instance1 = container.GetInstance<IUnitOfWork>();
    ///     // ...
    /// }
    /// ]]></code>
    /// </example>
    public class ExecutionContextScopeLifestyle : ScopedLifestyle
    {
        /// <summary>Initializes a new instance of the <see cref="ExecutionContextScopeLifestyle"/> class.
        /// The created and cached instance will be disposed when the created 
        /// <see cref="Scope"/> instance gets disposed and when the created object implements 
        /// <see cref="IDisposable"/>.
        /// </summary>
        public ExecutionContextScopeLifestyle()
            : this(disposeInstanceWhenScopeEnds: true)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ExecutionContextScopeLifestyle"/> class.</summary>
        /// <param name="disposeInstanceWhenScopeEnds">
        /// Specifies whether the created and cached instance will be disposed when the created 
        /// <see cref="Scope"/> instance gets disposed and when the created object implements 
        /// <see cref="IDisposable"/>. 
        /// </param>
        public ExecutionContextScopeLifestyle(bool disposeInstanceWhenScopeEnds)
            : this("Execution Context Scope", disposeInstanceWhenScopeEnds)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionContextScopeLifestyle"/> class.
        /// </summary>
        /// <param name="name">The user friendly name of this lifestyle.</param>
        /// <param name="disposeInstanceWhenScopeEnds">
        /// Specifies whether the created and cached instance will be disposed when the created.</param>
        protected ExecutionContextScopeLifestyle(string name, bool disposeInstanceWhenScopeEnds)
            : base(name, disposeInstanceWhenScopeEnds)
        {
        }

        /// <summary>
        /// Returns the current <see cref="Scope"/> for this lifestyle and the given 
        /// <paramref name="container"/>, or null when this method is executed outside the context of a scope.
        /// </summary>
        /// <param name="container">The container instance that is related to the scope to return.</param>
        /// <returns>A <see cref="Scope"/> instance or null when there is no scope active in this context.</returns>
        protected override Scope GetCurrentScopeCore(Container container)
        {
            return container.GetExecutionContextScopeManager().CurrentScope;
        }

        /// <summary>
        /// Creates a delegate that upon invocation return the current <see cref="Scope"/> for this
        /// lifestyle and the given <paramref name="container"/>, or null when the delegate is executed outside
        /// the context of such scope.
        /// </summary>
        /// <param name="container">The container for which the delegate gets created.</param>
        /// <returns>A <see cref="Func{T}"/> delegate. This method never returns null.</returns>
        protected override Func<Scope> CreateCurrentScopeProvider(Container container)
        {
            var manager = container.GetExecutionContextScopeManager();

            return () => manager.CurrentScope;
        }
    }
}