using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentIcons.Common;
using Humanizer;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Metrics;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using LiveChartsCore.Defaults;
using Microsoft.Extensions.Logging;

namespace KubeUI.Avalonia.Features.Resources.Metrics.Controls;

internal static class MetricsControlPrometheusBackend
{
    private static readonly TimeSpan s_prometheusPanelTimeout = TimeSpan.FromSeconds(15);

    public static async Task<(MetricTabSnapshot? Snapshot, bool HadRequestFailures)> LoadPrometheusTabSnapshotAsync(
        ClusterWorkspace cluster,
        MetricTabDefinition tabDefinition,
        MetricTimeRangeOption? selectedTimeRange,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var panelTasks = new Task<(MetricPanelSnapshot? Snapshot, bool HadFailure)>[tabDefinition.Panels.Count];

        for (var i = 0; i < tabDefinition.Panels.Count; i++)
        {
            panelTasks[i] = LoadPrometheusPanelSnapshotAsync(cluster, tabDefinition.Title, tabDefinition.Panels[i], selectedTimeRange, logger, cancellationToken);
        }

        var panelResults = await Task.WhenAll(panelTasks).ConfigureAwait(false);
        var panels = new List<MetricPanelSnapshot>(panelResults.Length);
        var hadRequestFailures = false;

        for (var i = 0; i < panelResults.Length; i++)
        {
            var result = panelResults[i];
            hadRequestFailures |= result.HadFailure;

            if (result.Snapshot != null)
            {
                panels.Add(result.Snapshot);
            }
        }

        return (panels.Count == 0 ? null : new MetricTabSnapshot(tabDefinition.Title, tabDefinition.Icon, panels), hadRequestFailures);
    }

    public static async Task<(MetricPanelSnapshot? Snapshot, bool HadFailure)> LoadPrometheusPanelSnapshotAsync(
        ClusterWorkspace cluster,
        string tabTitle,
        MetricPanelDefinition panel,
        MetricTimeRangeOption? selectedTimeRange,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            using CancellationTokenSource timeoutCts = new();
            timeoutCts.CancelAfter(s_prometheusPanelTimeout);

            MetricRequest request = ApplySelectedTimeRange(panel.Request, selectedTimeRange);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
            var result = await cluster.Runtime.RequestMetricsAsync(request, linkedCts.Token).ConfigureAwait(false);
            IReadOnlyList<MetricSeriesSnapshot> series = CreateSeries(panel, result);
            if (series.Count == 0)
            {
                logger.LogDebug("Prometheus metrics panel {PanelTitle} for tab {TabTitle} returned no series for cluster {ClusterName}.", panel.Title, tabTitle, cluster.Runtime.Name);
                return (null, false);
            }

            return (new MetricPanelSnapshot(panel.Title, series), false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, "Prometheus metrics panel {PanelTitle} for tab {TabTitle} timed out after {Timeout} on cluster {ClusterName}.", panel.Title, tabTitle, s_prometheusPanelTimeout, cluster.Runtime.Name);
            return (null, true);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Prometheus metrics panel {PanelTitle} for tab {TabTitle} failed on cluster {ClusterName}.", panel.Title, tabTitle, cluster.Runtime.Name);
            return (null, true);
        }
    }

    public static async Task<(MetricTabSnapshot? Snapshot, bool HadRequestFailures)> LoadPodContainerPrometheusTabSnapshotAsync(
        ClusterWorkspace cluster,
        MetricPanelDefinition tabDefinition,
        V1Pod? pod,
        V1Container? container,
        MetricTimeRangeOption? selectedTimeRange,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        (MetricPanelSnapshot? Snapshot, bool HadFailure) panelResult = await LoadPodContainerPrometheusPanelSnapshotAsync(cluster, tabDefinition.Title, tabDefinition, pod, container, selectedTimeRange, logger, cancellationToken).ConfigureAwait(false);
        return (panelResult.Snapshot == null
            ? null
            : new MetricTabSnapshot(tabDefinition.Title, GetPodContainerTabIcon(tabDefinition.Title), [panelResult.Snapshot]), panelResult.HadFailure);
    }

    public static async Task<(MetricPanelSnapshot? Snapshot, bool HadFailure)> LoadPodContainerPrometheusPanelSnapshotAsync(
        ClusterWorkspace cluster,
        string tabTitle,
        MetricPanelDefinition panel,
        V1Pod? pod,
        V1Container? container,
        MetricTimeRangeOption? selectedTimeRange,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            using CancellationTokenSource timeoutCts = new();
            timeoutCts.CancelAfter(s_prometheusPanelTimeout);

            MetricRequest request = ApplySelectedTimeRange(panel.Request, selectedTimeRange);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
            var result = await cluster.Runtime.RequestMetricsAsync(request, linkedCts.Token).ConfigureAwait(false);
            IReadOnlyList<MetricSeriesSnapshot> series = CreateSeries(panel, result);
            if (series.Count == 0)
            {
                logger.LogDebug("Prometheus metrics panel {PanelTitle} for tab {TabTitle} returned no series for container {ContainerName} in pod {PodName} on cluster {ClusterName}.", panel.Title, tabTitle, container?.Name, pod?.Name(), cluster.Runtime.Name);
                return (null, false);
            }

            return (new MetricPanelSnapshot(panel.Title, series), false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, "Prometheus metrics panel {PanelTitle} for tab {TabTitle} timed out after {Timeout} for container {ContainerName} in pod {PodName} on cluster {ClusterName}.", panel.Title, tabTitle, s_prometheusPanelTimeout, container?.Name, pod?.Name(), cluster.Runtime.Name);
            return (null, true);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Prometheus metrics panel {PanelTitle} for tab {TabTitle} failed for container {ContainerName} in pod {PodName} on cluster {ClusterName}.", panel.Title, tabTitle, container?.Name, pod?.Name(), cluster.Runtime.Name);
            return (null, true);
        }
    }

    public static Icon GetPodContainerTabIcon(string title)
    {
        return title switch
        {
            var value when value == Assets.Resources.Metrics_CPU => Icon.TopSpeed,
            var value when value == Assets.Resources.Metrics_Memory => Icon.Ram,
            var value when value == Assets.Resources.Metrics_Filesystem => Icon.HardDrive,
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
            CreatePanel(Assets.Resources.Metrics_CPU!, [("cpuUsage", Assets.Resources.Metrics_Usage!), ("cpuRequests", Assets.Resources.Metrics_Requests!), ("cpuLimits", Assets.Resources.Metrics_Limits!)]),
            CreatePanel(Assets.Resources.Metrics_Memory!, [("memoryUsage", Assets.Resources.Metrics_Usage!), ("memoryRequests", Assets.Resources.Metrics_Requests!), ("memoryLimits", Assets.Resources.Metrics_Limits!)]),
            CreatePanel(Assets.Resources.Metrics_Filesystem!, [("fsUsage", Assets.Resources.Metrics_Usage!), ("fsReads", Assets.Resources.Metrics_Reads!), ("fsWrites", Assets.Resources.Metrics_Writes!)]),
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
        var selectedRangeSeconds = selectedTimeRange?.RangeSeconds ?? request.RangeSeconds.GetValueOrDefault();
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

        var series = new List<MetricSeriesSnapshot>();

        foreach (var query in panel.Request.Queries)
        {
            if (!result.Metrics.TryGetValue(query.Name, out var metricSeries))
            {
                continue;
            }

            foreach (var item in metricSeries.Where(panel.Filter))
            {
                var legend = panel.LegendLabels.TryGetValue(query.Name, out var label)
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
        ClusterWorkspace cluster,
        V1Pod pod,
        string containerName,
        MetricsServerHistoryState? history)
    {
        var resourceKey = GetPodContainerResourceKey(pod, containerName);
        if (history == null || !string.Equals(history.ResourceKey, resourceKey, StringComparison.Ordinal))
        {
            history = new MetricsServerHistoryState(resourceKey);
        }

        var metric = cluster.Runtime.PodMetrics.FirstOrDefault(x =>
            string.Equals(x.Name(), pod.Name(), StringComparison.Ordinal)
            && string.Equals(x.Namespace(), pod.Namespace(), StringComparison.Ordinal));
        var containerMetric = metric?.Containers?.FirstOrDefault(x => string.Equals(x.Name, containerName, StringComparison.Ordinal));

        if (containerMetric == null)
        {
            return (history, CreateMetricsServerTabs(history));
        }

        var timestamp = NormalizeMetricsServerTimestamp(DateTimeOffset.UtcNow);
        history.Upsert(timestamp, new MetricsServerSample(containerMetric.Usage["cpu"].ToDecimal(), containerMetric.Usage["memory"].ToInt64()));
        return (history, CreateMetricsServerTabs(history));
    }

    public static (MetricsServerHistoryState? History, IReadOnlyList<MetricTabSnapshot> Tabs) TryCaptureMetricsServerCharts(
        ClusterWorkspace cluster,
        IKubernetesObject<V1ObjectMeta> resource,
        MetricsServerHistoryState? history)
    {
        var resourceKey = GetMetricsServerResourceKey(resource);
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

    public static MetricsServerSample? TryCreatePodMetricsServerSample(ClusterWorkspace cluster, V1Pod pod)
    {
        var metric = cluster.Runtime.PodMetrics.FirstOrDefault(x =>
            string.Equals(x.Name(), pod.Name(), StringComparison.Ordinal)
            && string.Equals(x.Namespace(), pod.Namespace(), StringComparison.Ordinal));

        if (metric == null || metric.Containers == null || metric.Containers.Count == 0)
        {
            return null;
        }

        var cpu = metric.Containers.Sum(static x => x.Usage["cpu"].ToDecimal());
        var memory = metric.Containers.Sum(static x => x.Usage["memory"].ToInt64());
        return new MetricsServerSample(cpu, memory);
    }

    public static MetricsServerSample? TryCreateNodeMetricsServerSample(ClusterWorkspace cluster, V1Node node)
    {
        var metric = cluster.Runtime.NodeMetrics.FirstOrDefault(x => string.Equals(x.Name(), node.Name(), StringComparison.Ordinal));
        if (metric == null)
        {
            return null;
        }

        var cpu = metric.Usage["cpu"].ToDecimal();
        var memory = (double)metric.Usage["memory"].ToInt64();
        return new MetricsServerSample(cpu, memory);
    }

    public static DateTimeOffset NormalizeMetricsServerTimestamp(DateTimeOffset timestamp)
    {
        var seconds = (long)(timestamp.ToUnixTimeSeconds() / s_metricsServerSampleInterval.TotalSeconds * s_metricsServerSampleInterval.TotalSeconds);
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
            new MetricTabSnapshot(Assets.Resources.Metrics_CPU!, Icon.TopSpeed,
            [
                new MetricPanelSnapshot(Assets.Resources.Metrics_CPU!,
                [
                    new MetricSeriesSnapshot(
                        Assets.Resources.Metrics_Usage!,
                        cpuPoints),
                ]),
            ]),
            new MetricTabSnapshot(Assets.Resources.Metrics_Memory!, Icon.Ram,
            [
                new MetricPanelSnapshot(Assets.Resources.Metrics_Memory!,
                [
                    new MetricSeriesSnapshot(
                        Assets.Resources.Metrics_Usage!,
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
            var sample = samples[0];
            var value = selector(sample);
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
