using System;
using System.Threading;
using SimpleInjector.Advanced;

namespace SimpleInjector.Integration.AspNetCore
{
    /// <summary>
    /// Provides settings for auto cross-wiring.
    /// </summary>
    public sealed class AutoCrossWireSettings
    {
        private static readonly object ItemsKey = new object();

        private readonly ThreadLocal<bool> isDisabled = new ThreadLocal<bool>();

        /// <summary>
        /// Returns true when auto cross-wiring is enabled or temporarily disabled for this thread.
        /// </summary>
        /// <param name="container">The container instance.</param>
        /// <returns>True if auto cross-wiring is enabled.</returns>
        public static bool IsAutoCrossWiringEnabledForThisThread(Container container)
        {
            AutoCrossWireSettings settings = GetAutoCrossWireSettings(container);

            return !settings.isDisabled.Value;
        }

        /// <summary>
        /// Temporarily disable auto cross-wiring. This method returns a scope object. When the object is
        /// disposed, auto cross-wiring will be enabled again.
        /// </summary>
        /// <param name="container">The container instance.</param>
        /// <returns>A scope instance that should be disposed if auto cross-wiring should be enabled again for this thread.</returns>
        public static IDisposable SuppressAutoCrossWiringForThisThread(Container container)
        {
            AutoCrossWireSettings settings = GetAutoCrossWireSettings(container);

            return new AutoCrossWiringDisabledScope(settings.isDisabled);
        }

        internal static void Configure(Container container)
        {
            container.SetItem(ItemsKey, new AutoCrossWireSettings());
        }

        private static AutoCrossWireSettings GetAutoCrossWireSettings(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            var settings = (AutoCrossWireSettings)container.GetItem(ItemsKey);

            if (settings == null)
            {
                throw new InvalidOperationException("Make sure container.AutoCrossWireAspNetComponents is called.");
            }

            return settings;
        }

        private sealed class AutoCrossWiringDisabledScope : IDisposable
        {
            private readonly ThreadLocal<bool> isDisabled;
            private readonly bool originalValue;

            public AutoCrossWiringDisabledScope(ThreadLocal<bool> isDisabled)
            {
                this.originalValue = this.isDisabled.Value;
                this.isDisabled = isDisabled;
                this.isDisabled.Value = true;
            }

            public void Dispose() => this.isDisabled.Value = this.originalValue;
        }
    }
}