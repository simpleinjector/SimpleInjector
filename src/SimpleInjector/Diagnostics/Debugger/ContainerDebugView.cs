// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics.Debugger
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using SimpleInjector.Diagnostics.Analyzers;

    internal sealed class ContainerDebugView
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Container container;

        public ContainerDebugView(Container container)
        {
            this.container = container;

            this.Initialize();
        }

        public ContainerOptions Options => this.container.Options;

        [DebuggerDisplay("")]
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public DebuggerViewItem[]? Items { get; private set; }

        private void Initialize()
        {
            if (!this.container.SuccesfullyVerified)
            {
                this.Items = new[]
                {
                    new DebuggerViewItem(
                        name: "How To View Diagnostic Info",
                        description: "Analysis info is available in this debug view after Verify() is " +
                            "called on this container instance.")
                };

                return;
            }

            try
            {
                this.Items = this.GetAnalysisResults().ToArray();
            }
            catch (Exception ex)
            {
                this.Items = GetDebuggerTypeProxyFailureResults(ex);
            }
        }

        private DebuggerViewItem[] GetAnalysisResults()
        {
            var registrations = this.container.GetCurrentRegistrations();

            var rootRegistrations = this.container.GetRootRegistrations();

            return new DebuggerViewItem[]
            {
                DebuggerGeneralWarningsContainerAnalyzer.Analyze(this.container),
                new DebuggerViewItem(
                    name: "Registrations",
                    description: "Count = " + registrations.Length,
                    value: registrations),
                new DebuggerViewItem(
                    name: "Root Registrations",
                    description: "Count = " + rootRegistrations.Length,
                    value: this.GroupProducers(rootRegistrations))
            };
        }

        private static DebuggerViewItem[] GetDebuggerTypeProxyFailureResults(Exception exception)
        {
            return new[]
            {
                new DebuggerViewItem(
                    "Failure",
                    "We're so so sorry. The Debugger Type Proxy failed to initialize.",
                    exception)
            };
        }

        private object[] GroupProducers(IEnumerable<InstanceProducer> producers) =>
            this.GroupProducers(producers, level: 0);

        private object[] GroupProducers(IEnumerable<InstanceProducer> producers, int level) => (
            from producer in producers
            group producer by TypeGeneralizer.MakeTypePartiallyGenericUpToLevel(producer.ServiceType, level)
                into resultGroup
            select this.BuildProducerGroup(resultGroup.Key, resultGroup.ToArray(), level + 1))
            .ToArray();

        private object BuildProducerGroup(
            Type groupType, InstanceProducer[] producersForGroup, int level)
        {
            if (producersForGroup.Length == 1)
            {
                var producer = producersForGroup[0];

                // This flattens the hierarchy when there is just one item in the group.
                return new DebuggerViewItem(
                    name: producer.ServiceType.ToFriendlyName(),
                    description: producer.DebuggerDisplay,
                    value: producersForGroup[0]);
            }
            else if (groupType.ContainsGenericParameters())
            {
                return this.BuildGenericGroup(groupType, producersForGroup, level);
            }
            else
            {
                return BuildNonGenericGroup(groupType, producersForGroup);
            }
        }

        private object BuildGenericGroup(
            Type groupType, InstanceProducer[] producersForGroup, int level)
        {
            object[] childGroups = this.GroupProducers(producersForGroup, level);

            if (childGroups.Length == 1)
            {
                // This flattens the hierarchy when there is just one item in the group.
                return childGroups[0];
            }

            return new DebuggerViewItem(
                name: groupType.ToFriendlyName(),
                description: "Count = " + producersForGroup.Length,
                value: childGroups);
        }

        private static DebuggerViewItem BuildNonGenericGroup(
            Type closedType, InstanceProducer[] producersForGroup) =>
            new DebuggerViewItem(
                name: closedType.ToFriendlyName(),
                description: "Count = " + producersForGroup.Length,
                value: producersForGroup.ToArray());
    }
}