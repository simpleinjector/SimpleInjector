#region Copyright (c) 2013 Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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

// This attribute will ensure that ASP.NET will call the SimpleInjectorWebInitializer.Initialize method on
// AppDomain startup.
[assembly: System.Web.PreApplicationStartMethod(
    typeof(SimpleInjector.Integration.Web.SimpleInjectorWebInitializer),
    "Initialize")]

namespace SimpleInjector.Integration.Web
{
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    /// <summary>
    /// Pre application start code.
    /// </summary>
    public static class SimpleInjectorWebInitializer
    {
        private static bool hasStarted;

        /// <summary>Registers an HttpModule that allows disposing instances that are registered as
        /// Per Web Request.</summary>
        public static void Initialize()
        {
            if (!hasStarted)
            {
                hasStarted = true;

                DynamicModuleUtility.RegisterModule(typeof(SimpleInjectorHttpModule));
            }
        }
    }
}