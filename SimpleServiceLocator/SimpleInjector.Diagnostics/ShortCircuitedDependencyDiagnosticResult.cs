namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using SimpleInjector.Advanced;

    public class ShortCircuitedDependencyDiagnosticResult : DiagnosticResult
    {
        internal ShortCircuitedDependencyDiagnosticResult(Type type, string name, string description,
            InstanceProducer registration, KnownRelationship actualDependency,
            IEnumerable<InstanceProducer> expectedDependencies)
            : base(type, name, description, DiagnosticResultType.ShortCircuitedDependency)
        {
            this.Registration = registration;
            this.ActualDependency = actualDependency;
            this.ExpectedDependencies = expectedDependencies.ToList().AsReadOnly();
        }

        public InstanceProducer Registration { get; private set; }

        public KnownRelationship ActualDependency { get; private set; }

        public ReadOnlyCollection<InstanceProducer> ExpectedDependencies { get; private set; }
    }
}