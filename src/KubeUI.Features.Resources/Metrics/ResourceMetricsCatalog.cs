using System.Text.RegularExpressions;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Resources.Metrics;

internal static class ResourceMetricsCatalog
{
    public static async Task<ResourceMetricsDescriptor?> CreateAsync(ClusterWorkspace cluster, object resource)
    {
        return resource switch
        {
            V1Pod pod => CreatePodDescriptor(pod),
            V1Node node => CreateNodeDescriptor(node),
            V1Namespace ns => CreateNamespaceDescriptor(ns),
            V1Deployment deployment => await CreateWorkloadDescriptorAsync(cluster, deployment.Namespace(), deployment.Spec?.Selector, ResourcesLanguageManager.Instance.Shared_Deployment.CurrentValue()!, "namespace"),
            V1StatefulSet statefulSet => await CreateWorkloadDescriptorAsync(cluster, statefulSet.Namespace(), statefulSet.Spec?.Selector, ResourcesLanguageManager.Instance.Shared_StatefulSet.CurrentValue()!, "namespace"),
            V1DaemonSet daemonSet => await CreateWorkloadDescriptorAsync(cluster, daemonSet.Namespace(), daemonSet.Spec?.Selector, ResourcesLanguageManager.Instance.Shared_DaemonSet.CurrentValue()!, "namespace"),
            V1ReplicaSet replicaSet => await CreateWorkloadDescriptorAsync(cluster, replicaSet.Namespace(), replicaSet.Spec?.Selector, ResourcesLanguageManager.Instance.Shared_ReplicaSet.CurrentValue()!, "namespace"),
            V1Job job => await CreateWorkloadDescriptorAsync(cluster, job.Namespace(), job.Spec?.Selector, ResourcesLanguageManager.Instance.Shared_Job.CurrentValue()!, "namespace"),
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
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_CPU.CurrentValue()!, Icon.TopSpeed,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_CPU.CurrentValue()!, MetricCategory.Pods, podQueryOptions, ("cpuUsage", ResourcesLanguageManager.Instance.Shared_Usage.CurrentValue()!), ("cpuRequests", ResourcesLanguageManager.Instance.Shared_Requests.CurrentValue()!), ("cpuLimits", ResourcesLanguageManager.Instance.Shared_Limits.CurrentValue()!)),
                ]),
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_Memory.CurrentValue()!, Icon.Ram,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_Memory.CurrentValue()!, MetricCategory.Pods, podQueryOptions, ("memoryUsage", ResourcesLanguageManager.Instance.Shared_Usage.CurrentValue()!), ("memoryRequests", ResourcesLanguageManager.Instance.Shared_Requests.CurrentValue()!), ("memoryLimits", ResourcesLanguageManager.Instance.Shared_Limits.CurrentValue()!)),
                ]),
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_Network.CurrentValue()!, Icon.VirtualNetwork,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_Network.CurrentValue()!, MetricCategory.Pods, podQueryOptions, ("networkReceive", ResourcesLanguageManager.Instance.Shared_Receive.CurrentValue()!), ("networkTransmit", ResourcesLanguageManager.Instance.Shared_Transmit.CurrentValue()!)),
                ]),
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_Filesystem.CurrentValue()!, Icon.HardDrive,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_Filesystem.CurrentValue()!, MetricCategory.Pods, podQueryOptions, ("fsUsage", ResourcesLanguageManager.Instance.Shared_Usage.CurrentValue()!), ("fsReads", ResourcesLanguageManager.Instance.Shared_Reads.CurrentValue()!), ("fsWrites", ResourcesLanguageManager.Instance.Shared_Writes.CurrentValue()!)),
                ]),
            ],
            ResourcesLanguageManager.Instance.ResourceMetricsCatalog_NoPrometheusPod.CurrentValue());
    }

    private static ResourceMetricsDescriptor CreateNodeDescriptor(V1Node node)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["instance"] = GetNodeInstancePattern(node),
            ["node"] = Regex.Escape(node.Name()),
            ["mountpoints"] = ".*",
        };

        var nodeNames = GetNodeInstanceNames(node);
        Func<MetricSeries, bool> filter = series =>
            series.Labels.TryGetValue("node", out var nodeName) && string.Equals(nodeName, node.Name(), StringComparison.Ordinal)
            || series.Labels.TryGetValue("instance", out var instance) && nodeNames.Any(name =>
                string.Equals(instance, name, StringComparison.OrdinalIgnoreCase)
                || instance.StartsWith(name + ":", StringComparison.OrdinalIgnoreCase));

        return new ResourceMetricsDescriptor(
            [
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_CPU.CurrentValue()!, Icon.TopSpeed,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_CPU.CurrentValue()!, MetricCategory.Nodes, options, filter, ("cpuUsage", ResourcesLanguageManager.Instance.Shared_Usage.CurrentValue()!), ("cpuCapacity", ResourcesLanguageManager.Instance.Shared_Capacity.CurrentValue()!), ("cpuAllocatableCapacity", ResourcesLanguageManager.Instance.Shared_Allocatable.CurrentValue()!)),
                ]),
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_Memory.CurrentValue()!, Icon.Ram,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_Memory.CurrentValue()!, MetricCategory.Nodes, options, filter, ("memoryUsage", ResourcesLanguageManager.Instance.Shared_Usage.CurrentValue()!), ("memoryCapacity", ResourcesLanguageManager.Instance.Shared_Capacity.CurrentValue()!), ("memoryAllocatableCapacity", ResourcesLanguageManager.Instance.Shared_Allocatable.CurrentValue()!)),
                ]),
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_Filesystem.CurrentValue()!, Icon.HardDrive,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_Filesystem.CurrentValue()!, MetricCategory.Nodes, options, filter, ("fsUsage", ResourcesLanguageManager.Instance.Shared_Usage.CurrentValue()!), ("fsSize", ResourcesLanguageManager.Instance.Shared_Size.CurrentValue()!)),
                ]),
            ],
            ResourcesLanguageManager.Instance.ResourceMetricsCatalog_NoPrometheusNode.CurrentValue());
    }

    internal static string GetNodeInstancePattern(V1Node node)
    {
        return string.Join("|", GetNodeInstanceNames(node).Select(Regex.Escape));
    }

    private static string[] GetNodeInstanceNames(V1Node node)
    {
        List<string> names = [];
        var nodeName = node.Name();
        if (!string.IsNullOrWhiteSpace(nodeName))
        {
            names.Add(nodeName);
        }

        if (node.Status?.Addresses != null)
        {
            foreach (var address in node.Status.Addresses)
            {
                if (!string.IsNullOrWhiteSpace(address.Address) && !names.Contains(address.Address, StringComparer.OrdinalIgnoreCase))
                {
                    names.Add(address.Address);
                }
            }
        }

        return names.ToArray();
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
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_CPU.CurrentValue()!, Icon.TopSpeed,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_CPU.CurrentValue()!, MetricCategory.Namespace, options, ("cpuUsage", ResourcesLanguageManager.Instance.Shared_Usage.CurrentValue()!), ("cpuRequests", ResourcesLanguageManager.Instance.Shared_Requests.CurrentValue()!), ("cpuLimits", ResourcesLanguageManager.Instance.Shared_Limits.CurrentValue()!)),
                ]),
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_Memory.CurrentValue()!, Icon.Ram,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_Memory.CurrentValue()!, MetricCategory.Namespace, options, ("memoryUsage", ResourcesLanguageManager.Instance.Shared_Usage.CurrentValue()!), ("memoryRequests", ResourcesLanguageManager.Instance.Shared_Requests.CurrentValue()!), ("memoryLimits", ResourcesLanguageManager.Instance.Shared_Limits.CurrentValue()!)),
                ]),
            ],
            ResourcesLanguageManager.Instance.ResourceMetricsCatalog_NoPrometheusNamespace.CurrentValue());
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
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_Storage.CurrentValue()!, Icon.Storage,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_PvcStorage.CurrentValue()!, MetricCategory.Pvc, options, ("diskUsage", ResourcesLanguageManager.Instance.Shared_Usage.CurrentValue()!), ("diskCapacity", ResourcesLanguageManager.Instance.Shared_Capacity.CurrentValue()!)),
                ]),
            ],
            ResourcesLanguageManager.Instance.ResourceMetricsCatalog_NoPrometheusPvc.CurrentValue());
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
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_BytesSent.CurrentValue()!, Icon.DataUsage,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_BytesSent.CurrentValue()!, MetricCategory.Ingress, options, ("bytesSentSuccess", ResourcesLanguageManager.Instance.Shared_Success.CurrentValue()!), ("bytesSentFailure", ResourcesLanguageManager.Instance.Shared_Failure.CurrentValue()!)),
                ]),
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_Latency.CurrentValue()!, Icon.DataUsage,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_Latency.CurrentValue()!, MetricCategory.Ingress, options, ("requestDurationSeconds", ResourcesLanguageManager.Instance.Shared_Request.CurrentValue()!), ("responseDurationSeconds", ResourcesLanguageManager.Instance.Shared_Response.CurrentValue()!)),
                ]),
            ],
            ResourcesLanguageManager.Instance.ResourceMetricsCatalog_NoPrometheusIngress.CurrentValue());
    }

    private static async Task<ResourceMetricsDescriptor?> CreateWorkloadDescriptorAsync(ClusterWorkspace cluster, string ns, V1LabelSelector? selector, string workloadName, string seriesSelector)
    {
        if (string.IsNullOrWhiteSpace(ns))
        {
            return null;
        }

        if (selector == null)
        {
            return new ResourceMetricsDescriptor([], string.Format(ResourcesLanguageManager.Instance.ResourceMetricsCatalog_NoPodsForWorkload.CurrentValue()!, workloadName));
        }

        var matchingPods = await GetMatchingPodsAsync(cluster, ns, selector).ConfigureAwait(false);
        if (matchingPods.Count == 0)
        {
            return new ResourceMetricsDescriptor([], string.Format(ResourcesLanguageManager.Instance.ResourceMetricsCatalog_NoPodsForWorkload.CurrentValue()!, workloadName));
        }

        // kube-state-metrics may not expose these Kubernetes labels as kube_pod_labels. Use the
        // matching Pod names unless label availability has been verified for this Prometheus source.
        var options = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["namespace"] = ns,
            ["pods"] = string.Join("|", matchingPods.Select(pod => Regex.Escape(pod.Name()))),
            ["selector"] = seriesSelector,
        };

        return new ResourceMetricsDescriptor(
            [
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_CPU.CurrentValue()!, Icon.TopSpeed,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_CPU.CurrentValue()!, MetricCategory.WorkloadPods, options, ("cpuUsage", ResourcesLanguageManager.Instance.Shared_Usage.CurrentValue()!), ("cpuRequests", ResourcesLanguageManager.Instance.Shared_Requests.CurrentValue()!), ("cpuLimits", ResourcesLanguageManager.Instance.Shared_Limits.CurrentValue()!)),
                ]),
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_Memory.CurrentValue()!, Icon.Ram,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_Memory.CurrentValue()!, MetricCategory.WorkloadPods, options, ("memoryUsage", ResourcesLanguageManager.Instance.Shared_Usage.CurrentValue()!), ("memoryRequests", ResourcesLanguageManager.Instance.Shared_Requests.CurrentValue()!), ("memoryLimits", ResourcesLanguageManager.Instance.Shared_Limits.CurrentValue()!)),
                ]),
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_Network.CurrentValue()!, Icon.VirtualNetwork,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_Network.CurrentValue()!, MetricCategory.WorkloadPods, options, ("networkReceive", ResourcesLanguageManager.Instance.Shared_Receive.CurrentValue()!), ("networkTransmit", ResourcesLanguageManager.Instance.Shared_Transmit.CurrentValue()!)),
                ]),
                new MetricTabDefinition(ResourcesLanguageManager.Instance.Shared_Filesystem.CurrentValue()!, Icon.HardDrive,
                [
                    MetricPanel(ResourcesLanguageManager.Instance.Shared_Filesystem.CurrentValue()!, MetricCategory.WorkloadPods, options, ("fsUsage", ResourcesLanguageManager.Instance.Shared_Usage.CurrentValue()!), ("fsReads", ResourcesLanguageManager.Instance.Shared_Reads.CurrentValue()!), ("fsWrites", ResourcesLanguageManager.Instance.Shared_Writes.CurrentValue()!)),
                ]),
            ],
            string.Format(ResourcesLanguageManager.Instance.ResourceMetricsCatalog_NoPrometheusWorkload.CurrentValue()!, workloadName));
    }

    /// <summary>Builds a Prometheus label matcher for a Kubernetes label selector.</summary>
    /// <remarks>
    /// The <c>kube_pod_labels</c> metric exposes every Kubernetes label as <c>label_&lt;key&gt;</c>,
    /// so a Kubernetes selector of <c>app=myapp</c> becomes the Prometheus matcher
    /// <c>label_app="myapp"</c>. Only a selector that requires exact label values can be expressed
    /// this way; set-based expressions are reported as unsupported and the caller falls back to
    /// matching pod names.
    /// </remarks>
    /// <param name="selector">The Kubernetes label selector.</param>
    /// <returns>The label matcher, or an empty string when the selector cannot be expressed.</returns>
    internal static string BuildPrometheusLabelSelector(V1LabelSelector? selector)
    {
        if (selector?.MatchLabels is not { Count: > 0 } matchLabels)
        {
            return string.Empty;
        }

        if (selector.MatchExpressions is { Count: > 0 })
        {
            return string.Empty;
        }

        foreach (var pair in matchLabels)
        {
            if (!IsValidPrometheusLabelName(pair.Key))
            {
                return string.Empty;
            }
        }

        return string.Join(",", matchLabels.Select(static pair => $"label_{pair.Key}=\"{pair.Value}\""));
    }

    private static bool IsValidPrometheusLabelName(string name)
    {
        if (name.Length == 0 || (!char.IsAsciiLetter(name[0]) && name[0] != '_'))
        {
            return false;
        }

        foreach (var character in name.AsSpan(1))
        {
            if (!char.IsAsciiLetterOrDigit(character) && character != '_')
            {
                return false;
            }
        }

        return true;
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
