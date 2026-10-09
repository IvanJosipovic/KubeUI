using System.Collections.Concurrent;
using System.Globalization;
using System.Reactive.Linq;
using System.Threading.Channels;
using Avalonia.Controls.Selection;
using Dock.Model.Core;
using DynamicData;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;

public sealed partial class MRDiffDetectionViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private const int PendingRecordCapacity = 2048;
    private const int MaximumRetainedDiffRows = 50_000;
    private readonly CrossplaneDiffLogParser _parser;
    private readonly CrossplaneProviderLogMonitor _monitor;
    private readonly ILogger<MRDiffDetectionViewModel> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IFactory _factory;
    private readonly Channel<PendingRecord> _pendingRecords = Channel.CreateBounded<PendingRecord>(
        new BoundedChannelOptions(PendingRecordCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
    private readonly ConcurrentDictionary<string, byte> _seededGvks = new(StringComparer.Ordinal);
    private readonly CancellationTokenSource _processingCancellation = new();
    private readonly CancellationToken _processingToken;
    private readonly object _seedTasksGate = new();
    private readonly List<Task> _seedTasks = [];
    private readonly SourceCache<CrossplaneDiffRow, string> _rowsSource = new(row => row.Key);
    private readonly DynamicTableViewSource<CrossplaneDiffRow, string> _tableSource;
    private readonly Task _processingTask;
    private IDisposable? _providerSubscription;
    private IDisposable? _podSubscription;
    private ISourceCache<GenericKubernetesObject, ResourceCacheKey>? _providerResources;
    private HashSet<string> _activePodKeys = new(StringComparer.Ordinal);
    private Dictionary<string, int> _activeContainerRestartCounts = new(StringComparer.Ordinal);
    private string? _monitoredProviderName;
    private volatile bool _disposed;
    private long _generation;
    private long _rowLimitGeneration = -1;
    private long _truncatedLogGeneration = -1;

    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    [ObservableProperty]
    public partial CrossplaneProviderOption? SelectedProvider { get; set; }

    [ObservableProperty]
    public partial string Status { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    public ObservableCollection<CrossplaneProviderOption> Providers { get; } = [];
    public IDynamicTableViewSource TableSource => _tableSource;
    public ISelectionModel SelectionModel => _tableSource.SelectionModel;

    public MRDiffDetectionViewModel(
        CrossplaneDiffLogParser parser,
        CrossplaneProviderLogMonitor monitor,
        ILogger<MRDiffDetectionViewModel> logger,
        IServiceProvider serviceProvider,
        IFactory factory)
    {
        _parser = parser;
        _monitor = monitor;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _factory = factory;
        _processingToken = _processingCancellation.Token;
        var columns = CreateColumns();
        _tableSource = new DynamicTableViewSource<CrossplaneDiffRow, string>(
            _rowsSource,
            static row => row.Key,
            columns,
            options: new DynamicTableViewSourceOptions
            {
                SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Key
            });
        _tableSource.SelectionModel.SingleSelect = false;
        _tableSource.SetSort([
            new DynamicTableViewSortDescriptor("kind", ListSortDirection.Ascending)
        ]);
        _processingTask = Task.Run(() => ProcessRecordsAsync(_processingToken));
        Title = Assets.Resources.MRDiffDetectionView_Title!;
    }

    internal static IReadOnlyList<DynamicTableViewColumn<CrossplaneDiffRow>> CreateColumns()
    {
        return [
            CreateColumn("name", Assets.Resources.MRDiffDetectionView_Name!, typeof(string), static row => row.Name),
            CreateColumn("namespace", Assets.Resources.MRDiffDetectionView_Namespace!, typeof(string), static row => row.Namespace),
            CreateColumn("apiVersion", Assets.Resources.MRDiffDetectionView_ApiVersion!, typeof(string), static row => row.ApiVersion),
            CreateColumn("kind", Assets.Resources.MRDiffDetectionView_Kind!, typeof(string), static row => row.Kind),
            CreateColumn("diffField", Assets.Resources.MRDiffDetectionView_DiffField!, typeof(string), static row => row.DiffField),
            CreateColumn(
                "oldValue",
                Assets.Resources.MRDiffDetectionView_OldValue!,
                typeof(string),
                static row => row.OldValue,
                static row => row.Sensitive ? Assets.Resources.MRDiffDetectionView_Sensitive! : row.OldValue),
            CreateColumn(
                "newValue",
                Assets.Resources.MRDiffDetectionView_NewValue!,
                typeof(string),
                static row => row.NewValue,
                static row => row.Sensitive ? Assets.Resources.MRDiffDetectionView_Sensitive! : row.NewValue),
            CreateColumn("newComputed", Assets.Resources.MRDiffDetectionView_NewComputed!, typeof(bool), static row => row.NewComputed),
            CreateColumn("newRemoved", Assets.Resources.MRDiffDetectionView_NewRemoved!, typeof(bool), static row => row.NewRemoved),
            CreateColumn("requiresNew", Assets.Resources.MRDiffDetectionView_RequiresNew!, typeof(bool), static row => row.RequiresNew),
            CreateColumn("instanceCount", Assets.Resources.MRDiffDetectionView_InstanceCount!, typeof(int), static row => row.InstanceCount),
            CreateColumn("occurrences", Assets.Resources.MRDiffDetectionView_Occurrences!, typeof(int), static row => row.Occurrences)
        ];
    }

    private static DynamicTableViewColumn<CrossplaneDiffRow> CreateColumn(
        string key,
        string name,
        Type valueType,
        Func<CrossplaneDiffRow, object?> valueSelector,
        Func<CrossplaneDiffRow, string>? displaySelector = null)
    {
        return new DynamicTableViewColumn<CrossplaneDiffRow>(
            key,
            name,
            valueType,
            valueSelector,
            displaySelector ?? (row => valueSelector(row)?.ToString() ?? string.Empty),
            cellTemplate: null);
    }

    public static bool IsAvailable(ClusterWorkspace cluster)
    {
        return cluster.GetResourceConfigs().Any(config =>
            config.IsCustomResource
            && string.Equals(config.Kind.Group, "pkg.crossplane.io", StringComparison.Ordinal)
            && string.Equals(config.Kind.Kind, "Provider", StringComparison.Ordinal)
            && config.PermissionsLoaded
            && config.CanListAndWatch);
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        if (ReferenceEquals(Cluster, cluster))
        {
            return;
        }

        _providerSubscription?.Dispose();
        _providerSubscription = null;
        _podSubscription?.Dispose();
        _podSubscription = null;
        _providerResources = null;
        _seededGvks.Clear();
        _monitor.Stop();
        _monitoredProviderName = null;
        _activePodKeys.Clear();
        _activeContainerRestartCounts.Clear();

        Cluster = cluster;
        Id = $"{nameof(MRDiffDetectionViewModel)}-{cluster.Runtime.Name}";
        _ = StartMonitoringAsync(null);

        var providerConfig = cluster.GetResourceConfigs().FirstOrDefault(config =>
            config.IsCustomResource
            && string.Equals(config.Kind.Group, "pkg.crossplane.io", StringComparison.Ordinal)
            && string.Equals(config.Kind.Kind, "Provider", StringComparison.Ordinal));
        if (providerConfig is null)
        {
            Providers.Clear();
            SelectedProvider = null;
            return;
        }

        _ = SeedAndBindResourcesAsync(cluster, providerConfig.Kind);
    }

    partial void OnSelectedProviderChanged(CrossplaneProviderOption? value)
    {
        if (string.Equals(_monitoredProviderName, value?.Name, StringComparison.Ordinal))
        {
            return;
        }

        _monitoredProviderName = value?.Name;
        _activePodKeys = value is null || Cluster is null
            ? new HashSet<string>(StringComparer.Ordinal)
            : CrossplaneProviderLogMonitor.GetProviderPodKeys(Cluster.Runtime, value.Name);
        _activeContainerRestartCounts = value is null || Cluster is null
            ? new Dictionary<string, int>(StringComparer.Ordinal)
            : CrossplaneProviderLogMonitor.GetProviderContainerRestartCounts(Cluster.Runtime, value.Name);
        _ = StartMonitoringAsync(value);
    }

    private void ReconcileProviderPods()
    {
        if (_disposed || Cluster is null || SelectedProvider is null)
        {
            return;
        }

        var podKeys = CrossplaneProviderLogMonitor.GetProviderPodKeys(Cluster.Runtime, SelectedProvider.Name);
        var restartCounts = CrossplaneProviderLogMonitor.GetProviderContainerRestartCounts(Cluster.Runtime, SelectedProvider.Name);
        if (_activePodKeys.SetEquals(podKeys) && RestartCountsEqual(_activeContainerRestartCounts, restartCounts))
        {
            return;
        }

        _activePodKeys = podKeys;
        _activeContainerRestartCounts = restartCounts;
        _ = StartMonitoringAsync(SelectedProvider, resetRows: false);
    }

    private static bool RestartCountsEqual(
        IReadOnlyDictionary<string, int> current,
        IReadOnlyDictionary<string, int> next)
    {
        if (current.Count != next.Count)
        {
            return false;
        }

        foreach (var pair in current)
        {
            if (!next.TryGetValue(pair.Key, out var restartCount) || restartCount != pair.Value)
            {
                return false;
            }
        }

        return true;
    }

    [RelayCommand]
    private void Clear()
    {
        _ = StartMonitoringAsync(SelectedProvider);
    }

    [RelayCommand(CanExecute = nameof(CanViewYaml))]
    private void ViewYaml(CrossplaneDiffRow? row)
    {
        if (Cluster is null || row is null || FindResource(row) is not { } resource)
        {
            return;
        }

        var yamlViewModel = _serviceProvider.GetRequiredService<ResourceYamlViewModel>();
        yamlViewModel.Initialize(Cluster, resource);
        _factory.AddToBottom(yamlViewModel);
    }

    private bool CanViewYaml(CrossplaneDiffRow? row)
    {
        return row is not null && FindResource(row) is not null;
    }

    private GenericKubernetesObject? FindResource(CrossplaneDiffRow row)
    {
        if (Cluster is null)
        {
            return null;
        }

        foreach (var pair in Cluster.Runtime.Objects)
        {
            if (pair.Value is not IResourceContainer container)
            {
                continue;
            }

            foreach (var resource in container.Snapshot().OfType<GenericKubernetesObject>())
            {
                if (string.Equals(resource.Metadata?.Uid, row.Uid, StringComparison.Ordinal)
                    && string.Equals(resource.Metadata?.Name, row.Name, StringComparison.Ordinal)
                    && string.Equals(resource.Metadata?.NamespaceProperty ?? string.Empty, row.Namespace, StringComparison.Ordinal)
                    && string.Equals(resource.ApiVersion, row.ApiVersion, StringComparison.Ordinal)
                    && string.Equals(resource.Kind, row.Kind, StringComparison.Ordinal))
                {
                    return resource;
                }
            }
        }

        return null;
    }

    private void RefreshProviders()
    {
        if (Cluster is null || _disposed)
        {
            return;
        }

        var selectedName = SelectedProvider?.Name;
        Providers.Clear();
        foreach (var resource in _providerResources?.Items.OrderBy(resource => resource.Metadata?.Name, StringComparer.Ordinal)
                     ?? Enumerable.Empty<GenericKubernetesObject>())
        {
            if (!string.IsNullOrWhiteSpace(resource.Metadata?.Name))
            {
                Providers.Add(new CrossplaneProviderOption(resource.Metadata.Name, resource));
            }
        }

        SelectedProvider = selectedName is null
            ? null
            : Providers.FirstOrDefault(provider => string.Equals(provider.Name, selectedName, StringComparison.Ordinal));
    }

    private async Task SeedAndBindResourcesAsync(ClusterWorkspace cluster, GroupApiVersionKind providerKind)
    {
        try
        {
            await Task.WhenAll(
                cluster.Runtime.SeedResource(providerKind, waitForReady: true, _processingToken),
                cluster.Runtime.SeedResource(GroupApiVersionKind.From<V1Pod>(), waitForReady: true, _processingToken)).ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_disposed || !ReferenceEquals(Cluster, cluster))
                {
                    return;
                }

                _providerResources = cluster.Runtime.GetResourceSourceCache<GenericKubernetesObject>(providerKind);
                _providerSubscription?.Dispose();
                _providerSubscription = _providerResources
                    .Connect()
                    .Subscribe(_ => Dispatcher.UIThread.Post(RefreshProviders));
                _podSubscription = cluster.Runtime.ConnectResources()
                    .Where(change => change.Kind == GroupApiVersionKind.From<V1Pod>())
                    .Throttle(TimeSpan.FromMilliseconds(50))
                    .Subscribe(_ => Dispatcher.UIThread.Post(ReconcileProviderPods));
                RefreshProviders();
            });
        }
        catch (OperationCanceledException) when (_processingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private async Task StartMonitoringAsync(CrossplaneProviderOption? provider, bool resetRows = true)
    {
        if (_disposed)
        {
            return;
        }

        var generation = resetRows
            ? Interlocked.Increment(ref _generation)
            : Volatile.Read(ref _generation);
        if (provider is null)
        {
            _monitor.Stop();
        }

        if (resetRows)
        {
            try
            {
                await _pendingRecords.Writer.WriteAsync(PendingRecord.Reset(generation), _processingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (_disposed)
            {
                return;
            }
            catch (ChannelClosedException) when (_disposed)
            {
                return;
            }
        }

        if (generation != Volatile.Read(ref _generation))
        {
            return;
        }

        if (provider is null)
        {
            SetStatus(Assets.Resources.MRDiffDetectionView_SelectProvider!, generation);
            return;
        }

        var cluster = Cluster;
        if (cluster is null)
        {
            return;
        }

        SetStatus(string.Format(Assets.Resources.MRDiffDetectionView_Connecting!, provider.Name), generation);
        try
        {
            SetMonitoringStatus(generation);
            await _monitor.StartAsync(
                cluster.Runtime,
                provider.Resource,
                (line, cancellationToken) => HandleLogLineAsync(generation, line, cancellationToken),
                CancellationToken.None,
                resetSeen: resetRows).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to monitor Crossplane provider {ProviderName}", provider.Name);
            SetStatus(ex.Message, generation);
        }
    }

    private async ValueTask HandleLogLineAsync(long generation, string line, CancellationToken cancellationToken)
    {
        if (generation != Volatile.Read(ref _generation))
        {
            return;
        }

        var records = _parser.Parse(line, out var wasTruncated);
        if (wasTruncated)
        {
            Volatile.Write(ref _truncatedLogGeneration, generation);
            SetTruncatedStatus(generation);
        }

        foreach (var record in records)
        {
            EnsureGvkSeeded(record);
            try
            {
                await _pendingRecords.Writer.WriteAsync(new PendingRecord(generation, record), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (ChannelClosedException) when (_disposed)
            {
                return;
            }
        }
    }

    private void EnsureGvkSeeded(CrossplaneDiffRecord record)
    {
        var cluster = Cluster;
        if (cluster is null)
        {
            return;
        }

        var gvkKey = string.Concat(record.ApiVersion, ", Kind=", record.Kind);
        if (!_seededGvks.TryAdd(gvkKey, 0))
        {
            return;
        }

        var separator = record.ApiVersion.LastIndexOf('/');
        if (separator <= 0 || separator == record.ApiVersion.Length - 1)
        {
            _seededGvks.TryRemove(gvkKey, out _);
            return;
        }

        var group = record.ApiVersion[..separator];
        var version = record.ApiVersion[(separator + 1)..];
        var config = cluster.GetResourceConfigs().FirstOrDefault(resourceConfig =>
            string.Equals(resourceConfig.Kind.Group, group, StringComparison.Ordinal)
            && string.Equals(resourceConfig.Kind.ApiVersion, version, StringComparison.Ordinal)
            && string.Equals(resourceConfig.Kind.Kind, record.Kind, StringComparison.Ordinal));
        if (config is null)
        {
            _seededGvks.TryRemove(gvkKey, out _);
            return;
        }

        lock (_seedTasksGate)
        {
            if (_disposed)
            {
                _seededGvks.TryRemove(gvkKey, out _);
                return;
            }

            _seedTasks.Add(SeedGvkAsync(config, gvkKey, _processingToken));
        }
    }

    private async Task SeedGvkAsync(IResourceConfig config, string gvkKey, CancellationToken cancellationToken)
    {
        try
        {
            await config.SeedResource(waitForReady: true, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _seededGvks.TryRemove(gvkKey, out _);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to seed Crossplane managed-resource kind {ApiVersion}, Kind={Kind}", config.Kind.ApiVersion, config.Kind.Kind);
            _seededGvks.TryRemove(gvkKey, out _);
        }
    }

    private async Task ProcessRecordsAsync(CancellationToken cancellationToken)
    {
        var aggregator = new CrossplaneDiffAggregator(MaximumRetainedDiffRows);
        var activeGeneration = Volatile.Read(ref _generation);
        var rowLimitReached = false;
        HashSet<(string ApiVersion, string Kind, string DiffField)> dirtyGroups = [];
        var reader = _pendingRecords.Reader;
        var readTask = reader.WaitToReadAsync(cancellationToken).AsTask();
        var refreshTask = Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
        try
        {
            while (true)
            {
                var completed = await Task.WhenAny(readTask, refreshTask).ConfigureAwait(false);
                if (ReferenceEquals(completed, refreshTask))
                {
                    await refreshTask.ConfigureAwait(false);
                    RefreshDirtyGroups();
                    refreshTask = Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
                    continue;
                }

                if (!await readTask.ConfigureAwait(false))
                {
                    break;
                }

                while (reader.TryRead(out var pending))
                {
                    if (pending.IsReset)
                    {
                        if (pending.Generation != Volatile.Read(ref _generation))
                        {
                            continue;
                        }

                        aggregator = new CrossplaneDiffAggregator(MaximumRetainedDiffRows);
                        activeGeneration = pending.Generation;
                        rowLimitReached = false;
                        Volatile.Write(ref _rowLimitGeneration, -1);
                        Volatile.Write(ref _truncatedLogGeneration, -1);
                        dirtyGroups.Clear();
                        _rowsSource.Clear();
                        continue;
                    }

                    if (pending.Generation != activeGeneration || pending.Generation != Volatile.Read(ref _generation))
                    {
                        continue;
                    }

                    var record = pending.Record!;
                    var snapshot = aggregator.AddAndGetRowSnapshot(record, out var added);
                    if (snapshot is null)
                    {
                        if (!rowLimitReached)
                        {
                            rowLimitReached = true;
                            Volatile.Write(ref _rowLimitGeneration, pending.Generation);
                            SetRowLimitStatus(pending.Generation);
                        }

                        continue;
                    }

                    UpsertSourceRow(snapshot);
                    if (added)
                    {
                        dirtyGroups.Add((record.ApiVersion, record.Kind, record.DiffField));
                    }
                }

                readTask = reader.WaitToReadAsync(cancellationToken).AsTask();
            }

            RefreshDirtyGroups();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Crossplane provider diff records");
            SetStatus(ex.Message);
        }
        finally
        {
            _rowsSource.Dispose();
        }

        void RefreshDirtyGroups()
        {
            foreach (var group in dirtyGroups)
            {
                foreach (var row in aggregator.GetGroupSnapshot(group.ApiVersion, group.Kind, group.DiffField))
                {
                    UpsertSourceRow(row);
                }
            }

            dirtyGroups.Clear();
        }
    }

    private void UpsertSourceRow(CrossplaneDiffRow snapshot)
    {
        _rowsSource.AddOrUpdate(snapshot);
    }

    private void SetStatus(string status, long? generation = null)
    {
        void UpdateStatus()
        {
            if (_disposed || (generation.HasValue && generation.Value != Volatile.Read(ref _generation)))
            {
                return;
            }

            Status = status;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            UpdateStatus();
        }
        else
        {
            Dispatcher.UIThread.Post(UpdateStatus);
        }
    }

    private void SetMonitoringStatus(long generation)
    {
        if (Volatile.Read(ref _rowLimitGeneration) == generation)
        {
            SetRowLimitStatus(generation);
        }
        else if (Volatile.Read(ref _truncatedLogGeneration) == generation)
        {
            SetTruncatedStatus(generation);
        }
        else
        {
            SetStatus(string.Empty, generation);
        }
    }

    private void SetRowLimitStatus(long generation)
    {
        SetStatus(
            string.Format(
                CultureInfo.CurrentCulture,
                Assets.Resources.MRDiffDetectionView_RowLimitReached!,
                MaximumRetainedDiffRows),
            generation);
    }

    private void SetTruncatedStatus(long generation)
    {
        void UpdateStatus()
        {
            if (_disposed
                || generation != Volatile.Read(ref _generation)
                || Volatile.Read(ref _rowLimitGeneration) == generation)
            {
                return;
            }

            Status = Assets.Resources.MRDiffDetectionView_TruncatedLog!;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            UpdateStatus();
        }
        else
        {
            Dispatcher.UIThread.Post(UpdateStatus);
        }
    }

    public void Dispose()
    {
        Task[] seedTasks;
        lock (_seedTasksGate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            seedTasks = _seedTasks.ToArray();
        }

        _processingCancellation.Cancel();
        _monitor.Dispose();
        _pendingRecords.Writer.TryComplete();
        _tableSource.Dispose();

        _providerSubscription?.Dispose();
        _providerSubscription = null;
        _podSubscription?.Dispose();
        _podSubscription = null;
        _providerResources = null;

        _ = DisposeBackgroundWorkAsync(seedTasks);
        GC.SuppressFinalize(this);
    }

    private async Task DisposeBackgroundWorkAsync(Task[] seedTasks)
    {
        try
        {
            await _processingTask.ConfigureAwait(false);
            await Task.WhenAll(seedTasks).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to finish Crossplane diff background work during disposal");
        }
        finally
        {
            _processingCancellation.Dispose();
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        _tableSource.SearchText = value;
    }

    private readonly record struct PendingRecord(long Generation, CrossplaneDiffRecord? Record, bool IsReset = false)
    {
        public static PendingRecord Reset(long generation) => new(generation, null, IsReset: true);
    }
}

public sealed record CrossplaneProviderOption(string Name, GenericKubernetesObject Resource);
