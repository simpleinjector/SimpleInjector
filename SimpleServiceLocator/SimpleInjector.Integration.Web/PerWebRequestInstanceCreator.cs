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

namespace SimpleInjector.Integration.Web
{
    using System;
    using System.Web;

    using SimpleInjector.Advanced;

    internal sealed class PerWebRequestInstanceCreator<TService> where TService : class
    {
        private readonly Container container;
        private readonly Func<TService> instanceCreator;
        private readonly bool disposeWhenRequestEnds;

        internal PerWebRequestInstanceCreator(Container container, Func<TService> instanceCreator,
            bool disposeWhenRequestEnds)
        {
            this.container = container;
            this.instanceCreator = instanceCreator;
            this.disposeWhenRequestEnds = disposeWhenRequestEnds;
        }

        // This method needs to be public, because the RegisterPerWebRequest extension methods build a
        // MethodCallExpression using this method, and this would fail in partial trust when the method is 
        // not public.
        public TService GetInstance()
        {
            var context = HttpContext.Current;

            if (context == null)
            {
                if (this.container.IsVerifying())
                {
                    // Return a transient instance when this method is called during verification
                    return this.instanceCreator();
                }

                throw new ActivationException("The " + typeof(TService).FullName + " is registered as " +
                    "'PerWebRequest', but the instance is requested outside the context of a HttpContext (" +
                    "HttpContext.Current is null). Make sure instances using this lifestyle are not " + 
                    "resolved during the application initialization phase and when running on a background " +
                    "thread. For resolving instances on background threads, try registering this instance " + 
                    "as 'Per Lifetime Scope': http://bit.ly/N1s8hN.");
            }

            TService instance = (TService)context.Items[this.GetType()];

            if (instance == null)
            {
                instance = this.CreateInstance(context);
            }

            return instance;
        }

        private TService CreateInstance(HttpContext context)
        {
            TService instance = this.instanceCreator();

            context.Items[this.GetType()] = instance;

            if (this.disposeWhenRequestEnds)
            {
                var disposable = instance as IDisposable;

                if (disposable != null)
                {
                    SimpleInjectorWebExtensions.RegisterDelegateForEndWebRequest(context, () =>
                    {
                        disposable.Dispose();
                    });
                }
            }

            return instance;
        }
    }
}