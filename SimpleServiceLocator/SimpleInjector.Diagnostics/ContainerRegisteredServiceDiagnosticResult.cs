namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using SimpleInjector.Advanced;

    public class ContainerRegisteredServiceDiagnosticResult : DiagnosticResult
    {
        public ContainerRegisteredServiceDiagnosticResult(Type type, string name, string description,
            IEnumerable<KnownRelationship> relationships)
            : base(type, name, description, DiagnosticResultType.ContainerRegisteredService)
        {
            this.Relationships = relationships.ToList().AsReadOnly();
        }

        public ReadOnlyCollection<KnownRelationship> Relationships { get; private set; }
    }
}