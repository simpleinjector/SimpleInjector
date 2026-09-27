// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using SimpleInjector.Diagnostics.Debugger;

    /// <summary>
    /// Diagnostic result that warns about when a multiple registrations map to the same implementation type
    /// and lifestyle, which might cause multiple instances to be created during the lifespan of that lifestyle.
    /// For more information, see: https://simpleinjector.org/diatl.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public class UnusedConditionalRegistration : DiagnosticResult
    {
        internal UnusedConditionalRegistration(InstanceProducer producer, string description)
            : base(
                producer.ServiceType,
                description,
                DiagnosticType.UnusedConditionalRegistration,
                DiagnosticSeverity.Information,
                CreateDebugValue(producer))
        {
            this.InstanceProducer = producer;
        }

        /// <summary>Gets the unused instance producer.</summary>
        public InstanceProducer InstanceProducer { get; }

        private static DebuggerViewItem[] CreateDebugValue(InstanceProducer producer)
        {
            return
            [
                new DebuggerViewItem(
                    name: "ServiceType",
                    description: producer.ServiceType.ToFriendlyName(),
                    value: producer.ServiceType),
                new DebuggerViewItem(
                    name: "ImplementationType",
                    description: producer.ImplementationType.ToFriendlyName(),
                    value: producer.ImplementationType),
                new DebuggerViewItem(
                    name: "InstanceProducer",
                    description: producer.ImplementationType.ToFriendlyName(),
                    value: producer.ImplementationType),
            ];
        }
    }
}