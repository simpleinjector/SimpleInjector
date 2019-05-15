// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using SimpleInjector.Diagnostics.Analyzers;

    internal static class ContainerAnalyzerProvider
    {
        internal static readonly ReadOnlyCollection<IContainerAnalyzer> Analyzers =
            new ReadOnlyCollection<IContainerAnalyzer>(new List<IContainerAnalyzer>
            {
                new LifestyleMismatchAnalyzer(),
                new ShortCircuitedDependencyAnalyzer(),
                new SingleResponsibilityViolationsAnalyzer(),
                new ContainerRegisteredServiceAnalyzer(),
                new TornLifestyleContainerAnalyzer(),
                new DisposableTransientComponentAnalyzer(),
                new AmbiguousLifestylesAnalyzer()
            });
    }
}