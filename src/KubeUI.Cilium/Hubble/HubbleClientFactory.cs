using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging;

namespace KubeUI.Cilium.Hubble;

public sealed class HubbleClientFactory
{
    private readonly ILogger<HubbleConnection> _logger;

    public HubbleClientFactory(ILogger<HubbleConnection> logger)
    {
        _logger = logger;
    }

    public HubbleConnection CreateConnection(IClusterRuntime cluster)
    {
        return new HubbleConnection(cluster, _logger);
    }
}