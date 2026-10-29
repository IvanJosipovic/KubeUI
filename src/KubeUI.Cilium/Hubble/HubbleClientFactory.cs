using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KubeUI.Cilium.Hubble;

/// <summary>Creates Hubble Relay connections from the registered application options.</summary>
public sealed class HubbleClientFactory : IHubbleConnectionFactory
{
    private readonly ILogger<HubbleConnection> _logger;
    private readonly IOptions<HubbleConnectionOptions> _options;

    /// <summary>Creates a factory using the configured options and Hubble logger.</summary>
    public HubbleClientFactory(ILogger<HubbleConnection> logger, IOptions<HubbleConnectionOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    /// <inheritdoc />
    public IHubbleConnection CreateConnection(IClusterRuntime cluster)
    {
        return new HubbleConnection(cluster, _logger, _options.Value);
    }
}
