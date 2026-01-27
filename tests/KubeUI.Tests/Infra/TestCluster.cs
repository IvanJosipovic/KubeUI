using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Collections;
using DynamicData;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubeUI;
using KubeUI.Client;
using KubeUI.Resources;
using KubeUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Kubernetes.Controller.Client;

namespace KubeUI.Tests.Infra;

public class TestCluster : ICluster
{
    public static ICluster Get()
    {
        var cluster = new TestCluster();

        var ns = new V1Namespace()
        {
            Metadata = new() { Name = "default" }
        };

        cluster.AddOrUpdateResource(ns).GetAwaiter().GetResult();

        cluster.SelectedNamespaces.Add(ns);

        return cluster;
    }

    public AvaloniaDictionary<GroupApiVersionKind, object> Objects { get; } = [];

    public bool Connected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool IsMetricsAvailable => false;

    public bool ListNamespaces { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ClusterStatus Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IKubernetes? Client { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IReadOnlyList<V1Namespace> Namespaces { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public K8SConfiguration KubeConfig { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ModelCache ModelCache { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ObservableCollection<NavigationItem> NavigationItems { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ObservableCollection<NodeMetrics> NodeMetrics { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ObservableCollection<PodMetrics> PodMetrics { get; set; } = [];
    public ObservableCollection<PortForwarder> PortForwarders { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ObservableCollection<V1Namespace> SelectedNamespaces { get; } = [];
    public string KubeConfigPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string Name { get; set; } = "test";
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
        // ResourceListViewModel expects a ResourceConfigBase<T> for the requested kind.
        // Use the app DI container to resolve the real config for that resource type.
        if (Application.Current is null)
            throw new InvalidOperationException("Avalonia Application.Current is not initialized.");

        var svc = Application.Current.GetRequiredService<IServiceProvider>();
        if (kind == GroupApiVersionKind.From<V1Pod>())
        {
            var cfg = svc.GetRequiredService<ResourceConfigBase<V1Pod>>();
            cfg.Initialize(this);
            return cfg;
        }

        throw new NotSupportedException($"TestCluster.GetResourceConfig only supports V1Pod in unit tests. Requested: {kind}");
    }

    private sealed class TestResourceConfig : IResourceConfig
    {
        public GroupApiVersionKind Kind { get; }

        public bool IsNamespaced { get; } = true;

        public bool ShowNewResource { get; } = false;

        public int Order { get; } = 0;

        public string Name => Kind.Kind;

        public string? Category { get; } = null;

        public Type Type => typeof(object);

        public TestResourceConfig(GroupApiVersionKind kind)
        {
            Kind = kind;
        }

        public void Initialize(ICluster cluster)
        {
        }

        public IList<IResourceListColumn> Columns() => new List<IResourceListColumn>();

        public IList<ResourceMenuItem> MenuItems() => new List<ResourceMenuItem>();

        public Avalonia.Styling.IStyle ListStyle() => null;

        public Task UpdatePermissions() => Task.CompletedTask;
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
        return true;
    }

    public bool CanIAnyNamespace<T>(Client.Cluster.Verb verb, string? subresource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return true;
    }

    public Task DeleteResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Task.CompletedTask;
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
        var kind = GroupApiVersionKind.From<T>();
        if (!Objects.TryGetValue(kind, out var obj) || obj is not ContainerClass<T> container)
            throw new InvalidOperationException($"Container not seeded for: {kind}");

        return container.Items;
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
