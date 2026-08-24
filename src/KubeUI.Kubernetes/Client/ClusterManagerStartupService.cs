using Microsoft.Extensions.Hosting;

namespace KubeUI.Kubernetes;

internal sealed class ClusterManagerStartupService(
    ClusterManager clusterManager,
    ILogger<ClusterManagerStartupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await clusterManager.LoadClustersAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error loading kubeconfig files during startup");
        }
    }
}
