using System.Text;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubeUI.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Testing;

public sealed class KindClusterScenarioHarness : IClusterScenarioHarness
{
    private readonly ServiceProvider _services;
    private readonly KubernetesTestSettingsStore _settingsStore = new();
    private string _name = Guid.NewGuid().ToString("N");
    private readonly string _kubeConfigPath = Path.Combine(Path.GetTempPath(), $"kubeui-kind-{Guid.NewGuid():N}.yaml");
    private readonly List<KubeUI.Kubernetes.Cluster> _connectedClusters = [];

    public KindClusterScenarioHarness()
    {
        _services = KubernetesTestServices.Build(_settingsStore);
        _services.ConfigureKubeUIKubernetesJsonLogging();
    }

    public IClusterRuntime Cluster { get; private set; } = null!;

    public k8s.Kubernetes Kubernetes { get; private set; } = null!;

    public K8SConfiguration KubeConfig { get; private set; } = null!;

    public bool SupportsLimitedAccessScenarios => true;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await Kind.DownloadClient(cancellationToken).ConfigureAwait(false);
        await Kind.CreateCluster(_name, kubeConfigPath: _kubeConfigPath, cancellationToken: cancellationToken).ConfigureAwait(false);

        KubeConfig = await Kind.GetK8SConfiguration(_name, cancellationToken).ConfigureAwait(false);
        Kubernetes = await Kind.GetKubernetesClient(_name, cancellationToken).ConfigureAwait(false);
        await WaitForDefaultServiceAccountAsync(cancellationToken).ConfigureAwait(false);
        Cluster = await CreateClusterAsync(
            $"kind-{_name}",
            KubeConfig,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        await PrimePermissionsAsync((KubeUI.Kubernetes.Cluster)Cluster, "default", cancellationToken);
    }

    public async Task<T> CreateDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var client = Kubernetes.GetGenericClient<T>();

        if (string.IsNullOrEmpty(item.Namespace()))
        {
            return await client.CreateAsync<T>(item, cancellationToken);
        }

        return await client.CreateNamespacedAsync<T>(item, item.Namespace(), cancellationToken);
    }

    public async Task<T> ReplaceDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var client = Kubernetes.GetGenericClient<T>();
        T current = string.IsNullOrEmpty(item.Namespace())
            ? await client.ReadAsync<T>(item.Name(), cancellationToken)
            : await client.ReadNamespacedAsync<T>(item.Namespace(), item.Name(), cancellationToken);
        item.Metadata.ResourceVersion = current.Metadata.ResourceVersion;
        return await client.ReplaceAsync<T>(item, item.Name(), cancellationToken);
    }

    public async Task DeleteDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var client = Kubernetes.GetGenericClient<T>();
        if (string.IsNullOrEmpty(item.Namespace()))
            await client.DeleteAsync<T>(item.Name(), cancellationToken);
        else
            await client.DeleteNamespacedAsync<T>(item.Namespace(), item.Name(), cancellationToken);
    }

    public async Task CreateCustomResourceDefinitionAsync(V1CustomResourceDefinition crd, CancellationToken cancellationToken = default)
    {
        await Kubernetes.CreateCustomResourceDefinitionAsync(crd, cancellationToken: cancellationToken);
    }

    public async Task<IClusterRuntime> CreateLimitedAccessClusterAsync(bool includeNamespaceFallback, CancellationToken cancellationToken = default)
    {
        var yaml = includeNamespaceFallback ? SharedScenarioData.LimitedAccessNoNamespaceYaml : SharedScenarioData.LimitedAccessYaml;

        await Cluster.ImportYaml(new MemoryStream(Encoding.UTF8.GetBytes(yaml))).WaitAsync(cancellationToken);
        var rootCluster = (KubeUI.Kubernetes.Cluster)Cluster;
        await rootCluster.UpdateCanI<V1ServiceAccount>(Verb.List).WaitAsync(cancellationToken);
        await rootCluster.UpdateCanI<V1ServiceAccount>(Verb.Watch).WaitAsync(cancellationToken);
        await Cluster.SeedResource<V1ServiceAccount>(true).WaitAsync(cancellationToken);
        await WaitForResourceAsync<V1ServiceAccount>(Cluster, "my-app", "my-serviceaccount", cancellationToken: cancellationToken);

        var config = KubeUI.Kubernetes.Serialization.KubernetesYaml.Deserialize<K8SConfiguration>(KubeUI.Kubernetes.Serialization.KubernetesYaml.Serialize(KubeConfig));
        var clusterName = includeNamespaceFallback ? "limited-fallback" : "limited";
        var token = await CreateServiceAccountTokenAsync("my-app", "my-serviceaccount", cancellationToken);

        config.Clusters.First().Name = clusterName;
        var context = config.Contexts.First();
        context.Name = clusterName;
        context.ContextDetails.Cluster = clusterName;
        context.ContextDetails.User = clusterName;

        var user = config.Users.First();
        user.Name = clusterName;
        user.UserCredentials = new() { Token = token };

        var limited = await CreateClusterAsync(
            clusterName,
            config,
            includeNamespaceFallback ? ["my-app"] : null,
            cancellationToken);
        await limited.Connect().WaitAsync(cancellationToken);
        await PrimePermissionsAsync((KubeUI.Kubernetes.Cluster)limited, "my-app", cancellationToken);

        return limited;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            foreach (var cluster in _connectedClusters)
            {
                try
                {
                    await cluster.Disconnect();
                }
                catch
                {
                }
            }
        }
        finally
        {
            try
            {
                await _services.DisposeAsync();
            }
            finally
            {
                try
                {
                    await Kind.DeleteCluster(_name);
                }
                catch
                {
                }

                try
                {
                    File.Delete(_kubeConfigPath);
                }
                catch
                {
                }
            }
        }
    }

    private async Task<string> CreateServiceAccountTokenAsync(string @namespace, string name, CancellationToken cancellationToken)
    {
        var tokenRequest = new Authenticationv1TokenRequest
        {
            ApiVersion = Authenticationv1TokenRequest.KubeGroup + "/" + Authenticationv1TokenRequest.KubeApiVersion,
            Kind = Authenticationv1TokenRequest.KubeKind,
            Spec = new V1TokenRequestSpec
            {
                ExpirationSeconds = 3600
            }
        };

        var response = await Kubernetes.CoreV1.CreateNamespacedServiceAccountTokenAsync(tokenRequest, name, @namespace, cancellationToken: cancellationToken);
        var token = response.Status?.Token;

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException($"Unable to create a service account token for '{@namespace}/{name}'.");
        }

        return token;
    }

    private static async Task<T?> WaitForResourceAsync<T>(IClusterRuntime cluster, string? @namespace, string name, CancellationToken cancellationToken = default)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            var resource = cluster.GetResource<T>(@namespace, name);
            if (resource is not null)
            {
                return resource;
            }

            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
            await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false);
        }

        throw new TimeoutException($"Resource {typeof(T).Name}/{@namespace}/{name} was not observed.");
    }

    private async Task WaitForDefaultServiceAccountAsync(CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                await Kubernetes.CoreV1.ReadNamespacedServiceAccountAsync(
                    "default",
                    "default",
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (k8s.Autorest.HttpOperationException exception) when (exception.Response?.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
                await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        throw new TimeoutException("The Kind cluster did not create the default/default ServiceAccount.");
    }

    private async Task<IClusterRuntime> CreateClusterAsync(
        string name,
        K8SConfiguration config,
        IReadOnlyCollection<string>? fallbackNamespaces = null,
        CancellationToken cancellationToken = default)
    {
        var cluster = _services.GetRequiredService<IClusterRuntime>();
        if (cluster is KubeUI.Kubernetes.Cluster concreteCluster)
        {
            _connectedClusters.Add(concreteCluster);
            if (fallbackNamespaces is not null)
            {
                _settingsStore.SetClusterNamespaces(concreteCluster, [.. fallbackNamespaces]);
            }
        }

        cluster.Name = name;
        cluster.KubeConfig = config;
        cluster.KubeConfigPath = string.Empty;
        await cluster.Connect().WaitAsync(cancellationToken).ConfigureAwait(false);
        return cluster;
    }

    private static async Task PrimePermissionsAsync(
        KubeUI.Kubernetes.Cluster cluster,
        string @namespace,
        CancellationToken cancellationToken)
    {
        foreach (var type in new[]
        {
            typeof(V1Namespace),
            typeof(V1Node),
            typeof(V1Secret),
            typeof(V1Service),
            typeof(V1EndpointSlice),
            typeof(Corev1Event),
            typeof(V1Pod),
            typeof(V1Deployment),
            typeof(V1ServiceAccount),
            typeof(V1CronJob),
            typeof(V1Job),
            typeof(V1CustomResourceDefinition)
        })
        {
            foreach (var verb in Enum.GetValues<Verb>())
            {
                await cluster.UpdateCanI(type, verb).WaitAsync(cancellationToken);
                if (type != typeof(V1Namespace) && type != typeof(V1Node))
                {
                    await cluster.UpdateCanI(type, verb, @namespace).WaitAsync(cancellationToken);
                }
            }
        }

        await cluster.UpdateCanI(typeof(V1Pod), Verb.Get, subresource: "log").WaitAsync(cancellationToken);
        await cluster.UpdateCanI(typeof(V1Pod), Verb.Create, subresource: "exec").WaitAsync(cancellationToken);
        await cluster.UpdateCanI(typeof(V1Pod), Verb.Create, subresource: "portforward").WaitAsync(cancellationToken);
        await cluster.UpdateCanI(typeof(V1Pod), Verb.Get, @namespace, "log").WaitAsync(cancellationToken);
        await cluster.UpdateCanI(typeof(V1Pod), Verb.Create, @namespace, "exec").WaitAsync(cancellationToken);
        await cluster.UpdateCanI(typeof(V1Pod), Verb.Create, @namespace, "portforward").WaitAsync(cancellationToken);
    }

}


