namespace KubeUI.Avalonia.Features.Clusters.Error.ViewModels;

public sealed partial class ClusterErrorViewModel : ViewModelBase
{
    public ClusterErrorViewModel()
    {
        Title = Assets.Resources.ClusterErrorViewModel_Title;
        Id = nameof(ClusterErrorViewModel);
    }

    [ObservableProperty]
    public partial string? Error { get; set; }
}

