// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Diagnostics
{
    /// <summary>
    /// Specifies the list of severity levels that diagnostic results can have.
    /// </summary>
    public enum DiagnosticSeverity
    {
        /// <summary>Information messages and tips about the configuration.</summary>
        Information = 0,

        /// <summary>Warning messages that are likely to cause problems in your application.</summary>
        Warning = 1
    }
}