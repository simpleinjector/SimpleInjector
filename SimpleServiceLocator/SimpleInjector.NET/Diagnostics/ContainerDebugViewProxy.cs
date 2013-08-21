namespace SimpleInjector.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    
    internal sealed class ContainerDebugViewProxy
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static Type debugViewType;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static Exception exception;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object debugView;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Container container;

        static ContainerDebugViewProxy()
        {
            try
            {
                var assembly = Assembly.Load("SimpleInjector.Diagnostics");
                debugViewType = assembly.GetType("SimpleInjector.Diagnostics.Debugger.ContainerDebugView");
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }

        public ContainerDebugViewProxy(Container container)
        {
            this.container = container;

            debugView = CreateNewDebugViewInstance(container);
        }

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
                    exception = ex;
                }
            }

            return null;
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
                            exception)
                    };
                }

                return debugViewType.GetProperty("Items").GetValue(this.debugView, null);
            }
        }
    }
}