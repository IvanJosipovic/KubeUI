using DynamicData;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

public interface IClusterRuntime
{
    IReadOnlyDictionary<GroupApiVersionKind, object> Objects { get; }
    bool Connected { get; set; }
    ClusterStatus Status { get; set; }
    string? LastError { get; set; }
    /// <summary>Gets metrics capability for Kubernetes Metrics Server and Prometheus backends.</summary>
    IClusterMetrics Metrics { get; }
    bool ListNamespaces { get; set; }
    event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;
    event Action<IClusterRuntime>? NamespaceSelectionRequired;
    event Action<IClusterRuntime, GroupApiVersionKind>? ResourceSeeded;
    event Action<IClusterRuntime, GroupApiVersionKind>? ResourceUnseeded;
    /// <summary>Raised after a complete API discovery refresh.</summary>
    event Action<IClusterRuntime, ApiDiscoverySnapshot>? ApiDiscoverySnapshotRefreshed;
    IKubernetes? Client { get; set; }
    K8SConfiguration KubeConfig { get; set; }
    /// <summary>
    /// Gets or sets the model catalog owned by this cluster runtime.
    /// </summary>
    ClusterModelCatalog ModelCatalog { get; set; }
    /// <summary>Gets the last complete API discovery snapshot.</summary>
    ApiDiscoverySnapshot? ApiDiscoverySnapshot { get; }
    string KubeConfigPath { get; set; }
    string Name { get; set; }
    ReadOnlyObservableCollection<V1Namespace> Namespaces { get; }
    ObservableCollection<PortForwarder> PortForwarders { get; }
    IClusterAuthorization Permissions { get; }
    bool IsResourceNamespaced(GroupApiVersionKind kind);
    bool IsResourceNamespaced<T>();
    PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort);
    Task AddPodEphemeralDebugContainer(V1Pod pod, string? targetContainerName, string image);
    PortForwarder AddServicePortForward(string @namespace, string serviceName, int servicePort);
    void RemovePortForward(PortForwarder pf);
    Task AddOrUpdateResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task Connect();
    /// <summary>Refreshes authenticated `/api` and `/apis` discovery.</summary>
    Task RefreshApiDiscoveryAsync();
    /// <summary>Reapplies the current cluster metrics settings when the cluster is connected.</summary>
    Task ReinitializeMetricsAsync() => Task.CompletedTask;
    Task EnsureOpenApiSchemasAsync();
    Task Disconnect();
    Task DeleteResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task DryRunYaml(Stream stream);
    Task ImportFolder(string path);
    Task ImportYaml(Stream stream);
    Task SeedResource<T>(bool waitForReady = false) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task SeedResource<T>(bool waitForReady, CancellationToken cancellationToken) where T : class, IKubernetesObject<V1ObjectMeta>, new()
        => SeedResource<T>(waitForReady);
    Task SeedResource(GroupApiVersionKind kind, bool waitForReady = false);
    Task SeedResource(GroupApiVersionKind kind, bool waitForReady, CancellationToken cancellationToken)
        => SeedResource(kind, waitForReady);
    Task<bool> IsResourceReady<T>(CancellationToken? token = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    T? GetResource<T>(string? @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    IReadOnlyList<T> GetResourceList<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    ISourceCache<T, ResourceCacheKey> GetResourceSourceCache<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    ISourceCache<T, ResourceCacheKey> GetResourceSourceCache<T>(GroupApiVersionKind kind) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    IObservable<int> GetResourceCount(GroupApiVersionKind kind);
    IObservable<int> GetResourceCount<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    IObservable<ResourceChange> ConnectResources() => ClusterResourceChangeFeed.Connect(this);
}
