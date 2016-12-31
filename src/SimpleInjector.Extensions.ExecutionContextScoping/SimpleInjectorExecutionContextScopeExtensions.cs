#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014-2016 Simple Injector Contributors
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

// Compared to the other lifestyles, this extensions class is NOT placed in the root namespace. Other
// lifestyles, such as the WebApiRequestLifestyle will use the ExecutionContextScopingLifestyle and users are
// expected to use these lifestyles and their extension methods, because this is much clearer. By not placing
// this class in the root namespace, we prevent the Container from being cluttered with methods that the user
// is not expected to use under normal conditions.
namespace SimpleInjector.Extensions.ExecutionContextScoping
{
    using System;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Extension methods for enabling execution context scoping for the Simple Injector.
    /// </summary>
    public static class SimpleInjectorExecutionContextScopeExtensions
    {
        /// <summary>
        /// Begins a new execution context scope for the given <paramref name="container"/>. 
        /// Services, registered using the <see cref="ExecutionContextScopeLifestyle"/> are cached during the 
        /// lifetime of that scope. The scope should be disposed explicitly.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>A new <see cref="Scope"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <example>
        /// <code lang="cs"><![CDATA[
        /// using (container.BeginExecutionContextScope())
        /// {
        ///     var handler container.GetInstance(rootType) as IRequestHandler;
        ///
        ///     handler.Handle(request);
        /// }
        /// ]]></code>
        /// </example>
        [Obsolete("BeginExecutionContextScope is obsolete. Please use SimpleInjector.Lifestyles." +
            "AsyncScopedLifestyle.BeginScope(Container) instead.", error: false)]
        public static Scope BeginExecutionContextScope(this Container container)
        {
            return AsyncScopedLifestyle.BeginScope(container);
        }

        /// <summary>
        /// GetCurrentExecutionContextScope has been deprecated. This method throws an exception.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>This method throws an exception.</returns>
        [Obsolete("GetCurrentExecutionContextScope has been deprecated. " +
            "Please use Lifestyle.Scoped.GetCurrentScope(Container) instead.",
            error: true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static Scope GetCurrentExecutionContextScope(this Container container)
        {
            throw new NotSupportedException(
                "GetCurrentExecutionContextScope has been deprecated. " +
                "Please use Lifestyle.Scoped.GetCurrentScope(Container) instead.");
        }
    }
}