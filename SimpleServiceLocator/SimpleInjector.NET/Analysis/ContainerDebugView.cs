#region Copyright (c) 2013 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2013 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
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

namespace SimpleInjector.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    internal sealed class ContainerDebugView
    {
        private static IEnumerable<IContainerAnalyzer> analyzers;

        private Container container;
        private InstanceProducer[] registrations;

        static ContainerDebugView()
        {
            analyzers = new IContainerAnalyzer[]
            {
                new PotentialLifestyleMismatchContainerAnalyzer()
            };
        }

        public ContainerDebugView(Container container)
        {
            this.container = container;

            container.IsVerifying = true;

            try
            {
                this.Items = this.GetVerificationErrorResults() ?? this.GetAnalysisResults();
            }
            catch (Exception ex)
            {
                this.Items = GetDebuggerTypeProxyFailureResults(ex);
            }
            finally
            {
                try
                {
                    this.Initialize();
                }
                finally
                {
                    container.IsVerifying = false;

                    this.ClearCache();
                }
            }
        }

        [DebuggerDisplay("Count = {Registrations.Length}")]
        public InstanceProducer[] Registrations
        {
            get { return this.registrations; }
        }

        public ContainerOptions Options
        {
            get { return this.container.Options; }
        }

        [DebuggerDisplay("")]
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public DebuggerViewItem[] Items { get; private set; }

        private DebuggerViewItem[] GetVerificationErrorResults()
        {
            var errors = this.container
                .GetVerificationErrorsForRegistrations()
                .Union(
                    this.container.GetVerificationErrorsForCollections())
                .ToArray();

            return errors.Any() ? new[] { CreateErrorsDebuggerViewItem(errors) } : null;
        }

        private static DebuggerViewItem CreateErrorsDebuggerViewItem(VerificationError[] errors)
        {
            var errorViews = (
                from error in errors
                select new DebuggerViewItem(
                    error.Registration.ServiceType.ToFriendlyName(),
                    error.Exception.Message,
                    error.Exception))
                .ToArray();

            string description = errors.Length == 1 ? errors[0].Exception.Message : errors.Length + " errors.";

            return new DebuggerViewItem("Configuration Errors", description, errorViews);
        }

        private DebuggerViewItem[] GetAnalysisResults()
        {
            return (
                from analyzer in analyzers
                select analyzer.Analyse(this.container))
                .ToArray();
        }

        private static DebuggerViewItem[] GetDebuggerTypeProxyFailureResults(Exception ex)
        {
            return new[] 
            {
                new DebuggerViewItem(
                    "Failure", 
                    "We're so so sorry. The Debugger Type Proxy failed to initialize.", 
                    ex)
            };
        }

        private void Initialize()
        {
            this.registrations = this.container.GetCurrentRegistrations();
        }

        private void ClearCache()
        {
            if (!this.container.IsLocked)
            {
                // We must clear the cache (this removes any cached delegates and expressions). Compiled
                // delegates contain information about how to create their dependencies and since this
                // could still be changed, we could corrupt the container's configuration.
                this.container.ClearCache();
            }
        }
    }
}