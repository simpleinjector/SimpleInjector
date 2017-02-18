#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2016 Simple Injector Contributors
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

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    [DebuggerDisplay(nameof(DefaultPropertySelectionBehavior))]
    internal sealed class DefaultPropertySelectionBehavior : IPropertySelectionBehavior
    {
        // The default behavior is to not inject any properties. This has the following rational:
        // 1. We don't want to do implicit property injection (where all properties are skipped that
        //    can't be injected), because this leads to a configuration that is hard to verify.
        // 2. We can't do explicit property injection, because this required users to use a framework
        //    defined attribute and application code should not depend on the DI container.
        // 3. In general, property injection should not be used, since this leads to Temporal Coupling. 
        //    Constructor injection should be used, and if a constructor gets too many parameters 
        //    (constructor over-injection), this is an indication of a violation of the SRP.
        public bool SelectProperty(Type implementationType, PropertyInfo propertyInfo) => false;
    }
}