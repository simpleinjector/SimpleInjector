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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector.Advanced;

    internal sealed class ContainerRegisteredServiceContainerAnalyzer : IContainerAnalyzer
    {
        public DiagnosticType DiagnosticType
        {
            get { return DiagnosticType.ContainerRegisteredService; }
        }

        public string Name
        {
            get { return "Unregistered types"; }
        }

        public string GetRootDescription(IEnumerable<DiagnosticResult> results)
        {
            int unregisteredServicesCount = (
                from result in results.Cast<ContainerRegisteredServiceDiagnosticResult>()
                from relationship in result.Relationships
                select relationship.Dependency.ServiceType)
                .Distinct()
                .Count();

            var componentCount = results.Select(result => result.Type).Distinct().Count();

            return
                componentCount + " " + ComponentPlural(componentCount) + " depend on " +
                unregisteredServicesCount + " container-registered " +
                TypePlural(unregisteredServicesCount) + ".";
        }

        public string GetGroupDescription(IEnumerable<DiagnosticResult> results)
        {
            // TODO: We can do better than this.
            return results.First().Description;
        }

        DiagnosticResult[] IContainerAnalyzer.Analyze(Container container)
        {
            return this.Analyze(container);
        }

        public ContainerRegisteredServiceDiagnosticResult[] Analyze(Container container)
        {
            var registrations = container.GetCurrentRegistrations();

            var autoRegisteredServices = new HashSet<Type>(
                from producer in container.GetCurrentRegistrations()
                where producer.IsContainerAutoRegistered
                select producer.ServiceType);

            return (
                from registration in registrations
                from relationship in registration.GetRelationships()
                where autoRegisteredServices.Contains(relationship.Dependency.ServiceType)
                group relationship by registration into relationshipGroup
                select BuildDiagnosticResult(relationshipGroup.Key, relationshipGroup.ToArray()))
                .ToArray();
        }

        private static ContainerRegisteredServiceDiagnosticResult BuildDiagnosticResult(
            InstanceProducer registration, KnownRelationship[] relationships)
        {
            return new ContainerRegisteredServiceDiagnosticResult(
                type: registration.ServiceType,
                description: BuildDescription(registration, relationships),
                relationships: relationships);
        }

        private static string BuildDescription(InstanceProducer registration, KnownRelationship[] relationships)
        {
            string componentName = BuildComponentName(registration, relationships);
            string unregisteredTypeName = BuildUnregisteredTypeDescription(relationships);

            return componentName + " depends on " + unregisteredTypeName + ".";
        }

        private static string BuildComponentName(InstanceProducer registration, KnownRelationship[] relationships)
        {
            var consumingTypes = (
                from relationship in relationships
                select relationship.ImplementationType)
                .Distinct()
                .ToArray();

            if (consumingTypes.Length == 1)
            {
                return consumingTypes.First().ToFriendlyName();
            }
            else
            {
                return registration.ServiceType.ToFriendlyName();
            }
        }

        private static string BuildUnregisteredTypeDescription(KnownRelationship[] relationships)
        {
            var unregisteredTypes = (
                from relationship in relationships
                select relationship.Dependency.ServiceType)
                .Distinct()
                .ToArray();

            if (unregisteredTypes.Length == 1)
            {
                return "container-registered type " + unregisteredTypes[0].ToFriendlyName();
            }
            else
            {
                return unregisteredTypes.Length + " container-registered types";
            }
        }

        private static string ComponentPlural(int number)
        {
            return number == 1 ? "component" : "components";
        }

        private static string TypePlural(int number)
        {
            return number == 1 ? "type" : "types";
        }
    }
}