#region Copyright (c) 2013 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2013 S. van Deursen
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

namespace SimpleInjector.Diagnostics.Analyzers
{
    using System.Globalization;
    using System.Linq;
    using SimpleInjector.Advanced;

    internal sealed class PotentialLifestyleMismatchContainerAnalyzer : IContainerAnalyzer
    {
        DiagnosticResult[] IContainerAnalyzer.Analyze(Container container)
        {
            return this.Analyze(container);
        }

        public PotentialLifestyleMismatchDiagnosticResult[] Analyze(Container container)
        {
            return (
              from producer in container.GetCurrentRegistrations()
              from dependency in producer.GetRelationships()
              where LifestyleMismatchServices.DependencyHasPossibleLifestyleMismatch(dependency)
              select new PotentialLifestyleMismatchDiagnosticResult(
                  type: producer.ServiceType,
                  name: "Mismatch",
                  description: BuildRelationshipDescription(dependency),
                  relationship: dependency))
              .ToArray();          
        }
        
        private static string BuildRelationshipDescription(KnownRelationship relationship)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0} ({1}) depends on {2} ({3}).",
                Helpers.ToFriendlyName(relationship.ImplementationType),
                relationship.Lifestyle.Name,
                Helpers.ToFriendlyName(relationship.Dependency.ServiceType),
                relationship.Dependency.Lifestyle.Name);
        }
    }
}