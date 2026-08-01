using System.Reflection;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Testing.Kubernetes.Bootstrap;

public sealed class TestCluster : IDisposable, IAsyncDisposable
{
    private readonly Func<CancellationToken, Task>? _cleanup;
    private readonly FakeKubernetesHttpApi? _fakeApi;
    private readonly IServiceProvider _services;
    private readonly List<KubeUI.Kubernetes.Cluster> _additionalClusters = [];
    private IClusterRuntimeCatalog? _runtimeCatalog;
    private int _disposed;

    internal TestCluster(
        IKubernetes client,
        K8SConfiguration kubeConfig,
        KubernetesClientConfiguration clientConfiguration,
        KubeUI.Kubernetes.Cluster cluster,
        IServiceProvider services,
        Func<CancellationToken, Task>? cleanup = null,
        FakeKubernetesHttpApi? fakeApi = null)
    {
        Client = client;
        KubeConfig = kubeConfig;
        ClientConfiguration = clientConfiguration;
        Cluster = cluster;
        _services = services;
        _cleanup = cleanup;
        _fakeApi = fakeApi;
    }

    public IKubernetes Client { get; }

    public K8SConfiguration KubeConfig { get; }

    public KubernetesClientConfiguration ClientConfiguration { get; }

    public KubeUI.Kubernetes.Cluster Cluster { get; }

    public int AuthorizationRequestCount => _fakeApi?.AuthorizationRequestCount ?? 0;

    public FakeKubernetesHttpApi? FakeApi => _fakeApi;

    public void RegisterWith(IClusterRuntimeCatalog runtimeCatalog)
    {
        ArgumentNullException.ThrowIfNull(runtimeCatalog);

        if (_disposed != 0)
        {
            throw new ObjectDisposedException(nameof(TestCluster));
        }

        runtimeCatalog.AddCluster(Cluster);
        _runtimeCatalog = runtimeCatalog;
    }

    public async Task<IClusterRuntime> CreateLimitedAccessAsync(
        string yaml,
        bool useNamespaceFallback = true,
        CancellationToken cancellationToken = default)
    {
        if (_fakeApi is not null)
        {
            _fakeApi.AddYaml(yaml);

            const string name = "http-limited";
            K8SConfiguration kubeConfig = CloneKubeConfig(name);
            KubernetesClientConfiguration configuration = new() { Host = "http://fake-kubernetes" };
            IKubernetes client = new k8s.Kubernetes(
                configuration,
                _fakeApi.CreateClient(KubernetesRbac.ServiceAccountUser, useRoleBasedAuthorization: true));
            return await CreateAdditionalClusterAsync(client, kubeConfig, name, useNamespaceFallback, cancellationToken).ConfigureAwait(false);
        }

        await ApplyYamlAsync(Client, yaml, cancellationToken).ConfigureAwait(false);
        const string kindName = "http-limited";
        K8SConfiguration kindKubeConfig = CloneKubeConfig(kindName);
        Context context = kindKubeConfig.Contexts.First(context => context.Name == kindName);
        User user = kindKubeConfig.Users.First(user => user.Name == context.ContextDetails.User);
        user.UserCredentials.Impersonate = KubernetesRbac.ServiceAccountUser;
        KubernetesClientConfiguration kindConfiguration = KubernetesClientConfiguration.BuildConfigFromConfigObject(
            kindKubeConfig,
            kindName,
            masterUrl: null);
        IKubernetes kindClient = new k8s.Kubernetes(kindConfiguration);
        return await CreateAdditionalClusterAsync(kindClient, kindKubeConfig, kindName, useNamespaceFallback, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IClusterRuntime> CreateAdditionalClusterAsync(
        IKubernetes client,
        K8SConfiguration kubeConfig,
        string name,
        bool useNamespaceFallback,
        CancellationToken cancellationToken)
    {
        KubeUI.Kubernetes.Cluster cluster = _services.GetRequiredService<KubeUI.Kubernetes.Cluster>();
        cluster.Name = name;
        cluster.KubeConfig = kubeConfig;
        cluster.KubeConfigPath = string.Empty;
        cluster.KubernetesClientFactory = _ => client;
        _additionalClusters.Add(cluster);

        if (_services.GetService<IClusterSettingsStore>() is KubernetesTestSettingsStore settings)
        {
            settings.SetClusterNamespaces(cluster, "my-app");
        }

        await cluster.Connect().WaitAsync(cancellationToken).ConfigureAwait(false);
        return cluster;
    }

    private K8SConfiguration CloneKubeConfig(string name)
    {
        K8SConfiguration kubeConfig = KubeUI.Kubernetes.Serialization.KubernetesYaml.Deserialize<K8SConfiguration>(
            KubeUI.Kubernetes.Serialization.KubernetesYaml.Serialize(KubeConfig))
            ?? throw new InvalidOperationException("Unable to clone the kubeconfig.");
        string currentContext = kubeConfig.CurrentContext
            ?? throw new InvalidOperationException("The kubeconfig has no current context.");
        Context context = kubeConfig.Contexts.First(item => item.Name == currentContext);
        string currentUser = context.ContextDetails.User;
        User user = kubeConfig.Users.First(item => item.Name == currentUser);

        kubeConfig.CurrentContext = name;
        context.Name = name;
        context.ContextDetails.User = name;
        user.Name = name;
        if (_fakeApi is not null)
        {
            user.UserCredentials.Token = "fake-token";
        }

        return kubeConfig;
    }

    private static async Task ApplyYamlAsync(IKubernetes client, string yaml, CancellationToken cancellationToken)
    {
        MethodInfo createMethod = typeof(TestCluster)
            .GetMethod(nameof(CreateResourceAsync), BindingFlags.Static | BindingFlags.NonPublic)!;

        foreach (IKubernetesObject resource in KubeUI.Kubernetes.Serialization.KubernetesYaml.LoadAllFromString(yaml).Cast<IKubernetesObject>())
        {
            Task createTask = (Task)createMethod
                .MakeGenericMethod(resource.GetType())
                .Invoke(null, [client, resource, cancellationToken])!;
            await createTask.ConfigureAwait(false);
        }
    }

    private static async Task CreateResourceAsync<T>(
        IKubernetes client,
        T resource,
        CancellationToken cancellationToken)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var genericClient = client.GetGenericClient<T>();
        if (string.IsNullOrEmpty(resource.Namespace()))
        {
            await genericClient.CreateAsync(resource, cancellationToken).ConfigureAwait(false);
            return;
        }

        await genericClient.CreateNamespacedAsync(resource, resource.Namespace(), cancellationToken).ConfigureAwait(false);
    }

    public Task<T> CreateAsync<T>(T item, CancellationToken cancellationToken = default)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        ArgumentNullException.ThrowIfNull(item);
        using var client = Cluster.Client!.GetGenericClient<T>();
        return string.IsNullOrEmpty(item.Namespace())
            ? client.CreateAsync(item, cancellationToken)
            : client.CreateNamespacedAsync(item, item.Namespace(), cancellationToken);
    }

    public async Task<T> ReplaceAsync<T>(T item, CancellationToken cancellationToken = default)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        ArgumentNullException.ThrowIfNull(item);
        using var client = Cluster.Client!.GetGenericClient<T>();
        T current = string.IsNullOrEmpty(item.Namespace())
            ? await client.ReadAsync<T>(item.Name(), cancellationToken).ConfigureAwait(false)
            : await client.ReadNamespacedAsync<T>(item.Namespace(), item.Name(), cancellationToken).ConfigureAwait(false);
        item.Metadata.ResourceVersion = current.Metadata.ResourceVersion;
        return string.IsNullOrEmpty(item.Namespace())
            ? await client.ReplaceAsync(item, item.Name(), cancellationToken).ConfigureAwait(false)
            : await client.ReplaceNamespacedAsync(item, item.Namespace(), item.Name(), cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync<T>(T item, CancellationToken cancellationToken = default)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        ArgumentNullException.ThrowIfNull(item);
        using var client = Cluster.Client!.GetGenericClient<T>();
        if (string.IsNullOrEmpty(item.Namespace()))
        {
            await client.DeleteAsync<T>(item.Name(), cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await client.DeleteNamespacedAsync<T>(item.Namespace(), item.Name(), cancellationToken).ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _runtimeCatalog?.RemoveCluster(Cluster);
        foreach (KubeUI.Kubernetes.Cluster cluster in _additionalClusters)
        {
            await cluster.Disconnect().ConfigureAwait(false);
        }
        await Cluster.DisposeAsync().ConfigureAwait(false);
        if (Client is IDisposable client)
        {
            client.Dispose();
        }
        if (_cleanup is not null)
        {
            await _cleanup(CancellationToken.None).ConfigureAwait(false);
        }
    }
}
