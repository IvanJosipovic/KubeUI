namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

public sealed partial class ClusterOverviewSummaryCardViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Value { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Detail { get; set; }
}
