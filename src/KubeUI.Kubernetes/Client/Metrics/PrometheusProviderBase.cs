using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes;

public abstract class PrometheusProviderBase : IPrometheusProvider
{
    public abstract PrometheusProviderKind Kind { get; }

    public abstract string Name { get; }

    public abstract bool IsConfigurable { get; }

    public virtual string BuildQuery(MetricCategory category, string queryName, IReadOnlyDictionary<string, string> options) =>
        category switch
        {
            MetricCategory.Pods or MetricCategory.WorkloadPods or MetricCategory.Namespace => BuildPodQuery(queryName, options),
            MetricCategory.Nodes => BuildNodeQuery(queryName, options),
            MetricCategory.Pvc => BuildPvcQuery(queryName, options),
            MetricCategory.Ingress => BuildIngressQuery(queryName, options),
            _ => throw new InvalidOperationException($"Unsupported metrics category '{category}'."),
        };

    public abstract Task<ResolvedPrometheusEndpoint?> TryResolveServiceAsync(k8s.Kubernetes client, ClusterMetricsSettings settings, CancellationToken cancellationToken = default);

    protected virtual string BuildNodeQuery(string queryName, IReadOnlyDictionary<string, string> options)
    {
        var instance = GetOption(options, "instance", ".*");
        var node = GetOption(options, "node", instance);
        var mountpoints = GetOption(options, "mountpoints", "^/$");
        var instanceSelector = EscapePrometheusString(instance);
        var nodeSelector = EscapePrometheusString(node);
        var mountpointSelector = EscapePrometheusString(mountpoints);

        return queryName switch
        {
            "cpuUsage" => $$"""sum(rate(node_cpu_seconds_total{instance=~"({{instanceSelector}})(:[0-9]+)?",mode=~"user|system"}[{{GetRateAccuracy(options)}}])) by (instance)""",
            "cpuCapacity" => $$"""sum(kube_node_status_capacity{node=~"{{nodeSelector}}",resource="cpu"}) by (node)""",
            "cpuAllocatableCapacity" => $$"""sum(kube_node_status_allocatable{node=~"{{nodeSelector}}",resource="cpu"}) by (node)""",
            "memoryUsage" => $$"""sum((node_memory_MemTotal_bytes{instance=~"{{instanceSelector}}"} - (node_memory_MemFree_bytes{instance=~"{{instanceSelector}}"} + node_memory_Buffers_bytes{instance=~"{{instanceSelector}}"} + node_memory_Cached_bytes{instance=~"{{instanceSelector}}"}))) by (instance)""",
            "workloadMemoryUsage" => """sum(container_memory_working_set_bytes{image!="",pod!=""}) by (node)""",
            "memoryCapacity" => $$"""sum(kube_node_status_capacity{node=~"{{nodeSelector}}",resource="memory"}) by (node)""",
            "memoryAllocatableCapacity" => $$"""sum(kube_node_status_allocatable{node=~"{{nodeSelector}}",resource="memory"}) by (node)""",
            "podUsage" => """sum(kube_pod_info) by (node)""",
            "podCapacity" => """sum(kube_node_status_capacity{resource="pods"}) by (node)""",
            "podAllocatableCapacity" => """sum(kube_node_status_allocatable{resource="pods"}) by (node)""",
            "fsSize" => $$"""sum(node_filesystem_size_bytes{mountpoint=~"{{mountpointSelector}}"} * on (pod,namespace) group_left(node) kube_pod_info) by (node)""",
            "fsUsage" => $$"""sum((node_filesystem_size_bytes{mountpoint=~"{{mountpointSelector}}"} - node_filesystem_avail_bytes{mountpoint=~"{{mountpointSelector}}"}) * on (pod, namespace) group_left(node) kube_pod_info) by (node)""",
            _ => throw new InvalidOperationException($"Unsupported node query '{queryName}'."),
        };
    }

    protected virtual string BuildPodQuery(string queryName, IReadOnlyDictionary<string, string> options)
    {
        var pods = GetOption(options, "pods", ".*");
        var ns = GetRequiredOption(options, "namespace");
        var selector = GetSeriesSelector(options);
        var rateAccuracy = GetRateAccuracy(options);
        var container = GetOptionalOption(options, "container");
        var labelSelector = GetOptionalOption(options, "labelSelector");
        var escapedPods = EscapePrometheusString(pods);
        var escapedNamespace = EscapePrometheusString(ns);
        var escapedContainer = container is null ? null : EscapePrometheusString(container);
        var containerUsageMatcher = string.IsNullOrWhiteSpace(container)
            ? "container!=\"\""
            : $"container=\"{escapedContainer}\"";
        var containerResourceMatcher = string.IsNullOrWhiteSpace(container)
            ? string.Empty
            : $",container=\"{escapedContainer}\"";

        // When the workload is identified by a label selector, the query matches on the label through
        // kube_pod_labels instead of on a regex alternation of every matching pod name. Enumerating the
        // pod names produces a very large regex that Prometheus has to evaluate against every series
        // in the cluster, which is expensive for large deployments.
        if (!string.IsNullOrWhiteSpace(labelSelector))
        {
            // The label selector is already expressed as Prometheus label matchers by the caller, so it
            // is inserted as-is; only the label names and values it carries are validated there.
            var labelMatch = $"namespace=\"{escapedNamespace}\",{labelSelector}";
            return BuildPodQueryByLabel(queryName, selector, rateAccuracy, containerUsageMatcher, containerResourceMatcher, labelMatch);
        }

        return queryName switch
        {
            "cpuUsage" => $$"""sum(rate(container_cpu_usage_seconds_total{image!="",{{containerUsageMatcher}},pod=~"{{escapedPods}}",namespace="{{escapedNamespace}}"}[{{rateAccuracy}}])) by ({{selector}})""",
            "cpuRequests" => $$"""sum(kube_pod_container_resource_requests{pod=~"{{escapedPods}}",resource="cpu",namespace="{{escapedNamespace}}"{{containerResourceMatcher}}} ) by ({{selector}})""",
            "cpuLimits" => $$"""sum(kube_pod_container_resource_limits{pod=~"{{escapedPods}}",resource="cpu",namespace="{{escapedNamespace}}"{{containerResourceMatcher}}} ) by ({{selector}})""",
            "memoryUsage" => $$"""sum(container_memory_working_set_bytes{image!="",{{containerUsageMatcher}},pod=~"{{escapedPods}}",namespace="{{escapedNamespace}}"}) by ({{selector}})""",
            "memoryRequests" => $$"""sum(kube_pod_container_resource_requests{pod=~"{{escapedPods}}",resource="memory",namespace="{{escapedNamespace}}"{{containerResourceMatcher}}} ) by ({{selector}})""",
            "memoryLimits" => $$"""sum(kube_pod_container_resource_limits{pod=~"{{escapedPods}}",resource="memory",namespace="{{escapedNamespace}}"{{containerResourceMatcher}}} ) by ({{selector}})""",
            "fsUsage" => $$"""sum(container_fs_usage_bytes{image!="",{{containerUsageMatcher}},pod=~"{{escapedPods}}",namespace="{{escapedNamespace}}"}) by ({{selector}})""",
            "fsWrites" => $$"""sum(rate(container_fs_writes_bytes_total{image!="",{{containerUsageMatcher}},pod=~"{{escapedPods}}",namespace="{{escapedNamespace}}"}[{{rateAccuracy}}])) by ({{selector}})""",
            "fsReads" => $$"""sum(rate(container_fs_reads_bytes_total{image!="",{{containerUsageMatcher}},pod=~"{{escapedPods}}",namespace="{{escapedNamespace}}"}[{{rateAccuracy}}])) by ({{selector}})""",
            "networkReceive" => $$"""sum(rate(container_network_receive_bytes_total{pod=~"{{escapedPods}}",namespace="{{escapedNamespace}}"}[{{rateAccuracy}}])) by ({{selector}})""",
            "networkTransmit" => $$"""sum(rate(container_network_transmit_bytes_total{pod=~"{{escapedPods}}",namespace="{{escapedNamespace}}"}[{{rateAccuracy}}])) by ({{selector}})""",
            _ => throw new InvalidOperationException($"Unsupported pod query '{queryName}'."),
        };
    }

    /// <summary>Builds a pod query that resolves the workload through <c>kube_pod_labels</c>.</summary>
    /// <param name="queryName">The metric to query.</param>
    /// <param name="selector">The grouping labels for the result series.</param>
    /// <param name="rateAccuracy">The rate window used by the query.</param>
    /// <param name="containerUsageMatcher">The container matcher applied to usage metrics.</param>
    /// <param name="containerResourceMatcher">The container matcher applied to resource metrics.</param>
    /// <param name="labelMatch">The label matcher joining the container series to the pod labels.</param>
    /// <returns>The PromQL query.</returns>
    protected virtual string BuildPodQueryByLabel(
        string queryName,
        string selector,
        string rateAccuracy,
        string containerUsageMatcher,
        string containerResourceMatcher,
        string labelMatch)
    {
        var workloadLabels = $"kube_pod_labels{{{labelMatch}}}";
        var join = $"* on (namespace, pod) group_left() {workloadLabels}";

        return queryName switch
        {
            "cpuUsage" => $$"""sum(rate(container_cpu_usage_seconds_total{image!="",{{containerUsageMatcher}}}[{{rateAccuracy}}]) {{join}}) by ({{selector}})""",
            "cpuRequests" => $$"""sum(kube_pod_container_resource_requests{resource="cpu"{{containerResourceMatcher}}} {{join}}) by ({{selector}})""",
            "cpuLimits" => $$"""sum(kube_pod_container_resource_limits{resource="cpu"{{containerResourceMatcher}}} {{join}}) by ({{selector}})""",
            "memoryUsage" => $$"""sum(container_memory_working_set_bytes{image!="",{{containerUsageMatcher}}} {{join}}) by ({{selector}})""",
            "memoryRequests" => $$"""sum(kube_pod_container_resource_requests{resource="memory"{{containerResourceMatcher}}} {{join}}) by ({{selector}})""",
            "memoryLimits" => $$"""sum(kube_pod_container_resource_limits{resource="memory"{{containerResourceMatcher}}} {{join}}) by ({{selector}})""",
            "fsUsage" => $$"""sum(container_fs_usage_bytes{image!="",{{containerUsageMatcher}}} {{join}}) by ({{selector}})""",
            "fsWrites" => $$"""sum(rate(container_fs_writes_bytes_total{image!="",{{containerUsageMatcher}}}[{{rateAccuracy}}]) {{join}}) by ({{selector}})""",
            "fsReads" => $$"""sum(rate(container_fs_reads_bytes_total{image!="",{{containerUsageMatcher}}}[{{rateAccuracy}}]) {{join}}) by ({{selector}})""",
            "networkReceive" => $$"""sum(rate(container_network_receive_bytes_total{}[{{rateAccuracy}}]) {{join}}) by ({{selector}})""",
            "networkTransmit" => $$"""sum(rate(container_network_transmit_bytes_total{}[{{rateAccuracy}}]) {{join}}) by ({{selector}})""",
            _ => throw new InvalidOperationException($"Unsupported pod query '{queryName}'."),
        };
    }

    protected virtual string BuildPvcQuery(string queryName, IReadOnlyDictionary<string, string> options)
    {
        var pvc = EscapePrometheusString(GetRequiredOption(options, "pvc"));
        var ns = EscapePrometheusString(GetRequiredOption(options, "namespace"));

        return queryName switch
        {
            "diskUsage" => $$"""sum(kubelet_volume_stats_used_bytes{persistentvolumeclaim="{{pvc}}", namespace="{{ns}}"}) by (persistentvolumeclaim, namespace)""",
            "diskCapacity" => $$"""sum(kubelet_volume_stats_capacity_bytes{persistentvolumeclaim="{{pvc}}", namespace="{{ns}}"}) by (persistentvolumeclaim, namespace)""",
            _ => throw new InvalidOperationException($"Unsupported pvc query '{queryName}'."),
        };
    }

    protected virtual string BuildIngressQuery(string queryName, IReadOnlyDictionary<string, string> options)
    {
        var ingress = EscapePrometheusString(GetRequiredOption(options, "ingress"));
        var ns = EscapePrometheusString(GetRequiredOption(options, "namespace"));
        var rateAccuracy = GetRateAccuracy(options);

        return queryName switch
        {
            "bytesSentSuccess" => BytesSent(rateAccuracy, ingress, ns, "^2[0-9]*"),
            "bytesSentFailure" => BytesSent(rateAccuracy, ingress, ns, "^5[0-9]*"),
            "requestDurationSeconds" => $$"""sum(rate(nginx_ingress_controller_request_duration_seconds_sum{ingress="{{ingress}}", namespace="{{ns}}"}[{{rateAccuracy}}])) by (ingress, namespace)""",
            "responseDurationSeconds" => $$"""sum(rate(nginx_ingress_controller_response_duration_seconds_sum{ingress="{{ingress}}", namespace="{{ns}}"}[{{rateAccuracy}}])) by (ingress, namespace)""",
            _ => throw new InvalidOperationException($"Unsupported ingress query '{queryName}'."),
        };
    }

    protected static async Task<V1Service?> FindFirstServiceForAllNamespacesAsync(k8s.Kubernetes client, string labelSelector, CancellationToken cancellationToken)
    {
        var services = await client.CoreV1.ListServiceForAllNamespacesAsync(labelSelector: labelSelector, cancellationToken: cancellationToken).ConfigureAwait(false);
        return services.Items.FirstOrDefault();
    }

    protected static async Task<V1Service?> FindNamespacedServiceAsync(k8s.Kubernetes client, string serviceName, string ns, CancellationToken cancellationToken)
    {
        try
        {
            return await client.CoreV1.ReadNamespacedServiceAsync(serviceName, ns, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    protected static ResolvedPrometheusEndpoint CreateServiceEndpoint(PrometheusProviderKind providerKind, string providerName, bool isConfigurable, V1Service service, ClusterMetricsSettings settings)
    {
        var port = service.Spec?.Ports?.FirstOrDefault(static x => string.Equals(x.Name, "http-web", StringComparison.Ordinal))
            ?? service.Spec?.Ports?.FirstOrDefault();

        if (service.Metadata?.NamespaceProperty == null || service.Metadata.Name == null || port?.Port == null)
        {
            throw new InvalidOperationException($"Prometheus service for provider '{providerName}' is missing required metadata.");
        }

        return new ResolvedPrometheusEndpoint(
            providerKind,
            providerName,
            isConfigurable,
            service.Metadata.NamespaceProperty,
            service.Metadata.Name,
            port.Port,
            settings.PrometheusDirectUrl,
            settings.PrometheusUseHttps,
            NormalizePrefix(settings.PrometheusPathPrefix),
            settings.PrometheusBearerToken);
    }

    protected static string GetRateAccuracy(IReadOnlyDictionary<string, string> options) => GetOption(options, "rateAccuracy", "1m");

    /// <summary>
    /// Reads the <c>selector</c> option and validates that it only contains Prometheus label names.
    /// </summary>
    /// <remarks>
    /// The selector is spliced verbatim into the <c>by (...)</c> clause of the generated PromQL, so it must
    /// never be able to carry arbitrary PromQL. Only <c>[a-zA-Z_][a-zA-Z0-9_]*</c> label names separated by
    /// commas are accepted.
    /// </remarks>
    /// <param name="options">The metric query options.</param>
    /// <returns>The validated comma-separated label names.</returns>
    /// <exception cref="InvalidOperationException">The selector contains an invalid label name.</exception>
    protected static string GetSeriesSelector(IReadOnlyDictionary<string, string> options)
    {
        var selector = GetOption(options, "selector", "pod, namespace");
        var names = selector.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var name in names)
        {
            if (!IsValidPrometheusLabelName(name))
            {
                throw new InvalidOperationException($"Metrics option 'selector' contains an invalid label name '{name}'.");
            }
        }

        return names.Length == 0 ? "pod, namespace" : string.Join(", ", names);
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

    protected static string GetRequiredOption(IReadOnlyDictionary<string, string> options, string key)
    {
        if (options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        throw new InvalidOperationException($"Metrics option '{key}' is required.");
    }

    protected static string GetOption(IReadOnlyDictionary<string, string> options, string key, string defaultValue)
    {
        return options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
    }

    protected static string? GetOptionalOption(IReadOnlyDictionary<string, string> options, string key)
    {
        return options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;
    }

    protected static string NormalizePrefix(string? prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return string.Empty;
        }

        return prefix.StartsWith('/') ? prefix.TrimEnd('/') : "/" + prefix.TrimEnd('/');
    }

    private static string EscapePrometheusString(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("\"", "\\\"", StringComparison.Ordinal)
        .Replace("\n", "\\n", StringComparison.Ordinal)
        .Replace("\r", "\\r", StringComparison.Ordinal);

    private static string BytesSent(string rateAccuracy, string ingress, string ns, string statuses)
    {
        return $$"""sum(rate(nginx_ingress_controller_bytes_sent_sum{ingress="{{ingress}}",namespace="{{ns}}",status=~"{{statuses}}"}[{{rateAccuracy}}])) by (ingress, namespace)""";
    }
}
