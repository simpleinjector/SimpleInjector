namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using SimpleInjector.Diagnostics.Debugger;

    public class SingleResponsibilityViolationDiagnosticResult : DiagnosticResult
    {
        public SingleResponsibilityViolationDiagnosticResult(Type type, string description,
            Type implementationType, IEnumerable<InstanceProducer> dependencies)
            : base(type, "SRP Violation", description, DiagnosticType.SingleResponsibilityViolation,
                GetDebugValue(implementationType, dependencies.ToArray()))
        {
            this.ImplementationType = implementationType;
            this.Dependencies = dependencies.ToList().AsReadOnly();
        }

        public Type ImplementationType { get; private set; }

        public ReadOnlyCollection<InstanceProducer> Dependencies { get; private set; }

        private static DebuggerViewItem[] GetDebugValue(Type implementationType, InstanceProducer[] dependencies)
        {
            return new[]
            {
                new DebuggerViewItem("ImplementationType", implementationType.ToFriendlyName(), implementationType),
                new DebuggerViewItem("Dependencies", dependencies.Length + " dependencies.", dependencies),
            };
        }
    }
}