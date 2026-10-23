using System.Collections.Specialized;
using System.ComponentModel;
using DynamicData;
using Helm.NET;
using Avalonia.Threading;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using k8s.Models;
using NuGet.Versioning;

namespace KubeUI.Avalonia.Features.Helm.Charts;

/// <summary>Searches indexed Helm charts and opens prefilled install editors.</summary>
public sealed partial class HelmChartsViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private readonly IHelmService _helmService;
    private readonly IHelmDocumentOpener _documentOpener;
    private readonly SourceCache<HelmChartSearchResult, string> _charts = new(chart => chart.Reference);
    private readonly DynamicTableViewSource<HelmChartSearchResult, string> _tableSource;
    private IReadOnlyList<HelmChartSearchResult> _matches = [];
    private HelmConnection? _connection;
    private CancellationTokenSource? _searchDebounce;
    private static readonly TimeSpan SearchDebounce = TimeSpan.FromMilliseconds(300);
    private readonly ObservableCollection<V1Namespace> _localSelectedNamespaces = [];

    /// <summary>Workspace owning this chart document.</summary>
    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    /// <summary>Configured repository names.</summary>
    public ObservableCollection<string> Repositories { get; } = [];

    /// <summary>Repository filtering search results.</summary>
    [ObservableProperty]
    public partial string? SelectedRepository { get; set; }

    /// <summary>Chart-name substring to search.</summary>
    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    /// <summary>Visible chart count.</summary>
    [ObservableProperty]
    public partial int ItemCount { get; private set; }

    /// <summary>Discovery guidance and operation errors.</summary>
    [ObservableProperty]
    public partial string Status { get; private set; } = Assets.Resources.HelmChartsView_SearchNotice!;

    /// <summary>Whether discovery is running.</summary>
    [ObservableProperty]
    public partial bool IsLoading { get; private set; }

    /// <summary>Whether namespace selection is linked to the cluster workspace.</summary>
    [ObservableProperty]
    public partial bool IsNamespaceSelectionLinked { get; set; } = true;

    /// <summary>Selected namespaces for install target; linked to cluster when <see cref="IsNamespaceSelectionLinked"/> is true.</summary>
    public ObservableCollection<V1Namespace> SelectedNamespaces
        => IsNamespaceSelectionLinked && Cluster != null ? Cluster.SelectedNamespaces : _localSelectedNamespaces;

    /// <summary>Table data, with one row per qualified chart reference.</summary>
    public IDynamicTableViewSource TableSource => _tableSource;

    /// <summary>Creates the charts document.</summary>
    public HelmChartsViewModel(IHelmService helmService, IHelmDocumentOpener documentOpener)
    {
        _helmService = helmService;
        _documentOpener = documentOpener;
        _tableSource = new DynamicTableViewSource<HelmChartSearchResult, string>(
            _charts, chart => chart.Reference,
            [
                CreateColumn("chart", Assets.Resources.HelmChartsView_Chart!, static chart => chart.ChartName, 2),
                CreateColumn("reference", Assets.Resources.HelmChartsView_Reference!, static chart => chart.Reference.Split('/')[0], 1),
                CreateColumn("version", Assets.Resources.HelmChartsView_LatestVersion!, static chart => chart.Version, 1)
            ],
            options: new DynamicTableViewSourceOptions { SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Key });
        _tableSource.SelectionModel.SingleSelect = true;
        _tableSource.SetSort([new("chart", ListSortDirection.Ascending)]);
        Title = Assets.Resources.HelmChartsView_Title;
    }

    /// <inheritdoc />
    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
        UpdateConnectionNamespace();
        Id = $"{nameof(HelmChartsViewModel)}-{cluster.Runtime.Name}";
        _ = RefreshCommand.ExecuteAsync(null);
    }

    partial void OnIsNamespaceSelectionLinkedChanged(bool value)
    {
        if (Cluster == null)
        {
            OnPropertyChanged(nameof(SelectedNamespaces));
            return;
        }

        if (!value)
        {
            CopyNamespaces(Cluster.SelectedNamespaces, _localSelectedNamespaces);
        }

        SubscribeToSelectedNamespaces();
        OnPropertyChanged(nameof(SelectedNamespaces));
        UpdateConnectionNamespace();
    }

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateConnectionNamespace();
    }

    private void UpdateConnectionNamespace()
    {
        if (_connection is not null)
        {
            var namespaceName = SelectedNamespaces.FirstOrDefault()?.Metadata?.Name ?? "default";
            _connection = new HelmConnection(
                _connection.KubeConfigPath,
                _connection.Context,
                namespaceName);
        }
    }

    private void SubscribeToSelectedNamespaces()
    {
        UnsubscribeFromSelectedNamespaces();
        SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;
    }

    private void UnsubscribeFromSelectedNamespaces()
    {
        Cluster?.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
        _localSelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
    }

    private static void CopyNamespaces(IEnumerable<V1Namespace> source, ObservableCollection<V1Namespace> target)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task Refresh(CancellationToken cancellationToken)
    {
        if (_connection is null)
        {
            return;
        }

        IsLoading = true;
        try
        {
            var repositories = await _helmService.ListRepositoriesAsync(_connection, cancellationToken);
            var selected = SelectedRepository;
            Repositories.Clear();
            Repositories.Add(Assets.Resources.HelmChartsView_AllRepositories!);
            foreach (var repository in repositories.OrderBy(static repository => repository.Name, StringComparer.OrdinalIgnoreCase))
            {
                Repositories.Add(repository.Name);
            }

            SelectedRepository = selected is not null && Repositories.Contains(selected)
                ? selected : Repositories.FirstOrDefault();
            _matches = await _helmService.ListChartsAsync(_connection, cancellationToken: cancellationToken);
            ApplyRepositoryFilter();
            Status = ItemCount == 0 ? Assets.Resources.Shared_NoCharts! : string.Empty;
        }
        catch (Exception exception)
        {
            _matches = [];
            ApplyRepositoryFilter();
            Status = exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanRefresh() => !IsLoading;

    private static DynamicTableViewColumn<HelmChartSearchResult> CreateColumn<TValue>(
        string key,
        string header,
        Func<HelmChartSearchResult, TValue> selector,
        double starWidth)
    {
        var column = DynamicTableViewColumn<HelmChartSearchResult>.Create(key, header, selector);
        column.WidthMode = DynamicTableViewWidthMode.Star;
        column.Width = starWidth;
        return column;
    }

    [RelayCommand]
    private async Task InstallChart(HelmChartSearchResult? chart)
    {
        if (chart is null || Cluster is null || _connection is null)
        {
            return;
        }

        try
        {
            await _documentOpener.OpenReleaseEditorForChartAsync(Cluster, _connection, chart);
            Status = string.Empty;
        }
        catch (Exception exception)
        {
            Status = exception.Message;
        }
    }

    partial void OnIsLoadingChanged(bool value) => RefreshCommand.NotifyCanExecuteChanged();

    partial void OnSelectedRepositoryChanged(string? value) => ApplyRepositoryFilter();

    partial void OnSearchTextChanged(string value)
    {
        CancellationTokenSource next = new();
        CancellationTokenSource? previous = Interlocked.Exchange(ref _searchDebounce, next);
        previous?.Cancel();
        previous?.Dispose();
        _ = ApplySearchAfterDelayAsync(next);
    }

    private async Task ApplySearchAfterDelayAsync(CancellationTokenSource debounce)
    {
        try
        {
            await Task.Delay(SearchDebounce, debounce.Token).ConfigureAwait(false);
            Dispatcher.UIThread.Post(() =>
            {
                if (!debounce.IsCancellationRequested && ReferenceEquals(Volatile.Read(ref _searchDebounce), debounce))
                {
                    ApplyRepositoryFilter();
                }
            });
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ApplyRepositoryFilter()
    {
        var allRepositories = string.Equals(SelectedRepository, Assets.Resources.HelmChartsView_AllRepositories, StringComparison.Ordinal);
        Dictionary<string, (HelmChartSearchResult Chart, NuGetVersion Version)> latestByReference = new(StringComparer.Ordinal);
        foreach (var chart in _matches)
        {
            if (!allRepositories && SelectedRepository is not null && !chart.Reference.StartsWith($"{SelectedRepository}/", StringComparison.Ordinal)
                || (!string.IsNullOrWhiteSpace(SearchText)
                    && !chart.ChartName.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (!NuGetVersion.TryParse(chart.Version, out var version) || version.IsPrerelease)
            {
                continue;
            }

            if (!latestByReference.TryGetValue(chart.Reference, out var current)
                || VersionComparer.VersionRelease.Compare(version, current.Version) > 0)
            {
                latestByReference[chart.Reference] = (chart, version);
            }
        }

        _charts.Edit(updater =>
        {
            updater.Clear();
            updater.AddOrUpdate(latestByReference.Values.Select(static item => item.Chart));
        });
        ItemCount = _charts.Count;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        UnsubscribeFromSelectedNamespaces();
        var debounce = Interlocked.Exchange(ref _searchDebounce, null);
        debounce?.Cancel();
        debounce?.Dispose();
        _tableSource.Dispose();
        _charts.Dispose();
    }
}
