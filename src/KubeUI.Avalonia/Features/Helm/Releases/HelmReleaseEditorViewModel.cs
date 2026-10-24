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
    private bool _applyingChartDefaults;
    private bool _valuesWereEdited;
    private CancellationTokenSource? _chartValuesLoadCancellation;

    /// <summary>Configured repositories and repositories returned by cached catalogs.</summary>
    public ObservableCollection<string> Repositories { get; } = [];

    /// <summary>Whether user-entered OCI chart reference is selected instead of repository catalog.</summary>
    [ObservableProperty]
    public partial bool IsOciSource { get; set; }

    /// <summary>Whether repository catalog chart selection is active.</summary>
    public bool IsRepositorySource
    {
        get => !IsOciSource;
        set
        {
            if (value)
            {
                IsOciSource = false;
            }
        }
    }

    /// <summary>Whether chart dropdown can be used.</summary>
    public bool CanSelectRepositoryChart => !IsLoading && !string.IsNullOrWhiteSpace(SelectedRepository);

    /// <summary>Whether repository chart-version dropdown can be used.</summary>
    public bool CanSelectRepositoryChartVersion => !IsLoading && !string.IsNullOrWhiteSpace(SelectedChart);

    /// <summary>Whether OCI chart-version text can be edited.</summary>
    public bool CanEditOciChartVersion => !IsLoading && !string.IsNullOrWhiteSpace(Chart);

    /// <summary>Chart names available in the selected repository.</summary>
    public ObservableCollection<string> AvailableCharts { get; } = [];

    /// <summary>Repository selected for chart discovery.</summary>
    [ObservableProperty]
    public partial string? SelectedRepository { get; set; }

    /// <summary>Chart selected within the repository.</summary>
    [ObservableProperty]
    public partial string? SelectedChart { get; set; }

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
    public partial string ValuesYaml { get; set; } = string.Empty;

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
        CancelChartValuesLoad();
        _valuesWereEdited = false;
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
        IsOciSource = false;
        AvailableVersions.Clear();
        SelectedVersion = null;
        _valuesLoaded = release is null;
        ReleaseName = release?.Name ?? string.Empty;
        Chart = string.Empty;
        _applyingChartDefaults = true;
        ValuesYaml = string.Empty;
        _applyingChartDefaults = false;
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
        OnPropertyChanged(nameof(CanSelectRepositoryChart));
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

        if (!IsOciSource)
        {
            Chart = SelectedChart is null || SelectedRepository is null ? string.Empty : $"{SelectedRepository}/{SelectedChart}";
        }

        if (SelectedChart is null)
        {
            SelectedVersion = null;
        }
    }

    partial void OnSelectedChartChanged(string? value)
    {
        OnPropertyChanged(nameof(CanSelectRepositoryChartVersion));
        if (!_updatingChartSelection && !IsOciSource)
        {
            Chart = value is null || SelectedRepository is null ? string.Empty : $"{SelectedRepository}/{value}";
        }
    }

    partial void OnIsOciSourceChanged(bool value)
    {
        OnPropertyChanged(nameof(IsRepositorySource));
        if (_updatingChartSelection)
        {
            return;
        }

        _updatingChartSelection = true;
        try
        {
            SelectedChart = null;
        }
        finally
        {
            _updatingChartSelection = false;
        }

        Chart = string.Empty;
        SelectedVersion = null;
        if (!value)
        {
            UpdateAvailableCharts();
        }

        SaveCommand.NotifyCanExecuteChanged();
        PreviewCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save(CancellationToken cancellationToken = default)
    {
        if (_connection is null || _cluster is null)
        {
            return;
        }

        if (IsUpgrade && string.IsNullOrWhiteSpace(Chart))
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

    private bool CanSave() => CanPreview();

    private bool CanPreview()
        => !IsLoading && _valuesLoaded
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
        OnPropertyChanged(nameof(CanEditOciChartVersion));
        if (!_updatingChartSelection && !string.IsNullOrWhiteSpace(value))
        {
            _valuesWereEdited = false;
        }

        if (!_updatingChartSelection && !IsOciSource)
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
        else if (!_updatingChartSelection)
        {
            _updatingChartSelection = true;
            try
            {
                SelectedChart = null;
            }
            finally
            {
                _updatingChartSelection = false;
            }
        }

        UpdateAvailableVersions();
        if (!_updatingChartSelection)
        {
            ScheduleChartDefaultsLoad(debounce: IsOciSource);
        }
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

        var isInstalledChart = string.IsNullOrWhiteSpace(Chart)
            || (!string.IsNullOrWhiteSpace(_installedChartName) && SelectedChart == _installedChartName);
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
        if (!_updatingChartSelection)
        {
            _valuesWereEdited = false;
            ScheduleChartDefaultsLoad(debounce: IsOciSource);
        }

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
        if (!_applyingChartDefaults && _release is null && _connection is not null)
        {
            _valuesWereEdited = true;
            CancelChartValuesLoad();
        }

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
        OnPropertyChanged(nameof(CanSelectRepositoryChart));
        OnPropertyChanged(nameof(CanSelectRepositoryChartVersion));
        OnPropertyChanged(nameof(CanEditOciChartVersion));
        SaveCommand.NotifyCanExecuteChanged();
        PreviewCommand.NotifyCanExecuteChanged();
    }

    private void ScheduleChartDefaultsLoad(bool debounce)
    {
        if (_release is not null || _connection is null || string.IsNullOrWhiteSpace(Chart))
        {
            CancelChartValuesLoad();
            return;
        }

        CancelChartValuesLoad();
        var cancellation = new CancellationTokenSource();
        _chartValuesLoadCancellation = cancellation;
        _ = LoadChartDefaultsAsync(Chart, SelectedVersion, debounce, cancellation);
    }

    private async Task LoadChartDefaultsAsync(string chart, string? version, bool debounce, CancellationTokenSource cancellation)
    {
        try
        {
            if (debounce)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(400), cancellation.Token);
            }

            var values = await _helmService.GetChartValuesAsync(_connection!, chart, version, cancellation.Token);
            if (!cancellation.IsCancellationRequested
                && ReferenceEquals(_chartValuesLoadCancellation, cancellation)
                && Chart == chart
                && SelectedVersion == version
                && !_valuesWereEdited)
            {
                _applyingChartDefaults = true;
                ValuesYaml = values;
                _applyingChartDefaults = false;
                Error = string.Empty;
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            if (!cancellation.IsCancellationRequested && ReferenceEquals(_chartValuesLoadCancellation, cancellation))
            {
                Error = exception.Message;
            }
        }
        finally
        {
            if (ReferenceEquals(_chartValuesLoadCancellation, cancellation))
            {
                _chartValuesLoadCancellation = null;
            }

            cancellation.Dispose();
        }
    }

    private void CancelChartValuesLoad()
    {
        var cancellation = _chartValuesLoadCancellation;
        _chartValuesLoadCancellation = null;
        cancellation?.Cancel();
    }
}
