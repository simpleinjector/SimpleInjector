namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    [DebuggerDisplay("{Description,nq}", Name = "{Name,nq}")]
    internal class DebuggerErrorViewItem
    {
        public DebuggerErrorViewItem(string name, string description, Exception exception)
        {
            this.Name = name;
            this.Description = description;
            this.Exception = exception;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Name { get; private set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Description { get; private set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Called by the Visual Studio debugger.")]
        public Exception Exception { get; private set; }

        public override string ToString()
        {
            return this.Description;
        }
    }
}