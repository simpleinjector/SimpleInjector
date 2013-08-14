namespace SimpleInjector.Diagnostics
{
    using System;
    using SimpleInjector.Advanced;

    public class PotentialLifestyleMismatchDiagnosticResult : DiagnosticResult
    {
        public PotentialLifestyleMismatchDiagnosticResult(Type type, string name, string description,
            KnownRelationship relationship)
            : base(type, name, description, DiagnosticResultType.PotentialLifestyleMismatch)
        {
            this.Relationship = relationship;
        }

        public KnownRelationship Relationship { get; private set; }
    }
}