using System.Collections.Specialized;
using System.ComponentModel;
using DynamicData;
using Helm.NET;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace KubeUI.Avalonia.Features.Helm.Charts;

/// <summary>Searches indexed Helm charts and opens prefilled install editors.</summary>
public sealed partial class HelmChartsViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private readonly IHelmService _helmService;
    private readonly IHelmDocumentOpener _documentOpener;
    private readonly ILogger<HelmChartsViewModel>? _logger;
    private readonly SourceCache<HelmChartSearchResult, string> _charts = new(chart => chart.Reference);
    private readonly DynamicTableViewSource<HelmChartSearchResult, string> _tableSource;
    private readonly INotifyCollectionChanged? _itemsNotifications;
    private IReadOnlyList<HelmChartSearchResult> _matches = [];
    private HelmConnection? _connection;
    private string? _requestedRepository;
    private bool _repositoriesLoaded;

    /// <summary>Workspace owning this chart document.</summary>
    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    /// <summary>Configured repository names.</summary>
    public ObservableCollection<string> Repositories { get; } = [];

    /// <summary>Repository filtering search results.</summary>
    [ObservableProperty]
    public partial string? SelectedRepository { get; set; }

    /// <summary>Chart-name search query.</summary>
    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    /// <summary>Visible chart count.</summary>
    [ObservableProperty]
    public partial int ItemCount { get; private set; }

    /// <summary>Discovery guidance and operation errors.</summary>
    [ObservableProperty]
    public partial string Status { get; private set; } = string.Empty;

    /// <summary>Whether discovery is running.</summary>
    [ObservableProperty]
    public partial bool IsLoading { get; private set; }

    /// <summary>Table data, with one row per qualified chart reference.</summary>
    public IDynamicTableViewSource TableSource => _tableSource;

    /// <summary>Creates the charts document.</summary>
    public HelmChartsViewModel(IHelmService helmService, IHelmDocumentOpener documentOpener, ILogger<HelmChartsViewModel>? logger = null)
    {
        _helmService = helmService;
        _documentOpener = documentOpener;
        _logger = logger;
        _tableSource = new DynamicTableViewSource<HelmChartSearchResult, string>(
            _charts, chart => chart.Reference,
            [
                CreateColumn("chart", Assets.Resources.HelmChartsView_Chart!, static chart => chart.ChartName, 2),
                CreateColumn("description", Assets.Resources.HelmChartsView_Description!, static chart => chart.Description, 1),
                CreateColumn("version", Assets.Resources.HelmChartsView_LatestVersion!, static chart => chart.Version, 1)
            ],
            options: new DynamicTableViewSourceOptions { SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Key });
        _tableSource.SelectionModel.SingleSelect = true;
        _tableSource.SetSort([new("chart", ListSortDirection.Ascending)]);
        _tableSource.SearchText = SearchText;
        _itemsNotifications = _tableSource.Items as INotifyCollectionChanged;
        if (_itemsNotifications is not null)
        {
            _itemsNotifications.CollectionChanged += ItemsOnCollectionChanged;
        }

        Title = Assets.Resources.HelmChartsView_Title;
    }

    /// <inheritdoc />
    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
        _connection = new HelmConnection(
            cluster.Runtime.KubeConfigPath,
            cluster.Runtime.Name,
            cluster.SelectedNamespaces.FirstOrDefault()?.Metadata?.Name ?? "default");
        Id = $"{nameof(HelmChartsViewModel)}-{cluster.Runtime.Name}";
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
            var selected = _requestedRepository ?? SelectedRepository;
            Repositories.Clear();
            foreach (var repository in repositories.OrderBy(static repository => repository.Name, StringComparer.OrdinalIgnoreCase))
            {
                Repositories.Add(repository.Name);
            }

            _repositoriesLoaded = true;
            _requestedRepository = null;
            SelectedRepository = selected is not null && Repositories.Contains(selected)
                ? selected : Repositories.FirstOrDefault();
            _matches = await _helmService.ListChartsAsync(_connection, cancellationToken: cancellationToken);
            ApplyRepositoryFilter();
            Status = _charts.Count == 0 ? Assets.Resources.Shared_NoCharts! : string.Empty;
        }
        catch (Exception exception)
        {
            _matches = [];
            ApplyRepositoryFilter();
            Status = Assets.Resources.Shared_ErrorOccurred;
            _logger?.LogError(exception, "Helm refresh failed");
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
            Status = Assets.Resources.Shared_ErrorOccurred;
            _logger?.LogError(exception, "Failed to open install editor");
        }
    }

    partial void OnIsLoadingChanged(bool value) => RefreshCommand.NotifyCanExecuteChanged();

    partial void OnSelectedRepositoryChanged(string? value) => ApplyRepositoryFilter();

    partial void OnSearchTextChanged(string value) => _tableSource.SearchText = value;

    /// <summary>Applies an explicit repository filter to the chart list.</summary>
    /// <param name="repositoryName">Repository name to filter by.</param>
    public void SelectRepository(string repositoryName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryName);
        if (_repositoriesLoaded)
        {
            SelectedRepository = repositoryName;
        }
        else
        {
            _requestedRepository = repositoryName;
        }
    }

    private void ApplyRepositoryFilter()
    {
        Dictionary<string, (HelmChartSearchResult Chart, NuGetVersion Version)> latestByReference = new(StringComparer.Ordinal);
        foreach (var chart in _matches)
        {
            if (SelectedRepository is not null && !chart.Reference.StartsWith($"{SelectedRepository}/", StringComparison.Ordinal))
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
        UpdateItemCount();
    }

    private void ItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        => UpdateItemCount();

    private void UpdateItemCount()
    {
        if (_tableSource.Items is ICollection items)
        {
            ItemCount = items.Count;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_itemsNotifications is not null)
        {
            _itemsNotifications.CollectionChanged -= ItemsOnCollectionChanged;
        }

        _tableSource.Dispose();
        _charts.Dispose();
    }
}
