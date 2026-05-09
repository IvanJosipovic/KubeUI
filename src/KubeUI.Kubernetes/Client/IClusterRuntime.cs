using DynamicData;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

public readonly record struct AuthorizationRequest(Type ResourceType, Verb Verb, string? Subresource);

public interface IClusterRuntime
{
    IReadOnlyDictionary<GroupApiVersionKind, object> Objects { get; }
    bool Connected { get; set; }
    ClusterStatus Status { get; set; }
    string? LastError { get; set; }
    bool RequiresNamespaceSelectionPrompt { get; set; }
    bool IsMetricsAvailable { get; }
    ActiveMetricsBackend ActiveMetricsBackend { get; }
    bool ListNamespaces { get; set; }
    event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;
    event Action<V1CustomResourceDefinition>? OnCustomResourceDefinitionReady;
    IKubernetes? Client { get; set; }
    K8SConfiguration KubeConfig { get; set; }
    ModelCache ModelCache { get; set; }
    string KubeConfigPath { get; set; }
    string Name { get; set; }
    ReadOnlyObservableCollection<V1Namespace> Namespaces { get; }
    ObservableCollection<NodeMetrics> NodeMetrics { get; }
    ObservableCollection<PodMetrics> PodMetrics { get; }
    ObservableCollection<PortForwarder> PortForwarders { get; }
    bool CanI(Type type, Verb verb, string? @namespace = null, string? subresource = null);
    bool CanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    bool CanIAnyNamespace(Type type, Verb verb, string? subresource = null);
    bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    bool IsResourceNamespaced(Type type);
    bool IsResourceNamespaced<T>();
    PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort);
    Task AddPodEphemeralDebugContainer(V1Pod pod, string? targetContainerName, string image);
    PortForwarder AddServicePortForward(string @namespace, string serviceName, int servicePort);
    void RemovePortForward(PortForwarder pf);
    Task AddOrUpdateResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task Connect();
    Task Disconnect();
    Task DeleteResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task DryRunYaml(Stream stream);
    Task ImportFolder(string path);
    Task ImportYaml(Stream stream);
    Task<MetricResultSet> RequestMetricsAsync(MetricRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MetricProviderInfo>> GetAvailablePrometheusProvidersAsync();
    Task SeedResource<T>(bool waitForReady = false) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task<bool> IsResourceReady<T>(CancellationToken? token = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    T? GetResource<T>(string? @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    IReadOnlyList<T> GetResourceList<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    ISourceCache<T, string> GetResourceSourceCache<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    IObservable<int> GetResourceCount(Type type);
    IObservable<int> GetResourceCount<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task UpdatePermissionsAllNamespaceAsync(Type type, Verb verb, string? subresource = null);
    Task UpdatePermissionsAllNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task<bool> UpdateCanI(Type type, Verb verb, string? @namespace = null, string? subresource = null);
    Task<bool> UpdateCanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task<bool> UpdateCanIAnyNamespaceAsync(Type type, Verb verb, string? subresource = null);
    Task<bool> UpdateCanIAnyNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
}

