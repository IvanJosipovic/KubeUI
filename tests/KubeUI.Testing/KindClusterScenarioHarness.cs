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
        Cluster = await CreateClusterAsync($"kind-{_name}", KubeConfig, cancellationToken).ConfigureAwait(false);
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

    public async Task CreateCustomResourceDefinitionAsync(V1CustomResourceDefinition crd, CancellationToken cancellationToken = default)
    {
        await Kubernetes.CreateCustomResourceDefinitionAsync(crd, cancellationToken: cancellationToken);
    }

    public async Task<IClusterRuntime> CreateLimitedAccessClusterAsync(bool includeNamespaceFallback, CancellationToken cancellationToken = default)
    {
        var yaml = includeNamespaceFallback ? SharedScenarioData.LimitedAccessNoNamespaceYaml : SharedScenarioData.LimitedAccessYaml;

        await Cluster.ImportYaml(new MemoryStream(Encoding.UTF8.GetBytes(yaml))).WaitAsync(cancellationToken);
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

        var limited = await CreateClusterAsync(clusterName, config, cancellationToken);

        if (includeNamespaceFallback)
        {
            _settingsStore.SetClusterNamespaces(limited, "my-app");
        }

        return limited;
    }

    public async ValueTask DisposeAsync()
    {
        await _services.DisposeAsync();

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

    private async Task<IClusterRuntime> CreateClusterAsync(string name, K8SConfiguration config, CancellationToken cancellationToken = default)
    {
        var cluster = _services.GetRequiredService<IClusterRuntime>();
        cluster.Name = name;
        cluster.KubeConfig = config;
        cluster.KubeConfigPath = string.Empty;
        await cluster.Connect().WaitAsync(cancellationToken).ConfigureAwait(false);
        return cluster;
    }

}


