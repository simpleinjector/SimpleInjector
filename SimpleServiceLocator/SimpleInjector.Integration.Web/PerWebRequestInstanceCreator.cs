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

    internal sealed class PerWebRequestInstanceCreator<T> where T : class
    {
        private readonly Func<T> instanceCreator;
        private readonly bool disposeWhenRequestEnds;

        internal PerWebRequestInstanceCreator(Func<T> instanceCreator, bool disposeWhenRequestEnds)
        {
            this.instanceCreator = instanceCreator;
            this.disposeWhenRequestEnds = disposeWhenRequestEnds;
        }

        // This method needs to be public, because the RegisterPerWebRequest extension methods build a
        // MethodCallExpression using this method, and this would fail in partial trust when the method is 
        // not public.
        public T GetInstance()
        {
            var context = HttpContext.Current;

            if (context == null)
            {
                // No HttpContext: Let's create a transient object.
                return this.instanceCreator();
            }

            T instance = (T)context.Items[this.GetType()];

            if (instance == null)
            {
                instance = this.CreateInstance(context);
            }

            return instance;
        }

        private T CreateInstance(HttpContext context)
        {
            T instance = this.instanceCreator();

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