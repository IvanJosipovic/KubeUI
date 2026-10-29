namespace KubeUI.Cilium.Hubble;

/// <summary>Provides one live Hubble flow stream for a Kubernetes cluster.</summary>
public interface IHubbleConnection : IAsyncDisposable
{
    /// <summary>Raised when the connection state changes.</summary>
    event Action<ConnectionStatus>? StatusChanged;

    /// <summary>Gets the live flow event stream.</summary>
    IObservable<FlowViewModel> Flows { get; }

    /// <summary>Gets a live flow stream filtered to the selected Kubernetes namespaces.</summary>
    /// <param name="namespaces">At least one namespace to include.</param>
    IObservable<FlowViewModel> ObserveFlows(IReadOnlyCollection<string> namespaces);

    /// <summary>Starts the port forward and prepares the Hubble client.</summary>
    Task StartAsync(CancellationToken cancellationToken = default);
}

/// <summary>Creates Hubble connections for cluster runtimes.</summary>
public interface IHubbleConnectionFactory
{
    /// <summary>Creates a connection bound to one cluster.</summary>
    IHubbleConnection CreateConnection(IClusterRuntime cluster);
}
