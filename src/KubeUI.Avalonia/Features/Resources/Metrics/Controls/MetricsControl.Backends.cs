using System.Text.RegularExpressions;
using FluentIcons.Common;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Kubernetes;
using LiveChartsCore.Defaults;

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

            var request = ApplySelectedTimeRange(panel.Request, selectedTimeRange);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
            var result = await cluster.Runtime.RequestMetricsAsync(request, linkedCts.Token).ConfigureAwait(false);
            var series = CreateSeries(panel, result);
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
        var panelResult = await LoadPodContainerPrometheusPanelSnapshotAsync(cluster, tabDefinition.Title, tabDefinition, pod, container, selectedTimeRange, logger, cancellationToken).ConfigureAwait(false);
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

            var request = ApplySelectedTimeRange(panel.Request, selectedTimeRange);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
            var result = await cluster.Runtime.RequestMetricsAsync(request, linkedCts.Token).ConfigureAwait(false);
            var series = CreateSeries(panel, result);
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

        return series
            .OrderBy(static snapshot => snapshot.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

}

internal static class MetricsControlMetricsServerBackend
{
    public static IReadOnlyList<MetricTabSnapshot> CapturePodContainerMetricsServerCharts(
        ClusterWorkspace cluster,
        V1Pod pod,
        string containerName)
    {
        var samples = new List<MetricsServerSamplePoint>();
        foreach (var metric in cluster.Runtime.PodMetrics)
        {
            if (!string.Equals(metric.Name(), pod.Name(), StringComparison.Ordinal)
                || !string.Equals(metric.Namespace(), pod.Namespace(), StringComparison.Ordinal)
                || !metric.Timestamp.HasValue)
            {
                continue;
            }

            var containerMetric = metric.Containers?.FirstOrDefault(x => string.Equals(x.Name, containerName, StringComparison.Ordinal));
            if (containerMetric == null)
            {
                continue;
            }

            samples.Add(new MetricsServerSamplePoint(
                metric.Timestamp.Value.ToLocalTime(),
                (double)containerMetric.Usage["cpu"].ToDecimal(),
                containerMetric.Usage["memory"].ToInt64()));
        }

        SortSamples(samples);
        return CreateMetricsServerTabs(samples);
    }

    public static async Task<IReadOnlyList<MetricTabSnapshot>> TryCaptureMetricsServerChartsAsync(
        ClusterWorkspace cluster,
        IKubernetesObject<V1ObjectMeta> resource,
        CancellationToken cancellationToken)
    {
        if (resource is V1Pod pod)
        {
            return CreateMetricsServerTabs(CreatePodMetricsServerSamples(cluster, [pod]));
        }

        if (resource is V1Node node)
        {
            return CreateMetricsServerTabs(CreateNodeMetricsServerSamples(cluster, node));
        }

        var workloadScope = GetPodScope(resource);
        if (workloadScope == null || (!workloadScope.Value.IsNamespace && workloadScope.Value.Selector == null))
        {
            return [];
        }

        var pods = await ResourceMetricsCatalog.GetMatchingPodsAsync(
            cluster,
            workloadScope.Value.Namespace,
            workloadScope.Value.Selector,
            cancellationToken);
        return CreateMetricsServerTabs(CreatePodMetricsServerSamples(cluster, pods));
    }

    private static (string Namespace, V1LabelSelector? Selector, bool IsNamespace)? GetPodScope(IKubernetesObject<V1ObjectMeta> resource)
    {
        return resource switch
        {
            V1Deployment deployment => (deployment.Namespace(), deployment.Spec?.Selector, false),
            V1StatefulSet statefulSet => (statefulSet.Namespace(), statefulSet.Spec?.Selector, false),
            V1DaemonSet daemonSet => (daemonSet.Namespace(), daemonSet.Spec?.Selector, false),
            V1ReplicaSet replicaSet => (replicaSet.Namespace(), replicaSet.Spec?.Selector, false),
            V1Job job => (job.Namespace(), job.Spec?.Selector, false),
            V1Namespace ns => (ns.Name(), null, true),
            _ => null,
        };
    }

    private static List<MetricsServerSamplePoint> CreatePodMetricsServerSamples(ClusterWorkspace cluster, IReadOnlyList<V1Pod> pods)
    {
        HashSet<(string? Namespace, string? Name)> selectedPods = [];
        foreach (var pod in pods)
        {
            selectedPods.Add((pod.Namespace(), pod.Name()));
        }

        Dictionary<DateTime, (double Cpu, double Memory)> aggregates = [];
        foreach (var metric in cluster.Runtime.PodMetrics)
        {
            if (!metric.Timestamp.HasValue
                || !selectedPods.Contains((metric.Namespace(), metric.Name())))
            {
                continue;
            }

            var timestamp = metric.Timestamp.Value.ToUniversalTime();
            aggregates.TryGetValue(timestamp, out var aggregate);
            if (metric.Containers != null)
            {
                foreach (var containerMetric in metric.Containers)
                {
                    aggregate.Cpu += (double)containerMetric.Usage["cpu"].ToDecimal();
                    aggregate.Memory += containerMetric.Usage["memory"].ToInt64();
                }
            }

            aggregates[timestamp] = aggregate;
        }

        var samples = new List<MetricsServerSamplePoint>();
        foreach (var (timestamp, aggregate) in aggregates)
        {
            samples.Add(new MetricsServerSamplePoint(timestamp.ToLocalTime(), aggregate.Cpu, aggregate.Memory));
        }

        SortSamples(samples);
        return samples;
    }

    private static List<MetricsServerSamplePoint> CreateNodeMetricsServerSamples(ClusterWorkspace cluster, V1Node node)
    {
        var samples = new List<MetricsServerSamplePoint>();
        foreach (var metric in cluster.Runtime.NodeMetrics)
        {
            if (!string.Equals(metric.Name(), node.Name(), StringComparison.Ordinal)
                || !metric.Timestamp.HasValue)
            {
                continue;
            }

            samples.Add(new MetricsServerSamplePoint(
                metric.Timestamp.Value.ToLocalTime(),
                (double)metric.Usage["cpu"].ToDecimal(),
                metric.Usage["memory"].ToInt64()));
        }

        return samples;
    }

    private static void SortSamples(List<MetricsServerSamplePoint> samples)
    {
        samples.Sort(static (left, right) => left.Timestamp.CompareTo(right.Timestamp));
    }

    private static IReadOnlyList<MetricTabSnapshot> CreateMetricsServerTabs(IReadOnlyList<MetricsServerSamplePoint> samples)
    {
        if (samples.Count == 0)
        {
            return [];
        }

        var cpuPoints = CreateMetricsServerChartPoints(samples, static x => x.Cpu);
        var memoryPoints = CreateMetricsServerChartPoints(samples, static x => x.Memory);

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

    private static IReadOnlyList<DateTimePoint> CreateMetricsServerChartPoints(
        IReadOnlyList<MetricsServerSamplePoint> samples,
        Func<MetricsServerSamplePoint, double> selector)
    {
        return samples
            .Select(sample => new DateTimePoint(sample.Timestamp, selector(sample)))
            .ToArray();
    }
}
