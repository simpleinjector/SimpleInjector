using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc.ViewComponents
{
    internal abstract partial class _DefaultViewComponentInvoker
    {
        protected _DefaultViewComponentInvoker(DiagnosticSource diagnosticSource, ILogger logger)
        {
            _diagnosticSource = diagnosticSource;
            _logger = logger;
        }
    }
}