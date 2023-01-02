using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace KubeUI.UI;

public class ErrorBoundaryLogger : IErrorBoundaryLogger
{
    private readonly ILogger<ErrorBoundary> _errorBoundaryLogger;

    public ErrorBoundaryLogger(ILogger<ErrorBoundary> errorBoundaryLogger)
    {
        _errorBoundaryLogger = errorBoundaryLogger ?? throw new ArgumentNullException(nameof(errorBoundaryLogger)); ;
    }

    public ValueTask LogErrorAsync(Exception exception)
    {
        // For, client-side code, all internal state is visible to the end user. We can just
        // log directly to the console.
        _errorBoundaryLogger.LogError(exception.ToString());
        return ValueTask.CompletedTask;
    }
}