using System.Text;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubernetesClient.Informer.Client;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Testing;

public sealed class FakeClusterScenarioHarness : IClusterScenarioHarness
{
    private const string LimitedNamespace = "my-app";
    private const string LimitedServiceAccountName = "my-serviceaccount";

    private readonly ServiceProvider _services;
    private readonly KubernetesTestSettingsStore _settingsStore = new();
    private readonly FakeKubernetesHttpApi _api = new();
    private readonly List<FakeKubernetesHttpApi> _additionalApis = [];
    private readonly List<KubeUI.Kubernetes.Cluster> _connectedClusters = [];
    private int _disposeStarted;

    public FakeClusterScenarioHarness()
    {
        RegisterSupportedResources(_api);

        _services = KubernetesTestServices.Build(_settingsStore);
        _services.ConfigureKubeUIKubernetesJsonLogging();
    }

    public IClusterRuntime Cluster { get; private set; } = null!;

    public int AuthorizationRequestCount => _api.AuthorizationRequestCount;

    public bool DefaultPermissionAllowed
    {
        get => _api.DefaultPermissionAllowed;
        set => _api.DefaultPermissionAllowed = value;
    }

    public bool FailConnection
    {
        get => _api.FailConnection;
        set => _api.FailConnection = value;
    }

    public TimeSpan ResponseDelay
    {
        get => _api.ResponseDelay;
        set => _api.ResponseDelay = value;
    }

    public bool SupportsLimitedAccessScenarios => true;

    public void SetPermission<T>(Verb verb, bool allowed, string? @namespace = null, string? subresource = null)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var api = GroupApiVersionKind.From<T>();
        _api.SetPermission(api.PluralName, verb.ToString().ToLowerInvariant(), allowed, @namespace, subresource);
    }

    public void AddInitialResource<T>(T resource)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
        => _api.Add(resource);

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var configuredDefaultPermission = _api.DefaultPermissionAllowed;
        // A scenario may intentionally deny namespace discovery, but the
        // harness still needs to bootstrap the cluster before that policy is
        // applied. Tests can refresh the specific authorization entries they
        // are exercising after initialization.
        _api.DefaultPermissionAllowed = true;
        Cluster = CreateCluster(_api, "http-mock");
        await Cluster.Connect().WaitAsync(cancellationToken).ConfigureAwait(false);
        if (Cluster is KubeUI.Kubernetes.Cluster connected && !connected.Connected)
        {
            throw new InvalidOperationException(connected.LastError ?? "The fake Kubernetes cluster did not connect.");
        }

        _api.DefaultPermissionAllowed = configuredDefaultPermission;

        if (Cluster is KubeUI.Kubernetes.Cluster connectedCluster)
        {
            foreach (var type in new[] { typeof(V1Namespace), typeof(V1Node), typeof(V1Secret), typeof(V1Service), typeof(V1EndpointSlice), typeof(V1ConfigMap), typeof(Corev1Event), typeof(V1Pod), typeof(V1Deployment), typeof(V1ServiceAccount), typeof(V1CronJob), typeof(V1Job), typeof(V1CustomResourceDefinition) })
            {
                foreach (var verb in Enum.GetValues<Verb>())
                {
                    await connectedCluster.UpdateCanI(type, verb);
                }
            }

            await connectedCluster.UpdateCanI(typeof(V1Pod), Verb.Get, subresource: "log");
            await connectedCluster.UpdateCanI(typeof(V1Pod), Verb.Create, subresource: "exec");
            await connectedCluster.UpdateCanI(typeof(V1Pod), Verb.Create, subresource: "portforward");
        }


        await Cluster.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "default" },
        });
        await Cluster.AddOrUpdateResource(new V1Node
        {
            Metadata = new V1ObjectMeta { Name = "node-1" },
        });

    }

    public void InitializeDisconnected()
    {
        Cluster = CreateCluster(_api, "http-mock");
        if (Cluster is KubeUI.Kubernetes.Cluster cluster)
        {
            cluster.Connected = false;
            cluster.Status = ClusterStatus.None;
        }
    }

    public async Task<T> CreateDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var client = Cluster.Client!.GetGenericClient<T>();
        return string.IsNullOrEmpty(item.Namespace())
            ? await client.CreateAsync<T>(item, cancellationToken)
            : await client.CreateNamespacedAsync<T>(item, item.Namespace(), cancellationToken);
    }

    public async Task<T> ReplaceDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var client = Cluster.Client!.GetGenericClient<T>();
        return string.IsNullOrEmpty(item.Namespace())
            ? await client.ReplaceAsync<T>(item, item.Name(), cancellationToken)
            : await client.ReplaceNamespacedAsync<T>(item, item.Namespace(), item.Name(), cancellationToken);
    }

    public async Task DeleteDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var client = Cluster.Client!.GetGenericClient<T>();
        if (string.IsNullOrEmpty(item.Namespace()))
            await client.DeleteAsync<T>(item.Name(), cancellationToken);
        else
            await client.DeleteNamespacedAsync<T>(item.Namespace(), item.Name(), cancellationToken);
    }

    public async Task CreateCustomResourceDefinitionAsync(V1CustomResourceDefinition crd, CancellationToken cancellationToken = default)
    {
        var client = (k8s.Kubernetes)Cluster.Client!;
        await client.ApiextensionsV1.CreateCustomResourceDefinitionAsync(crd, cancellationToken: cancellationToken);
    }

    public async Task<IClusterRuntime> CreateLimitedAccessClusterAsync(LimitedAccessScenario scenario, CancellationToken cancellationToken = default)
    {
        await EnsureLimitedAccessResourcesAsync(scenario, cancellationToken);

        var api = new FakeKubernetesHttpApi
        {
            DefaultPermissionAllowed = false,
            UseRoleBasedAuthorization = true,
            AuthenticatedUser = $"system:serviceaccount:{LimitedNamespace}:{LimitedServiceAccountName}",
        };
        _additionalApis.Add(api);
        RegisterSupportedResources(api);
        api.AddYaml(scenario.Yaml);
        api.Add(new V1Node { Metadata = new V1ObjectMeta { Name = "node-1" } });

        var name = scenario.FallbackNamespaces is null ? "http-limited" : "http-limited-fallback";
        var limited = CreateCluster(api, name);
        if (scenario.FallbackNamespaces is not null)
        {
            _settingsStore.SetClusterNamespaces(limited, [.. scenario.FallbackNamespaces]);
        }

        await limited.Connect().WaitAsync(cancellationToken);
        await PrimePermissionsAsync(limited, cancellationToken);
        await limited.SeedResource<V1Node>(true).WaitAsync(cancellationToken);
        await limited.SeedResource<V1Secret>(true).WaitAsync(cancellationToken);
        return limited;
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
        {
            return;
        }

        _api.Shutdown();
        foreach (var api in _additionalApis)
        {
            api.Shutdown();
        }

        Task[] disconnectTasks = _connectedClusters
            .Select(cluster => DisconnectAsync(cluster))
            .ToArray();

        await Task.WhenAll(disconnectTasks);
        await _services.DisposeAsync();
    }

    private static async Task DisconnectAsync(KubeUI.Kubernetes.Cluster cluster)
    {
        try
        {
            await cluster.Disconnect();
        }
        catch
        {
        }
    }

    private KubeUI.Kubernetes.Cluster CreateCluster(FakeKubernetesHttpApi api, string name)
    {
        var cluster = _services.GetRequiredService<KubeUI.Kubernetes.Cluster>();
        _connectedClusters.Add(cluster);
        cluster.Name = name;
        cluster.KubeConfig = CreateKubeConfig(name);
        cluster.KubeConfigPath = string.Empty;
        cluster.KubernetesClientFactory = configuration => new k8s.Kubernetes(configuration, api);
        return cluster;
    }

    private async Task EnsureLimitedAccessResourcesAsync(LimitedAccessScenario scenario, CancellationToken cancellationToken)
    {
        await Cluster.ImportYaml(new MemoryStream(Encoding.UTF8.GetBytes(scenario.Yaml))).WaitAsync(cancellationToken);
        await Cluster.SeedResource<V1ServiceAccount>(true).WaitAsync(cancellationToken);
    }

    private static async Task PrimePermissionsAsync(KubeUI.Kubernetes.Cluster cluster, CancellationToken cancellationToken)
    {
        foreach (var type in new[] { typeof(V1Namespace), typeof(V1Node), typeof(V1Secret), typeof(V1Service), typeof(V1EndpointSlice), typeof(V1ConfigMap), typeof(Corev1Event), typeof(V1Pod), typeof(V1Deployment), typeof(V1ServiceAccount), typeof(V1CronJob), typeof(V1Job), typeof(V1CustomResourceDefinition) })
        {
            foreach (var verb in Enum.GetValues<Verb>())
            {
                await cluster.UpdateCanI(type, verb).WaitAsync(cancellationToken);
                if (type != typeof(V1Namespace) && type != typeof(V1Node))
                {
                    await cluster.UpdateCanI(type, verb, LimitedNamespace).WaitAsync(cancellationToken);
                }
            }
        }

        await cluster.UpdateCanI(typeof(V1Pod), Verb.Get, LimitedNamespace, "log").WaitAsync(cancellationToken);
        await cluster.UpdateCanI(typeof(V1Pod), Verb.Create, LimitedNamespace, "exec").WaitAsync(cancellationToken);
        await cluster.UpdateCanI(typeof(V1Pod), Verb.Create, LimitedNamespace, "portforward").WaitAsync(cancellationToken);
    }

    private static void RegisterSupportedResources(FakeKubernetesHttpApi api)
    {
        api.Register<V1Namespace>();
        api.Register<V1Node>();
        api.Register<V1Secret>();
        api.Register<V1Service>();
        api.Register<V1EndpointSlice>();
        api.Register<V1ConfigMap>();
        api.Register<Corev1Event>();
        api.Register<V1Pod>();
        api.Register<V1Deployment>();
        api.Register<V1ServiceAccount>();
        api.Register<V1CronJob>();
        api.Register<V1Job>();
        api.Register<V1CustomResourceDefinition>();
        api.Register<V1Role>();
        api.Register<V1RoleBinding>();
        api.Register<V1ClusterRole>();
        api.Register<V1ClusterRoleBinding>();
    }

    private static K8SConfiguration CreateKubeConfig(string name)
    {
        const string clusterName = "fake-cluster";
        const string userName = "fake-user";
        return new K8SConfiguration
        {
            ApiVersion = "v1",
            Kind = "Config",
            CurrentContext = name,
            Clusters = [new k8s.KubeConfigModels.Cluster { Name = clusterName, ClusterEndpoint = new ClusterEndpoint { Server = "http://fake-kubernetes/" } }],
            Users = [new User { Name = userName, UserCredentials = new UserCredentials { Token = "fake-token" } }],
            Contexts = [new Context { Name = name, ContextDetails = new ContextDetails { Cluster = clusterName, User = userName } }],
        };
    }
}
