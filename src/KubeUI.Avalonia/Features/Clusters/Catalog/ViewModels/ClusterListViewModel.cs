using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Dialogs;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Avalonia.Features.Clusters.Catalog.ViewModels;

public sealed partial class ClusterListViewModel : ViewModelBase
{
    private readonly IContentDialogService _dialogService;

    [ObservableProperty]
    public partial ClusterWorkspaceCatalog ClusterCatalog { get; set; }

    [ObservableProperty]
    public partial ISettingsService Settings { get; set; }

    public ClusterListViewModel(
        IServiceProvider serviceProvider,
        ClusterWorkspaceCatalog clusterCatalog,
        ISettingsService settings,
        IContentDialogService dialogService)
    {
        ClusterCatalog = clusterCatalog;
        Settings = settings;
        _dialogService = dialogService;

        Title = Assets.Resources.ClusterListViewModel_Title;
        Id = nameof(ClusterListViewModel);
    }

    [ObservableProperty]
    public partial ClusterWorkspaceViewModel? SelectedItem { get; set; }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task Delete(ClusterWorkspaceViewModel cluster)
    {
        ContentDialogSettings settings = new()
        {
            Title = Assets.Resources.ClusterListViewModel_Delete_Title,
            Content = string.Format(Assets.Resources.ClusterListViewModel_Delete_Content, cluster.Name),
            PrimaryButtonText = Assets.Resources.ClusterListViewModel_Delete_Primary,
            SecondaryButtonText = Assets.Resources.ClusterListViewModel_Delete_Secondary,
            DefaultButton = FAContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        if (result == FAContentDialogResult.Primary)
        {
            ClusterCatalog.RemoveCluster(cluster);
        }
    }

    private bool CanDelete(ClusterWorkspaceViewModel cluster)
    {
        return cluster != null;
    }
}



