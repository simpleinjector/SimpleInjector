#region Copyright Simple Injector Contributors
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

namespace SimpleInjector
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;

#if !PUBLISH && (NET40 || NET45)
    /// <summary>Common Container methods specific for the full .NET version of Simple Injector.</summary>
#endif
#if NET40 || NET45
    public partial class Container
    {
        private static readonly Lazy<ModuleBuilder> LazyBuilder = new Lazy<ModuleBuilder>(() => 
            AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("SimpleInjector.Compiled"),
                AssemblyBuilderAccess.Run)
                .DefineDynamicModule("SimpleInjector.CompiledModule"));

        internal static ModuleBuilder ModuleBuilder => LazyBuilder.Value;

        [DebuggerStepThrough]
        static partial void GetStackTrace(ref string stackTrace)
        {
            stackTrace = new StackTrace(fNeedFileInfo: true, skipFrames: 2).ToString();
        }
    }
#endif
}