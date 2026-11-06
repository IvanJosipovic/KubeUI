namespace KubeUI.Features.Clusters.Error;

public sealed partial class ClusterErrorViewModel : ViewModelBase
{
    public ClusterErrorViewModel()
    {
        Title = L10n.Instance.ClusterErrorView_Title.CurrentValue()!;
        Id = nameof(ClusterErrorViewModel);
    }

    [ObservableProperty]
    public partial string? Error { get; set; }
}
