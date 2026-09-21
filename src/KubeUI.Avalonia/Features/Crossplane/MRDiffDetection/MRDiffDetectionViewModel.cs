using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Channels;
using Avalonia.Controls;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Avalonia.Infrastructure.DataGrid;
using KubeUI.Avalonia.Resources;
using Dock.Model.Core;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;

public sealed partial class MRDiffDetectionViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private readonly CrossplaneDiffLogParser _parser;
    private readonly CrossplaneProviderLogMonitor _monitor;
    private readonly IServiceProvider _serviceProvider;
    private readonly IFactory _factory;
    private readonly Channel<PendingRecord> _pendingRecords = Channel.CreateUnbounded<PendingRecord>();
    private readonly ConcurrentDictionary<string, CrossplaneDiffRow> _pendingRows = new(StringComparer.Ordinal);
    private readonly CancellationTokenSource _processingCancellation = new();
    private readonly SourceCache<CrossplaneDiffRow, string> _rowsSource = new(row => row.Key);
    private readonly Subject<IComparer<CrossplaneDiffRow>> _sortSubject;
    private readonly Subject<Func<CrossplaneDiffRow, bool>> _filterSubject;
    private readonly Subject<Func<CrossplaneDiffRow, bool>> _searchSubject;
    private IDisposable? _rowsSubscription;
    private ReadOnlyObservableCollection<CrossplaneDiffRow>? _view;
    private IDisposable? _providerSubscription;
    private IDisposable? _podSubscription;
    private ISourceCache<GenericKubernetesObject, ResourceCacheKey>? _providerResources;
    private HashSet<string> _activePodKeys = new(StringComparer.Ordinal);
    private bool _disposed;
    private long _generation;

    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    [ObservableProperty]
    public partial CrossplaneProviderOption? SelectedProvider { get; set; }

    [ObservableProperty]
    public partial string Status { get; private set; } = string.Empty;

    public ObservableCollection<CrossplaneProviderOption> Providers { get; } = [];
    public IList View => _view ?? throw new InvalidOperationException("MR diff view has not been initialized.");
    public ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; } = [];
    public IDataGridSortingAdapterFactory SortingAdapterFactory => _sortingAdapterFactory;
    public IDataGridFilteringAdapterFactory FilteringAdapterFactory => _filteringAdapterFactory;
    public IDataGridSearchAdapterFactory SearchAdapterFactory => _searchAdapterFactory;
    public ISortingModel SortingModel { get; } = new SortingModel { MultiSort = true, CycleMode = SortCycleMode.AscendingDescendingNone, OwnsViewSorts = true };
    public IFilteringModel FilteringModel { get; } = new FilteringModel { OwnsViewFilter = true };
    public ISearchModel SearchModel { get; } = new SearchModel { HighlightMode = SearchHighlightMode.None, HighlightCurrent = false, WrapNavigation = true, UpdateSelectionOnNavigate = false };
    private readonly DynamicDataSortingAdapterFactory<CrossplaneDiffRow> _sortingAdapterFactory;
    private readonly DynamicDataFilteringAdapterFactory<CrossplaneDiffRow> _filteringAdapterFactory;
    private readonly DynamicDataSearchAdapterFactory<CrossplaneDiffRow> _searchAdapterFactory;

    public MRDiffDetectionViewModel(
        CrossplaneDiffLogParser parser,
        CrossplaneProviderLogMonitor monitor,
        IServiceProvider serviceProvider,
        IFactory factory)
    {
        _parser = parser;
        _monitor = monitor;
        _serviceProvider = serviceProvider;
        _factory = factory;
        var columns = CreateColumns();
        var columnsByKey = columns.ToDictionary(column => column.Key, StringComparer.OrdinalIgnoreCase);
        _sortingAdapterFactory = new DynamicDataSortingAdapterFactory<CrossplaneDiffRow>(columnsByKey);
        _filteringAdapterFactory = new DynamicDataFilteringAdapterFactory<CrossplaneDiffRow>(columnsByKey);
        _searchAdapterFactory = new DynamicDataSearchAdapterFactory<CrossplaneDiffRow>(columnsByKey);
        _sortSubject = new();
        _filterSubject = new();
        _searchSubject = new();
        _sortSubject.OnNext(_sortingAdapterFactory.SortComparer);
        _filterSubject.OnNext(_filteringAdapterFactory.FilterPredicate);
        _searchSubject.OnNext(_searchAdapterFactory.SearchPredicate);
        SortingModel.SortingChanged += SortingModelOnSortingChanged;
        FilteringModel.FilteringChanged += FilteringModelOnFilteringChanged;
        SearchModel.SearchChanged += SearchModelOnSearchChanged;
        BuildColumnDefinitions(columns);
        _rowsSubscription = _rowsSource.Connect()
            .ObserveOn(TaskPoolScheduler.Default)
            .Filter(_filterSubject)
            .Filter(_searchSubject)
            .SortAndBind(out _view, _sortSubject, new() { ResetOnFirstTimeLoad = true, UseReplaceForUpdates = true, Scheduler = AvaloniaScheduler.Instance })
            .Subscribe();
        OnPropertyChanged(nameof(View));
        _ = ProcessRecordsAsync(_processingCancellation.Token);
        _ = FlushRowsAsync(_processingCancellation.Token);
        Title = Assets.Resources.MRDiffDetectionView_Title!;
    }

    private static IReadOnlyList<IResourceListColumn> CreateColumns()
    {
        return [
            new DataGridValueColumn<CrossplaneDiffRow, string> { Key = "name", Name = Assets.Resources.MRDiffDetectionView_Name!, Field = row => row.Name },
            new DataGridValueColumn<CrossplaneDiffRow, string> { Key = "namespace", Name = Assets.Resources.MRDiffDetectionView_Namespace!, Field = row => row.Namespace },
            new DataGridValueColumn<CrossplaneDiffRow, string> { Key = "apiVersion", Name = Assets.Resources.MRDiffDetectionView_ApiVersion!, Field = row => row.ApiVersion },
            new DataGridValueColumn<CrossplaneDiffRow, string> { Key = "kind", Name = Assets.Resources.MRDiffDetectionView_Kind!, Field = row => row.Kind, Sort = SortDirection.Ascending },
            new DataGridValueColumn<CrossplaneDiffRow, string> { Key = "diffField", Name = Assets.Resources.MRDiffDetectionView_DiffField!, Field = row => row.DiffField },
            new DataGridValueColumn<CrossplaneDiffRow, string> { Key = "oldValue", Name = Assets.Resources.MRDiffDetectionView_OldValue!, Field = row => row.OldValue },
            new DataGridValueColumn<CrossplaneDiffRow, string> { Key = "newValue", Name = Assets.Resources.MRDiffDetectionView_NewValue!, Field = row => row.NewValue },
            new DataGridValueColumn<CrossplaneDiffRow, bool> { Key = "newComputed", Name = Assets.Resources.MRDiffDetectionView_NewComputed!, Field = row => row.NewComputed },
            new DataGridValueColumn<CrossplaneDiffRow, bool> { Key = "newRemoved", Name = Assets.Resources.MRDiffDetectionView_NewRemoved!, Field = row => row.NewRemoved },
            new DataGridValueColumn<CrossplaneDiffRow, bool> { Key = "requiresNew", Name = Assets.Resources.MRDiffDetectionView_RequiresNew!, Field = row => row.RequiresNew },
            new DataGridValueColumn<CrossplaneDiffRow, bool> { Key = "sensitive", Name = Assets.Resources.MRDiffDetectionView_Sensitive!, Field = row => row.Sensitive },
            new DataGridValueColumn<CrossplaneDiffRow, int> { Key = "instanceCount", Name = Assets.Resources.MRDiffDetectionView_InstanceCount!, Field = row => row.InstanceCount },
            new DataGridValueColumn<CrossplaneDiffRow, int> { Key = "occurrences", Name = Assets.Resources.MRDiffDetectionView_Occurrences!, Field = row => row.Occurrences }
        ];
    }

    private void BuildColumnDefinitions(IReadOnlyList<IResourceListColumn> columns)
    {
        var converter = new DataGridLengthConverter();
        foreach (var column in columns)
        {
            var binding = DataGridBindingDefinition.Create<CrossplaneDiffRow, CrossplaneDiffRow>(row => row);
            binding.Mode = BindingMode.OneWay;
            binding.Converter = new FuncValueConverter<CrossplaneDiffRow, string>(row => column.DisplayValue(row));
            var definition = new DataGridTextColumnDefinition
            {
                Header = column.Name,
                ColumnKey = column.Key,
                Tag = column,
                Binding = binding,
                CanUserSort = true,
                ShowFilterButton = true,
                MinWidth = column.MinWidth,
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                ValueAccessor = column.ValueAccessor,
                ValueType = column.ValueType
            };
            ColumnDefinitions.Add(definition);
            if (column.Sort != SortDirection.None)
            {
                SortingModel.SetOrUpdate(new(definition,
                    column.Sort == SortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending,
                    null,
                    Comparer<object>.Create(static (_, _) => 0)));
            }
        }
    }

    private void SortingModelOnSortingChanged(object? sender, SortingChangedEventArgs e)
    {
        _sortingAdapterFactory.UpdateComparer(e.NewDescriptors);
        _sortSubject.OnNext(_sortingAdapterFactory.SortComparer);
    }

    private void FilteringModelOnFilteringChanged(object? sender, FilteringChangedEventArgs e)
    {
        _filteringAdapterFactory.UpdateFilter(e.NewDescriptors);
        _filterSubject.OnNext(_filteringAdapterFactory.FilterPredicate);
    }

    private void SearchModelOnSearchChanged(object? sender, SearchChangedEventArgs e)
    {
        _searchAdapterFactory.UpdatePredicate(e.NewDescriptors);
        _searchSubject.OnNext(_searchAdapterFactory.SearchPredicate);
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

        Cluster = cluster;
        Id = $"{nameof(MRDiffDetectionViewModel)}-{cluster.Runtime.Name}";

        var providerConfig = cluster.GetResourceConfigs().FirstOrDefault(config =>
            config.IsCustomResource
            && string.Equals(config.Kind.Group, "pkg.crossplane.io", StringComparison.Ordinal)
            && string.Equals(config.Kind.Kind, "Provider", StringComparison.Ordinal));
        if (providerConfig is null)
        {
            Providers.Clear();
            return;
        }

        _ = SeedAndBindResourcesAsync(cluster, providerConfig.Kind);
    }

    partial void OnSelectedProviderChanged(CrossplaneProviderOption? value)
    {
        _activePodKeys = value is null || Cluster is null
            ? new HashSet<string>(StringComparer.Ordinal)
            : CrossplaneProviderLogMonitor.GetProviderPodKeys(Cluster.Runtime, value.Name);
        _ = StartMonitoringAsync(value);
    }

    private void ReconcileProviderPods()
    {
        if (_disposed || Cluster is null || SelectedProvider is null)
        {
            return;
        }

        var podKeys = CrossplaneProviderLogMonitor.GetProviderPodKeys(Cluster.Runtime, SelectedProvider.Name);
        if (_activePodKeys.SetEquals(podKeys))
        {
            return;
        }

        _activePodKeys = podKeys;
        _ = StartMonitoringAsync(SelectedProvider, resetRows: false);
    }

    [RelayCommand]
    private void Clear()
    {
        Interlocked.Increment(ref _generation);
        DrainPendingRecords();
        _pendingRows.Clear();
        _rowsSource.Clear();
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
                cluster.Runtime.SeedResource(providerKind, waitForReady: true),
                cluster.Runtime.SeedResource(GroupApiVersionKind.From<V1Pod>(), waitForReady: true)).ConfigureAwait(false);

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
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    private async Task StartMonitoringAsync(CrossplaneProviderOption? provider, bool resetRows = true)
    {
        if (Cluster is null || _disposed)
        {
            return;
        }

        if (resetRows)
        {
            Interlocked.Increment(ref _generation);
            DrainPendingRecords();
            _pendingRows.Clear();
            _rowsSource.Clear();
        }
        if (provider is null)
        {
            Status = Assets.Resources.MRDiffDetectionView_SelectProvider!;
            return;
        }

        Status = string.Format(Assets.Resources.MRDiffDetectionView_Connecting!, provider.Name);
        try
        {
            Status = string.Format(Assets.Resources.MRDiffDetectionView_Monitoring!, provider.Name);
            await _monitor.StartAsync(Cluster.Runtime, provider.Resource, HandleLogLine, CancellationToken.None).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    private void HandleLogLine(string line)
    {
        foreach (var record in _parser.Parse(line))
        {
            _pendingRecords.Writer.TryWrite(new PendingRecord(Volatile.Read(ref _generation), record));
        }
    }

    private void DrainPendingRecords()
    {
        while (_pendingRecords.Reader.TryRead(out _))
        {
        }
    }

    private async Task ProcessRecordsAsync(CancellationToken cancellationToken)
    {
        var aggregator = new CrossplaneDiffAggregator();
        var activeGeneration = Volatile.Read(ref _generation);
        try
        {
            await foreach (var pending in _pendingRecords.Reader.ReadAllAsync(cancellationToken))
            {
                if (pending.Generation != activeGeneration)
                {
                    aggregator = new CrossplaneDiffAggregator();
                    activeGeneration = pending.Generation;
                }

                foreach (var row in aggregator.AddAndGetAffectedRows(pending.Record))
                {
                    _pendingRows[row.Key] = row;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async Task FlushRowsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken).ConfigureAwait(false);
                if (_pendingRows.IsEmpty)
                {
                    continue;
                }

                var batch = new List<CrossplaneDiffRow>(Math.Min(_pendingRows.Count, 500));
                foreach (var pair in _pendingRows)
                {
                    if (batch.Count >= 500 || !_pendingRows.TryRemove(pair.Key, out var row))
                    {
                        break;
                    }

                    batch.Add(row);
                }

                if (batch.Count > 0)
                {
                    _rowsSource.AddOrUpdate(batch);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _processingCancellation.Cancel();
        _pendingRecords.Writer.TryComplete();
        _rowsSubscription?.Dispose();
        _rowsSubscription = null;
        _rowsSource.Dispose();

        _providerSubscription?.Dispose();
        _providerSubscription = null;
        _podSubscription?.Dispose();
        _podSubscription = null;
        _providerResources = null;

        _monitor.Dispose();
        _processingCancellation.Dispose();
        GC.SuppressFinalize(this);
    }

    private readonly record struct PendingRecord(long Generation, CrossplaneDiffRecord Record);
}

public sealed record CrossplaneProviderOption(string Name, GenericKubernetesObject Resource);

