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

    public bool ThrowOnUninstall { get; init; }

    /// <summary>
    /// Gets or sets how many <see cref="ListReleases()"/> entries block until <see cref="Release"/> is set.
    /// </summary>
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

    public List<string> RemoveRepositoryCalls { get; } = [];

    public int UpdateRepositoriesCalls { get; private set; }

    public List<(bool AllNamespaces, string? Filter)> ListReleasesCalls { get; } = [];

    public List<(string ReleaseName, string Chart, Dictionary<string, object?>? Values, bool CreateNamespace, bool Wait, TimeSpan? Timeout)> InstallCalls { get; } = [];

    public List<(string ReleaseName, string Chart, bool InstallIfMissing)> UpgradeCalls { get; } = [];

    public List<string> StatusCalls { get; } = [];

    public List<string> HistoryCalls { get; } = [];

    public List<string> GetValuesCalls { get; } = [];

    public List<(string ReleaseName, int Revision, bool Wait)> RollbackCalls { get; } = [];

    public List<string> UninstallCalls { get; } = [];

    public List<bool> UninstallWaitCalls { get; } = [];

    public List<(string ReleaseName, string Chart, Dictionary<string, object?>? Values)> TemplateCalls { get; } = [];

    public IReadOnlyList<HelmRelease> ListReleases() => Releases;

    public IReadOnlyList<HelmRelease> ListReleases(bool allNamespaces, string? filter = null)
    {
        ListReleasesCalls.Add((allNamespaces, filter));
        return Releases;
    }

    public IReadOnlyList<HelmRepository> ListRepositories()
    {
        Enter();
        return Repositories;
    }

    public void AddRepository(string name, string url, string? username = null, string? password = null)
        => AddRepositoryCalls.Add((name, url, username, password));

    public void RemoveRepository(string name) => RemoveRepositoryCalls.Add(name);

    public void UpdateRepositories() => UpdateRepositoriesCalls++;

    public HelmRelease Install(
        string releaseName,
        string chart,
        IEnumerable<string>? valuesFiles = null,
        IReadOnlyDictionary<string, object?>? values = null,
        bool createNamespace = false,
        bool wait = false,
        TimeSpan? timeout = null)
    {
        InstallCalls.Add((releaseName, chart, values as Dictionary<string, object?>, createNamespace, wait, timeout));
        return InstallRelease ?? throw new InvalidOperationException("No install release configured.");
    }

    public HelmRelease Upgrade(
        string releaseName,
        string chart,
        IEnumerable<string>? valuesFiles = null,
        IReadOnlyDictionary<string, object?>? values = null,
        bool installIfMissing = false,
        bool createNamespace = false,
        bool wait = false,
        TimeSpan? timeout = null)
    {
        UpgradeCalls.Add((releaseName, chart, installIfMissing));
        return UpgradeRelease ?? throw new InvalidOperationException("No upgrade release configured.");
    }

    public HelmRelease Status(string releaseName)
    {
        StatusCalls.Add(releaseName);
        return StatusRelease ?? throw new InvalidOperationException("No status release configured.");
    }

    public IReadOnlyList<HelmRelease> History(string releaseName)
    {
        HistoryCalls.Add(releaseName);
        return HistoryReleases;
    }

    public void Rollback(string releaseName, int revision = 0) => RollbackCalls.Add((releaseName, revision, false));

    public void Rollback(string releaseName, int revision, bool wait) => RollbackCalls.Add((releaseName, revision, wait));

    public void Uninstall(string releaseName)
    {
        UninstallCalls.Add(releaseName);
        if (ThrowOnUninstall)
        {
            throw new HelmActionException("release not found");
        }
    }

    public void Uninstall(string releaseName, bool wait)
    {
        Uninstall(releaseName);
        UninstallWaitCalls.Add(wait);
    }

    public IReadOnlyDictionary<string, JsonElement> GetValues(string releaseName)
    {
        GetValuesCalls.Add(releaseName);
        return new Dictionary<string, JsonElement>();
    }

    public string Template(
        string releaseName,
        string chart,
        IEnumerable<string>? valuesFiles = null,
        IReadOnlyDictionary<string, object?>? values = null)
    {
        TemplateCalls.Add((releaseName, chart, values as Dictionary<string, object?>));
        return TemplateResult;
    }

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
