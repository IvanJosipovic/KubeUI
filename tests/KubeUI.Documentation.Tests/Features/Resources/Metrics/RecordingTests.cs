using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Model.Controls;
using Dock.Model.Core;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Documentation.Tests.Infra;
using KubeUI.Kubernetes;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.DependencyInjection;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Documentation.Tests.Features.Resources.Metrics;

public sealed class RecordingTests
{
    [DocumentationVideoTheory]
    [Obsolete]
    public Task Monitor_pod_metrics_video(string theme)
    {
        ClusterWorkspace? workspace = null;
        return KubeUIWalkthrough.Create(theme, "monitoring")
            .Intro(
                "Monitor Kubernetes workloads",
                "Compare Pod CPU and memory history, then inspect CPU, memory, network, and filesystem charts.")
            .FakeCluster("demo-cluster")
            .ConfigureWorkspace(cluster =>
            {
                workspace = cluster;
                var settings = KubeUIWalkthroughServices.GetRequiredServices()
                    .GetRequiredService<IClusterSettingsStore>()
                    .GetClusterMetricsSettings(cluster.Runtime);
                settings.MetricsServiceType = MetricsServiceType.Prometheus;
                settings.PrometheusProviderKind = PrometheusProviderKind.External;
                settings.PrometheusDirectUrl = "http://prometheus.docs.invalid";
            })
            .LoadView<MainView, MainViewModel>(
                context =>
                {
                    context.ViewModel.Initialize();
                    var rightDock = KubeUIWalkthroughServices.GetRequiredServices()
                        .GetRequiredService<IFactory>()
                        .GetDockable<IToolDock>("RightDock")
                        ?? throw new InvalidOperationException("The Pod properties dock was not found.");
                    rightDock.Proportion = 0.36;
                },
                connectToCluster: false)
            .Speak("I'll connect to the demo cluster.")
            .OpenCluster("demo-cluster")
            .Speak("Prometheus is ready. Open Pods to compare CPU and memory consumption.",
                _ => workspace?.Runtime is { IsMetricsAvailable: true, ActiveMetricsBackend.Type: MetricsServiceType.Prometheus })
            .SelectNavigation("Pods")
            .Speak(
                "The Pod list shows CPU and memory usage. I'll find the web Pod.",
                root => HasVisibleHistory<MetricsHistoryCPUCellView>(root)
                    && HasVisibleHistory<MetricsHistoryMemoryCellView>(root))
            .SearchPods(WalkthroughDemoResources.FeaturedPodName)
            .Speak(
                "Select the web Pod to inspect its resource usage.",
                root => HasVisibleHistory<MetricsHistoryCPUCellView>(root)
                    && HasVisibleHistory<MetricsHistoryMemoryCellView>(root))
            .SelectPod(WalkthroughDemoResources.FeaturedPodName)
            .Speak("Open its properties to inspect the same Pod's metrics over time.")
            .RightClickPod(WalkthroughDemoResources.FeaturedPodName)
            .SelectContextMenuItem("View")
            .Speak(
                "Expand Metrics to view the Pod's CPU usage chart.",
                root => GetPodMetrics(root) is { } metrics && HasPlottedSamples(metrics),
                root => $"Properties: {root.GetVisualDescendants().OfType<ResourcePropertiesView<V1Pod>>().Count()}, "
                    + $"metrics: {root.GetVisualDescendants().OfType<MetricsControl>().Count()}, "
                    + $"tabs: {GetPodMetrics(root)?.Tabs.Count}, "
                    + $"status: {GetPodMetrics(root)?.StatusText}")
            .ExpandPodMetrics()
            .Speak(
                "The CPU chart compares usage, requests, and limits. Switch to memory.",
                root => ChartIsVisible(root, AppResources.Metrics_CPU!, "cpu"))
            .SelectPodMetric(AppResources.Metrics_Memory!)
            .Speak(
                "The memory chart compares usage, requests, and limits for this Pod.",
                root => ChartIsVisible(root, AppResources.Metrics_Memory!, "memory"))
            .SelectPodMetric(AppResources.Metrics_Network!)
            .Speak(
                "Network shows receive and transmit rates over the full hour.",
                root => ChartIsVisible(root, AppResources.Metrics_Network!, "network"))
            .SelectPodMetric(AppResources.Metrics_Filesystem!)
            .Speak(
                "Filesystem shows usage, read rate, and write rate over the full hour.",
                root => ChartIsVisible(root, AppResources.Metrics_Filesystem!, "fs"))
            .RecordAsync();
    }

    private static MetricsControl? GetPodMetrics(Control root) =>
        root.GetVisualDescendants()
            .OfType<ResourcePropertiesView<V1Pod>>()
            .Where(view => view.IsVisible
                && view.DataContext is ResourcePropertiesViewModel<V1Pod> model
                && model.Object?.Metadata?.Name == WalkthroughDemoResources.FeaturedPodName)
            .SelectMany(view => view.GetVisualDescendants().OfType<MetricsControl>())
            .FirstOrDefault(control => control.DataContext is V1Pod);

    private static bool ChartIsVisible(Control root, string tab, string metric)
    {
        var metrics = GetPodMetrics(root);
        return metrics is
        {
            ShowTabs: true,
            SelectedTab.Title: var selected,
            SelectedPanel.Series.Count: > 0,
        }
            && selected == tab
            && HasPlottedSamples(metrics, metric)
            && metrics.GetVisualDescendants().OfType<ResponsiveCartesianChart>()
                .Any(chart => chart.IsVisible && chart.Bounds.Width > 0 && chart.Bounds.Height > 0);
    }

    private static bool HasPlottedSamples(MetricsControl metrics, string metric = "cpu")
    {
        (string Name, string Metric)[] expectedSeries = metric switch
        {
            "cpu" or "memory" =>
            [
                (Name: AppResources.Metrics_Usage!, Metric: metric + "Usage"),
                (Name: AppResources.Metrics_Requests!, Metric: metric + "Requests"),
                (Name: AppResources.Metrics_Limits!, Metric: metric + "Limits"),
            ],
            "network" =>
            [
                (Name: AppResources.Metrics_Receive!, Metric: "networkReceive"),
                (Name: AppResources.Metrics_Transmit!, Metric: "networkTransmit"),
            ],
            "fs" =>
            [
                (Name: AppResources.Metrics_Usage!, Metric: "fsUsage"),
                (Name: AppResources.Metrics_Reads!, Metric: "fsReads"),
                (Name: AppResources.Metrics_Writes!, Metric: "fsWrites"),
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(metric), metric, "Unsupported Pod chart."),
        };
        return metrics.ShowTabs
            && expectedSeries.All(expected => metrics.SelectedPanel?.Series
                .OfType<StepLineSeries<DateTimePoint>>()
                .Any(series => series.Name == expected.Name
                    && HasHourOfMatchingPoints(series, expected.Metric)) == true);
    }

    private static bool HasHourOfMatchingPoints(StepLineSeries<DateTimePoint> series, string metric)
    {
        var points = series.Values?.OfType<DateTimePoint>()
            .Where(point => point.Value is > 0
                && Math.Abs(point.Value.Value - WalkthroughPrometheusQueryClient.SampleValue(
                    metric,
                    WalkthroughDemoResources.FeaturedPodName,
                    (int)(new DateTimeOffset(point.DateTime).ToUnixTimeSeconds() / 60))) < 0.000001)
            .OrderBy(point => point.DateTime)
            .ToArray();
        return points is { Length: >= 61 }
            && points[^1].DateTime - points[0].DateTime >= TimeSpan.FromHours(1);
    }

    private static bool HasVisibleHistory<TCell>(Control root) where TCell : Control =>
        root.GetVisualDescendants().OfType<TCell>()
            .Any(cell => cell.IsVisible && cell.GetVisualDescendants().OfType<Border>()
                .Any(bar => bar.Height > 0));
}
