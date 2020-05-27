// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using SimpleInjector.Advanced;

    /// <summary>
    /// Visualization options for providing various information about instances.
    /// </summary>
    public class VisualizationOptions : ApiObject
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include lifestyle information in the visualization.
        /// The default value is <b>true</b>.
        /// </summary>
        /// <value>The value to include life style information.</value>
        public bool IncludeLifestyleInformation { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to use fully qualified type names in the visualization.
        /// The default value is <b>false</b>.
        /// </summary>
        /// <value>The value to use fully qualified type names.</value>
        public bool UseFullyQualifiedTypeNames { get; set; }
    }
}