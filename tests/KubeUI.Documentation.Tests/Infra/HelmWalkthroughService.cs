using System.Collections.Concurrent;
using System.Text.Json;
using Helm.NET;
using KubeUI.Kubernetes;
using Moq;

namespace KubeUI.Documentation.Tests.Infra;

/// <summary>Uses a live Helm repository and chart, with isolated release bookkeeping for recordings.</summary>
internal static class HelmWalkthroughService
{
    private const string RepositoryName = "prometheus-community";
    private const string RepositoryUrl = "https://prometheus-community.github.io/helm-charts";
    private const string ChartName = "prometheus";

    public static IHelmService Create(HelmService realHelm)
    {
        ArgumentNullException.ThrowIfNull(realHelm);
        var releases = new ConcurrentDictionary<string, HelmRelease>(StringComparer.Ordinal);
        var valuesByContext = new ConcurrentDictionary<string, JsonElement>(StringComparer.Ordinal);
        var service = new Mock<IHelmService>();

        service.Setup(helm => helm.ListRepositoriesAsync(
                It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .Returns((HelmConnection connection, CancellationToken cancellationToken) =>
                GetRepositoriesAsync(connection, cancellationToken));
        service.Setup(helm => helm.ListChartsAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns((HelmConnection connection, string? repository, CancellationToken cancellationToken) =>
                realHelm.ListChartsAsync(connection, repository, cancellationToken));
        service.Setup(helm => helm.GetChartValuesAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns((HelmConnection connection, string chart, string? version, CancellationToken cancellationToken) =>
                realHelm.GetChartValuesAsync(connection, chart, version, cancellationToken));
        service.Setup(helm => helm.TemplateVersionAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IReadOnlyDictionary<string, object?>?>(), It.IsAny<CancellationToken>()))
            .Returns((HelmConnection connection, string releaseName, string chart, string version,
                IReadOnlyDictionary<string, object?>? values, CancellationToken cancellationToken) =>
                realHelm.TemplateVersionAsync(connection, releaseName, chart, version, values, cancellationToken));
        service.Setup(helm => helm.AddRepositoryAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns((HelmConnection connection, string name, string url, string? username, string? password,
                CancellationToken cancellationToken) =>
                realHelm.AddRepositoryAsync(connection, name, url, username, password, cancellationToken));
        service.Setup(helm => helm.RemoveRepositoryAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((HelmConnection connection, string name, CancellationToken cancellationToken) =>
                realHelm.RemoveRepositoryAsync(connection, name, cancellationToken));
        service.Setup(helm => helm.UpdateRepositoriesAsync(
                It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .Returns((HelmConnection connection, CancellationToken cancellationToken) =>
                realHelm.UpdateRepositoriesAsync(connection, cancellationToken));

        service.Setup(helm => helm.InstallAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IReadOnlyDictionary<string, object?>?>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>(), It.IsAny<string?>()))
            .Returns((HelmConnection connection, string name, string chart,
                IReadOnlyDictionary<string, object?>? values, bool createNamespace, bool wait,
                TimeSpan? timeout, CancellationToken cancellationToken, string? version) =>
            {
                var key = GetContext(connection);
                var release = CreateRelease(name, connection.Namespace ?? "default", 1, version ?? "latest");
                releases[key] = release;
                valuesByContext[key] = SerializeValues(values);
                return Task.FromResult(release);
            });
        service.Setup(helm => helm.ListReleasesAsync(
                It.IsAny<HelmConnection>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns((HelmConnection connection, bool allNamespaces, string? filter, CancellationToken cancellationToken) =>
            {
                var result = releases.TryGetValue(GetContext(connection), out var release)
                    && (filter is null || release.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    ? (IReadOnlyList<HelmRelease>)[release]
                    : [];
                return Task.FromResult(result);
            });
        service.Setup(helm => helm.GetValuesAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((HelmConnection connection, string releaseName, CancellationToken cancellationToken) =>
            {
                var values = valuesByContext.TryGetValue(GetContext(connection), out var storedValues)
                    ? storedValues
                    : JsonSerializer.SerializeToElement(new { server = new { replicaCount = 1 } });
                return Task.FromResult(values);
            });
        service.Setup(helm => helm.StatusAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((HelmConnection connection, string releaseName, CancellationToken cancellationToken) =>
                Task.FromResult(releases[GetContext(connection)]));
        service.Setup(helm => helm.UpgradeAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IReadOnlyDictionary<string, object?>?>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>(), It.IsAny<string?>()))
            .Returns((HelmConnection connection, string name, string chart,
                IReadOnlyDictionary<string, object?>? values, bool installIfMissing, bool createNamespace,
                bool wait, TimeSpan? timeout, CancellationToken cancellationToken, string? version) =>
            {
                var key = GetContext(connection);
                var revision = releases.TryGetValue(key, out var previous) ? previous.Revision + 1 : 1;
                var release = CreateRelease(name, connection.Namespace ?? "default", revision, version ?? "latest");
                releases[key] = release;
                valuesByContext[key] = SerializeValues(values);
                return Task.FromResult(release);
            });

        return service.Object;

        async Task<IReadOnlyList<HelmRepository>> GetRepositoriesAsync(
            HelmConnection connection,
            CancellationToken cancellationToken)
        {
            var repositories = await realHelm.ListRepositoriesAsync(connection, cancellationToken).ConfigureAwait(false);
            if (!repositories.Any(repository => repository.Name == RepositoryName))
            {
                await realHelm.AddRepositoryAsync(connection, RepositoryName, RepositoryUrl, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                await realHelm.UpdateRepositoriesAsync(connection, cancellationToken).ConfigureAwait(false);
            }

            return await realHelm.ListRepositoriesAsync(connection, cancellationToken).ConfigureAwait(false);
        }
    }

    public static string GetRepositoryName() => RepositoryName;

    public static string GetChartName() => ChartName;

    private static string GetContext(HelmConnection connection) => connection.Context ?? "default";

    private static JsonElement SerializeValues(IReadOnlyDictionary<string, object?>? values)
        => values is null
            ? JsonSerializer.SerializeToElement(new Dictionary<string, object?>())
            : JsonSerializer.SerializeToElement(values);

    private static HelmRelease CreateRelease(string name, string releaseNamespace, int revision, string version)
        => new(
            name,
            releaseNamespace,
            revision,
            DateTimeOffset.UnixEpoch,
            "deployed",
            $"{ChartName}-{version}",
            version,
            ChartName,
            version);

    public static void ConfigureHelmRepositoryPaths()
    {
        var helmHome = Path.Combine(
            Path.GetTempPath(),
            "kubeui-docs-helm",
            Environment.ProcessId.ToString(),
            Guid.NewGuid().ToString("N"));
        var configDirectory = Path.Combine(helmHome, "config");
        var cacheDirectory = Path.Combine(helmHome, "cache");
        Directory.CreateDirectory(configDirectory);
        Directory.CreateDirectory(cacheDirectory);
        Environment.SetEnvironmentVariable("HELM_CONFIG_HOME", configDirectory);
        Environment.SetEnvironmentVariable("HELM_CACHE_HOME", cacheDirectory);
        Environment.SetEnvironmentVariable("HELM_REPOSITORY_CONFIG", Path.Combine(configDirectory, "repositories.yaml"));
        Environment.SetEnvironmentVariable("HELM_REPOSITORY_CACHE", cacheDirectory);
    }
}
