// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    using System.Collections.Generic;

    internal interface IContainerAnalyzer
    {
        DiagnosticType DiagnosticType { get; }

        string Name { get; }

        string GetRootDescription(DiagnosticResult[] results);

        string GetGroupDescription(IEnumerable<DiagnosticResult> results);

        DiagnosticResult[] Analyze(IEnumerable<InstanceProducer> producers);
    }
}