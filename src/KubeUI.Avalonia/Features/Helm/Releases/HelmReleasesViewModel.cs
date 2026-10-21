using System.Text;
using System.Text.Json;
using Avalonia.Controls.Selection;
using Dock.Model.Core;
using DynamicData;
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

public sealed partial class HelmReleasesViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private readonly IHelmService _helmService;
    private readonly IFactory _factory;
    private readonly IDialogService _dialogService;
    private readonly IHelmReleasePreviewService _previewService;
    private readonly SourceCache<HelmRelease, string> _releases = new(static release => $"{release.Namespace}\u001f{release.Name}");
    private readonly DynamicTableViewSource<HelmRelease, string> _tableSource;
    private HelmConnection? _connection;

    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    [ObservableProperty]
    public partial HelmRelease? SelectedRelease { get; set; }

    [ObservableProperty]
    public partial bool AllNamespaces { get; set; } = true;

    [ObservableProperty]
    public partial string Status { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial int RollbackRevision { get; set; }

    [ObservableProperty]
    public partial bool WaitForActions { get; set; }

    public IDynamicTableViewSource TableSource => _tableSource;

    public ISelectionModel SelectionModel => _tableSource.SelectionModel;

    public HelmReleasesViewModel(
        IHelmService helmService,
        IFactory factory,
        IDialogService dialogService,
        IHelmReleasePreviewService previewService)
    {
        _helmService = helmService;
        _factory = factory;
        _dialogService = dialogService;
        _previewService = previewService;
        _tableSource = new DynamicTableViewSource<HelmRelease, string>(
            _releases,
            static release => $"{release.Namespace}\u001f{release.Name}",
            [
                CreateColumn("name", Assets.Resources.HelmReleasesView_Name!, static release => release.Name, DynamicTableViewWidthMode.Pixel, 180),
                CreateColumn("namespace", Assets.Resources.HelmReleasesView_Namespace!, static release => release.Namespace, DynamicTableViewWidthMode.Pixel, 140),
                CreateColumn("revision", Assets.Resources.HelmReleasesView_Revision!, static release => release.Revision, DynamicTableViewWidthMode.Pixel, 80),
                CreateColumn("status", Assets.Resources.HelmReleasesView_Status!, static release => release.Status, DynamicTableViewWidthMode.Pixel, 120),
                CreateColumn("chart", Assets.Resources.HelmReleasesView_Chart!, static release => release.Chart, DynamicTableViewWidthMode.Star, 1),
                CreateColumn("app-version", Assets.Resources.HelmReleasesView_AppVersion!, static release => release.AppVersion, DynamicTableViewWidthMode.Pixel, 120),
                CreateColumn("updated", Assets.Resources.HelmReleasesView_Updated!, static release => release.Updated, DynamicTableViewWidthMode.Pixel, 180)
            ],
            options: new DynamicTableViewSourceOptions { SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Key });
        _tableSource.SelectionModel.SingleSelect = true;
        _tableSource.SelectionModel.SelectionChanged += SelectionChanged;
        Title = Assets.Resources.HelmReleasesView_Title;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
        Id = $"{nameof(HelmReleasesViewModel)}-{cluster.Runtime.Name}";
        _connection = new HelmConnection(
            cluster.Runtime.KubeConfigPath,
            cluster.Runtime.Name,
            cluster.SelectedNamespaces.FirstOrDefault()?.Metadata?.Name ?? "default");
        _ = RefreshCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task Refresh(CancellationToken cancellationToken = default)
    {
        if (_connection is null)
        {
            return;
        }

        try
        {
            var connection = _connection with
            {
                Namespace = Cluster?.SelectedNamespaces.FirstOrDefault()?.Metadata?.Name ?? _connection.Namespace
            };
            var releases = await _helmService.ListReleasesAsync(connection, AllNamespaces, cancellationToken: cancellationToken);
            _releases.Edit(updater =>
            {
                updater.Clear();
                updater.AddOrUpdate(releases);
            });
            Status = string.Empty;
        }
        catch (Exception exception)
        {
            Status = exception.Message;
        }
    }

    [RelayCommand]
    private void NewRelease() => OpenEditor(null);

    [RelayCommand]
    private void UpgradeRelease(HelmRelease? release)
    {
        if (release is not null)
        {
            OpenEditor(release);
        }
    }

    [RelayCommand]
    private async Task VisualizeRelease(HelmRelease? release, CancellationToken cancellationToken = default)
    {
        if (_connection is null || Cluster is null || release is null)
        {
            return;
        }

        try
        {
            var connection = _connection with { Namespace = release.Namespace };
            var releaseValues = await _helmService.GetValuesAsync(connection, release.Name, cancellationToken);
            var values = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var (key, value) in releaseValues)
            {
                values[key] = value.Clone();
            }

            await _previewService.PreviewAsync(Cluster, connection, release.Name, release.Chart, values, cancellationToken);
            Status = string.Empty;
        }
        catch (Exception exception)
        {
            Status = exception.Message;
        }
    }

    [RelayCommand]
    private async Task ViewStatus(HelmRelease? release, CancellationToken cancellationToken = default)
    {
        if (_connection is null || release is null)
        {
            return;
        }

        try
        {
            var status = await _helmService.StatusAsync(_connection with { Namespace = release.Namespace }, release.Name, cancellationToken);
            Status = $"{status.Name} · {status.Status} · {status.Chart} · {status.Updated}";
        }
        catch (Exception exception)
        {
            Status = exception.Message;
        }
    }

    [RelayCommand]
    private async Task ViewValues(HelmRelease? release, CancellationToken cancellationToken = default)
    {
        if (_connection is null || release is null)
        {
            return;
        }

        try
        {
            var values = await _helmService.GetValuesAsync(_connection with { Namespace = release.Namespace }, release.Name, cancellationToken);
            Status = FormatValues(values);
        }
        catch (Exception exception)
        {
            Status = exception.Message;
        }
    }

    [RelayCommand]
    private async Task ViewHistory(HelmRelease? release, CancellationToken cancellationToken = default)
    {
        if (_connection is null || release is null)
        {
            return;
        }

        try
        {
            var history = await _helmService.HistoryAsync(_connection with { Namespace = release.Namespace }, release.Name, cancellationToken);
            Status = string.Join(Environment.NewLine, history.Select(item => $"{item.Revision}: {item.Status} · {item.Chart} · {item.Updated}"));
        }
        catch (Exception exception)
        {
            Status = exception.Message;
        }
    }

    [RelayCommand]
    private async Task Rollback(HelmRelease? release, CancellationToken cancellationToken = default)
    {
        if (_connection is null || release is null)
        {
            return;
        }

        try
        {
            await _helmService.RollbackAsync(_connection with { Namespace = release.Namespace }, release.Name, RollbackRevision, cancellationToken, WaitForActions);
            await Refresh(cancellationToken);
        }
        catch (Exception exception)
        {
            Status = exception.Message;
        }
    }

    [RelayCommand]
    private async Task Uninstall(HelmRelease? release, CancellationToken cancellationToken = default)
    {
        if (_connection is null || release is null)
        {
            return;
        }

        var result = await _dialogService.ShowContentDialogAsync(this, new ContentDialogSettings
        {
            Title = Assets.Resources.HelmReleasesView_UninstallTitle,
            Content = string.Format(Assets.Resources.HelmReleasesView_UninstallContent!, release.Name, release.Namespace),
            PrimaryButtonText = Assets.Resources.HelmReleasesView_Uninstall,
            SecondaryButtonText = Assets.Resources.HelmRepositoryEditorView_Cancel,
            DefaultButton = FAContentDialogButton.Secondary
        });

        if (result != FAContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            await _helmService.UninstallAsync(_connection with { Namespace = release.Namespace }, release.Name, cancellationToken, WaitForActions);
            await Refresh(cancellationToken);
        }
        catch (Exception exception)
        {
            Status = exception.Message;
        }
    }

    private void OpenEditor(HelmRelease? release)
    {
        if (Cluster is null || _connection is null)
        {
            return;
        }

        var editor = new HelmReleaseEditorViewModel(_helmService, _factory, this, _previewService);
        editor.Initialize(Cluster, _connection, release);
        _factory.AddToDocuments(editor);
    }

    private void SelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs args)
        => SelectedRelease = _tableSource.SelectionModel.SelectedItem as HelmRelease;

    private static string FormatValues(IReadOnlyDictionary<string, JsonElement> values)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            foreach (var (name, value) in values)
            {
                writer.WritePropertyName(name);
                value.WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static DynamicTableViewColumn<HelmRelease> CreateColumn<TValue>(
        string key,
        string header,
        Func<HelmRelease, TValue> selector,
        DynamicTableViewWidthMode widthMode,
        double width)
    {
        var column = DynamicTableViewColumn<HelmRelease>.Create(key, header, selector);
        column.WidthMode = widthMode;
        column.Width = width;
        return column;
    }

    partial void OnAllNamespacesChanged(bool value) => _ = RefreshCommand.ExecuteAsync(null);

    public void Dispose()
    {
        _tableSource.SelectionModel.SelectionChanged -= SelectionChanged;
        _tableSource.Dispose();
        _releases.Dispose();
    }
}
