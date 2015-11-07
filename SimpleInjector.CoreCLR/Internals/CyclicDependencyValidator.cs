#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 - 2015 Simple Injector Contributors
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

namespace SimpleInjector.Internals
{
    using System;
    using System.Threading;

    /// <summary>
    /// Allows verifying whether a given type has a direct or indirect dependency on itself. Verifying is done
    /// by preventing recursive calls to a IInstanceProvider. An instance of this type is related to a single 
    /// instance of a IInstanceProvider. A RecursiveDependencyValidator instance checks a single 
    /// IInstanceProvider and therefore a single service type.
    /// </summary>
    internal sealed class CyclicDependencyValidator
    {
        private readonly ThreadLocal<bool> cycleDetected = new ThreadLocal<bool>();
        private readonly Type typeToValidate;

        internal CyclicDependencyValidator(Type typeToValidate)
        {
            this.typeToValidate = typeToValidate;
        }

        // Checks whether this is a recursive call (and thus a cyclic dependency) and throw in that case.
        internal void Check()
        {
            if (this.cycleDetected.Value)
            {
                throw new CyclicDependencyException(this.typeToValidate);
            }

            this.cycleDetected.Value = true;
        }

        // Resets the validator to its initial state.
        internal void Reset()
        {
            this.cycleDetected.Value = false;   
        }
    }
}