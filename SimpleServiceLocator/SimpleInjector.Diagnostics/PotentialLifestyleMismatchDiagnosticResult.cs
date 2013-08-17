namespace SimpleInjector.Diagnostics
{
    using System;
    using SimpleInjector.Advanced;

    public class PotentialLifestyleMismatchDiagnosticResult : DiagnosticResult
    {
        public PotentialLifestyleMismatchDiagnosticResult(Type type, string description,
            KnownRelationship relationship)
            : base(type,  "Lifestyle Mismatch", description, DiagnosticType.PotentialLifestyleMismatch)
        {
            this.Relationship = relationship;
        }

        public KnownRelationship Relationship { get; private set; }
    }
}