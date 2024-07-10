namespace KubeUI.ViewModels;

public sealed partial class ClusterErrorViewModel : ViewModelBase
{
    public ClusterErrorViewModel()
    {
        Title = Resources.ClusterErrorViewModel_Title;
        Id = nameof(ClusterErrorViewModel);
    }

    [ObservableProperty]
    private string? _error;
}
