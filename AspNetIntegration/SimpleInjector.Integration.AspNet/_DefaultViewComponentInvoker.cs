﻿namespace Microsoft.AspNet.Mvc.ViewComponents
{
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    internal partial class _DefaultViewComponentInvoker
    {
        protected _DefaultViewComponentInvoker(DiagnosticSource diagnosticSource, ILogger logger)
        {
            _diagnosticSource = diagnosticSource;
            _logger = logger;
        }

        internal static class Resources
        {
            internal static readonly string ViewComponent_MustReturnValue =
                "A view component must return a non-null value.";

            internal static string FormatViewComponent_CannotFindMethod(string syncMethodName) =>
                $"Could not find an '{syncMethodName}' method matching the parameters.";

            internal static string FormatViewComponent_CannotFindMethod_WithFallback(string a, string b) =>
                $"Could not find an '{a}' or '{b}' method matching the parameters.";

            internal static string FormatViewComponent_InvalidReturnValue(string a, string b, string c) =>
                $"View components only support returning {a}, {b} or {c}.";
        }
    }
}