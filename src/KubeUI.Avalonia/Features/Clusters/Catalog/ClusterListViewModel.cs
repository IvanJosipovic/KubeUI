using Avalonia.Controls.Notifications;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Platform.Storage;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Clusters.Catalog;

public sealed partial class ClusterListViewModel : ViewModelBase, IDisposable
{
    private readonly IDialogService _dialogService;
    private readonly IClusterRuntimeCatalog _runtimeCatalog;
    private readonly IPlatformServices _platformServices;
    private readonly ISettingsService _settingsService;
    private readonly IKubeConfigPathProvider _kubeConfigPathProvider;
    private readonly IKubeConfigDiscovery _kubeConfigDiscovery;
    private readonly INotificationManager _notificationManager;
    private readonly IObservableCache<ClusterWorkspace, string> _clusterCache;
    private DynamicTableViewSource<ClusterWorkspace, string>? _tableSource;

    [ObservableProperty]
    public partial ClusterWorkspaceCatalog ClusterCatalog { get; set; }

    public ClusterListViewModel(
        ClusterWorkspaceCatalog clusterCatalog,
        IClusterRuntimeCatalog runtimeCatalog,
        IDialogService dialogService,
        IPlatformServices platformServices,
        ISettingsService settingsService,
        IKubeConfigPathProvider kubeConfigPathProvider,
        IKubeConfigDiscovery kubeConfigDiscovery,
        INotificationManager notificationManager)
    {
        ClusterCatalog = clusterCatalog;
        _runtimeCatalog = runtimeCatalog;
        _dialogService = dialogService;
        _platformServices = platformServices;
        _settingsService = settingsService;
        _kubeConfigPathProvider = kubeConfigPathProvider;
        _kubeConfigDiscovery = kubeConfigDiscovery;
        _notificationManager = notificationManager;
        KubeConfigAutoDiscoveryEnabled = runtimeCatalog.KubeConfigAutoDiscoveryEnabled;
        _settingsService.Settings.PropertyChanged += SettingsOnPropertyChanged;
        RefreshRemovableKubeConfigs();

        Title = Assets.Resources.ClusterListView_Title!;
        Id = nameof(ClusterListViewModel);
        _clusterCache = ClusterCatalog.Clusters
            .ToObservableChangeSet(static cluster => cluster.Runtime.Name)
            .AsObservableCache();
        _tableSource = new DynamicTableViewSource<ClusterWorkspace, string>(
            _clusterCache,
            static cluster => cluster.Runtime.Name,
            CreateColumns(),
            options: new DynamicTableViewSourceOptions
            {
                SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Reference
            });
        _tableSource.SelectionModel.SingleSelect = true;
        _tableSource.SelectionModel.SelectionChanged += SelectionModelOnSelectionChanged;
    }

    /// <summary>
    /// Gets or sets whether kubeconfig files in the kubeconfig directory are discovered automatically.
    /// </summary>
    [ObservableProperty]
    public partial bool KubeConfigAutoDiscoveryEnabled { get; set; }

    /// <summary>
    /// Gets the manually added kubeconfig paths that can be removed from KubeUI.
    /// </summary>
    public ObservableCollection<string> RemovableKubeConfigs { get; } = [];

    partial void OnKubeConfigAutoDiscoveryEnabledChanged(bool value)
    {
        _runtimeCatalog.SetKubeConfigAutoDiscovery(value);
    }

    public IDynamicTableViewSource TableSource => _tableSource ?? throw new ObjectDisposedException(nameof(ClusterListViewModel));

    internal static DynamicTableViewColumn<ClusterWorkspace>[] CreateColumns()
    {
        var nameColumn = DynamicTableViewColumn<ClusterWorkspace>.Create(
            "name",
            Assets.Resources.ClusterListView_Name,
            static cluster => cluster.Runtime.Name);
        nameColumn.WidthMode = DynamicTableViewWidthMode.Star;
        nameColumn.Width = 1;

        var aliasColumn = DynamicTableViewColumn<ClusterWorkspace>.Create("alias", Assets.Resources.ClusterListView_Alias,
            static cluster => cluster.Alias ?? string.Empty,
            cellTemplate: CreateAliasCellTemplate());
        aliasColumn.WidthMode = DynamicTableViewWidthMode.Star;
        aliasColumn.Width = 1;

        var kubeconfigColumn = DynamicTableViewColumn<ClusterWorkspace>.Create(
            "kubeconfig",
            Assets.Resources.ClusterListView_KubeConfig,
            static cluster => cluster.Runtime.KubeConfigPath);
        kubeconfigColumn.WidthMode = DynamicTableViewWidthMode.Star;
        kubeconfigColumn.Width = 2;

        return [
            DynamicTableViewColumn<ClusterWorkspace>.Create("enabled", Assets.Resources.ClusterListView_Enabled,
                static cluster => cluster.IsEnabled,
                cellTemplate: CreateEnabledCellTemplate()),
            aliasColumn,
            nameColumn, kubeconfigColumn];
    }

    [ObservableProperty]
    public partial ClusterWorkspace? SelectedItem { get; set; }

    partial void OnSelectedItemChanged(ClusterWorkspace? value)
    {
        if (!ReferenceEquals(_tableSource?.SelectionModel.SelectedItem, value))
            if (_tableSource is not null)
                _tableSource.SelectionModel.SelectedItem = value;
    }

    public void Dispose()
    {
        if (_tableSource is null)
            return;
        _settingsService.Settings.PropertyChanged -= SettingsOnPropertyChanged;
        _tableSource.SelectionModel.SelectionChanged -= SelectionModelOnSelectionChanged;
        _tableSource.Dispose();
        _clusterCache.Dispose();
        _tableSource = null;
    }

    private void SelectionModelOnSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
        => SelectedItem = _tableSource?.SelectionModel.SelectedItem as ClusterWorkspace;

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task Delete(ClusterWorkspace cluster)
    {
        ContentDialogSettings settings = new()
        {
            Title = Assets.Resources.ClusterListView_Delete_Title!,
            Content = string.Format(Assets.Resources.ClusterListView_Delete_Content!, cluster.Runtime.Name)!,
            PrimaryButtonText = Assets.Resources.ClusterListView_Delete_Primary!,
            SecondaryButtonText = Assets.Resources.ClusterListView_Delete_Secondary!,
            DefaultButton = FAContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings).ConfigureAwait(true);

        if (result == FAContentDialogResult.Primary)
        {
            _runtimeCatalog.RemoveCluster(cluster.Runtime);
        }
    }

    private bool CanDelete(ClusterWorkspace cluster)
    {
        return cluster != null;
    }

    /// <summary>
    /// Lets the user pick a kubeconfig file from any location and loads its clusters.
    /// </summary>
    [RelayCommand]
    private async Task AddKubeConfigAsync()
    {
        var files = await _platformServices.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Assets.Resources.ClusterListView_AddKubeConfig,
            AllowMultiple = true,
        }).ConfigureAwait(true);

        List<string> paths = [];
        foreach (var file in files)
        {
            var path = file.TryGetLocalPath();
            if (!string.IsNullOrWhiteSpace(path))
            {
                paths.Add(path);
            }
        }

        AddKubeConfigPaths(paths);
    }

    internal void AddKubeConfigPaths(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            if (!_kubeConfigDiscovery.IsKubeConfig(path))
            {
                _notificationManager.Show(new Notification(
                    Assets.Resources.ClusterListView_AddKubeConfig,
                    string.Format(Assets.Resources.ClusterListView_InvalidKubeConfig!, path),
                    NotificationType.Warning));
                continue;
            }

            _runtimeCatalog.LoadFromConfigFromPath(path);
        }
    }

    /// <summary>
    /// Stops tracking a manually added kubeconfig after confirmation. The file on disk is not modified.
    /// </summary>
    /// <param name="path">The kubeconfig path to remove.</param>
    [RelayCommand]
    private async Task RemoveKubeConfigAsync(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        ContentDialogSettings settings = new()
        {
            Title = Assets.Resources.ClusterListView_RemoveKubeConfig!,
            Content = string.Format(Assets.Resources.ClusterListView_RemoveKubeConfig_Content!, path),
            PrimaryButtonText = Assets.Resources.ClusterListView_Delete_Primary!,
            SecondaryButtonText = Assets.Resources.ClusterListView_Delete_Secondary!,
            DefaultButton = FAContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings).ConfigureAwait(true);
        if (result == FAContentDialogResult.Primary)
        {
            _runtimeCatalog.RemoveKubeConfig(path);
        }
    }

    private void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Options.Settings.KubeConfigs))
        {
            Dispatcher.UIThread.Post(RefreshRemovableKubeConfigs);
        }
    }

    private void RefreshRemovableKubeConfigs()
    {
        RemovableKubeConfigs.Clear();
        foreach (var path in _settingsService.Settings.KubeConfigs)
        {
            if (!string.Equals(path, _kubeConfigPathProvider.DefaultPath, StringComparison.Ordinal))
            {
                RemovableKubeConfigs.Add(path);
            }
        }
    }

    private static FuncDataTemplate<ClusterWorkspace> CreateEnabledCellTemplate()
    {
        return new FuncDataTemplate<ClusterWorkspace>((_, _) =>
            new CheckBox()
                .HorizontalAlignment(HorizontalAlignment.Center)
                .IsChecked(CompiledBinding.Create<ClusterWorkspace, bool>(x => x.IsEnabled, mode: BindingMode.TwoWay)));
    }

    private static FuncDataTemplate<ClusterWorkspace> CreateAliasCellTemplate()
    {
        return new FuncDataTemplate<ClusterWorkspace>((_, _) =>
            new TextBox()
                .BorderThickness(new Thickness(0))
                .Background(Brushes.Transparent)
                .BindValue(TextBox.PlaceholderTextProperty, CompiledBinding.Create<ClusterWorkspace, string>(x => x.Runtime.Name))
                .Text(CompiledBinding.Create<ClusterWorkspace, string?>(x => x.Alias, mode: BindingMode.TwoWay)));
    }
}
