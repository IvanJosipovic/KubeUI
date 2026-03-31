namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

public sealed partial class ClusterOverviewBreakdownPanelViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmptyText { get; set; } = "No data available.";

    [ObservableProperty]
    public partial IReadOnlyList<ClusterOverviewBreakdownRow> Rows { get; set; } = [];

    [ObservableProperty]
    public partial bool HasRows { get; set; }

    public bool HasNoRows => !HasRows;

    public void SetRows(IReadOnlyList<ClusterOverviewBreakdownRow> rows)
    {
        Rows = rows;
        HasRows = rows.Count > 0;
        OnPropertyChanged(nameof(HasNoRows));
    }
}
