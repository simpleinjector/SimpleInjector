// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// A hierarchical group of <see cref="DiagnosticResult"/>.
    /// </summary>
    [DebuggerDisplay(nameof(DiagnosticGroup) + " (Name: {" + nameof(DiagnosticGroup.Name) + ", nq})")]
    public class DiagnosticGroup
    {
        internal DiagnosticGroup(
            DiagnosticType diagnosticType,
            Type groupType,
            string name,
            string description,
            IEnumerable<DiagnosticGroup> children,
            IEnumerable<DiagnosticResult> results)
        {
            this.DiagnosticType = diagnosticType;
            this.GroupType = groupType;
            this.Name = name;
            this.Description = description;
            this.Children = new ReadOnlyCollection<DiagnosticGroup>(children.ToList());
            this.Results = new ReadOnlyCollection<DiagnosticResult>(results.ToList());

            this.InitializeChildren();
            this.InitializeResults();
        }

        /// <summary>
        /// Gets the base <see cref="DiagnosticType"/> that describes the service types of its
        /// <see cref="Results"/>. The value often be either <see cref="System.Object"/> (in case this is a
        /// root group) or a partial generic type to allow hierarchical grouping of a large number of related
        /// generic types.
        /// </summary>
        /// <value>The <see cref="Type"/>.</value>
        [DebuggerDisplay("{" + TypesExtensions.FriendlyName + "(GroupType), nq}")]
        public Type GroupType { get; }

        /// <summary>Gets the friendly name of the group.</summary>
        /// <value>The name.</value>
        [DebuggerDisplay("{Name, nq}")]
        public string Name { get; }

        /// <summary>Gets the description of the group.</summary>
        /// <value>The description.</value>
        [DebuggerDisplay("{Description, nq}")]
        public string Description { get; }

        /// <summary>Gets the diagnostic type of all grouped <see cref="DiagnosticResult"/> instances.</summary>
        /// <value>The <see cref="DiagnosticType"/>.</value>
        public DiagnosticType DiagnosticType { get; }

        /// <summary>Gets the parent <see cref="DiagnosticGroup"/> or null when this is the
        /// root group.</summary>
        /// <value>The <see cref="DiagnosticGroup"/>.</value>
        public DiagnosticGroup? Parent { get; private set; }

        /// <summary>Gets the collection of child <see cref="DiagnosticGroup"/>s.</summary>
        /// <value>A collection of <see cref="DiagnosticGroup"/> elements.</value>
        public ReadOnlyCollection<DiagnosticGroup> Children { get; }

        /// <summary>Gets the collection of <see cref="DiagnosticResult"/> instances.</summary>
        /// /// <value>A collection of <see cref="DiagnosticResult"/> elements.</value>
        public ReadOnlyCollection<DiagnosticResult> Results { get; }

        private void InitializeChildren()
        {
            foreach (var child in this.Children)
            {
                child.Parent = this;
            }
        }

        private void InitializeResults()
        {
            foreach (var result in this.Results)
            {
                result.Group = this;
            }
        }
    }
}