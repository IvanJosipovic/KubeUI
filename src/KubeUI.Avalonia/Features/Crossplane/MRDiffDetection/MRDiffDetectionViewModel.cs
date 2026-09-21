using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
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
using Dock.Model.Core;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;

public sealed partial class MRDiffDetectionViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private readonly CrossplaneDiffLogParser _parser;
    private readonly CrossplaneDiffAggregator _aggregator = new();
    private readonly CrossplaneProviderLogMonitor _monitor;
    private readonly IServiceProvider _serviceProvider;
    private readonly IFactory _factory;
    private readonly ConcurrentQueue<CrossplaneDiffRecord> _pendingRecords = new();
    private IDisposable? _providerSubscription;
    private IDisposable? _podSubscription;
    private ISourceCache<GenericKubernetesObject, ResourceCacheKey>? _providerResources;
    private HashSet<string> _activePodKeys = new(StringComparer.Ordinal);
    private DispatcherTimer? _rowsRefreshTimer;
    private bool _disposed;

    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    [ObservableProperty]
    public partial CrossplaneProviderOption? SelectedProvider { get; set; }

    [ObservableProperty]
    public partial string Status { get; private set; } = string.Empty;

    public ObservableCollection<CrossplaneProviderOption> Providers { get; } = [];
    public ObservableCollection<CrossplaneDiffRow> Rows { get; } = [];

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
        Title = Assets.Resources.MRDiffDetectionView_Title!;
        _rowsRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _rowsRefreshTimer.Tick += RowsRefreshTimer_Tick;
        _rowsRefreshTimer.Start();
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
        DrainPendingRecords();
        _aggregator.Clear();

        Rows.Clear();
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
            DrainPendingRecords();
            _aggregator.Clear();
            Rows.Clear();
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
            _pendingRecords.Enqueue(record);
        }
    }

    private void RowsRefreshTimer_Tick(object? sender, EventArgs e)
    {
        if (_disposed)
        {
            return;
        }

        const int maxRecordsPerRefresh = 100;
        var processedRecords = 0;
        while (processedRecords < maxRecordsPerRefresh && _pendingRecords.TryDequeue(out var record))
        {
            _aggregator.Add(record);
            processedRecords++;
        }

        if (processedRecords == 0)
        {
            return;
        }

        var rows = _aggregator.GetSnapshot()
            .OrderBy(row => row.ApiVersion)
            .ThenBy(row => row.Kind)
            .ThenBy(row => row.DiffField)
            .ThenBy(row => row.Namespace)
            .ThenBy(row => row.Name);
        var existingRows = Rows.ToDictionary(row => row.Key, StringComparer.Ordinal);
        foreach (var snapshot in rows)
        {
            if (existingRows.TryGetValue(snapshot.Key, out var existing))
            {
                existing.Apply(snapshot);
            }
            else
            {
                Rows.Add(snapshot);
            }
        }
    }

    private void DrainPendingRecords()
    {
        while (_pendingRecords.TryDequeue(out _))
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
        if (_rowsRefreshTimer is not null)
        {
            _rowsRefreshTimer.Stop();
            _rowsRefreshTimer.Tick -= RowsRefreshTimer_Tick;
            _rowsRefreshTimer = null;
        }

        _providerSubscription?.Dispose();
        _providerSubscription = null;
        _podSubscription?.Dispose();
        _podSubscription = null;
        _providerResources = null;

        _monitor.Dispose();
        GC.SuppressFinalize(this);
    }
}

public sealed record CrossplaneProviderOption(string Name, GenericKubernetesObject Resource);
