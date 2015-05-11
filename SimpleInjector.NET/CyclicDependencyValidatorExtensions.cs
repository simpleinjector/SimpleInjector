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
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Extension methods for the RecursiveDependencyValidator class.
    /// </summary>
    internal static class CyclicDependencyValidatorExtensions
    {
        // Prevents any recursive calls from taking place.
        // This method will be inlined by the JIT.
#if NET45
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void CheckForRecursiveCalls(this CyclicDependencyValidator validator)
        {
            if (validator != null)
            {
                validator.Check();
            }
        }

        // This method will be inlined by the JIT.
        // Resets the validator to its initial state. This is important when a IInstanceProvider threw an
        // exception, because a new call to that provider would otherwise make the validator think it is a
        // recursive call and throw an exception, and this would hide the exception that would otherwise be
        // thrown by the provider itself.
        internal static void Reset(this CyclicDependencyValidator validator)
        {
            if (validator != null)
            {
                validator.RollBack();
            }
        }
    }
}