using LiveChartsCore.Defaults;

namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

public sealed record ClusterOverviewMetricSeries(string Name, IReadOnlyList<DateTimePoint> Points, bool UseMax = false);
