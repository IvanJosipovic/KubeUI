using KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using FluentIcons.Common;
using Humanizer;
using k8s;
using k8s.Models;
using LiveChartsCore.Defaults;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KubeUI.Avalonia.Features.Resources.Metrics.Controls;

internal static class MetricsControlPrometheusBackend
{
    private static readonly TimeSpan s_prometheusPanelTimeout = TimeSpan.FromSeconds(15);

    public static async Task<(MetricTabSnapshot? Snapshot, bool HadRequestFailures)> LoadPrometheusTabSnapshotAsync(
        ClusterWorkspaceViewModel cluster,
        MetricTabDefinition tabDefinition,
        MetricTimeRangeOption? selectedTimeRange,
        ILogger logger)
    {
        Task<(MetricPanelSnapshot? Snapshot, bool HadFailure)>[] panelTasks = new Task<(MetricPanelSnapshot? Snapshot, bool HadFailure)>[tabDefinition.Panels.Count];

        for (int i = 0; i < tabDefinition.Panels.Count; i++)
        {
            panelTasks[i] = LoadPrometheusPanelSnapshotAsync(cluster, tabDefinition.Title, tabDefinition.Panels[i], selectedTimeRange, logger);
        }

        (MetricPanelSnapshot? Snapshot, bool HadFailure)[] panelResults = await Task.WhenAll(panelTasks).ConfigureAwait(false);
        List<MetricPanelSnapshot> panels = new(panelResults.Length);
        bool hadRequestFailures = false;

        for (int i = 0; i < panelResults.Length; i++)
        {
            (MetricPanelSnapshot? Snapshot, bool HadFailure) result = panelResults[i];
            hadRequestFailures |= result.HadFailure;

            if (result.Snapshot != null)
            {
                panels.Add(result.Snapshot);
            }
        }

        return (panels.Count == 0 ? null : new MetricTabSnapshot(tabDefinition.Title, tabDefinition.Icon, panels), hadRequestFailures);
    }

    public static async Task<(MetricPanelSnapshot? Snapshot, bool HadFailure)> LoadPrometheusPanelSnapshotAsync(
        ClusterWorkspaceViewModel cluster,
        string tabTitle,
        MetricPanelDefinition panel,
        MetricTimeRangeOption? selectedTimeRange,
        ILogger logger)
    {
        try
        {
            using CancellationTokenSource timeoutCts = new();
            timeoutCts.CancelAfter(s_prometheusPanelTimeout);

            MetricRequest request = ApplySelectedTimeRange(panel.Request, selectedTimeRange);
            MetricResultSet result = await cluster.RequestMetricsAsync(request, timeoutCts.Token).ConfigureAwait(false);
            IReadOnlyList<MetricSeriesSnapshot> series = CreateSeries(panel, result);
            if (series.Count == 0)
            {
                logger.LogDebug("Prometheus metrics panel {PanelTitle} for tab {TabTitle} returned no series for cluster {ClusterName}.", panel.Title, tabTitle, cluster.Name);
                return (null, false);
            }

            return (new MetricPanelSnapshot(panel.Title, series), false);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, "Prometheus metrics panel {PanelTitle} for tab {TabTitle} timed out after {Timeout} on cluster {ClusterName}.", panel.Title, tabTitle, s_prometheusPanelTimeout, cluster.Name);
            return (null, true);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Prometheus metrics panel {PanelTitle} for tab {TabTitle} failed on cluster {ClusterName}.", panel.Title, tabTitle, cluster.Name);
            return (null, true);
        }
    }

    public static async Task<(MetricTabSnapshot? Snapshot, bool HadRequestFailures)> LoadPodContainerPrometheusTabSnapshotAsync(
        ClusterWorkspaceViewModel cluster,
        MetricPanelDefinition tabDefinition,
        V1Pod? pod,
        V1Container? container,
        MetricTimeRangeOption? selectedTimeRange,
        ILogger logger)
    {
        (MetricPanelSnapshot? Snapshot, bool HadFailure) panelResult = await LoadPodContainerPrometheusPanelSnapshotAsync(cluster, tabDefinition.Title, tabDefinition, pod, container, selectedTimeRange, logger).ConfigureAwait(false);
        return (panelResult.Snapshot == null
            ? null
            : new MetricTabSnapshot(tabDefinition.Title, GetPodContainerTabIcon(tabDefinition.Title), [panelResult.Snapshot]), panelResult.HadFailure);
    }

    public static async Task<(MetricPanelSnapshot? Snapshot, bool HadFailure)> LoadPodContainerPrometheusPanelSnapshotAsync(
        ClusterWorkspaceViewModel cluster,
        string tabTitle,
        MetricPanelDefinition panel,
        V1Pod? pod,
        V1Container? container,
        MetricTimeRangeOption? selectedTimeRange,
        ILogger logger)
    {
        try
        {
            using CancellationTokenSource timeoutCts = new();
            timeoutCts.CancelAfter(s_prometheusPanelTimeout);

            MetricRequest request = ApplySelectedTimeRange(panel.Request, selectedTimeRange);
            MetricResultSet result = await cluster.RequestMetricsAsync(request, timeoutCts.Token).ConfigureAwait(false);
            IReadOnlyList<MetricSeriesSnapshot> series = CreateSeries(panel, result);
            if (series.Count == 0)
            {
                logger.LogDebug("Prometheus metrics panel {PanelTitle} for tab {TabTitle} returned no series for container {ContainerName} in pod {PodName} on cluster {ClusterName}.", panel.Title, tabTitle, container?.Name, pod?.Name(), cluster.Name);
                return (null, false);
            }

            return (new MetricPanelSnapshot(panel.Title, series), false);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, "Prometheus metrics panel {PanelTitle} for tab {TabTitle} timed out after {Timeout} for container {ContainerName} in pod {PodName} on cluster {ClusterName}.", panel.Title, tabTitle, s_prometheusPanelTimeout, container?.Name, pod?.Name(), cluster.Name);
            return (null, true);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Prometheus metrics panel {PanelTitle} for tab {TabTitle} failed for container {ContainerName} in pod {PodName} on cluster {ClusterName}.", panel.Title, tabTitle, container?.Name, pod?.Name(), cluster.Name);
            return (null, true);
        }
    }

    public static Icon GetPodContainerTabIcon(string title)
    {
        return title switch
        {
            "CPU" => Icon.TopSpeed,
            "Memory" => Icon.Ram,
            "Filesystem" => Icon.HardDrive,
            _ => Icon.TopSpeed,
        };
    }

    public static MetricPanelDefinition[] CreatePodContainerPrometheusRequest(V1Pod pod, string containerName)
    {
        Dictionary<string, string> options = new(StringComparer.Ordinal)
        {
            ["namespace"] = pod.Namespace(),
            ["pods"] = Regex.Escape(pod.Name()),
            ["selector"] = "container, pod, namespace",
            ["container"] = containerName,
        };

        return
        [
            CreatePanel("CPU", [("cpuUsage", "Usage"), ("cpuRequests", "Requests"), ("cpuLimits", "Limits")]),
            CreatePanel("Memory", [("memoryUsage", "Usage"), ("memoryRequests", "Requests"), ("memoryLimits", "Limits")]),
            CreatePanel("Filesystem", [("fsUsage", "Usage"), ("fsReads", "Reads"), ("fsWrites", "Writes")]),
        ];

        MetricPanelDefinition CreatePanel(string title, IReadOnlyList<(string QueryName, string Label)> queries)
        {
            return new MetricPanelDefinition
            {
                Title = title,
                Filter = static _ => true,
                LegendLabels = queries.ToDictionary(static x => x.QueryName, static x => x.Label, StringComparer.Ordinal),
                Request = new MetricRequest
                {
                    Category = MetricCategory.Pods,
                    StepSeconds = 60,
                    RangeSeconds = 3600,
                    Queries = queries.Select(query => new MetricQueryDefinition
                    {
                        Name = query.QueryName,
                        Options = new Dictionary<string, string>(options, StringComparer.Ordinal),
                    }).ToArray(),
                },
            };
        }
    }

    public static MetricRequest ApplySelectedTimeRange(MetricRequest request, MetricTimeRangeOption? selectedTimeRange)
    {
        int selectedRangeSeconds = selectedTimeRange?.RangeSeconds ?? request.RangeSeconds.GetValueOrDefault();
        if (request.RangeSeconds == selectedRangeSeconds)
        {
            return request;
        }

        return new MetricRequest
        {
            Category = request.Category,
            Queries = request.Queries,
            Start = request.Start,
            End = request.End,
            StepSeconds = request.StepSeconds,
            RangeSeconds = selectedRangeSeconds,
            Frames = request.Frames,
        };
    }

    public static IReadOnlyList<MetricSeriesSnapshot> CreateSeries(MetricPanelDefinition panel, MetricResultSet result)
    {
        if (result.IsEmpty)
        {
            return [];
        }

        List<MetricSeriesSnapshot> series = [];

        foreach (MetricQueryDefinition query in panel.Request.Queries)
        {
            if (!result.Metrics.TryGetValue(query.Name, out var metricSeries))
            {
                continue;
            }

            foreach (var item in metricSeries.Where(panel.Filter))
            {
                string legend = panel.LegendLabels.TryGetValue(query.Name, out string? label)
                    ? label
                    : query.Name;

                if (item.Labels.Count > 0 && metricSeries.Count > 1)
                {
                    legend += $" ({item.Labels.Values.FirstOrDefault()})";
                }

                series.Add(new MetricSeriesSnapshot(
                    legend,
                    item.Points.Select(static value => new DateTimePoint(value.Timestamp.LocalDateTime, value.Value)).ToArray()));
            }
        }

        return series;
    }
}

internal static class MetricsControlMetricsServerBackend
{
    private static readonly TimeSpan s_metricsServerSampleInterval = TimeSpan.FromSeconds(30);

    public static (MetricsServerHistoryState? History, IReadOnlyList<MetricTabSnapshot> Tabs) CapturePodContainerMetricsServerCharts(
        ClusterWorkspaceViewModel cluster,
        V1Pod pod,
        string containerName,
        MetricsServerHistoryState? history)
    {
        string resourceKey = GetPodContainerResourceKey(pod, containerName);
        if (history == null || !string.Equals(history.ResourceKey, resourceKey, StringComparison.Ordinal))
        {
            history = new MetricsServerHistoryState(resourceKey);
        }

        var metric = cluster.PodMetrics.FirstOrDefault(x =>
            string.Equals(x.Name(), pod.Name(), StringComparison.Ordinal)
            && string.Equals(x.Namespace(), pod.Namespace(), StringComparison.Ordinal));
        var containerMetric = metric?.Containers?.FirstOrDefault(x => string.Equals(x.Name, containerName, StringComparison.Ordinal));

        if (containerMetric == null)
        {
            return (history, CreateMetricsServerTabs(history));
        }

        DateTimeOffset timestamp = NormalizeMetricsServerTimestamp(DateTimeOffset.UtcNow);
        history.Upsert(timestamp, new MetricsServerSample(containerMetric.Usage["cpu"].ToDecimal(), (double)containerMetric.Usage["memory"].ToInt64()));
        return (history, CreateMetricsServerTabs(history));
    }

    public static (MetricsServerHistoryState? History, IReadOnlyList<MetricTabSnapshot> Tabs) TryCaptureMetricsServerCharts(
        ClusterWorkspaceViewModel cluster,
        IKubernetesObject<V1ObjectMeta> resource,
        MetricsServerHistoryState? history)
    {
        string? resourceKey = GetMetricsServerResourceKey(resource);
        if (resourceKey == null)
        {
            return (null, []);
        }

        if (history == null || !string.Equals(history.ResourceKey, resourceKey, StringComparison.Ordinal))
        {
            history = new MetricsServerHistoryState(resourceKey);
        }

        MetricsServerSample? sample = resource switch
        {
            V1Pod pod => TryCreatePodMetricsServerSample(cluster, pod),
            V1Node node => TryCreateNodeMetricsServerSample(cluster, node),
            _ => null,
        };

        if (sample == null)
        {
            return (history, CreateMetricsServerTabs(history));
        }

        DateTimeOffset timestamp = NormalizeMetricsServerTimestamp(DateTimeOffset.UtcNow);
        history.Upsert(timestamp, sample.Value);
        return (history, CreateMetricsServerTabs(history));
    }

    public static string? GetMetricsServerResourceKey(IKubernetesObject<V1ObjectMeta> resource)
    {
        return resource switch
        {
            V1Pod pod => $"pod:{pod.Namespace()}:{pod.Name()}",
            V1Node node => $"node::{node.Name()}",
            _ => null,
        };
    }

    public static MetricsServerSample? TryCreatePodMetricsServerSample(ClusterWorkspaceViewModel cluster, V1Pod pod)
    {
        var metric = cluster.PodMetrics.FirstOrDefault(x =>
            string.Equals(x.Name(), pod.Name(), StringComparison.Ordinal)
            && string.Equals(x.Namespace(), pod.Namespace(), StringComparison.Ordinal));

        if (metric == null || metric.Containers == null || metric.Containers.Count == 0)
        {
            return null;
        }

        decimal cpu = metric.Containers.Sum(static x => x.Usage["cpu"].ToDecimal());
        double memory = metric.Containers.Sum(static x => (double)x.Usage["memory"].ToInt64());
        return new MetricsServerSample(cpu, memory);
    }

    public static MetricsServerSample? TryCreateNodeMetricsServerSample(ClusterWorkspaceViewModel cluster, V1Node node)
    {
        var metric = cluster.NodeMetrics.FirstOrDefault(x => string.Equals(x.Name(), node.Name(), StringComparison.Ordinal));
        if (metric == null)
        {
            return null;
        }

        decimal cpu = metric.Usage["cpu"].ToDecimal();
        double memory = (double)metric.Usage["memory"].ToInt64();
        return new MetricsServerSample(cpu, memory);
    }

    public static DateTimeOffset NormalizeMetricsServerTimestamp(DateTimeOffset timestamp)
    {
        long seconds = (long)(timestamp.ToUnixTimeSeconds() / s_metricsServerSampleInterval.TotalSeconds * s_metricsServerSampleInterval.TotalSeconds);
        return DateTimeOffset.FromUnixTimeSeconds(seconds);
    }

    public static IReadOnlyList<MetricTabSnapshot> CreateMetricsServerTabs(MetricsServerHistoryState history)
    {
        if (history.Samples.Count == 0)
        {
            return [];
        }

        IReadOnlyList<DateTimePoint> cpuPoints = CreateMetricsServerChartPoints(history.Samples, static x => (double)x.Cpu);
        IReadOnlyList<DateTimePoint> memoryPoints = CreateMetricsServerChartPoints(history.Samples, static x => x.Memory);

        return
        [
            new MetricTabSnapshot("CPU", Icon.TopSpeed,
            [
                new MetricPanelSnapshot("CPU",
                [
                    new MetricSeriesSnapshot(
                        "Usage",
                        cpuPoints),
                ]),
            ]),
            new MetricTabSnapshot("Memory", Icon.Ram,
            [
                new MetricPanelSnapshot("Memory",
                [
                    new MetricSeriesSnapshot(
                        "Usage",
                        memoryPoints),
                ]),
            ]),
        ];
    }

    public static IReadOnlyList<DateTimePoint> CreateMetricsServerChartPoints(
        IReadOnlyList<MetricsServerSamplePoint> samples,
        Func<MetricsServerSamplePoint, double> selector)
    {
        if (samples.Count == 1)
        {
            MetricsServerSamplePoint sample = samples[0];
            double value = selector(sample);
            return
            [
                new DateTimePoint(sample.Timestamp.Subtract(s_metricsServerSampleInterval).LocalDateTime, value),
                new DateTimePoint(sample.Timestamp.LocalDateTime, value),
            ];
        }

        return samples
            .Select(sample => new DateTimePoint(sample.Timestamp.LocalDateTime, selector(sample)))
            .ToArray();
    }

    private static string GetPodContainerResourceKey(V1Pod pod, string containerName)
    {
        return $"pod:{pod.Namespace()}:{pod.Name()}:{containerName}";
    }
}
