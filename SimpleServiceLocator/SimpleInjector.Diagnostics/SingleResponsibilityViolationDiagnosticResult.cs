namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class SingleResponsibilityViolationDiagnosticResult : DiagnosticResult
    {
        public SingleResponsibilityViolationDiagnosticResult(Type type, string name, string description,
            Type implementationType, IEnumerable<InstanceProducer> dependencies)
            : base(type, name, description, DiagnosticResultType.SingleResponsibilityViolation)
        {
            this.ImplementationType = implementationType;
            this.Dependencies = dependencies.ToList().AsReadOnly();
        }

        public Type ImplementationType { get; private set; }

        public ReadOnlyCollection<InstanceProducer> Dependencies { get; private set; }
    }
}