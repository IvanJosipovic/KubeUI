using System.Text.Json;
using Helm.NET;

namespace KubeUI.Kubernetes.Tests.Helm;

/// <summary>
/// In-memory <see cref="IHelmClient"/> used to verify <see cref="HelmService"/> behavior without the native Helm shim.
/// </summary>
internal sealed class FakeHelmClient : IHelmClient
{
    private int _current;
    private int _entryCount;
    private int _maxConcurrent;
    private int _blockingEntries;

    public List<HelmRepository> Repositories { get; } = [];
    public List<HelmRelease> Releases { get; } = [];
    public List<HelmRelease> HistoryReleases { get; } = [];
    public HelmRelease? InstallRelease { get; init; }
    public HelmRelease? UpgradeRelease { get; init; }
    public HelmRelease? StatusRelease { get; init; }
    public string TemplateResult { get; init; } = string.Empty;
    public string ChartValuesResult { get; init; } = string.Empty;
    public List<(string Chart, string? Version)> ShowValuesCalls { get; } = [];
    public List<HelmChartSearchResult> Charts { get; } = [];
    public List<string?> ListChartsCalls { get; } = [];
    public List<string> SearchCalls { get; } = [];
    public List<(string? Keyword, bool Versions, bool Devel)> SearchRepoCalls { get; } = [];
    public List<(string Name, string? NamespaceName, int Revision)> GetManifestCalls { get; } = [];
    public string? TemplateVersion { get; private set; }
    public JsonElement Values { get; set; } = JsonDocument.Parse("{}").RootElement.Clone();
    public bool ThrowOnUninstall { get; init; }

    /// <summary>Gets or sets how many list calls block until <see cref="Release"/> is set.</summary>
    public int BlockingEntries
    {
        get => Volatile.Read(ref _blockingEntries);
        set => Volatile.Write(ref _blockingEntries, value);
    }

    public TaskCompletionSource FirstEntry { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public int EntryCount => Volatile.Read(ref _entryCount);
    public int MaxConcurrent => Volatile.Read(ref _maxConcurrent);
    public List<(string Name, string Url, string? Username, string? Password)> AddRepositoryCalls { get; } = [];
    public List<bool> AddRepositoryForceUpdateCalls { get; } = [];
    public List<string> RemoveRepositoryCalls { get; } = [];
    public int UpdateRepositoriesCalls { get; private set; }
    public List<(string? NamespaceName, bool AllNamespaces, string? Filter)> ListReleasesCalls { get; } = [];
    public List<(string ReleaseName, string Chart, string? NamespaceName, JsonElement? Values, bool CreateNamespace, bool Wait, TimeSpan? Timeout)> InstallCalls { get; } = [];
    public List<string?> InstallVersions { get; } = [];
    public List<(string ReleaseName, string Chart, string? NamespaceName, bool InstallIfMissing)> UpgradeCalls { get; } = [];
    public List<JsonElement?> UpgradeValues { get; } = [];
    public List<string?> UpgradeVersions { get; } = [];
    public List<(string Name, string? NamespaceName)> StatusCalls { get; } = [];
    public List<(string Name, string? NamespaceName)> HistoryCalls { get; } = [];
    public List<(string Name, string? NamespaceName)> GetValuesCalls { get; } = [];
    public List<(string ReleaseName, int Revision, string? NamespaceName, bool Wait)> RollbackCalls { get; } = [];
    public List<(string Name, string? NamespaceName, bool Wait)> UninstallCalls { get; } = [];
    public List<bool> UninstallWaitCalls { get; } = [];
    public List<(string ReleaseName, string Chart, string? NamespaceName, JsonElement? Values)> TemplateCalls { get; } = [];

    public IReadOnlyList<HelmRelease> List(
        string? namespaceName = null,
        bool allNamespaces = false,
        string? filter = null,
        int max = 256,
        int offset = 0,
        string? selector = null,
        bool deployed = false,
        bool failed = false,
        bool pending = false,
        bool superseded = false,
        bool uninstalled = false,
        bool uninstalling = false,
        bool dateSort = false,
        bool reverse = false)
    {
        Enter();
        ListReleasesCalls.Add((namespaceName, allNamespaces, filter));
        return Releases;
    }

    public string Template(
        string name,
        string chart,
        string? namespaceName = null,
        IEnumerable<string>? values = null,
        JsonElement? setValues = null,
        string? version = null,
        IEnumerable<string>? apiVersions = null,
        string? kubeVersion = null,
        bool includeCrds = false,
        bool skipTests = false,
        bool dependencyUpdate = false,
        bool isUpgrade = false,
        string? dryRun = null,
        bool hideSecret = false,
        bool disableHooks = false,
        string? postRenderer = null,
        IEnumerable<string>? postRendererArgs = null,
        string? repo = null,
        string? username = null,
        string? password = null,
        string? caFile = null,
        string? certFile = null,
        string? keyFile = null,
        bool insecureSkipTlsVerify = false,
        bool passCredentials = false,
        bool plainHttp = false)
    {
        TemplateCalls.Add((name, chart, namespaceName, setValues));
        TemplateVersion = version;
        return TemplateResult;
    }

    public string GetManifest(string releaseName, string? namespaceName = null, int revision = 0)
    {
        GetManifestCalls.Add((releaseName, namespaceName, revision));
        return TemplateResult;
    }

    public string ShowValues(
        string chart,
        string? version = null,
        string? repo = null,
        string? username = null,
        string? password = null,
        string? caFile = null,
        string? certFile = null,
        string? keyFile = null,
        bool insecureSkipTlsVerify = false,
        bool passCredentials = false,
        bool plainHttp = false)
    {
        ShowValuesCalls.Add((chart, version));
        return ChartValuesResult;
    }

    public HelmRelease Install(
        string name,
        string chart,
        string? namespaceName = null,
        IEnumerable<string>? values = null,
        JsonElement? setValues = null,
        bool createNamespace = false,
        bool wait = false,
        TimeSpan? timeout = null,
        string? version = null,
        string? dryRun = null,
        bool hideSecret = false,
        bool disableHooks = false,
        string? postRenderer = null,
        IEnumerable<string>? postRendererArgs = null,
        bool waitForJobs = false,
        bool rollbackOnFailure = false,
        bool dependencyUpdate = false,
        bool skipCrds = false,
        bool devel = false,
        bool skipSchemaValidation = false,
        bool disableOpenApiValidation = false,
        string? description = null,
        string? waitStrategy = null,
        bool replace = false,
        bool forceReplace = false,
        bool forceConflicts = false,
        bool takeOwnership = false,
        string? repo = null,
        string? username = null,
        string? password = null,
        string? caFile = null,
        string? certFile = null,
        string? keyFile = null,
        bool insecureSkipTlsVerify = false,
        bool passCredentials = false,
        bool plainHttp = false)
    {
        InstallCalls.Add((name, chart, namespaceName, setValues, createNamespace, wait, timeout));
        InstallVersions.Add(version);
        return InstallRelease ?? throw new InvalidOperationException("No install release configured.");
    }

    public HelmRelease Upgrade(
        string release,
        string chart,
        string? namespaceName = null,
        IEnumerable<string>? values = null,
        JsonElement? setValues = null,
        bool install = false,
        bool createNamespace = false,
        bool wait = false,
        TimeSpan? timeout = null,
        string? version = null,
        string? dryRun = null,
        bool hideSecret = false,
        bool disableHooks = false,
        string? postRenderer = null,
        IEnumerable<string>? postRendererArgs = null,
        bool waitForJobs = false,
        bool rollbackOnFailure = false,
        bool resetValues = false,
        bool reuseValues = false,
        bool resetThenReuseValues = false,
        bool skipCrds = false,
        bool devel = false,
        bool skipSchemaValidation = false,
        bool disableOpenApiValidation = false,
        string? description = null,
        bool dependencyUpdate = false,
        string? waitStrategy = null,
        bool forceReplace = false,
        bool forceConflicts = false,
        bool takeOwnership = false,
        bool cleanupOnFail = false,
        string? repo = null,
        string? username = null,
        string? password = null,
        string? caFile = null,
        string? certFile = null,
        string? keyFile = null,
        bool insecureSkipTlsVerify = false,
        bool passCredentials = false,
        bool plainHttp = false)
    {
        UpgradeCalls.Add((release, chart, namespaceName, install));
        UpgradeValues.Add(setValues);
        UpgradeVersions.Add(version);
        return UpgradeRelease ?? throw new InvalidOperationException("No upgrade release configured.");
    }

    public HelmRelease Status(string releaseName, string? namespaceName = null, int revision = 0)
    {
        StatusCalls.Add((releaseName, namespaceName));
        return StatusRelease ?? throw new InvalidOperationException("No status release configured.");
    }

    public IReadOnlyList<HelmRelease> History(string releaseName, string? namespaceName = null, int max = 256)
    {
        HistoryCalls.Add((releaseName, namespaceName));
        return HistoryReleases;
    }

    public JsonElement GetValues(string releaseName, string? namespaceName = null, int revision = 0, bool all = false)
    {
        GetValuesCalls.Add((releaseName, namespaceName));
        return Values;
    }

    public void Rollback(
        string releaseName,
        int revision = 0,
        string? namespaceName = null,
        bool wait = false,
        string? waitStrategy = null,
        TimeSpan? timeout = null,
        bool waitForJobs = false,
        bool disableHooks = false,
        string? dryRun = null,
        bool forceReplace = false,
        bool forceConflicts = false,
        bool cleanupOnFail = false,
        int historyMax = 10,
        string? description = null)
        => RollbackCalls.Add((releaseName, revision, namespaceName, wait));

    public void Uninstall(
        string releaseName,
        string? namespaceName = null,
        bool wait = false,
        string? waitStrategy = null,
        TimeSpan? timeout = null,
        bool disableHooks = false,
        bool dryRun = false,
        bool ignoreNotFound = false,
        bool keepHistory = false,
        string cascade = "background",
        string? description = null)
    {
        UninstallCalls.Add((releaseName, namespaceName, wait));
        UninstallWaitCalls.Add(wait);
        if (ThrowOnUninstall)
        {
            throw new HelmActionException("release not found");
        }
    }

    public IReadOnlyList<HelmRepository> RepoList()
    {
        Enter();
        return Repositories;
    }

    public IReadOnlyList<HelmChartSearchResult> SearchRepo(
        string? keyword = null,
        bool versions = false,
        bool devel = false,
        bool regexp = false,
        string? version = null,
        bool failOnNoResult = false)
    {
        SearchRepoCalls.Add((keyword, versions, devel));
        if (string.IsNullOrEmpty(keyword))
        {
            ListChartsCalls.Add(null);
        }
        else
        {
            SearchCalls.Add(keyword);
        }

        return Charts;
    }

    public void RepoAdd(string name, string url, string? username = null, string? password = null, bool forceUpdate = false)
    {
        Enter();
        AddRepositoryCalls.Add((name, url, username, password));
        AddRepositoryForceUpdateCalls.Add(forceUpdate);
    }

    public void RepoRemove(string repository) => RemoveRepositoryCalls.Add(repository);
    public void RepoUpdate() => UpdateRepositoriesCalls++;

    private void Enter()
    {
        var current = Interlocked.Increment(ref _current);
        Interlocked.Increment(ref _entryCount);

        var observedMax = Volatile.Read(ref _maxConcurrent);
        while (current > observedMax)
        {
            observedMax = Interlocked.CompareExchange(ref _maxConcurrent, current, observedMax);
        }

        var blocks = Interlocked.Decrement(ref _blockingEntries) >= 0;
        if (blocks)
        {
            FirstEntry.TrySetResult();
            Release.Task.Wait(TestContext.Current.CancellationToken);
        }

        Interlocked.Decrement(ref _current);
    }
}
