using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class ClusterListViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ClusterManager ClusterManager { get; set; }

    private readonly IDialogService _dialogService;

    public ClusterListViewModel()
    {
        ClusterManager = Application.Current.GetRequiredService<ClusterManager>();
        _dialogService = Application.Current.GetRequiredService<IDialogService>();

        Title = Resources.ClusterListViewModel_Title;
        Id = nameof(ClusterListViewModel);
        SelectedItem = ClusterManager.Clusters.FirstOrDefault();
    }

    [ObservableProperty]
    public partial ICluster SelectedItem { get; set; }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task Delete(ICluster cluster)
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

    private bool CanDelete(ICluster cluster)
    {
        return cluster != null;
    }
}
