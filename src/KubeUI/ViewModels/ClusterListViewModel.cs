using Avalonia.Controls.Models.TreeDataGrid;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class ClusterListViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ClusterManager ClusterManager { get; set; }

    [ObservableProperty]
    public partial FlatTreeDataGridSource<ICluster> Source { get; set; }

    private readonly IDialogService _dialogService;

    public ClusterListViewModel()
    {
        ClusterManager = Application.Current.GetRequiredService<ClusterManager>();
        _dialogService = Application.Current.GetRequiredService<IDialogService>();

        Title = Assets.Resources.ClusterListViewModel_Title;
        Id = nameof(ClusterListViewModel);

        Source = new FlatTreeDataGridSource<ICluster>(ClusterManager.Clusters)
        {
            Columns =
            {
                new TextColumn<ICluster, string>("Name", x => x.Name, new GridLength(1, GridUnitType.Star)),
                new TextColumn<ICluster, string>("KubeConfig", x => x.KubeConfigPath),
            },
        };

        ((ITreeDataGridSource)Source).SortBy(Source.Columns[0], ListSortDirection.Ascending);
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task Delete(ICluster cluster)
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
            ClusterManager.RemoveCluster(cluster);
        }
    }

    private bool CanDelete(ICluster cluster)
    {
        return cluster != null;
    }
}
