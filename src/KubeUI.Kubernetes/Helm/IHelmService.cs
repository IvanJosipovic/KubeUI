using System.Text.Json;
using Helm.NET;

namespace KubeUI.Kubernetes;

/// <summary>
/// Runs Helm operations asynchronously against a Kubernetes connection.
/// </summary>
/// <remarks>
/// Implementations serialize native Helm calls because the underlying shim and default repository configuration are
/// process-wide, and surface Helm failures as <see cref="HelmActionException"/>.
/// </remarks>
public interface IHelmService
{
    /// <summary>
    /// Lists the configured chart repositories.
    /// </summary>
    /// <param name="connection">The Kubernetes connection Helm runs against.</param>
    /// <param name="cancellationToken">Cancels waiting for the operation to start.</param>
    /// <returns>The configured repositories.</returns>
    Task<IReadOnlyList<HelmRepository>> ListRepositoriesAsync(HelmConnection connection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a chart repository, replacing an existing repository with the same name.
    /// </summary>
    /// <param name="connection">The Kubernetes connection Helm runs against.</param>
    /// <param name="name">The repository's local name.</param>
    /// <param name="url">The repository index URL.</param>
    /// <param name="username">Optional basic-auth username.</param>
    /// <param name="password">Optional basic-auth password.</param>
    /// <param name="cancellationToken">Cancels waiting for the operation to start.</param>
    /// <returns>A task that completes when the repository is saved.</returns>
    Task AddRepositoryAsync(HelmConnection connection, string name, string url, string? username = null, string? password = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a configured chart repository.
    /// </summary>
    /// <param name="connection">The Kubernetes connection Helm runs against.</param>
    /// <param name="name">The repository's local name.</param>
    /// <param name="cancellationToken">Cancels waiting for the operation to start.</param>
    /// <returns>A task that completes when the repository is removed.</returns>
    Task RemoveRepositoryAsync(HelmConnection connection, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the latest index for every configured chart repository.
    /// </summary>
    /// <param name="connection">The Kubernetes connection Helm runs against.</param>
    /// <param name="cancellationToken">Cancels waiting for the operation to start.</param>
    /// <returns>A task that completes when the indexes are refreshed.</returns>
    Task UpdateRepositoriesAsync(HelmConnection connection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists releases.
    /// </summary>
    /// <param name="connection">The Kubernetes connection Helm runs against.</param>
    /// <param name="allNamespaces">Search every namespace instead of only the connection namespace.</param>
    /// <param name="filter">Optional substring filter on the release name.</param>
    /// <param name="cancellationToken">Cancels waiting for the operation to start.</param>
    /// <returns>The matching releases.</returns>
    Task<IReadOnlyList<HelmRelease>> ListReleasesAsync(HelmConnection connection, bool allNamespaces = true, string? filter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a release.
    /// </summary>
    /// <param name="connection">The Kubernetes connection Helm runs against.</param>
    /// <param name="releaseName">The release name.</param>
    /// <param name="cancellationToken">Cancels waiting for the operation to start.</param>
    /// <returns>The release status.</returns>
    Task<HelmRelease> StatusAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the revision history of a release.
    /// </summary>
    /// <param name="connection">The Kubernetes connection Helm runs against.</param>
    /// <param name="releaseName">The release name.</param>
    /// <param name="cancellationToken">Cancels waiting for the operation to start.</param>
    /// <returns>The release revisions, oldest first.</returns>
    Task<IReadOnlyList<HelmRelease>> HistoryAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user-supplied values of a release.
    /// </summary>
    /// <param name="connection">The Kubernetes connection Helm runs against.</param>
    /// <param name="releaseName">The release name.</param>
    /// <param name="cancellationToken">Cancels waiting for the operation to start.</param>
    /// <returns>The release values.</returns>
    Task<IReadOnlyDictionary<string, JsonElement>> GetValuesAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Installs a chart as a new release.
    /// </summary>
    /// <param name="connection">The Kubernetes connection Helm runs against.</param>
    /// <param name="releaseName">The release name.</param>
    /// <param name="chart">The chart reference, for example <c>bitnami/nginx</c>.</param>
    /// <param name="values">Optional chart values.</param>
    /// <param name="createNamespace">Create the release namespace when missing.</param>
    /// <param name="wait">Wait for the release resources to become ready.</param>
    /// <param name="timeout">Optional timeout applied when waiting.</param>
    /// <param name="cancellationToken">Cancels waiting for the operation to start.</param>
    /// <returns>The created release.</returns>
    Task<HelmRelease> InstallAsync(HelmConnection connection, string releaseName, string chart, IReadOnlyDictionary<string, object?>? values = null, bool createNamespace = false, bool wait = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upgrades an existing release, optionally installing it when it is missing.
    /// </summary>
    /// <param name="connection">The Kubernetes connection Helm runs against.</param>
    /// <param name="releaseName">The release name.</param>
    /// <param name="chart">The chart reference, for example <c>bitnami/nginx</c>.</param>
    /// <param name="values">Optional chart values.</param>
    /// <param name="installIfMissing">Install the release when it does not exist yet.</param>
    /// <param name="createNamespace">Create the release namespace when missing.</param>
    /// <param name="wait">Wait for the release resources to become ready.</param>
    /// <param name="timeout">Optional timeout applied when waiting.</param>
    /// <param name="cancellationToken">Cancels waiting for the operation to start.</param>
    /// <returns>The upgraded release.</returns>
    Task<HelmRelease> UpgradeAsync(HelmConnection connection, string releaseName, string chart, IReadOnlyDictionary<string, object?>? values = null, bool installIfMissing = false, bool createNamespace = false, bool wait = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls a release back to a revision, or to its previous revision when <paramref name="revision"/> is zero.
    /// </summary>
    /// <param name="connection">The Kubernetes connection Helm runs against.</param>
    /// <param name="releaseName">The release name.</param>
    /// <param name="revision">The revision to roll back to.</param>
    /// <param name="cancellationToken">Cancels waiting for the operation to start.</param>
    /// <param name="wait">Wait for the release resources to become ready after rollback.</param>
    /// <returns>A task that completes when the rollback finishes.</returns>
    Task RollbackAsync(HelmConnection connection, string releaseName, int revision = 0, CancellationToken cancellationToken = default, bool wait = false);

    /// <summary>
    /// Uninstalls a release.
    /// </summary>
    /// <param name="connection">The Kubernetes connection Helm runs against.</param>
    /// <param name="releaseName">The release name.</param>
    /// <param name="cancellationToken">Cancels waiting for the operation to start.</param>
    /// <param name="wait">Wait for release resources to be deleted.</param>
    /// <returns>A task that completes when the release is removed.</returns>
    Task UninstallAsync(HelmConnection connection, string releaseName, CancellationToken cancellationToken = default, bool wait = false);

    /// <summary>
    /// Renders a chart locally without installing it.
    /// </summary>
    /// <param name="connection">The Kubernetes connection Helm runs against.</param>
    /// <param name="releaseName">The release name used while rendering.</param>
    /// <param name="chart">The chart reference, for example <c>bitnami/nginx</c>.</param>
    /// <param name="values">Optional chart values.</param>
    /// <param name="cancellationToken">Cancels waiting for the operation to start.</param>
    /// <returns>The rendered manifest.</returns>
    Task<string> TemplateAsync(HelmConnection connection, string releaseName, string chart, IReadOnlyDictionary<string, object?>? values = null, CancellationToken cancellationToken = default);
}
