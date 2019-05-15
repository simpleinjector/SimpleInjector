// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    /// <summary>
    /// This enumeration defines in which way the container should run the verification process.
    /// </summary>
    public enum VerificationOption
    {
        /// <summary>
        /// Specifies that the container performs verification only, which means that it will test whether
        /// all registrations can be constructed by iterating the registrations and letting the container
        /// create at least one instance of each registration. An <see cref="System.InvalidOperationException"/>
        /// will be thrown in case the configuration is invalid.
        /// </summary>
        VerifyOnly = 0,

        /// <summary>
        /// Specifies that the container will run diagnostic analysis after the verification succeeded. The
        /// container will diagnose the configuration with a subset of the available diagnostic warnings, that 
        /// are most likely an indication of a configuration mistake. A complete set of diagnostic warnings
        /// can be retrieved by calling 
        /// <see cref="SimpleInjector.Diagnostics.Analyzer.Analyze">Analyzer.Analyze</see> or by viewing the 
        /// container in the Visual Studio debugger, after the verification has succeeded.
        /// </summary>
        VerifyAndDiagnose = 1
    }
}