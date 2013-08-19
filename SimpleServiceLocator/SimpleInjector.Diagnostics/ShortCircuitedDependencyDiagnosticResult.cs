namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using SimpleInjector.Advanced;
    using SimpleInjector.Diagnostics.Debugger;

    public class ShortCircuitedDependencyDiagnosticResult : DiagnosticResult
    {
        internal ShortCircuitedDependencyDiagnosticResult(Type type, string description,
            InstanceProducer registration, KnownRelationship actualDependency,
            IEnumerable<InstanceProducer> expectedDependencies)
            : base(type, "Short Circuit", description, DiagnosticType.ShortCircuitedDependency,
                CreateDebugValue(registration, actualDependency, expectedDependencies.ToArray()))
        {
            this.Registration = registration;
            this.ActualDependency = actualDependency;
            this.ExpectedDependencies = expectedDependencies.ToList().AsReadOnly();
        }

        public InstanceProducer Registration { get; private set; }

        public KnownRelationship ActualDependency { get; private set; }

        public ReadOnlyCollection<InstanceProducer> ExpectedDependencies { get; private set; }

        private static DebuggerViewItem[] CreateDebugValue(InstanceProducer registration,
            KnownRelationship actualDependency, 
            InstanceProducer[] possibleSkippedRegistrations)
        {
            return new[]
            {
                new DebuggerViewItem(
                    name: "Registration", 
                    description: registration.ServiceType.ToFriendlyName(), 
                    value: registration),
                new DebuggerViewItem(
                    name: "Actual Dependency", 
                    description: actualDependency.Dependency.ServiceType.ToFriendlyName(), 
                    value: actualDependency),
                new DebuggerViewItem(
                    name: "Expected Dependency", 
                    description: possibleSkippedRegistrations.First().ServiceType.ToFriendlyName(),
                    value: possibleSkippedRegistrations.Length == 1 ? 
                        (object)possibleSkippedRegistrations[0] : 
                        possibleSkippedRegistrations),
            };
        }
    }
}