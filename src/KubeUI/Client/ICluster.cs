using Avalonia.Collections;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using Kubernetes.Controller.Client;
using KubeUI.Resources;
using static KubeUI.Client.Cluster;
using Kubernetes.Controller;
using DynamicData;

namespace KubeUI.Client;

public interface ICluster
{
    AvaloniaDictionary<GroupApiVersionKind, object> Objects { get; }
    IReadOnlyList<T> GetResourceList<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    bool CanI(Type type, Verb verb, string? @namespace = null, string? subresource = null);
    bool CanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    bool CanIAnyNamespace(Type type, Verb verb, string? subresource = null);
    bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    bool Connected { get; set; }
    bool IsMetricsAvailable { get; }
    bool IsResourceNamespaced(Type type);
    bool IsResourceNamespaced<T>();
    bool ListNamespaces { get; set; }
    ClusterStatus Status { get; set; }
    event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;
    IKubernetes? Client { get; set; }
    IResourceConfig GetResourceConfig(GroupApiVersionKind kind);
    IResourceConfig GetResourceConfig<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    IReadOnlyList<V1Namespace> Namespaces { get; set; }
    K8SConfiguration KubeConfig { get; set; }
    ModelCache ModelCache { get; set; }
    ObservableCollection<NavigationItem> NavigationItems { get; set; }
    ObservableCollection<NodeMetrics> NodeMetrics { get; set; }
    ObservableCollection<PodMetrics> PodMetrics { get; set; }
    ObservableCollection<PortForwarder> PortForwarders { get; set; }
    ObservableCollection<V1Namespace> SelectedNamespaces { get; }
    PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort);
    PortForwarder AddServicePortForward(string @namespace, string serviceName, int servicePort);
    string KubeConfigPath { get; set; }
    string Name { get; set; }
    bool IsExpanded { get; set; }

    Task AddOrUpdateResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task Connect();
    Task DeleteResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task ImportFolder(string path);
    Task ImportYaml(Stream stream);
    Task SeedResource<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task UpdatePermissionsAllNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task<bool> IsResourceReady<T>(CancellationToken? token = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task<bool> UpdateCanI(Type type, Verb verb, string? @namespace = null, string? subresource = null);
    Task<bool> UpdateCanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task<bool> UpdateCanIAnyNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    T? GetResource<T>(string? @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    void RemovePortForward(PortForwarder pf);
    ISourceCache<T, string> GetResourceSourceCache<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    IObservable<int> GetResourceCount(Type type);
    IObservable<int> GetResourceCount<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
}
