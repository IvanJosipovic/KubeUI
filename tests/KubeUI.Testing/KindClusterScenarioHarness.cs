using System.Text;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Testing;

public sealed class KindClusterScenarioHarness : IClusterScenarioHarness
{
    private readonly ServiceProvider _services;
    private readonly KubernetesTestSettingsStore _settingsStore = new();
    private readonly string _name = Guid.NewGuid().ToString("N");
    private readonly string _kubeConfigPath = Path.Combine(Path.GetTempPath(), $"kubeui-kind-{Guid.NewGuid():N}.yaml");
    private readonly List<KubeUI.Kubernetes.Cluster> _connectedClusters = [];
    private bool _clusterCreated;
    private int _disposeStarted;

    public KindClusterScenarioHarness()
    {
        _services = KubernetesTestServices.Build(_settingsStore);
        _services.ConfigureKubeUIKubernetesJsonLogging();
    }

    public IClusterRuntime Cluster { get; private set; } = null!;

    public K8SConfiguration KubeConfig { get; private set; } = null!;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await Kind.DownloadClient(cancellationToken).ConfigureAwait(false);
        await Kind.CreateCluster(_name, kubeConfigPath: _kubeConfigPath, cancellationToken: cancellationToken).ConfigureAwait(false);
        _clusterCreated = true;

        KubeConfig = await Kind.GetK8SConfiguration(_name, cancellationToken).ConfigureAwait(false);
        Cluster = await CreateClusterAsync(
            $"kind-{_name}",
            KubeConfig,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<T> CreateDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        ArgumentNullException.ThrowIfNull(item);
        using var client = Cluster.Client!.GetGenericClient<T>();

        if (string.IsNullOrEmpty(item.Namespace()))
        {
            return await client.CreateAsync<T>(item, cancellationToken).ConfigureAwait(false);
        }

        return await client.CreateNamespacedAsync<T>(item, item.Namespace(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<T> ReplaceDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        ArgumentNullException.ThrowIfNull(item);
        using var client = Cluster.Client!.GetGenericClient<T>();
        T current = string.IsNullOrEmpty(item.Namespace())
            ? await client.ReadAsync<T>(item.Name(), cancellationToken).ConfigureAwait(false)
            : await client.ReadNamespacedAsync<T>(item.Namespace(), item.Name(), cancellationToken).ConfigureAwait(false);
        item.Metadata.ResourceVersion = current.Metadata.ResourceVersion;
        return string.IsNullOrEmpty(item.Namespace())
            ? await client.ReplaceAsync<T>(item, item.Name(), cancellationToken).ConfigureAwait(false)
            : await client.ReplaceNamespacedAsync<T>(item, item.Namespace(), item.Name(), cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        ArgumentNullException.ThrowIfNull(item);
        using var client = Cluster.Client!.GetGenericClient<T>();
        if (string.IsNullOrEmpty(item.Namespace()))
            await client.DeleteAsync<T>(item.Name(), cancellationToken).ConfigureAwait(false);
        else
            await client.DeleteNamespacedAsync<T>(item.Namespace(), item.Name(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<IClusterRuntime> CreateLimitedAccessClusterAsync(LimitedAccessScenario scenario, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        await Cluster.ImportYaml(new MemoryStream(Encoding.UTF8.GetBytes(scenario.Yaml))).WaitAsync(cancellationToken).ConfigureAwait(false);

        var config = KubeUI.Kubernetes.Serialization.KubernetesYaml.Deserialize<K8SConfiguration>(KubeUI.Kubernetes.Serialization.KubernetesYaml.Serialize(KubeConfig))
            ?? throw new InvalidOperationException("Unable to clone the Kind kubeconfig.");
        var clusterName = scenario.FallbackNamespaces is null ? "limited" : "limited-fallback";
        var token = await CreateServiceAccountTokenAsync("my-app", "my-serviceaccount", cancellationToken).ConfigureAwait(false);

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
            scenario.FallbackNamespaces,
            cancellationToken).ConfigureAwait(false);

        return limited;
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
        {
            return;
        }

        try
        {
            foreach (var cluster in _connectedClusters)
            {
                try
                {
                    await cluster.Disconnect().ConfigureAwait(false);
                }
                catch (InvalidOperationException)
                {
                }
            }
        }
        finally
        {
            try
            {
                await DeleteClusterAsync().ConfigureAwait(false);
            }
            finally
            {
                try
                {
                    await _services.DisposeAsync().ConfigureAwait(false);
                }
                finally
                {
                    try
                    {
                        File.Delete(_kubeConfigPath);
                    }
                    catch (IOException exception)
                    {
                        await Console.Error.WriteLineAsync($"Unable to delete Kind kubeconfig '{_kubeConfigPath}': {exception.Message}").ConfigureAwait(false);
                    }
                    catch (UnauthorizedAccessException exception)
                    {
                        await Console.Error.WriteLineAsync($"Unable to delete Kind kubeconfig '{_kubeConfigPath}': {exception.Message}").ConfigureAwait(false);
                    }

                }
            }
        }
    }

    private async Task DeleteClusterAsync()
    {
        if (!_clusterCreated)
        {
            return;
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        Exception? lastException = null;

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                await Kind.DeleteCluster(_name, timeout.Token).ConfigureAwait(false);
                _clusterCreated = false;
                return;
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested)
            {
                lastException = new TimeoutException($"Timed out deleting Kind cluster '{_name}'.");
                break;
            }
            catch (InvalidOperationException exception) when (attempt < 3)
            {
                lastException = exception;
                await TestWait.NextPollAsync(TimeSpan.FromSeconds(attempt), timeout.Token).ConfigureAwait(false);
            }
            catch (InvalidOperationException exception)
            {
                lastException = exception;
            }
        }

        await Console.Error.WriteLineAsync($"Unable to delete Kind cluster '{_name}' after retries: {lastException}").ConfigureAwait(false);
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

        var client = (k8s.Kubernetes)(Cluster.Client ?? throw new InvalidOperationException("The root cluster is not connected."));
        var response = await client.CoreV1.CreateNamespacedServiceAccountTokenAsync(tokenRequest, name, @namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
        var token = response.Status?.Token;

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException($"Unable to create a service account token for '{@namespace}/{name}'.");
        }

        return token;
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
        if (!cluster.Connected)
        {
            throw new InvalidOperationException(cluster.LastError ?? $"The Kind Kubernetes cluster '{name}' did not connect.");
        }

        return cluster;
    }

}

