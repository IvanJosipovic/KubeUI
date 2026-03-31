namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

public sealed record ClusterOverviewEventRow(
    string Type,
    string Namespace,
    string InvolvedObject,
    string Reason,
    string Message,
    string Source,
    int Count,
    string LastSeen,
    bool IsWarning);
