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

namespace SimpleInjector.Diagnostics
{
    /// <summary>
    /// Specifies the list of diagnostic types that are currently supported by the diagnostic 
    /// <see cref="Analyzer"/>. Note that new diagnostic types might be added in future versions.
    /// For more information, please read the 
    /// <a href="https://simpleinjector.codeplex.com/wikipage?title=Diagnostics">Diagnosing your configuration
    /// using the Debug Diagnostic Services</a> wiki documentation.
    /// </summary>
    public enum DiagnosticType
    {
        /// <summary>
        /// Diagnostic type that warns about 
        /// a concrete type that was not registered explicitly and was not resolved using unregistered type 
        /// resolution, but was created by the container using the transient lifestyle.
        /// For more information, see: https://simpleinjector.codeplex.com/wikipage?title=UnregisteredTypes.
        /// </summary>
        ContainerRegisteredComponent,

        /// <summary>
        /// Diagnostic type that warns when a 
        /// component depends on a service with a lifestyle that is shorter than that of the component.
        /// For more information, see: https://simpleinjector.codeplex.com/wikipage?title=PotentialLifestyleMismatches.
        /// </summary>
        PotentialLifestyleMismatch,

        /// <summary>
        /// Diagnostic type that warns when a
        /// component depends on an unregistered concrete type and this concrete type has a lifestyle that is 
        /// different than the lifestyle of an explicitly registered type that uses this concrete type as its 
        /// implementation.
        /// For more information, see: https://simpleinjector.codeplex.com/wikipage?title=ShortCircuitedDependencies.
        /// </summary>
        ShortCircuitedDependency,

        /// <summary>
        /// Diagnostic type that warns when a
        /// component depends on (too) many services.
        /// For more information, see: https://simpleinjector.codeplex.com/wikipage?title=PotentialSingleResponsibilityViolations.
        /// </summary>
        SingleResponsibilityViolation
    }
}