#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015 Simple Injector Contributors
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
    /// <summary>
    /// This enumeration defines in which way the container should run the verification process.
    /// </summary>
    public enum VerificationOption
    {
        /// <summary>
        /// Specifies that the container performs verification only, which means that it will test whether
        /// all registrations can be constructed by iterating the registrations and letting the container
        /// create at least one instance of each registration. An <see cref="System.InvalidOperationException"/>
        /// will be thrown in case the configuration is invalid.
        /// </summary>
        VerifyOnly = 0,

        /// <summary>
        /// Specifies that the container will run diagnostic analysis after the verification succeeded. The
        /// container will diagnose the configuration with a subset of the available diagnostic warnings, that 
        /// are most likely an indication of a configuration mistake. A complete set of diagnostic warnings
        /// can be retrieved by calling 
        /// <see cref="SimpleInjector.Diagnostics.Analyzer.Analyze">Analyzer.Analyze</see> or by viewing the 
        /// container in the Visual Studio debugger, after the verification has succeeded.
        /// </summary>
        VerifyAndDiagnose = 1
    }
}