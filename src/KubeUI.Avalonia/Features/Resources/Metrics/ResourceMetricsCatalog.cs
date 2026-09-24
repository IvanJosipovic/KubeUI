using System.Text.RegularExpressions;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Metrics;

internal static class ResourceMetricsCatalog
{
    public static async Task<ResourceMetricsDescriptor?> CreateAsync(ClusterWorkspace cluster, object resource)
    {
        return resource switch
        {
            V1Pod pod => CreatePodDescriptor(pod),
            V1Node node => CreateNodeDescriptor(node),
            V1Namespace ns => CreateNamespaceDescriptor(ns),
            V1Deployment deployment => await CreateWorkloadDescriptorAsync(cluster, deployment.Namespace(), deployment.Spec?.Selector, Assets.Resources.Metrics_Deployment!, "namespace"),
            V1StatefulSet statefulSet => await CreateWorkloadDescriptorAsync(cluster, statefulSet.Namespace(), statefulSet.Spec?.Selector, Assets.Resources.Metrics_StatefulSet!, "namespace"),
            V1DaemonSet daemonSet => await CreateWorkloadDescriptorAsync(cluster, daemonSet.Namespace(), daemonSet.Spec?.Selector, Assets.Resources.Metrics_DaemonSet!, "namespace"),
            V1ReplicaSet replicaSet => await CreateWorkloadDescriptorAsync(cluster, replicaSet.Namespace(), replicaSet.Spec?.Selector, Assets.Resources.Metrics_ReplicaSet!, "namespace"),
            V1Job job => await CreateWorkloadDescriptorAsync(cluster, job.Namespace(), job.Spec?.Selector, Assets.Resources.Metrics_Job!, "namespace"),
            V1PersistentVolumeClaim pvc => CreatePvcDescriptor(pvc),
            V1Ingress ingress => CreateIngressDescriptor(ingress),
            _ => null,
        };
    }

    private static ResourceMetricsDescriptor CreatePodDescriptor(V1Pod pod)
    {
        var escapedPod = Regex.Escape(pod.Name());
        var ns = pod.Namespace();
        var podQueryOptions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["namespace"] = ns,
            ["pods"] = escapedPod,
            ["selector"] = "pod, namespace",
        };

        return new ResourceMetricsDescriptor(
            [
                new MetricTabDefinition(Assets.Resources.Metrics_CPU!, Icon.TopSpeed,
                [
                    MetricPanel(Assets.Resources.Metrics_CPU!, MetricCategory.Pods, podQueryOptions, ("cpuUsage", Assets.Resources.Metrics_Usage!), ("cpuRequests", Assets.Resources.Metrics_Requests!), ("cpuLimits", Assets.Resources.Metrics_Limits!)),
                ]),
                new MetricTabDefinition(Assets.Resources.Metrics_Memory!, Icon.Ram,
                [
                    MetricPanel(Assets.Resources.Metrics_Memory!, MetricCategory.Pods, podQueryOptions, ("memoryUsage", Assets.Resources.Metrics_Usage!), ("memoryRequests", Assets.Resources.Metrics_Requests!), ("memoryLimits", Assets.Resources.Metrics_Limits!)),
                ]),
                new MetricTabDefinition(Assets.Resources.Metrics_Network!, Icon.VirtualNetwork,
                [
                    MetricPanel(Assets.Resources.Metrics_Network!, MetricCategory.Pods, podQueryOptions, ("networkReceive", Assets.Resources.Metrics_Receive!), ("networkTransmit", Assets.Resources.Metrics_Transmit!)),
                ]),
                new MetricTabDefinition(Assets.Resources.Metrics_Filesystem!, Icon.HardDrive,
                [
                    MetricPanel(Assets.Resources.Metrics_Filesystem!, MetricCategory.Pods, podQueryOptions, ("fsUsage", Assets.Resources.Metrics_Usage!), ("fsReads", Assets.Resources.Metrics_Reads!), ("fsWrites", Assets.Resources.Metrics_Writes!)),
                ]),
            ],
            Assets.Resources.Metrics_NoPrometheusPod);
    }

    private static ResourceMetricsDescriptor CreateNodeDescriptor(V1Node node)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["nodes"] = Regex.Escape(node.Name()),
            ["mountpoints"] = ".*",
        };

        Func<MetricSeries, bool> filter = series => series.Labels.TryGetValue("node", out var value) ? value == node.Name() : true;

        return new ResourceMetricsDescriptor(
            [
                new MetricTabDefinition(Assets.Resources.Metrics_CPU!, Icon.TopSpeed,
                [
                    MetricPanel(Assets.Resources.Metrics_CPU!, MetricCategory.Nodes, options, filter, ("cpuUsage", Assets.Resources.Metrics_Usage!), ("cpuCapacity", Assets.Resources.Metrics_Capacity!), ("cpuAllocatableCapacity", Assets.Resources.Metrics_Allocatable!)),
                ]),
                new MetricTabDefinition(Assets.Resources.Metrics_Memory!, Icon.Ram,
                [
                    MetricPanel(Assets.Resources.Metrics_Memory!, MetricCategory.Nodes, options, filter, ("memoryUsage", Assets.Resources.Metrics_Usage!), ("memoryCapacity", Assets.Resources.Metrics_Capacity!), ("memoryAllocatableCapacity", Assets.Resources.Metrics_Allocatable!)),
                ]),
                new MetricTabDefinition(Assets.Resources.Metrics_Filesystem!, Icon.HardDrive,
                [
                    MetricPanel(Assets.Resources.Metrics_Filesystem!, MetricCategory.Nodes, options, filter, ("fsUsage", Assets.Resources.Metrics_Usage!), ("fsSize", Assets.Resources.Metrics_Size!)),
                ]),
            ],
            Assets.Resources.Metrics_NoPrometheusNode);
    }

    private static ResourceMetricsDescriptor CreateNamespaceDescriptor(V1Namespace ns)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["namespace"] = ns.Name(),
            ["pods"] = ".*",
            ["selector"] = "namespace",
        };

        return new ResourceMetricsDescriptor(
            [
                new MetricTabDefinition(Assets.Resources.Metrics_CPU!, Icon.TopSpeed,
                [
                    MetricPanel(Assets.Resources.Metrics_CPU!, MetricCategory.Namespace, options, ("cpuUsage", Assets.Resources.Metrics_Usage!), ("cpuRequests", Assets.Resources.Metrics_Requests!), ("cpuLimits", Assets.Resources.Metrics_Limits!)),
                ]),
                new MetricTabDefinition(Assets.Resources.Metrics_Memory!, Icon.Ram,
                [
                    MetricPanel(Assets.Resources.Metrics_Memory!, MetricCategory.Namespace, options, ("memoryUsage", Assets.Resources.Metrics_Usage!), ("memoryRequests", Assets.Resources.Metrics_Requests!), ("memoryLimits", Assets.Resources.Metrics_Limits!)),
                ]),
            ],
            Assets.Resources.Metrics_NoPrometheusNamespace);
    }

    private static ResourceMetricsDescriptor CreatePvcDescriptor(V1PersistentVolumeClaim pvc)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["namespace"] = pvc.Namespace(),
            ["pvc"] = pvc.Name(),
        };

        return new ResourceMetricsDescriptor(
            [
                new MetricTabDefinition(Assets.Resources.Metrics_Storage!, Icon.Storage,
                [
                    MetricPanel(Assets.Resources.Metrics_PvcStorage!, MetricCategory.Pvc, options, ("diskUsage", Assets.Resources.Metrics_Usage!), ("diskCapacity", Assets.Resources.Metrics_Capacity!)),
                ]),
            ],
            Assets.Resources.Metrics_NoPrometheusPvc);
    }

    private static ResourceMetricsDescriptor CreateIngressDescriptor(V1Ingress ingress)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["namespace"] = ingress.Namespace(),
            ["ingress"] = ingress.Name(),
        };

        return new ResourceMetricsDescriptor(
            [
                new MetricTabDefinition(Assets.Resources.Metrics_BytesSent!, Icon.DataUsage,
                [
                    MetricPanel(Assets.Resources.Metrics_BytesSent!, MetricCategory.Ingress, options, ("bytesSentSuccess", Assets.Resources.Metrics_Success!), ("bytesSentFailure", Assets.Resources.Metrics_Failure!)),
                ]),
                new MetricTabDefinition(Assets.Resources.Metrics_Latency!, Icon.DataUsage,
                [
                    MetricPanel(Assets.Resources.Metrics_Latency!, MetricCategory.Ingress, options, ("requestDurationSeconds", Assets.Resources.Metrics_Request!), ("responseDurationSeconds", Assets.Resources.Metrics_Response!)),
                ]),
            ],
            Assets.Resources.Metrics_NoPrometheusIngress);
    }

    private static async Task<ResourceMetricsDescriptor?> CreateWorkloadDescriptorAsync(ClusterWorkspace cluster, string ns, V1LabelSelector? selector, string workloadName, string seriesSelector)
    {
        if (string.IsNullOrWhiteSpace(ns))
        {
            return null;
        }

        if (selector == null)
        {
            return new ResourceMetricsDescriptor([], string.Format(Assets.Resources.Metrics_NoPodsForWorkload!, workloadName));
        }

        var matchingPods = await GetMatchingPodsAsync(cluster, ns, selector).ConfigureAwait(false);
        var pods = matchingPods
            .Select(pod => Regex.Escape(pod.Name()))
            .ToArray();

        if (pods.Length == 0)
        {
            return new ResourceMetricsDescriptor([], string.Format(Assets.Resources.Metrics_NoPodsForWorkload!, workloadName));
        }

        var options = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["namespace"] = ns,
            ["pods"] = string.Join("|", pods),
            ["selector"] = seriesSelector,
        };

        return new ResourceMetricsDescriptor(
            [
                new MetricTabDefinition(Assets.Resources.Metrics_CPU!, Icon.TopSpeed,
                [
                    MetricPanel(Assets.Resources.Metrics_CPU!, MetricCategory.WorkloadPods, options, ("cpuUsage", Assets.Resources.Metrics_Usage!), ("cpuRequests", Assets.Resources.Metrics_Requests!), ("cpuLimits", Assets.Resources.Metrics_Limits!)),
                ]),
                new MetricTabDefinition(Assets.Resources.Metrics_Memory!, Icon.Ram,
                [
                    MetricPanel(Assets.Resources.Metrics_Memory!, MetricCategory.WorkloadPods, options, ("memoryUsage", Assets.Resources.Metrics_Usage!), ("memoryRequests", Assets.Resources.Metrics_Requests!), ("memoryLimits", Assets.Resources.Metrics_Limits!)),
                ]),
                new MetricTabDefinition(Assets.Resources.Metrics_Network!, Icon.VirtualNetwork,
                [
                    MetricPanel(Assets.Resources.Metrics_Network!, MetricCategory.WorkloadPods, options, ("networkReceive", Assets.Resources.Metrics_Receive!), ("networkTransmit", Assets.Resources.Metrics_Transmit!)),
                ]),
                new MetricTabDefinition(Assets.Resources.Metrics_Filesystem!, Icon.HardDrive,
                [
                    MetricPanel(Assets.Resources.Metrics_Filesystem!, MetricCategory.WorkloadPods, options, ("fsUsage", Assets.Resources.Metrics_Usage!), ("fsReads", Assets.Resources.Metrics_Reads!), ("fsWrites", Assets.Resources.Metrics_Writes!)),
                ]),
            ],
            string.Format(Assets.Resources.Metrics_NoPrometheusWorkload!, workloadName));
    }

    internal static async Task<IReadOnlyList<V1Pod>> GetMatchingPodsAsync(
        ClusterWorkspace cluster,
        string ns,
        V1LabelSelector? selector,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ns))
        {
            return [];
        }

        await cluster.Runtime.SeedResource<V1Pod>(true, cancellationToken).ConfigureAwait(false);
        return cluster.Runtime.GetResourceList<V1Pod>()
            .Where(pod => string.Equals(pod.Namespace(), ns, StringComparison.Ordinal) && MatchesSelector(pod, selector))
            .ToArray();
    }

    private static MetricPanelDefinition MetricPanel(string title, MetricCategory category, IDictionary<string, string> options, params (string QueryName, string Label)[] queries)
    {
        return MetricPanel(title, category, options, static _ => true, queries);
    }

    private static MetricPanelDefinition MetricPanel(string title, MetricCategory category, IDictionary<string, string> options, Func<MetricSeries, bool> filter, params (string QueryName, string Label)[] queries)
    {
        return new MetricPanelDefinition
        {
            Title = title,
            Filter = filter,
            LegendLabels = queries.ToDictionary(static x => x.QueryName, static x => x.Label, StringComparer.Ordinal),
            Request = new MetricRequest
            {
                Category = category,
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

    private static bool MatchesSelector(V1Pod pod, V1LabelSelector? selector)
    {
        var labels = pod.Metadata?.Labels ?? new Dictionary<string, string>(StringComparer.Ordinal);

        if (selector?.MatchLabels != null)
        {
            foreach (var pair in selector.MatchLabels)
            {
                if (!labels.TryGetValue(pair.Key, out var value) || !string.Equals(value, pair.Value, StringComparison.Ordinal))
                {
                    return false;
                }
            }
        }

        if (selector?.MatchExpressions != null)
        {
            foreach (var expression in selector.MatchExpressions)
            {
                labels.TryGetValue(expression.Key, out var labelValue);

                switch (expression.OperatorProperty)
                {
                    case "In" when expression.Values?.Contains(labelValue) != true:
                    case "Exists" when labelValue == null:
                    case "NotIn" when labelValue != null && expression.Values?.Contains(labelValue) == true:
                    case "DoesNotExist" when labelValue != null:
                        return false;
                }
            }
        }

        return true;
    }
}

internal sealed record ResourceMetricsDescriptor(IReadOnlyList<MetricTabDefinition> Tabs, string? EmptyState);

internal sealed class MetricPanelDefinition
{
    public required string Title { get; init; }

    public required MetricRequest Request { get; init; }

    public required IReadOnlyDictionary<string, string> LegendLabels { get; init; }

    public required Func<MetricSeries, bool> Filter { get; init; }
}

internal sealed record MetricTabDefinition(string Title, Icon Icon, IReadOnlyList<MetricPanelDefinition> Panels);
