using System.Collections.Specialized;
using System.Globalization;
using Avalonia.Controls.Selection;
using Avalonia.Threading;
using Dock.Model.Core;
using DynamicData;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using Helm.NET;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Helm.Releases;

public sealed partial class HelmReleasesViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private const string NamespaceScopeFilterId = "helm-releases-namespace";
    private readonly IHelmService _helmService;
    private readonly IFactory _factory;
    private readonly IDialogService _dialogService;
    private readonly IHelmReleasePreviewService _previewService;
    private readonly Func<ResourceYamlViewModel> _createYamlViewer;
    private readonly IHelmDocumentOpener _documentOpener;
    private readonly IHelmListNotifier _listNotifier;
    private readonly SourceCache<HelmRelease, string> _releases = new(static release => $"{release.Namespace}\u001f{release.Name}");
    private readonly DynamicTableViewSource<HelmRelease, string> _tableSource;
    private readonly ObservableCollection<V1Namespace> _localSelectedNamespaces = [];
    private readonly INotifyCollectionChanged? _itemsNotifications;
    private HelmConnection? _connection;
    private bool _disposed;

    [ObservableProperty]
    public partial int ItemCount { get; private set; }

    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    [ObservableProperty]
    public partial HelmRelease? SelectedRelease { get; set; }

    [ObservableProperty]
    public partial bool IsNamespaceSelectionLinked { get; set; } = true;

    [ObservableProperty]
    public partial string Status { get; private set; } = string.Empty;

    public IDynamicTableViewSource TableSource => _tableSource;

    public ISelectionModel SelectionModel => _tableSource.SelectionModel;

    public ObservableCollection<V1Namespace> SelectedNamespaces
        => IsNamespaceSelectionLinked && Cluster is not null ? Cluster.SelectedNamespaces : _localSelectedNamespaces;

    public HelmReleasesViewModel(
        IHelmService helmService,
        IFactory factory,
        IDialogService dialogService,
        IHelmReleasePreviewService previewService,
        Func<ResourceYamlViewModel> createYamlViewer,
        IHelmDocumentOpener documentOpener,
        IHelmListNotifier listNotifier)
    {
        _helmService = helmService;
        _factory = factory;
        _dialogService = dialogService;
        _previewService = previewService;
        _createYamlViewer = createYamlViewer;
        _documentOpener = documentOpener;
        _listNotifier = listNotifier;
        _listNotifier.ReleasesChanged += OnReleasesChanged;
        _tableSource = new DynamicTableViewSource<HelmRelease, string>(
            _releases,
            static release => $"{release.Namespace}\u001f{release.Name}",
            [
                CreateColumn("name", Assets.Resources.HelmReleasesView_Name!, static release => release.Name, DynamicTableViewWidthMode.Star, 1.5),
                CreateColumn("namespace", Assets.Resources.HelmReleasesView_Namespace!, static release => release.Namespace, DynamicTableViewWidthMode.Star, 1.2),
                CreateColumn("app-version", Assets.Resources.HelmReleasesView_AppVersion!, static release => release.AppVersion, DynamicTableViewWidthMode.Star, 1),
                CreateColumn("status", Assets.Resources.HelmReleasesView_Status!, static release => release.Status, DynamicTableViewWidthMode.Star, 1),
                CreateColumn("updated", Assets.Resources.HelmReleasesView_Updated!, static release => FormatUpdated(release.Updated, DateTime.UtcNow), DynamicTableViewWidthMode.Star, 1)
            ],
            options: new DynamicTableViewSourceOptions { SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Key });
        _tableSource.SelectionModel.SingleSelect = true;
        _tableSource.SelectionModel.SelectionChanged += SelectionChanged;
        _itemsNotifications = _tableSource.Items as INotifyCollectionChanged;
        if (_itemsNotifications is not null)
        {
            _itemsNotifications.CollectionChanged += ItemsOnCollectionChanged;
        }

        Title = Assets.Resources.HelmReleasesView_Title;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        UnsubscribeFromSelectedNamespaces();
        Cluster = cluster;
        Id = $"{nameof(HelmReleasesViewModel)}-{cluster.Runtime.Name}";
        _connection = new HelmConnection(
            cluster.Runtime.KubeConfigPath,
            cluster.Runtime.Name,
            cluster.SelectedNamespaces.FirstOrDefault()?.Metadata?.Name ?? "default");
        SubscribeToSelectedNamespaces();
        SetNamespaceFilter();
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
            var releases = await _helmService.ListReleasesAsync(connection, allNamespaces: true, cancellationToken: cancellationToken);
            _releases.Edit(updater =>
            {
                updater.Clear();
                updater.AddOrUpdate(releases);
            });
            SetNamespaceFilter();
            UpdateItemCount();
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
    private void VisualizeRelease(HelmRelease? release)
    {
        if (_connection is null || Cluster is null || release is null)
        {
            return;
        }

        try
        {
            _previewService.VisualizeRelease(Cluster, release.Name, release.Namespace);
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
            var yaml = HelmValuesYaml.Serialize(values);
            var id = $"helm-values-{Cluster!.Runtime.Name}-{release.Namespace}-{release.Name}";
            var viewer = _factory.FindDockableById(id) is ResourceYamlViewModel existing
                ? existing
                : _createYamlViewer();
            viewer.InitializeReadOnly(
                string.Format(Assets.Resources.HelmReleasesView_ValuesTitle!, release.Name, release.Namespace),
                id,
                yaml);
            _factory.AddToBottom(viewer);
            Status = string.Empty;
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

        var maximumRevision = int.TryParse(release.Revision, out var revision) ? revision : 0;
        var dialogViewModel = new ReleaseActionDialogViewModel();
        var result = await _dialogService.ShowContentDialogAsync(this, new ContentDialogSettings
        {
            Title = Assets.Resources.HelmReleasesView_RollbackDialog_Title,
            Content = CreateRollbackDialogContent(dialogViewModel, release, maximumRevision),
            PrimaryButtonText = Assets.Resources.HelmReleasesView_Rollback,
            SecondaryButtonText = Assets.Resources.Shared_Cancel,
            DefaultButton = FAContentDialogButton.Secondary
        });

        if (result != FAContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            await _helmService.RollbackAsync(
                _connection with { Namespace = release.Namespace },
                release.Name,
                dialogViewModel.Revision,
                cancellationToken,
                dialogViewModel.WaitForAction);
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

        ReleaseActionDialogViewModel dialogViewModel = new();
        var result = await _dialogService.ShowContentDialogAsync(this, new ContentDialogSettings
        {
            Title = Assets.Resources.HelmReleasesView_UninstallTitle,
            Content = CreateUninstallDialogContent(dialogViewModel, release),
            PrimaryButtonText = Assets.Resources.HelmReleasesView_Uninstall,
            SecondaryButtonText = Assets.Resources.Shared_Cancel,
            DefaultButton = FAContentDialogButton.Secondary
        });

        if (result != FAContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            await _helmService.UninstallAsync(
                _connection with { Namespace = release.Namespace },
                release.Name,
                cancellationToken,
                dialogViewModel.WaitForAction);
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

        _documentOpener.OpenReleaseEditor(Cluster, _connection, release);
    }

    private void OnReleasesChanged(object? sender, EventArgs args)
    {
        if (_disposed)
        {
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            _ = RefreshCommand.ExecuteAsync(null);
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!_disposed)
            {
                _ = RefreshCommand.ExecuteAsync(null);
            }
        });
    }

    private void SelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs args)
        => SelectedRelease = _tableSource.SelectionModel.SelectedItem as HelmRelease;

    private static StackPanel CreateRollbackDialogContent(
        ReleaseActionDialogViewModel viewModel,
        HelmRelease release,
        int maximumRevision)
        => new StackPanel()
            .Spacing(8)
            .Children(
                new TextBlock().Text(string.Format(
                    Assets.Resources.HelmReleasesView_RollbackDialog_Content!,
                    release.Name,
                    release.Namespace)),
                new TextBlock().Text(Assets.Resources.HelmReleasesView_RollbackRevision),
                new NumericUpDown()
                    .Minimum(0)
                    .Maximum(maximumRevision)
                    .Value(viewModel, x => x.Revision, BindingMode.TwoWay),
                new CheckBox()
                    .Content(Assets.Resources.HelmReleasesView_Wait)
                    .IsChecked(viewModel, x => x.WaitForAction, BindingMode.TwoWay));

    private static StackPanel CreateUninstallDialogContent(ReleaseActionDialogViewModel viewModel, HelmRelease release)
        => new StackPanel()
            .Spacing(8)
            .Children(
                new TextBlock().Text(string.Format(
                    Assets.Resources.HelmReleasesView_UninstallContent!,
                    release.Name,
                    release.Namespace)),
                new CheckBox()
                    .Content(Assets.Resources.HelmReleasesView_Wait)
                    .IsChecked(viewModel, x => x.WaitForAction, BindingMode.TwoWay));

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

    internal static string FormatUpdated(string updated, DateTime utcNow)
        => DateTimeOffset.TryParse(updated, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateTime)
            ? RelativeTimeFormatter.FormatPrettyAge(dateTime.UtcDateTime, utcNow)
            : updated;

    partial void OnIsNamespaceSelectionLinkedChanged(bool value)
    {
        if (Cluster is null)
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
        SetNamespaceFilter();
    }

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        => SetNamespaceFilter();

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

    private void SetNamespaceFilter()
    {
        if (SelectedNamespaces.Count == 0)
        {
            _tableSource.SetScopeFilter(NamespaceScopeFilterId, null);
            return;
        }

        var namespaces = new HashSet<string>(StringComparer.Ordinal);
        foreach (var selectedNamespace in SelectedNamespaces)
        {
            if (selectedNamespace.Metadata?.Name is { Length: > 0 } name)
            {
                namespaces.Add(name);
            }
        }

        _tableSource.SetScopeFilter(
            NamespaceScopeFilterId,
            item => item is HelmRelease release && namespaces.Contains(release.Namespace));
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

    private static void CopyNamespaces(IEnumerable<V1Namespace> source, ObservableCollection<V1Namespace> target)
    {
        target.Clear();
        foreach (var selectedNamespace in source)
        {
            target.Add(selectedNamespace);
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _listNotifier.ReleasesChanged -= OnReleasesChanged;
        UnsubscribeFromSelectedNamespaces();
        if (_itemsNotifications is not null)
        {
            _itemsNotifications.CollectionChanged -= ItemsOnCollectionChanged;
        }

        _tableSource.SelectionModel.SelectionChanged -= SelectionChanged;
        _tableSource.Dispose();
        _releases.Dispose();
    }

    private sealed partial class ReleaseActionDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial int Revision { get; set; }

        [ObservableProperty]
        public partial bool WaitForAction { get; set; }
    }
}
