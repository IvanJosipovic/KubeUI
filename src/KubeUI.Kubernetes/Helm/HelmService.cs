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
    public Task<IReadOnlyList<HelmRepository>> ListRepositoriesAsync(HelmConnection connection, CancellationToken cancellationToken = default)
        => InvokeAsync(connection, static client => client.ListRepositories(), cancellationToken);

    /// <inheritdoc />
    public Task AddRepositoryAsync(HelmConnection connection, string name, string url, string? username = null, string? password = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        return InvokeVoidAsync(
            connection,
            client => client.AddRepository(name, url, username, password),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task RemoveRepositoryAsync(HelmConnection connection, string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return InvokeVoidAsync(connection, client => client.RemoveRepository(name), cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateRepositoriesAsync(HelmConnection connection, CancellationToken cancellationToken = default)
        => InvokeVoidAsync(connection, static client => client.UpdateRepositories(), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<HelmRelease>> ListReleasesAsync(HelmConnection connection, bool allNamespaces = true, string? filter = null, CancellationToken cancellationToken = default)
        => InvokeAsync(connection, client => client.ListReleases(allNamespaces, filter), cancellationToken);

    /// <inheritdoc />
    public Task<HelmRelease> StatusAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        return InvokeAsync(connection, client => client.Status(releaseName), cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<HelmRelease>> HistoryAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        return InvokeAsync(connection, client => client.History(releaseName), cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<string, JsonElement>> GetValuesAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        return InvokeAsync(connection, client => client.GetValues(releaseName), cancellationToken);
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
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(chart);

        return InvokeAsync(
            connection,
            client => client.Install(releaseName, chart, valuesFiles: null, values, createNamespace, wait, timeout),
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
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(chart);

        return InvokeAsync(
            connection,
            client => client.Upgrade(releaseName, chart, valuesFiles: null, values, installIfMissing, createNamespace, wait, timeout),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task RollbackAsync(HelmConnection connection, string releaseName, int revision = 0, CancellationToken cancellationToken = default, bool wait = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        return InvokeVoidAsync(connection, client => client.Rollback(releaseName, revision, wait), cancellationToken);
    }

    /// <inheritdoc />
    public Task UninstallAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default, bool wait = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        return InvokeVoidAsync(connection, client => client.Uninstall(releaseName, wait), cancellationToken);
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

        return InvokeAsync(connection, client => client.Template(releaseName, chart, valuesFiles: null, values), cancellationToken);
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
        cancellationToken.ThrowIfCancellationRequested();

        // Client construction is cheap and stays outside the native-call gate. The native operation itself
        // is serialized because Helm repository configuration and the Go shim are process-wide.
        var client = _clientFactory.Create(connection);

        await _nativeGate.WaitAsync(cancellationToken).ConfigureAwait(false);
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

    /// <inheritdoc />
    public void Dispose() => _nativeGate.Dispose();
}
