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

namespace SimpleInjector.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class GeneralWarningsContainerAnalyzer : List<IContainerAnalyzer>, IContainerAnalyzer
    {
        internal GeneralWarningsContainerAnalyzer()
        {
            this.Add(new AutoRegisteredContainerAnalyzer());
        }

        public DebuggerViewItem Analyse(Container container)
        {
            var analysisResults = (
                from analyzer in this
                let result = analyzer.Analyse(container)
                where result != null
                select result)
                .ToArray();

            if (!analysisResults.Any())
            {
                return new DebuggerViewItem(
                    "Configuration warnings",
                    "No warnings detected.",
                    null);
            }
            else if (analysisResults.Length == 1)
            {
                return analysisResults.Single();
            }
            else
            {
                return new DebuggerViewItem(
                    "Configuration warnings",
                    "See list below for all warnings.",
                    analysisResults);
            }
        }
    }

    internal sealed class AutoRegisteredContainerAnalyzer : IContainerAnalyzer
    {
        public DebuggerViewItem Analyse(Container container)
        {
            var registrations = container.GetCurrentRegistrations();

            var autoRegisteredServices = new HashSet<Type>((
                from producer in container.GetCurrentRegistrations()
                where producer.IsContainerAutoRegistered
                select producer.ServiceType));

            var registrationsThatDependOnAnUnregisteredType = (
                from registration in registrations
                from relationship in registration.GetRelationships()
                where autoRegisteredServices.Contains(relationship.Dependency.ServiceType)
                group relationship by registration into relationshipGroup
                select new 
                { 
                    Registration = relationshipGroup.Key, 
                    Relationships = relationshipGroup.ToArray() 
                })
                .ToArray();

            if (!registrationsThatDependOnAnUnregisteredType.Any())
            {
                return null;
            }
            else
            {
                var unregisteredTypeViewItems = (
                    from item in registrationsThatDependOnAnUnregisteredType
                    select BuildUnregisteredTypeViewItem(item.Registration, item.Relationships))
                    .ToArray();

                return new DebuggerViewItem(
                    "Unregistered Types",
                    autoRegisteredServices.Count + " container-registered services have been detected.", 
                    unregisteredTypeViewItems);
            }
        }

        private static DebuggerViewItem BuildUnregisteredTypeViewItem(InstanceProducer registration, 
            KnownRelationship[] relationships)
        {
            string description;

            if (relationships.Length == 1)
            {
                description =
                    registration.ServiceType.ToFriendlyName() + " depends on onregistered type " +
                    relationships.Single().Dependency.ServiceType.ToFriendlyName();
            }
            else
            {
                description = registration.ServiceType.ToFriendlyName() + " depends on " +
                    relationships.Length + " unregistered types.";
            }

            return new DebuggerViewItem(registration.ServiceType.ToFriendlyName(), description, relationships);
        }
    }
}
