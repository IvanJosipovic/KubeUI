using Avalonia.Controls.Selection;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Clusters.Catalog;

public sealed partial class ClusterListViewModel : ViewModelBase, IDisposable
{
    private readonly IDialogService _dialogService;
    private readonly IClusterRuntimeCatalog _runtimeCatalog;
    private readonly IObservableCache<ClusterWorkspace, string> _clusterCache;
    private DynamicTableViewSource<ClusterWorkspace, string>? _tableSource;

    [ObservableProperty]
    public partial ClusterWorkspaceCatalog ClusterCatalog { get; set; }

    public ClusterListViewModel(
        ClusterWorkspaceCatalog clusterCatalog,
        IClusterRuntimeCatalog runtimeCatalog,
        IDialogService dialogService)
    {
        ClusterCatalog = clusterCatalog;
        _runtimeCatalog = runtimeCatalog;
        _dialogService = dialogService;

        Title = Assets.Resources.ClusterListView_Title!;
        Id = nameof(ClusterListViewModel);
        _clusterCache = ClusterCatalog.Clusters
            .ToObservableChangeSet(static cluster => cluster.Runtime.Name)
            .AsObservableCache();
        _tableSource = new DynamicTableViewSource<ClusterWorkspace, string>(
            _clusterCache,
            static cluster => cluster.Runtime.Name,
            [
                DynamicTableViewColumn<ClusterWorkspace>.Create("name", Assets.Resources.ClusterListView_Name,
                    static cluster => cluster.Runtime.Name),
                DynamicTableViewColumn<ClusterWorkspace>.Create("kubeconfig", Assets.Resources.ClusterListView_KubeConfig,
                    static cluster => cluster.Runtime.KubeConfigPath)
            ],
            options: new DynamicTableViewSourceOptions
            {
                SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Reference
            });
        _tableSource.SelectionModel.SingleSelect = true;
        _tableSource.SelectionModel.SelectionChanged += SelectionModelOnSelectionChanged;
    }

    public IDynamicTableViewSource TableSource => _tableSource ?? throw new ObjectDisposedException(nameof(ClusterListViewModel));

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
}
