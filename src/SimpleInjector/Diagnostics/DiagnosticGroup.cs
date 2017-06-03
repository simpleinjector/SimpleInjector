#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

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
    [DebuggerDisplay(nameof(DiagnosticGroup) + " (Name: {Name, nq})")]
    public class DiagnosticGroup
    {
        internal DiagnosticGroup(DiagnosticType diagnosticType, Type groupType, string name, string description,
            IEnumerable<DiagnosticGroup> children, IEnumerable<DiagnosticResult> results)
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

        /// <summary>Gets the parent <see cref="DiagnosticGroup"/> or null (Nothing in VB) when this is the
        /// root group.</summary>
        /// <value>The <see cref="DiagnosticGroup"/>.</value>
        public DiagnosticGroup Parent { get; private set; }

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