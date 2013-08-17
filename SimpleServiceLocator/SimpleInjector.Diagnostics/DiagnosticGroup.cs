namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;

    [DebuggerDisplay("{Name}")]
    public class DiagnosticGroup
    {
        internal DiagnosticGroup(DiagnosticType diagnosticType, Type groupType, string name, string description,
            IEnumerable<DiagnosticGroup> children, IEnumerable<DiagnosticResult> results)
        {
            this.DiagnosticType = diagnosticType;
            this.GroupType = groupType;
            this.Name = name;
            this.Description = description;
            this.Children = children.ToList().AsReadOnly();
            this.Results = results.ToList().AsReadOnly();

            foreach (var child in this.Children)
            {
                child.Parent = this;
            }

            foreach (var result in this.Results)
            {
                result.Group = this;
            }
        }

        [DebuggerDisplay("{SimpleInjector.Helpers.ToFriendlyName(GroupType),nq}")]
        public Type GroupType { get; private set; }

        [DebuggerDisplay("{Name,nq}")]
        public string Name { get; private set; }

        [DebuggerDisplay("{Description,nq}")]
        public string Description { get; private set; }

        public DiagnosticType DiagnosticType { get; private set; }

        public DiagnosticGroup Parent { get; private set; }

        public ReadOnlyCollection<DiagnosticGroup> Children { get; private set; }

        public ReadOnlyCollection<DiagnosticResult> Results { get; private set; }
    }
}