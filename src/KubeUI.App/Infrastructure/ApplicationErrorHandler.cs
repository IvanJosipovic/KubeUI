using Avalonia.Threading;
using KubeUI.App.Infrastructure.Presentation;

namespace KubeUI.App.Infrastructure;

/// <summary>Reports application errors and routes Kubernetes exec errors to the shared document.</summary>
internal sealed class ApplicationErrorHandler(
    ILogger<ApplicationErrorHandler> logger,
    IApplicationTelemetryFlusher telemetryFlusher,
    IClusterErrorDocumentPresenter clusterErrorDocumentPresenter)
{
    public void ReportUnhandledException(Exception exception, bool isTerminating)
    {
        logger.LogCritical(exception, "Unhandled exception (terminating: {IsTerminating})", isTerminating);
        telemetryFlusher.TryFlush();
    }

    public void HandleKubernetesExecStdError(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            logger.LogTrace("Cluster ExecStdError: no data");
            return;
        }

        logger.LogWarning("Cluster ExecStdError: {Data}", error);
        Dispatcher.UIThread.Post(() => clusterErrorDocumentPresenter.Show(error), DispatcherPriority.Background);
    }
}
