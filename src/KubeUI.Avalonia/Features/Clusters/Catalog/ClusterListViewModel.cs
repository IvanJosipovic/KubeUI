using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Clusters.Catalog;

public sealed partial class ClusterListViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IClusterRuntimeCatalog _runtimeCatalog;

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
    }

    [ObservableProperty]
    public partial ClusterWorkspace? SelectedItem { get; set; }

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
