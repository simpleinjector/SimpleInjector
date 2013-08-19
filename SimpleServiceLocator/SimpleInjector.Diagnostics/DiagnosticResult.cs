namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{SimpleInjector.Helpers.ToFriendlyName(Type),nq}: {Description,nq}")]
    public class DiagnosticResult
    {
        internal DiagnosticResult(Type type, string name, string description, DiagnosticType resultType,
            object value)
        {
            this.Type = type;
            // this.Name = type.ToFriendlyName();
            this.Description = description;
            this.ResultType = resultType;
            this.Value = value;
        }

        public DiagnosticType ResultType { get; private set; }

        [DebuggerDisplay("{SimpleInjector.Helpers.ToFriendlyName(Type),nq}")]
        public Type Type { get; private set; }

        //[DebuggerDisplay("{Name,nq}")]
        //public string Name { get; private set; }

        [DebuggerDisplay("{Description,nq}")]
        public string Description { get; private set; }

        public DiagnosticGroup Group { get; internal set; }

        [DebuggerHidden]
        public object Value { get; private set; }
    }
}