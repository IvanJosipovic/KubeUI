using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class ClusterListViewModel : ViewModelBase
{
    [ObservableProperty]
    private ClusterManager _clusterManager;

    private readonly IDialogService _dialogService;

    public ClusterListViewModel()
    {
        _clusterManager = Application.Current.GetRequiredService<ClusterManager>();
        _dialogService = Application.Current.GetRequiredService<IDialogService>();

        Title = Resources.ClusterListViewModel_Title;
        Id = nameof(ClusterListViewModel);

        _selectedItem = _clusterManager.Clusters.FirstOrDefault();
    }

    [ObservableProperty]
    private Cluster _selectedItem;

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task Delete(Cluster cluster)
    {
        ContentDialogSettings settings = new()
        {
            Title = Resources.ClusterListViewModel_Delete_Title,
            Content = string.Format(Resources.ClusterListViewModel_Delete_Content, cluster.Name),
            PrimaryButtonText = Resources.ClusterListViewModel_Delete_Primary,
            SecondaryButtonText = Resources.ClusterListViewModel_Delete_Secondary,
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        if (result == ContentDialogResult.Primary)
        {
            ClusterManager.RemoveCluster(cluster);
        }
    }

    private bool CanDelete(Cluster cluster)
    {
        return cluster != null;
    }
}
