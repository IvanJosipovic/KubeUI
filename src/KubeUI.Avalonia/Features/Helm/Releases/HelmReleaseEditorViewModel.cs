using System.Text.Json;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using Helm.NET;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Helm.Releases;

public sealed partial class HelmReleaseEditorViewModel : ViewModelBase
{
    private readonly IHelmService _helmService;
    private readonly IFactory _factory;
    private readonly IDialogService _dialogService;
    private readonly HelmReleasesViewModel _parent;
    private readonly IHelmReleasePreviewService _previewService;
    private HelmConnection? _connection;
    private ClusterWorkspace? _cluster;
    private bool _editing;

    [ObservableProperty]
    public partial string ReleaseName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Chart { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Namespace { get; set; } = "default";

    [ObservableProperty]
    public partial string ValuesJson { get; set; } = "{}";

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
        HelmReleasesViewModel parent,
        IHelmReleasePreviewService previewService,
        IDialogService dialogService)
    {
        _helmService = helmService;
        _factory = factory;
        _parent = parent;
        _previewService = previewService;
        _dialogService = dialogService;
    }

    public void Initialize(ClusterWorkspace cluster, HelmConnection connection, HelmRelease? release)
    {
        _cluster = cluster;
        _connection = connection;
        _editing = release is not null;
        ReleaseName = release?.Name ?? string.Empty;
        Chart = release?.Chart ?? string.Empty;
        Namespace = release?.Namespace ?? connection.Namespace ?? "default";
        Title = release is null ? Assets.Resources.HelmReleaseEditorView_InstallTitle : Assets.Resources.HelmReleaseEditorView_UpgradeTitle;
        Id = $"{nameof(HelmReleaseEditorViewModel)}-{cluster.Runtime.Name}-{release?.Name ?? Guid.NewGuid().ToString("N")}";
        OnPropertyChanged(nameof(IsUpgrade));
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save(CancellationToken cancellationToken = default)
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
            SecondaryButtonText = Assets.Resources.HelmReleaseEditorView_Cancel,
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
                    cancellationToken);
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
                    cancellationToken);
            }

            await _parent.RefreshCommand.ExecuteAsync(null);
            _factory.RemoveDockable(this, false);
        }
        catch (Exception exception)
        {
            Error = exception.Message;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
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
            await _previewService.PreviewAsync(
                _cluster,
                _connection with { Namespace = Namespace.Trim() },
                ReleaseName.Trim(),
                Chart.Trim(),
                values,
                cancellationToken);
            Error = string.Empty;
        }
        catch (Exception exception)
        {
            Error = exception.Message;
        }
    }

    [RelayCommand]
    private void Cancel() => _factory.RemoveDockable(this, false);

    private bool CanSave()
        => !string.IsNullOrWhiteSpace(ReleaseName)
            && !string.IsNullOrWhiteSpace(Chart)
            && !string.IsNullOrWhiteSpace(Namespace)
            && TimeoutSeconds >= 0
            && TryParseValues(out _, out _);

    private bool TryParseValues(out IReadOnlyDictionary<string, object?>? values, out string error)
    {
        try
        {
            using var document = JsonDocument.Parse(ValuesJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                values = null;
                error = Assets.Resources.HelmReleaseEditorView_ValuesMustBeObject!;
                return false;
            }

            var parsedValues = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                parsedValues[property.Name] = property.Value.Clone();
            }

            values = parsedValues;
            error = string.Empty;
            return true;
        }
        catch (JsonException exception)
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
        SaveCommand.NotifyCanExecuteChanged();
        PreviewCommand.NotifyCanExecuteChanged();
    }

    partial void OnNamespaceChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

    partial void OnValuesJsonChanged(string value)
    {
        SaveCommand.NotifyCanExecuteChanged();
        PreviewCommand.NotifyCanExecuteChanged();
    }

    partial void OnTimeoutSecondsChanged(int value) => SaveCommand.NotifyCanExecuteChanged();
}
