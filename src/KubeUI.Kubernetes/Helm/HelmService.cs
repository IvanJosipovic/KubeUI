using System.Text.Json;
using System.Threading;
using Helm.NET;

namespace KubeUI.Kubernetes;

/// <summary>
/// Default <see cref="IHelmService"/> that runs Helm's native client off the calling thread.
/// Uses a reader-writer lock to allow concurrent read operations while serializing write operations,
/// because Helm repository configuration and the Go shim are process-wide.
/// </summary>
public sealed class HelmService : IHelmService, IDisposable
{
    private readonly IHelmClientFactory _clientFactory;
    private readonly ReaderWriterLockSlim _nativeGate = new(LockRecursionPolicy.NoRecursion);
    private readonly CancellationTokenSource _disposeCancellation = new();
    private int _disposed;

    // Timeout for waiting on the native gate during disposal
    private static readonly TimeSpan DisposeGateTimeout = TimeSpan.FromSeconds(30);

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
            cancellationToken,
            isWriteOperation: false);
    }

    /// <inheritdoc />
    public Task<string> GetChartValuesAsync(HelmConnection connection, string chart, string? version = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chart);
        return InvokeAsync(
            connection,
            client => client.ShowValues(chart, version),
            cancellationToken,
            isWriteOperation: false);
    }

    /// <inheritdoc />
    public Task<string> GetManifestAsync(HelmConnection connection, string releaseName, int revision = 0, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        ArgumentOutOfRangeException.ThrowIfNegative(revision);
        return InvokeAsync(
            connection,
            client => client.GetManifest(releaseName, connection.Namespace, revision),
            cancellationToken,
            isWriteOperation: false);
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
            cancellationToken,
            isWriteOperation: false);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<HelmRepository>> ListRepositoriesAsync(HelmConnection connection, CancellationToken cancellationToken = default)
        => InvokeAsync(connection, static client => client.RepoList(), cancellationToken, isWriteOperation: false);

    /// <inheritdoc />
    public Task AddRepositoryAsync(HelmConnection connection, string name, string url, string? username = null, string? password = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        return InvokeVoidAsync(
            connection,
            client => client.RepoAdd(name, url, username, password, forceUpdate: true),
            cancellationToken,
            isWriteOperation: true);
    }

    /// <inheritdoc />
    public Task RemoveRepositoryAsync(HelmConnection connection, string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return InvokeVoidAsync(connection, client => client.RepoRemove(name), cancellationToken, isWriteOperation: true);
    }

    /// <inheritdoc />
    public Task UpdateRepositoriesAsync(HelmConnection connection, CancellationToken cancellationToken = default)
        => InvokeVoidAsync(connection, static client => client.RepoUpdate(), cancellationToken, isWriteOperation: true);

    /// <inheritdoc />
    public Task<IReadOnlyList<HelmRelease>> ListReleasesAsync(HelmConnection connection, bool allNamespaces = true, string? filter = null, CancellationToken cancellationToken = default)
        => InvokeAsync(
            connection,
            client => client.List(
                namespaceName: allNamespaces ? null : connection.Namespace,
                allNamespaces: allNamespaces,
                filter: filter),
            cancellationToken,
            isWriteOperation: false);

    /// <inheritdoc />
    public Task<HelmRelease> StatusAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        return InvokeAsync(connection, client => client.Status(releaseName, connection.Namespace), cancellationToken, isWriteOperation: false);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<HelmRelease>> HistoryAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        return InvokeAsync(connection, client => client.History(releaseName, connection.Namespace), cancellationToken, isWriteOperation: false);
    }

    /// <inheritdoc />
    public Task<JsonElement> GetValuesAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        return InvokeAsync(
            connection,
            client =>
            {
                var values = client.GetValues(releaseName, connection.Namespace);
                if (values.ValueKind != JsonValueKind.Object)
                {
                    throw new JsonException("Helm release values must be a JSON object.");
                }

                return values.Clone();
            },
            cancellationToken,
            isWriteOperation: false);
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
            cancellationToken,
            isWriteOperation: true);
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
            cancellationToken,
            isWriteOperation: true);
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
            cancellationToken,
            isWriteOperation: true);
    }

    /// <inheritdoc />
    public Task UninstallAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default, bool wait = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        return InvokeVoidAsync(
            connection,
            client => client.Uninstall(releaseName, connection.Namespace, wait),
            cancellationToken,
            isWriteOperation: true);
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
            cancellationToken,
            isWriteOperation: false);
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

    private Task InvokeVoidAsync(HelmConnection connection, Action<IHelmClient> operation, CancellationToken cancellationToken, bool isWriteOperation = false)
        => InvokeAsync(
            connection,
            client =>
            {
                operation(client);
                return true;
            },
            cancellationToken,
            isWriteOperation);

    private Task<T> InvokeAsync<T>(HelmConnection connection, Func<IHelmClient, T> operation, CancellationToken cancellationToken, bool isWriteOperation = false)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(operation);
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        cancellationToken.ThrowIfCancellationRequested();

        // Client construction is cheap and stays outside the native-call gate.
        var client = _clientFactory.Create(connection);

        var disposeToken = _disposeCancellation.Token;
        return Task.Run(() =>
        {
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, disposeToken);
            var token = linkedCancellation.Token;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

                var entered = isWriteOperation
                    ? _nativeGate.TryEnterWriteLock(TimeSpan.FromMilliseconds(50))
                    : _nativeGate.TryEnterReadLock(TimeSpan.FromMilliseconds(50));
                if (!entered)
                {
                    continue;
                }

                try
                {
                    token.ThrowIfCancellationRequested();
                    ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
                    return operation(client);
                }
                finally
                {
                    if (isWriteOperation)
                    {
                        _nativeGate.ExitWriteLock();
                    }
                    else
                    {
                        _nativeGate.ExitReadLock();
                    }
                }
            }
        }, CancellationToken.None);
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

        // Wait for active operations to complete. Native Helm calls cannot be cancelled once started.
        var waitStart = DateTime.UtcNow;
        while (true)
        {
            var writerActive = !_nativeGate.TryEnterReadLock(0);
            if (!writerActive)
            {
                _nativeGate.ExitReadLock();
            }

            if (_nativeGate.CurrentReadCount == 0 && _nativeGate.WaitingReadCount == 0 && _nativeGate.WaitingWriteCount == 0 && !writerActive)
            {
                break;
            }

            if (DateTime.UtcNow - waitStart > DisposeGateTimeout)
            {
                // Keep synchronization objects alive if a native call is stuck; disposing them would race the call.
                System.Diagnostics.Debug.WriteLine($"HelmService.Dispose: Native operation still running after {DisposeGateTimeout}.");
                return;
            }
            Thread.Sleep(10);
        }

        _nativeGate.Dispose();
        _disposeCancellation.Dispose();
    }
}
