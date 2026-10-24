using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using Helm.NET;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using YamlDotNet.Core;

namespace KubeUI.Avalonia.Features.Helm.Releases;

public sealed partial class HelmReleaseEditorViewModel : ViewModelBase
{
    private readonly IHelmService _helmService;
    private readonly IFactory _factory;
    private readonly IDialogService _dialogService;
    private readonly IHelmListNotifier _listNotifier;
    private readonly IHelmReleasePreviewService _previewService;
    private HelmConnection? _connection;
    private ClusterWorkspace? _cluster;
    private bool _editing;
    private bool _valuesLoaded = true;
    private HelmRelease? _release;
    private IReadOnlyList<HelmChartSearchResult> _chartMatches = [];
    private string _installedVersion = string.Empty;
    private string _installedChartName = string.Empty;
    private bool _updatingChartSelection;

    /// <summary>Configured repositories and repositories returned by chart search.</summary>
    public ObservableCollection<string> Repositories { get; } = [];

    /// <summary>Chart names matching the search in the selected repository.</summary>
    public ObservableCollection<string> AvailableCharts { get; } = [];

    /// <summary>Repository selected for chart discovery.</summary>
    [ObservableProperty]
    public partial string? SelectedRepository { get; set; }

    /// <summary>Chart selected within the repository.</summary>
    [ObservableProperty]
    public partial string? SelectedChart { get; set; }

    /// <summary>Chart name substring used to search repository indexes.</summary>
    [ObservableProperty]
    public partial string ChartSearch { get; set; } = string.Empty;

    /// <summary>Chart versions available for the selected repository reference, including the installed version.</summary>
    public ObservableCollection<string> AvailableVersions { get; } = [];

    /// <summary>Chart version selected for preview and eventual upgrade.</summary>
    [ObservableProperty]
    public partial string? SelectedVersion { get; set; }

    /// <summary>Explains that Helm uses the selected version for install, upgrade, and preview.</summary>
    public string ChartVersionNotice => Assets.Resources.HelmReleaseEditorView_ChartVersionNotice!;

    [ObservableProperty]
    public partial string ReleaseName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Chart { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Namespace { get; set; } = "default";

    [ObservableProperty]
    public partial string ValuesYaml { get; set; } = "{}";

    /// <summary>Whether installed values and chart metadata are currently loading.</summary>
    [ObservableProperty]
    public partial bool IsLoading { get; private set; }

    /// <summary>Completion of the current editor initialization.</summary>
    public Task InitializationTask { get; private set; } = Task.CompletedTask;

    [ObservableProperty]
    public partial bool CreateNamespace { get; set; }

    [ObservableProperty]
    public partial bool WaitForReady { get; set; } = true;

    [ObservableProperty]
    public partial bool InstallIfMissing { get; set; }

    [ObservableProperty]
    public partial int TimeoutSeconds { get; set; } = 300;

    [ObservableProperty]
    public partial string Error { get; private set; } = string.Empty;

    public bool IsUpgrade => _editing;

    public HelmReleaseEditorViewModel(
        IHelmService helmService,
        IFactory factory,
        IHelmListNotifier listNotifier,
        IHelmReleasePreviewService previewService,
        IDialogService dialogService)
    {
        _helmService = helmService;
        _factory = factory;
        _listNotifier = listNotifier;
        _previewService = previewService;
        _dialogService = dialogService;
    }

    public void Initialize(ClusterWorkspace cluster, HelmConnection connection, HelmRelease? release)
    {
        _cluster = cluster;
        _connection = connection;
        _editing = release is not null;
        _release = release;
        _chartMatches = [];
        _installedVersion = release?.ChartVersion ?? string.Empty;
        _installedChartName = release?.ChartName ?? string.Empty;
        SelectedRepository = null;
        SelectedChart = null;
        Repositories.Clear();
        AvailableCharts.Clear();
        ChartSearch = _installedChartName;
        AvailableVersions.Clear();
        SelectedVersion = null;
        _valuesLoaded = release is null;
        ReleaseName = release?.Name ?? string.Empty;
        Chart = string.Empty;
        ValuesYaml = "{}";
        Error = string.Empty;
        Namespace = release?.Namespace ?? connection.Namespace ?? "default";
        Title = release is null ? Assets.Resources.HelmReleaseEditorView_InstallTitle : Assets.Resources.HelmReleaseEditorView_UpgradeTitle;
        Id = $"{nameof(HelmReleaseEditorViewModel)}-{cluster.Runtime.Name}-{release?.Name ?? Guid.NewGuid().ToString("N")}";
        OnPropertyChanged(nameof(IsUpgrade));
        InitializationTask = LoadReleaseCommand.ExecuteAsync(null);
    }

    /// <summary>Prefills an initialized install editor from a selected indexed chart.</summary>
    public async Task SelectInstallChartAsync(HelmChartSearchResult chart)
    {
        ArgumentNullException.ThrowIfNull(chart);
        await InitializationTask;
        var separator = chart.Reference.IndexOf('/');
        if (separator > 0)
        {
            SelectedRepository = chart.Reference[..separator];
        }

        ChartSearch = chart.ChartName;
        UpdateAvailableCharts();
        SelectedChart = chart.ChartName;
        Chart = chart.Reference;
        SelectedVersion = chart.Version;
    }

    [RelayCommand]
    private async Task LoadRelease(CancellationToken cancellationToken)
    {
        if (_connection is null)
        {
            return;
        }

        IsLoading = true;
        Error = string.Empty;
        try
        {
            HelmRelease? metadata = null;
            if (_release is not null)
            {
                var connection = _connection with { Namespace = _release.Namespace };
                var values = await _helmService.GetValuesAsync(connection, _release.Name, cancellationToken);
                ValuesYaml = HelmValuesYaml.Serialize(values);
                _valuesLoaded = true;
                metadata = string.IsNullOrWhiteSpace(_release.ChartName)
                    ? await _helmService.StatusAsync(connection, _release.Name, cancellationToken)
                    : _release;
                _installedVersion = metadata.ChartVersion;
                _installedChartName = metadata.ChartName;
                ChartSearch = metadata.ChartName;
            }

            var repositories = await _helmService.ListRepositoriesAsync(_connection, cancellationToken);
            foreach (var repository in repositories)
            {
                Repositories.Add(repository.Name);
            }

            var charts = await _helmService.ListChartsAsync(_connection, cancellationToken: cancellationToken);
            SetChartMatches(charts);

            if (metadata is not null)
            {
                var references = _chartMatches.Where(chart => chart.ChartName == metadata.ChartName)
                    .Select(chart => chart.Reference).Distinct(StringComparer.Ordinal).ToArray();
                if (references.Length == 1)
                {
                    Chart = references[0];
                }
                else
                {
                    UpdateAvailableVersions();
                    Error = Assets.Resources.HelmReleaseEditorView_ChooseChartReference!;
                }
            }
        }
        catch (Exception exception)
        {
            Error = exception.Message;
            UpdateAvailableVersions();
        }
        finally
        {
            IsLoading = false;
            SaveCommand.NotifyCanExecuteChanged();
            PreviewCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSearchCharts))]
    private async Task SearchCharts(CancellationToken cancellationToken)
    {
        if (_connection is null)
        {
            return;
        }

        IsLoading = true;
        try
        {
            var matches = await _helmService.ListChartsAsync(_connection, cancellationToken: cancellationToken);
            SetChartMatches(matches);
            Error = _chartMatches.Count == 0 ? Assets.Resources.Shared_NoCharts! : string.Empty;
        }
        catch (Exception exception)
        {
            Error = exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanSearchCharts() => !IsLoading && !string.IsNullOrWhiteSpace(ChartSearch);

    private void SetChartMatches(IReadOnlyList<HelmChartSearchResult> matches)
    {
        _chartMatches = matches;
        foreach (var match in matches)
        {
            var separator = match.Reference.IndexOf('/');
            if (separator > 0)
            {
                var repository = match.Reference[..separator];
                if (!Repositories.Contains(repository))
                {
                    Repositories.Add(repository);
                }
            }
        }

        UpdateAvailableCharts();
        UpdateAvailableVersions();
    }

    partial void OnSelectedRepositoryChanged(string? value)
    {
        if (!_updatingChartSelection)
        {
            UpdateAvailableCharts();
        }
    }

    private void UpdateAvailableCharts()
    {
        var previous = SelectedChart;
        _updatingChartSelection = true;
        try
        {
            AvailableCharts.Clear();
            foreach (var match in _chartMatches)
            {
                if (match.Reference.StartsWith($"{SelectedRepository}/", StringComparison.Ordinal)
                    && (string.IsNullOrWhiteSpace(ChartSearch)
                        || match.ChartName.Contains(ChartSearch.Trim(), StringComparison.OrdinalIgnoreCase))
                    && !AvailableCharts.Contains(match.ChartName))
                {
                    AvailableCharts.Add(match.ChartName);
                }
            }

            SelectedChart = previous is not null && AvailableCharts.Contains(previous) ? previous : null;
        }
        finally
        {
            _updatingChartSelection = false;
        }

        Chart = SelectedChart is null || SelectedRepository is null ? string.Empty : $"{SelectedRepository}/{SelectedChart}";
        if (SelectedChart is null)
        {
            SelectedVersion = null;
        }
    }

    partial void OnSelectedChartChanged(string? value)
    {
        if (!_updatingChartSelection)
        {
            Chart = value is null || SelectedRepository is null ? string.Empty : $"{SelectedRepository}/{value}";
        }
    }

    partial void OnChartSearchChanged(string value) => SearchChartsCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save(CancellationToken cancellationToken = default)
    {
        if (_connection is null || _cluster is null)
        {
            return;
        }

        if (IsUpgrade && !IsCatalogChartReference())
        {
            Error = Assets.Resources.HelmReleaseEditorView_ChooseChartReference!;
            return;
        }

        if (!TryParseValues(out var values, out var error))
        {
            Error = error;
            return;
        }

        var confirmation = await _dialogService.ShowContentDialogAsync(this, new ContentDialogSettings
        {
            Title = _editing
                ? Assets.Resources.HelmReleaseEditorView_ConfirmUpgradeTitle
                : Assets.Resources.HelmReleaseEditorView_ConfirmInstallTitle,
            Content = string.Format(
                _editing
                    ? Assets.Resources.HelmReleaseEditorView_ConfirmUpgradeContent!
                    : Assets.Resources.HelmReleaseEditorView_ConfirmInstallContent!,
                ReleaseName.Trim(),
                Chart.Trim(),
                Namespace.Trim()),
            PrimaryButtonText = Assets.Resources.HelmReleaseEditorView_InstallUpgrade,
            SecondaryButtonText = Assets.Resources.Shared_Cancel,
            DefaultButton = FAContentDialogButton.Secondary
        });

        if (confirmation != FAContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            var connection = _connection with { Namespace = Namespace.Trim() };
            var timeout = TimeoutSeconds > 0 ? TimeSpan.FromSeconds(TimeoutSeconds) : (TimeSpan?)null;
            if (_editing)
            {
                await _helmService.UpgradeAsync(
                    connection,
                    ReleaseName.Trim(),
                    Chart.Trim(),
                    values,
                    installIfMissing: InstallIfMissing,
                    createNamespace: CreateNamespace,
                    wait: WaitForReady,
                    timeout,
                    cancellationToken,
                    version: SelectedVersion);
            }
            else
            {
                await _helmService.InstallAsync(
                    connection,
                    ReleaseName.Trim(),
                    Chart.Trim(),
                    values,
                    CreateNamespace,
                    WaitForReady,
                    timeout,
                    cancellationToken,
                    version: SelectedVersion);
            }

            _listNotifier.NotifyReleasesChanged();
            _factory.RemoveDockable(this, false);
        }
        catch (Exception exception)
        {
            Error = exception.Message;
        }
    }

    [RelayCommand(CanExecute = nameof(CanPreview))]
    private async Task Preview(CancellationToken cancellationToken = default)
    {
        if (_connection is null || _cluster is null)
        {
            return;
        }

        if (!TryParseValues(out var values, out var error))
        {
            Error = error;
            return;
        }

        try
        {
            if (SelectedVersion is not null)
            {
                await _previewService.PreviewVersionAsync(
                    _cluster, _connection with { Namespace = Namespace.Trim() },
                    ReleaseName.Trim(), Chart.Trim(), SelectedVersion!, values, cancellationToken);
            }
            else
            {
                await _previewService.PreviewAsync(
                    _cluster,
                    _connection with { Namespace = Namespace.Trim() },
                    ReleaseName.Trim(),
                    Chart.Trim(),
                    values,
                    cancellationToken);
            }

            Error = string.Empty;
        }
        catch (Exception exception)
        {
            Error = exception.Message;
        }
    }

    [RelayCommand]
    private void Cancel() => _factory.RemoveDockable(this, false);

    private bool CanSave() => CanPreview() && (!IsUpgrade || IsCatalogChartReference());

    private bool IsCatalogChartReference()
        => SelectedRepository is not null
            && SelectedChart is not null
            && _chartMatches.Any(match => match.Reference == Chart
                && match.ChartName == SelectedChart
                && Chart.StartsWith($"{SelectedRepository}/", StringComparison.Ordinal));

    private bool CanPreview()
        => !IsLoading && _valuesLoaded
            && (!IsUpgrade || SelectedVersion is null || AvailableVersions.Contains(SelectedVersion))
            && !string.IsNullOrWhiteSpace(ReleaseName)
            && !string.IsNullOrWhiteSpace(Chart)
            && !string.IsNullOrWhiteSpace(Namespace)
            && TimeoutSeconds >= 0
            && TryParseValues(out _, out _);

    private bool TryParseValues(out IReadOnlyDictionary<string, object?>? values, out string error)
    {
        try
        {
            values = HelmValuesYaml.Parse(ValuesYaml);
            error = string.Empty;
            return true;
        }
        catch (Exception exception) when (exception is YamlException or InvalidOperationException)
        {
            values = null;
            error = exception.Message;
            return false;
        }
    }

    partial void OnReleaseNameChanged(string value)
    {
        SaveCommand.NotifyCanExecuteChanged();
        PreviewCommand.NotifyCanExecuteChanged();
    }

    partial void OnChartChanged(string value)
    {
        if (!_updatingChartSelection)
        {
            _updatingChartSelection = true;
            try
            {
                var match = _chartMatches.FirstOrDefault(item => item.Reference == value);
                if (match is not null)
                {
                    SelectedRepository = value[..value.IndexOf('/')];
                    AvailableCharts.Clear();
                    foreach (var item in _chartMatches)
                    {
                        if (item.Reference == $"{SelectedRepository}/{item.ChartName}" && !AvailableCharts.Contains(item.ChartName))
                        {
                            AvailableCharts.Add(item.ChartName);
                        }
                    }

                    SelectedChart = match.ChartName;
                }
                else
                {
                    SelectedChart = null;
                }

                SelectedVersion = null;
            }
            finally
            {
                _updatingChartSelection = false;
            }
        }

        UpdateAvailableVersions();
        SaveCommand.NotifyCanExecuteChanged();
        PreviewCommand.NotifyCanExecuteChanged();
    }

    private void UpdateAvailableVersions()
    {
        var previous = SelectedVersion;
        AvailableVersions.Clear();
        foreach (var match in _chartMatches)
        {
            if (match.Reference == Chart && !AvailableVersions.Contains(match.Version))
            {
                AvailableVersions.Add(match.Version);
            }
        }

        var isInstalledChart = SelectedChart is null || SelectedChart == _installedChartName;
        if (isInstalledChart && !string.IsNullOrWhiteSpace(_installedVersion) && !AvailableVersions.Contains(_installedVersion))
        {
            AvailableVersions.Add(_installedVersion);
        }

        SelectedVersion = previous is not null && AvailableVersions.Contains(previous)
            ? previous
            : AvailableVersions.Contains(_installedVersion) ? _installedVersion : null;
    }

    partial void OnSelectedVersionChanged(string? value)
    {
        SaveCommand.NotifyCanExecuteChanged();
        PreviewCommand.NotifyCanExecuteChanged();
    }

    partial void OnNamespaceChanged(string value)
    {
        SaveCommand.NotifyCanExecuteChanged();
        PreviewCommand.NotifyCanExecuteChanged();
    }

    partial void OnValuesYamlChanged(string value)
    {
        SaveCommand.NotifyCanExecuteChanged();
        PreviewCommand.NotifyCanExecuteChanged();
    }

    partial void OnTimeoutSecondsChanged(int value)
    {
        SaveCommand.NotifyCanExecuteChanged();
        PreviewCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsLoadingChanged(bool value)
    {
        SearchChartsCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        PreviewCommand.NotifyCanExecuteChanged();
    }
}
