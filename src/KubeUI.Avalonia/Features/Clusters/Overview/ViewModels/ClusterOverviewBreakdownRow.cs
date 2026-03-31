namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

public sealed record ClusterOverviewBreakdownRow(
    string Name,
    string PrimaryValue,
    string? SecondaryValue = null);
