using Avalonia.Collections;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubeUI.Client.Informer;
using KubeUI.Resources;
using static KubeUI.Client.Cluster;

namespace KubeUI.Client;

public interface ICluster
{
    bool CanI(Type type, Verb verb, string @namespace = "", string subresource = "");
    bool CanI<T>(Verb verb, string @namespace = "", string subresource = "") where T : class, IKubernetesObject<V1ObjectMeta>, new();
    bool CanIAnyNamespace(Type type, Verb verb, string subresource = "");
    bool CanIAnyNamespace<T>(Verb verb, string subresource = "") where T : class, IKubernetesObject<V1ObjectMeta>, new();
    bool Connected { get; set; }
    bool IsMetricsAvailable { get; }
    bool IsNamespaced(Type type);
    bool IsNamespaced<T>();
    bool ListNamespaces { get; set; }
    AvaloniaDictionary<GroupApiVersionKind, ContainerClass> Objects { get; }
    AvaloniaDictionary<NamespacedName, T> GetObjectDictionary<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    AvaloniaDictionary<NamespacedName, V1Namespace> Namespaces { get; set; }
    event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;
    IKubernetes? Client { get; set; }
    IResourceConfig GetResourceConfig(GroupApiVersionKind kind);
    ModelCache ModelCache { get; set; }
    ObservableCollection<NavigationItem> NavigationItems { get; set; }
    ObservableCollection<NodeMetrics> NodeMetrics { get; set; }
    ObservableCollection<PodMetrics> PodMetrics { get; set; }
    ObservableCollection<PortForwarder> PortForwarders { get; set; }
    ObservableCollection<V1Namespace> SelectedNamespaces { get; set; }
    PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort);
    PortForwarder AddServicePortForward(string @namespace, string serviceName, int servicePort);
    string KubeConfigPath { get; set; }
    string Name { get; set; }
    K8SConfiguration KubeConfig { get; set; }

    Task AddOrUpdate<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task Connect();
    Task Delete<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task ImportFolder(string path);
    Task ImportYaml(Stream stream);
    Task Seed<T>(bool waitForReady = false) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task<bool> UpdateCanIAnyNamespaceAsync(Type type, Verb verb, string subresource = "");
    Task<bool> UpdateCanIAnyNamespaceAsync<T>(Verb verb, string subresource = "") where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task<AvaloniaDictionary<NamespacedName, T>> GetObjectDictionaryAsync<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task<T?> GetObjectAsync<T>(string @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    void RemovePortForward(PortForwarder pf);
}
