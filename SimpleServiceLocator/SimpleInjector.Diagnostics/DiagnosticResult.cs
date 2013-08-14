namespace SimpleInjector.Diagnostics
{
    using System;

    public class DiagnosticResult
    {
        internal DiagnosticResult(Type type, string name, string description, DiagnosticResultType resultType)
        {
            this.Type = type;
            this.Name = name;
            this.Description = description;
            this.ResultType = resultType;
        }

        public DiagnosticResultType ResultType { get; private set; }

        public Type Type { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public DiagnosticGroup Group { get; internal set; }
    }
}