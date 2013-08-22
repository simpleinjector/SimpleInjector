namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    internal sealed class ContainerDebugViewProxy
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static Type debugViewType;
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object debugView;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Container container;
        
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline",
            Justification =
                "We don't care about the possible performance hit here, since this code runs in the " +
                "debugger and it can't easily be written inline.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification =
                "This code is run in the Visual Studio Debugger and by catching every exception we can " +
                "display this error to the user. If we don't catch this exception, the debugger will " +
                "simply fall back to the default debugger view and we will loose the exception information.")]
        static ContainerDebugViewProxy()
        {
            try
            {
                var assembly = Assembly.Load("SimpleInjector.Diagnostics");
                debugViewType = assembly.GetType("SimpleInjector.Diagnostics.Debugger.ContainerDebugView", 
                    throwOnError: true);
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }
 
        public ContainerDebugViewProxy(Container container)
        {
            this.container = container;

            this.debugView = CreateNewDebugViewInstance(container);
        }

        public object Options
        {
            get
            {
                if (this.debugView == null)
                {
                    return this.container.Options;
                }

                return debugViewType.GetProperty("Options").GetValue(this.debugView, null);
            }
        }

        [DebuggerDisplay("")]
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object Items
        {
            get
            {
                if (this.debugView == null)
                {
                    return new[] 
                    {
                        new DebuggerErrorViewItem(
                            "Failure", 
                            "The Debugger Type Proxy failed to initialize. " +
                            "For the debugger diagnostics to work, the SimpleInjector.Diagnostics.dll is needed, " +
                            "but this dll could not be found or loaded. See the exception for more info.", 
                            Exception)
                    };
                }

                return debugViewType.GetProperty("Items").GetValue(this.debugView, null);
            }
        }
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal static Exception Exception { get; private set; }
        
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "This code is run in the Visual Studio Debugger.")]
        private static object CreateNewDebugViewInstance(Container container)
        {
            if (debugViewType != null)
            {
                try
                {
                    return Activator.CreateInstance(debugViewType, container);
                }
                catch (Exception ex)
                {
                    Exception = ex;
                }
            }

            return null;
        }
    }
}