using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class ClusterListViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ClusterWorkspaceCatalog ClusterCatalog { get; set; }

    [ObservableProperty]
    public partial ISettingsService Settings { get; set; }

    private readonly IDialogService _dialogService;

    public ClusterListViewModel()
    {
        ClusterCatalog = Application.Current.GetRequiredService<ClusterWorkspaceCatalog>();
        Settings = Application.Current.GetRequiredService<ISettingsService>();
        _dialogService = Application.Current.GetRequiredService<IDialogService>();

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
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        if (result == ContentDialogResult.Primary)
        {
            ClusterCatalog.RemoveCluster(cluster);
        }
    }

    private bool CanDelete(ClusterWorkspaceViewModel cluster)
    {
        return cluster != null;
    }
}

