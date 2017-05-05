#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015-2016 Simple Injector Contributors
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


namespace Big.Paw.AgentService.SimpleInjectorMiddleware
{
    using Microsoft.Extensions.Options;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;
    using Microsoft.Extensions.Logging;
    using System.Collections;
    using System.Collections.Concurrent;

    class SimpleInjectorAsyncScope
    {
        private Container _container;
        private RequestDelegate _next;

        public SimpleInjectorAsyncScope(Container container, RequestDelegate next)
        {
            _container = container;
            _next = next;
        }

        internal async Task Scope(HttpContext context)
        {
            using (AsyncScopedLifestyle.BeginScope(_container))
            {
                await _next(context);
            }
        }
    }
}

