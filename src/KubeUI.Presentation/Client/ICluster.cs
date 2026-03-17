using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Resources;

namespace KubeUI.Client;

public interface ICluster : IClusterRuntime
{
    bool CanI(Type type, Verb verb, string? @namespace = null, string? subresource = null);
    bool CanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    bool CanIAnyNamespace(Type type, Verb verb, string? subresource = null);
    bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    bool IsMetricsAvailable { get; }
    bool IsResourceNamespaced(Type type);
    bool IsResourceNamespaced<T>();
    bool ListNamespaces { get; set; }
    IResourceConfig GetResourceConfig(GroupApiVersionKind kind);
    IResourceConfig GetResourceConfig<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    ReadOnlyObservableCollection<V1Namespace> Namespaces { get; set; }
    ObservableCollection<NavigationItem> NavigationItems { get; set; }
    ObservableCollection<NodeMetrics> NodeMetrics { get; set; }
    ObservableCollection<PodMetrics> PodMetrics { get; set; }
    ObservableCollection<PortForwarder> PortForwarders { get; set; }
    ObservableCollection<V1Namespace> SelectedNamespaces { get; }
    PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort);
    PortForwarder AddServicePortForward(string @namespace, string serviceName, int servicePort);
    bool IsExpanded { get; set; }
    IBrush ClusterColor { get; set; }
    Task UpdatePermissionsAllNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task<bool> UpdateCanI(Type type, Verb verb, string? @namespace = null, string? subresource = null);
    Task<bool> UpdateCanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task<bool> UpdateCanIAnyNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    void RemovePortForward(PortForwarder pf);
}

