using System.Collections.Concurrent;
using k8s;
using k8s.Models;
using KubeUI.Client.Informer;
using Swordfish.NET.Collections;
using static KubeUI.Client.Cluster;

namespace KubeUI.Client;

public interface ICluster
{
    bool Connected { get; set; }
    ConcurrentDictionary<GroupApiVersionKind, ContainerClass> Objects { get; }
    ConcurrentObservableDictionary<NamespacedName, T> GetObjectDictionary<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    ConcurrentObservableDictionary<NamespacedName, V1Namespace> Namespaces { get; set; }
    event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;
    IKubernetes? Client { get; set; }
    ModelCache ModelCache { get; set; }
    ObservableCollection<NavigationItem> NavigationItems { get; set; }
    ObservableCollection<NodeMetrics> NodeMetrics { get; set; }
    ObservableCollection<PodMetrics> PodMetrics { get; set; }
    ObservableCollection<PortForwarder> PortForwarders { get; set; }
    ObservableCollection<V1Namespace> SelectedNamespaces { get; set; }
    PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort);
    string KubeConfigPath { get; set; }
    string Name { get; set; }
    T? GetObject<T>(string @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task AddOrUpdate<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task Connect();
    Task Delete<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    bool IsMetricsAvailable { get; }
    Task ImportFolder(string path);
    Task ImportYaml(Stream stream);
    void RemovePortForward(PortForwarder pf);
    Task Seed<T>(bool waitForReady = false) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    bool CanI<T>(Verb verb, string @namespace = "", string subresource = "") where T : class, IKubernetesObject<V1ObjectMeta>, new();
    bool CanI(Type type, Verb verb, string @namespace = "", string subresource = "");
    Task<ConcurrentObservableDictionary<NamespacedName, T>> GetObjectDictionaryAsync<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();
    bool CanIAnyNamespace(Type type, Verb verb, string subresource = "");
    Task<T?> GetObjectAsync<T>(string @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    PortForwarder AddServicePortForward(string @namespace, string serviceName, int containerPort);

    bool ListNamespaces { get; set; }
}
