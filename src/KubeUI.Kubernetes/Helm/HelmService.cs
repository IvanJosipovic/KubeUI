using System.Text.Json;
using Helm.NET;

namespace KubeUI.Kubernetes;

/// <summary>
/// Default <see cref="IHelmService"/> that runs Helm's native client off the calling thread and
/// serializes native Helm operations because repository configuration is shared by default and the native shim is synchronous.
/// </summary>
public sealed class HelmService : IHelmService, IDisposable
{
    private readonly IHelmClientFactory _clientFactory;
    private readonly SemaphoreSlim _nativeGate = new(1, 1);
    private readonly CancellationTokenSource _disposeCancellation = new();
    private int _disposed;

    /// <summary>
    /// Initializes a Helm service.
    /// </summary>
    /// <param name="clientFactory">Factory creating the Helm client for each operation.</param>
    public HelmService(IHelmClientFactory clientFactory)
    {
        ArgumentNullException.ThrowIfNull(clientFactory);
        _clientFactory = clientFactory;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<HelmChartSearchResult>> ListChartsAsync(HelmConnection connection, string? repositoryName = null, CancellationToken cancellationToken = default)
    {
        if (repositoryName is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(repositoryName);
        }

        return InvokeAsync(
            connection,
            client =>
            {
                var charts = client.SearchRepo(keyword: string.Empty, versions: true, devel: true);
                if (repositoryName is null)
                {
                    return charts;
                }

                var prefix = $"{repositoryName}/";
                return charts.Where(chart => chart.Reference.StartsWith(prefix, StringComparison.Ordinal)).ToArray();
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> GetManifestAsync(HelmConnection connection, string releaseName, int revision = 0, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        ArgumentOutOfRangeException.ThrowIfNegative(revision);
        return InvokeAsync(
            connection,
            client => client.GetManifest(releaseName, connection.Namespace, revision),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> TemplateVersionAsync(HelmConnection connection, string releaseName, string chart, string version, IReadOnlyDictionary<string, object?>? values = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(chart);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        return InvokeAsync(
            connection,
            client => client.Template(
                name: releaseName,
                chart: chart,
                namespaceName: connection.Namespace,
                setValues: ToJsonElement(values),
                version: version),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<HelmRepository>> ListRepositoriesAsync(HelmConnection connection, CancellationToken cancellationToken = default)
        => InvokeAsync(connection, static client => client.RepoList(), cancellationToken);

    /// <inheritdoc />
    public Task AddRepositoryAsync(HelmConnection connection, string name, string url, string? username = null, string? password = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        return InvokeVoidAsync(
            connection,
            client => client.RepoAdd(name, url, username, password, forceUpdate: true),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task RemoveRepositoryAsync(HelmConnection connection, string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return InvokeVoidAsync(connection, client => client.RepoRemove(name), cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateRepositoriesAsync(HelmConnection connection, CancellationToken cancellationToken = default)
        => InvokeVoidAsync(connection, static client => client.RepoUpdate(), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<HelmRelease>> ListReleasesAsync(HelmConnection connection, bool allNamespaces = true, string? filter = null, CancellationToken cancellationToken = default)
        => InvokeAsync(
            connection,
            client => client.List(
                namespaceName: allNamespaces ? null : connection.Namespace,
                allNamespaces: allNamespaces,
                filter: filter),
            cancellationToken);

    /// <inheritdoc />
    public Task<HelmRelease> StatusAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        return InvokeAsync(connection, client => client.Status(releaseName, connection.Namespace), cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<HelmRelease>> HistoryAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        return InvokeAsync(connection, client => client.History(releaseName, connection.Namespace), cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<string, JsonElement>> GetValuesAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        return InvokeAsync(
            connection,
            client => GetValues(client.GetValues(releaseName, connection.Namespace)),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<HelmRelease> InstallAsync(
        HelmConnection connection,
        string releaseName,
        string chart,
        IReadOnlyDictionary<string, object?>? values = null,
        bool createNamespace = false,
        bool wait = false,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default,
        string? version = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(chart);

        return InvokeAsync(
            connection,
            client => client.Install(
                name: releaseName,
                chart: chart,
                namespaceName: connection.Namespace,
                setValues: ToJsonElement(values),
                createNamespace: createNamespace,
                wait: wait,
                timeout: timeout,
                version: version),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<HelmRelease> UpgradeAsync(
        HelmConnection connection,
        string releaseName,
        string chart,
        IReadOnlyDictionary<string, object?>? values = null,
        bool installIfMissing = false,
        bool createNamespace = false,
        bool wait = false,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default,
        string? version = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(chart);

        return InvokeAsync(
            connection,
            client => client.Upgrade(
                release: releaseName,
                chart: chart,
                namespaceName: connection.Namespace,
                setValues: ToJsonElement(values),
                install: installIfMissing,
                createNamespace: createNamespace,
                wait: wait,
                timeout: timeout,
                version: version),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task RollbackAsync(HelmConnection connection, string releaseName, int revision = 0, CancellationToken cancellationToken = default, bool wait = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        return InvokeVoidAsync(
            connection,
            client => client.Rollback(
                releaseName,
                revision,
                namespaceName: connection.Namespace,
                wait: wait),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task UninstallAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default, bool wait = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        return InvokeVoidAsync(
            connection,
            client => client.Uninstall(releaseName, connection.Namespace, wait),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> TemplateAsync(
        HelmConnection connection,
        string releaseName,
        string chart,
        IReadOnlyDictionary<string, object?>? values = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(chart);

        return InvokeAsync(
            connection,
            client => client.Template(
                name: releaseName,
                chart: chart,
                namespaceName: connection.Namespace,
                setValues: ToJsonElement(values)),
            cancellationToken);
    }

    private static JsonElement? ToJsonElement(IReadOnlyDictionary<string, object?>? values)
    {
        if (values is null)
        {
            return null;
        }

        using var document = JsonDocument.Parse(k8s.KubernetesJson.Serialize(values));
        return document.RootElement.Clone();
    }

    private static IReadOnlyDictionary<string, JsonElement> GetValues(JsonElement values)
    {
        if (values.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Helm release values must be a JSON object.");
        }

        var result = new Dictionary<string, JsonElement>(values.EnumerateObject().Count(), StringComparer.Ordinal);
        foreach (var property in values.EnumerateObject())
        {
            result.Add(property.Name, property.Value.Clone());
        }

        return result;
    }

    private Task InvokeVoidAsync(HelmConnection connection, Action<IHelmClient> operation, CancellationToken cancellationToken)
        => InvokeAsync(
            connection,
            client =>
            {
                operation(client);
                return true;
            },
            cancellationToken);

    private async Task<T> InvokeAsync<T>(HelmConnection connection, Func<IHelmClient, T> operation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(operation);
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        cancellationToken.ThrowIfCancellationRequested();

        // Client construction is cheap and stays outside the native-call gate. The native operation itself
        // is serialized because Helm repository configuration and the Go shim are process-wide.
        var client = _clientFactory.Create(connection);

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _disposeCancellation.Token);
        await _nativeGate.WaitAsync(linkedCancellation.Token).ConfigureAwait(false);
        try
        {
            // Helm's native calls are synchronous and cannot be cancelled once they started.
            return await Task.Run(() => operation(client), CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            _nativeGate.Release();
        }
    }

    /// <summary>
    /// Cancels queued operations and waits for the native call that owns the gate.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        // Native Helm calls cannot be cancelled once they start. Reject new operations, cancel queued ones,
        // and wait for the running call so the gate is never disposed under a waiter.
        _disposeCancellation.Cancel();
        _nativeGate.Wait();
        _nativeGate.Release();
        _nativeGate.Dispose();
        _disposeCancellation.Dispose();
    }
}
