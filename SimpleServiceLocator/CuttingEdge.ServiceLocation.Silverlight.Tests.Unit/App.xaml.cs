using System;
using System.Windows;

using Microsoft.Silverlight.Testing;

namespace SimpleInjector.Silverlight.Tests.Unit
{
    /// <summary>
    /// Silverlight unit test form.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>Initializes a new instance of the <see cref="App"/> class.</summary>
        public App()
        {
            this.Startup += this.Application_Startup;
            this.UnhandledException += this.Application_UnhandledException;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Load the main control
            this.RootVisual = UnitTestSystem.CreateTestPage();
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
        }
    }
}