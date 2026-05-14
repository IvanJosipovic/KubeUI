using System.Text.RegularExpressions;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

internal static class ResourceMetricsCatalog
{
    public static async Task<ResourceMetricsDescriptor?> CreateAsync(ClusterWorkspaceViewModel cluster, object resource)
    {
        return resource switch
        {
            V1Pod pod => CreatePodDescriptor(pod),
            V1Node node => CreateNodeDescriptor(node),
            V1Namespace ns => CreateNamespaceDescriptor(ns),
            V1Deployment deployment => await CreateWorkloadDescriptorAsync(cluster, deployment.Namespace(), deployment.Spec?.Selector, "Deployment", "namespace"),
            V1StatefulSet statefulSet => await CreateWorkloadDescriptorAsync(cluster, statefulSet.Namespace(), statefulSet.Spec?.Selector, "StatefulSet", "namespace"),
            V1DaemonSet daemonSet => await CreateWorkloadDescriptorAsync(cluster, daemonSet.Namespace(), daemonSet.Spec?.Selector, "DaemonSet", "namespace"),
            V1ReplicaSet replicaSet => await CreateWorkloadDescriptorAsync(cluster, replicaSet.Namespace(), replicaSet.Spec?.Selector, "ReplicaSet", "namespace"),
            V1Job job => await CreateWorkloadDescriptorAsync(cluster, job.Namespace(), job.Spec?.Selector, "Job", "namespace"),
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
                new MetricTabDefinition("CPU", Icon.TopSpeed,
                [
                    MetricPanel("CPU", MetricCategory.Pods, podQueryOptions, ("cpuUsage", "Usage"), ("cpuRequests", "Requests"), ("cpuLimits", "Limits")),
                ]),
                new MetricTabDefinition("Memory", Icon.Ram,
                [
                    MetricPanel("Memory", MetricCategory.Pods, podQueryOptions, ("memoryUsage", "Usage"), ("memoryRequests", "Requests"), ("memoryLimits", "Limits")),
                ]),
                new MetricTabDefinition("Network", Icon.VirtualNetwork,
                [
                    MetricPanel("Network", MetricCategory.Pods, podQueryOptions, ("networkReceive", "Receive"), ("networkTransmit", "Transmit")),
                ]),
                new MetricTabDefinition("Filesystem", Icon.HardDrive,
                [
                    MetricPanel("Filesystem", MetricCategory.Pods, podQueryOptions, ("fsUsage", "Usage"), ("fsReads", "Reads"), ("fsWrites", "Writes")),
                ]),
            ],
            "No Prometheus metrics are available for this pod.");
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
                new MetricTabDefinition("CPU", Icon.TopSpeed,
                [
                    MetricPanel("CPU", MetricCategory.Nodes, options, filter, ("cpuUsage", "Usage"), ("cpuCapacity", "Capacity"), ("cpuAllocatableCapacity", "Allocatable")),
                ]),
                new MetricTabDefinition("Memory", Icon.Ram,
                [
                    MetricPanel("Memory", MetricCategory.Nodes, options, filter, ("memoryUsage", "Usage"), ("memoryCapacity", "Capacity"), ("memoryAllocatableCapacity", "Allocatable")),
                ]),
                new MetricTabDefinition("Filesystem", Icon.HardDrive,
                [
                    MetricPanel("Filesystem", MetricCategory.Nodes, options, filter, ("fsUsage", "Usage"), ("fsSize", "Size")),
                ]),
            ],
            "No Prometheus metrics are available for this node.");
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
                new MetricTabDefinition("CPU", Icon.TopSpeed,
                [
                    MetricPanel("CPU", MetricCategory.Namespace, options, ("cpuUsage", "Usage"), ("cpuRequests", "Requests"), ("cpuLimits", "Limits")),
                ]),
                new MetricTabDefinition("Memory", Icon.Ram,
                [
                    MetricPanel("Memory", MetricCategory.Namespace, options, ("memoryUsage", "Usage"), ("memoryRequests", "Requests"), ("memoryLimits", "Limits")),
                ]),
            ],
            "No Prometheus metrics are available for this namespace.");
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
                new MetricTabDefinition("Storage", Icon.Storage,
                [
                    MetricPanel("PVC Storage", MetricCategory.Pvc, options, ("diskUsage", "Usage"), ("diskCapacity", "Capacity")),
                ]),
            ],
            "No Prometheus metrics are available for this persistent volume claim.");
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
                new MetricTabDefinition("Bytes Sent", Icon.DataUsage,
                [
                    MetricPanel("Bytes Sent", MetricCategory.Ingress, options, ("bytesSentSuccess", "Success"), ("bytesSentFailure", "Failure")),
                ]),
                new MetricTabDefinition("Latency", Icon.DataUsage,
                [
                    MetricPanel("Latency", MetricCategory.Ingress, options, ("requestDurationSeconds", "Request"), ("responseDurationSeconds", "Response")),
                ]),
            ],
            "No Prometheus ingress metrics are available for this ingress.");
    }

    private static async Task<ResourceMetricsDescriptor?> CreateWorkloadDescriptorAsync(ClusterWorkspaceViewModel cluster, string ns, V1LabelSelector? selector, string workloadName, string seriesSelector)
    {
        if (string.IsNullOrWhiteSpace(ns))
        {
            return null;
        }

        await cluster.SeedResource<V1Pod>(true).ConfigureAwait(false);
        var pods = cluster.GetResourceList<V1Pod>()
            .Where(pod => string.Equals(pod.Namespace(), ns, StringComparison.Ordinal) && MatchesSelector(pod, selector))
            .Select(pod => Regex.Escape(pod.Name()))
            .ToArray();

        if (pods.Length == 0)
        {
            return new ResourceMetricsDescriptor([], $"No pods were found for this {workloadName.ToLowerInvariant()}, so no metrics can be shown.");
        }

        var options = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["namespace"] = ns,
            ["pods"] = string.Join("|", pods),
            ["selector"] = seriesSelector,
        };

        return new ResourceMetricsDescriptor(
            [
                new MetricTabDefinition("CPU", Icon.TopSpeed,
                [
                    MetricPanel("CPU", MetricCategory.WorkloadPods, options, ("cpuUsage", "Usage"), ("cpuRequests", "Requests"), ("cpuLimits", "Limits")),
                ]),
                new MetricTabDefinition("Memory", Icon.Ram,
                [
                    MetricPanel("Memory", MetricCategory.WorkloadPods, options, ("memoryUsage", "Usage"), ("memoryRequests", "Requests"), ("memoryLimits", "Limits")),
                ]),
                new MetricTabDefinition("Network", Icon.VirtualNetwork,
                [
                    MetricPanel("Network", MetricCategory.WorkloadPods, options, ("networkReceive", "Receive"), ("networkTransmit", "Transmit")),
                ]),
                new MetricTabDefinition("Filesystem", Icon.HardDrive,
                [
                    MetricPanel("Filesystem", MetricCategory.WorkloadPods, options, ("fsUsage", "Usage"), ("fsReads", "Reads"), ("fsWrites", "Writes")),
                ]),
            ],
            $"No Prometheus metrics are available for this {workloadName.ToLowerInvariant()}.");
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
