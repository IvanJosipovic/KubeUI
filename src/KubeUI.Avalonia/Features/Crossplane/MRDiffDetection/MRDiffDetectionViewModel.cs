using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;

public sealed partial class MRDiffDetectionViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private readonly CrossplaneDiffLogParser _parser;
    private readonly CrossplaneDiffAggregator _aggregator = new();
    private readonly CrossplaneProviderLogMonitor _monitor;
    private IDisposable? _providerSubscription;
    private ISourceCache<GenericKubernetesObject, ResourceCacheKey>? _providerResources;
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
        CrossplaneProviderLogMonitor monitor)
    {
        _parser = parser;
        _monitor = monitor;
        Title = Assets.Resources.MRDiffDetectionView_Title!;
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
        _ = StartMonitoringAsync(value);
    }

    [RelayCommand]
    private void Clear()
    {
        _aggregator.Clear();
        Rows.Clear();
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
                RefreshProviders();
            });
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    private async Task StartMonitoringAsync(CrossplaneProviderOption? provider)
    {
        if (Cluster is null || _disposed)
        {
            return;
        }

        Rows.Clear();
        _aggregator.Clear();
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
            Dispatcher.UIThread.Post(() =>
            {
                if (_disposed)
                {
                    return;
                }

                _aggregator.Add(record);
                Rows.Clear();
                foreach (var row in _aggregator.Rows.OrderBy(row => row.ApiVersion).ThenBy(row => row.Kind).ThenBy(row => row.DiffField).ThenBy(row => row.Namespace).ThenBy(row => row.Name))
                {
                    Rows.Add(row);
                }
            });
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _providerSubscription?.Dispose();
        _providerSubscription = null;
        _providerResources = null;

        _monitor.Dispose();
        GC.SuppressFinalize(this);
    }
}

public sealed record CrossplaneProviderOption(string Name, GenericKubernetesObject Resource);
