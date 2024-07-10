namespace KubeUI.ViewModels;

public sealed partial class ClusterErrorViewModel : ViewModelBase
{
    public ClusterErrorViewModel()
    {
        Title = "Cluster Error";
        Id = nameof(ClusterErrorViewModel);
    }
    [ObservableProperty]
    private string? _error;
}
