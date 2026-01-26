using System.Collections.ObjectModel;
using Avalonia.Collections;
using DynamicData;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Resources;
using KubeUI.ViewModels;
using Yarp.Kubernetes.Controller.Client;

namespace KubeUI.Tests;

public class TestCluster : ICluster
{
    public AvaloniaDictionary<GroupApiVersionKind, object> Objects { get; } = [];

    public bool Connected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool IsMetricsAvailable => throw new NotImplementedException();

    public bool ListNamespaces { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ClusterStatus Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IKubernetes? Client { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IReadOnlyList<V1Namespace> Namespaces { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public K8SConfiguration KubeConfig { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ModelCache ModelCache { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ObservableCollection<NavigationItem> NavigationItems { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ObservableCollection<NodeMetrics> NodeMetrics { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ObservableCollection<PodMetrics> PodMetrics { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ObservableCollection<PortForwarder> PortForwarders { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ObservableCollection<V1Namespace> SelectedNamespaces { get; } = [];
    public string KubeConfigPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool IsExpanded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;

    public PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort)
    {
        throw new NotImplementedException();
    }

    public PortForwarder AddServicePortForward(string @namespace, string serviceName, int servicePort)
    {
        throw new NotImplementedException();
    }

    public bool CanI(Type type, Client.Cluster.Verb verb, string? @namespace = null, string? subresource = null)
    {
        throw new NotImplementedException();
    }

    public bool CanIAnyNamespace(Type type, Client.Cluster.Verb verb, string? subresource = null)
    {
        throw new NotImplementedException();
    }

    public Task Connect()
    {
        throw new NotImplementedException();
    }

    public IResourceConfig GetResourceConfig(GroupApiVersionKind kind)
    {
        throw new NotImplementedException();
    }

    public IObservable<int> GetResourceCount(Type type)
    {
        throw new NotImplementedException();
    }

    public Task ImportFolder(string path)
    {
        throw new NotImplementedException();
    }

    public Task ImportYaml(Stream stream)
    {
        throw new NotImplementedException();
    }

    public bool IsResourceNamespaced(Type type)
    {
        throw new NotImplementedException();
    }

    public bool IsResourceNamespaced<T>()
    {
        throw new NotImplementedException();
    }

    public void RemovePortForward(PortForwarder pf)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateCanI(Type type, Client.Cluster.Verb verb, string? @namespace = null, string? subresource = null)
    {
        throw new NotImplementedException();
    }

    public async Task AddOrUpdateResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await SeedResource<T>();

        var kind = GroupApiVersionKind.From<T>();

        var container = (ContainerClass<T>)Objects[kind];

        container.Items.AddOrUpdate(item);
    }

    public bool CanI<T>(Client.Cluster.Verb verb, string? @namespace, string? subresource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public bool CanIAnyNamespace<T>(Client.Cluster.Verb verb, string? subresource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public Task DeleteResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public T? GetResource<T>(string? @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public IResourceConfig GetResourceConfig<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public IObservable<int> GetResourceCount<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<T> GetResourceList<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public ISourceCache<T, string> GetResourceSourceCache<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsResourceReady<T>(CancellationToken? token) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public Task SeedResource<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var kind = GroupApiVersionKind.From<T>();

        if (!Objects.TryGetValue(kind, out _))
        {
            var container = new ContainerClass<T>();

            if (!Objects.TryAdd(kind, container))
            {
                throw new Exception($"Duplicate Object Set detected for: {kind}");
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> UpdateCanI<T>(Client.Cluster.Verb verb, string? @namespace, string? subresource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateCanIAnyNamespaceAsync<T>(Client.Cluster.Verb verb, string? subresource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }

    public Task UpdatePermissionsAllNamespaceAsync<T>(Client.Cluster.Verb verb, string? subresource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        throw new NotImplementedException();
    }
}
