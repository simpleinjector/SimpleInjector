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
    using SimpleInjector.Lifestyles;

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
    [Obsolete("This lifestyle is obsolete. Please use SimpleInjector.Lifestyles.AsyncScopedLifestyle instead.", error: false)]
    public class ExecutionContextScopeLifestyle : AsyncScopedLifestyle
    {
        /// <summary>Initializes a new instance of the <see cref="ExecutionContextScopeLifestyle"/> class.
        /// The created and cached instance will be disposed when the created 
        /// <see cref="Scope"/> instance gets disposed and when the created object implements 
        /// <see cref="IDisposable"/>.
        /// </summary>
        public ExecutionContextScopeLifestyle()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ExecutionContextScopeLifestyle"/> class.</summary>
        /// <param name="disposeInstanceWhenScopeEnds">
        /// Specifies whether the created and cached instance will be disposed when the created 
        /// <see cref="Scope"/> instance gets disposed and when the created object implements 
        /// <see cref="IDisposable"/>. 
        /// </param>
        [Obsolete("This constructor overload has been deprecated. " +
            "Please use ExecutionContextScopeLifestyle() instead.",
            error: true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public ExecutionContextScopeLifestyle(bool disposeInstanceWhenScopeEnds) : this()
        {
            throw new NotSupportedException(
                "This constructor overload has been deprecated. " +
                "Please use ExecutionContextScopeLifestyle() instead.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionContextScopeLifestyle"/> class.
        /// </summary>
        /// <param name="name">The user friendly name of this lifestyle.</param>
        protected ExecutionContextScopeLifestyle(string name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionContextScopeLifestyle"/> class.
        /// </summary>
        /// <param name="name">The user friendly name of this lifestyle.</param>
        /// <param name="disposeInstanceWhenScopeEnds">
        /// Specifies whether the created and cached instance will be disposed when the created.</param>
        [Obsolete("This constructor overload has been deprecated. " +
            "Please use ExecutionContextScopeLifestyle(string) instead.",
            error: true)]
        protected ExecutionContextScopeLifestyle(string name, bool disposeInstanceWhenScopeEnds)
        {
            throw new NotSupportedException(
                "This constructor overload has been deprecated. " +
                "Please use ExecutionContextScopeLifestyle(string) instead.");
        }
    }
}