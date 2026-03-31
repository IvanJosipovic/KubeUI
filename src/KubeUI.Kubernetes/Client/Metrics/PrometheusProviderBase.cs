using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes;

public abstract class PrometheusProviderBase : IPrometheusProvider
{
    public abstract PrometheusProviderKind Kind { get; }

    public abstract string Name { get; }

    public abstract bool IsConfigurable { get; }

    public virtual string BuildQuery(MetricCategory category, string queryName, IReadOnlyDictionary<string, string> options)
    {
        if (category is MetricCategory.Pods or MetricCategory.WorkloadPods or MetricCategory.Namespace)
        {
            return BuildPodQuery(queryName, options);
        }

        return category switch
        {
            MetricCategory.Nodes => BuildNodeQuery(queryName, options),
            MetricCategory.Pvc => BuildPvcQuery(queryName, options),
            MetricCategory.Ingress => BuildIngressQuery(queryName, options),
            MetricCategory.Cluster => BuildClusterQuery(queryName, options),
            _ => throw new InvalidOperationException($"Unsupported metrics category '{category}'."),
        };
    }

    public abstract Task<ResolvedPrometheusEndpoint?> TryResolveServiceAsync(k8s.Kubernetes client, ClusterMetricsSettings settings, CancellationToken cancellationToken = default);

    protected virtual string BuildClusterQuery(string queryName, IReadOnlyDictionary<string, string> options)
    {
        var nodes = GetOption(options, "nodes", ".*");
        var mountpoints = GetOption(options, "mountpoints", "^/$");

        return queryName switch
        {
            "cpuUsage" => $$"""sum(rate(node_cpu_seconds_total{node=~"{{nodes}}",mode=~"user|system"}[{{GetRateAccuracy(options)}}]))""",
            "cpuCapacity" => $$"""sum(kube_node_status_capacity{node=~"{{nodes}}", resource="cpu"})""",
            "cpuAllocatableCapacity" => $$"""sum(kube_node_status_allocatable{node=~"{{nodes}}", resource="cpu"})""",
            "memoryUsage" => $$"""sum(node_memory_MemTotal_bytes{node=~"{{nodes}}"} - (node_memory_MemFree_bytes{node=~"{{nodes}}"} + node_memory_Buffers_bytes{node=~"{{nodes}}"} + node_memory_Cached_bytes{node=~"{{nodes}}"}) )""",
            "workloadMemoryUsage" => $$"""sum(container_memory_working_set_bytes{image!="",pod!="" ,node=~"{{nodes}}"})""",
            "memoryCapacity" => $$"""sum(kube_node_status_capacity{node=~"{{nodes}}", resource="memory"})""",
            "memoryAllocatableCapacity" => $$"""sum(kube_node_status_allocatable{node=~"{{nodes}}", resource="memory"})""",
            "podUsage" => $$"""sum(kube_pod_info{node=~"{{nodes}}"})""",
            "podCapacity" => $$"""sum(kube_node_status_capacity{node=~"{{nodes}}", resource="pods"})""",
            "podAllocatableCapacity" => $$"""sum(kube_node_status_allocatable{node=~"{{nodes}}", resource="pods"})""",
            "fsSize" => $$"""sum(node_filesystem_size_bytes{mountpoint=~"{{mountpoints}}"} * on (pod,namespace) group_left(node) kube_pod_info{node=~"{{nodes}}"})""",
            "fsUsage" => $$"""sum((node_filesystem_size_bytes{mountpoint=~"{{mountpoints}}"} - node_filesystem_avail_bytes{mountpoint=~"{{mountpoints}}"}) * on (pod,namespace) group_left(node) kube_pod_info{node=~"{{nodes}}"})""",
            _ => throw new InvalidOperationException($"Unsupported cluster query '{queryName}'."),
        };
    }

    protected virtual string BuildNodeQuery(string queryName, IReadOnlyDictionary<string, string> options)
    {
        var mountpoints = GetOption(options, "mountpoints", "^/$");

        return queryName switch
        {
            "cpuUsage" => $$"""sum(rate(node_cpu_seconds_total{mode=~"user|system"}[{{GetRateAccuracy(options)}}])) by (node)""",
            "cpuCapacity" => """sum(kube_node_status_capacity{resource="cpu"}) by (node)""",
            "cpuAllocatableCapacity" => """sum(kube_node_status_allocatable{resource="cpu"}) by (node)""",
            "memoryUsage" => """sum(node_memory_MemTotal_bytes - (node_memory_MemFree_bytes + node_memory_Buffers_bytes + node_memory_Cached_bytes)) by (node)""",
            "workloadMemoryUsage" => """sum(container_memory_working_set_bytes{image!="",pod!=""}) by (node)""",
            "memoryCapacity" => """sum(kube_node_status_capacity{resource="memory"}) by (node)""",
            "memoryAllocatableCapacity" => """sum(kube_node_status_allocatable{resource="memory"}) by (node)""",
            "podUsage" => """sum(kube_pod_info) by (node)""",
            "podCapacity" => """sum(kube_node_status_capacity{resource="pods"}) by (node)""",
            "podAllocatableCapacity" => """sum(kube_node_status_allocatable{resource="pods"}) by (node)""",
            "fsSize" => $$"""sum(node_filesystem_size_bytes{mountpoint=~"{{mountpoints}}"} * on (pod,namespace) group_left(node) kube_pod_info) by (node)""",
            "fsUsage" => $$"""sum((node_filesystem_size_bytes{mountpoint=~"{{mountpoints}}"} - node_filesystem_avail_bytes{mountpoint=~"{{mountpoints}}"}) * on (pod, namespace) group_left(node) kube_pod_info) by (node)""",
            _ => throw new InvalidOperationException($"Unsupported node query '{queryName}'."),
        };
    }

    protected virtual string BuildPodQuery(string queryName, IReadOnlyDictionary<string, string> options)
    {
        var pods = GetOption(options, "pods", ".*");
        var ns = GetRequiredOption(options, "namespace");
        var selector = GetOption(options, "selector", "pod, namespace");
        var rateAccuracy = GetRateAccuracy(options);
        var container = GetOptionalOption(options, "container");
        var containerUsageMatcher = string.IsNullOrWhiteSpace(container)
            ? "container!=\"\""
            : $"container=\"{container}\"";
        var containerResourceMatcher = string.IsNullOrWhiteSpace(container)
            ? string.Empty
            : $",container=\"{container}\"";

        return queryName switch
        {
            "cpuUsage" => $$"""sum(rate(container_cpu_usage_seconds_total{image!="",{{containerUsageMatcher}},pod=~"{{pods}}",namespace="{{ns}}"}[{{rateAccuracy}}])) by ({{selector}})""",
            "cpuRequests" => $$"""sum(kube_pod_container_resource_requests{pod=~"{{pods}}",resource="cpu",namespace="{{ns}}"{{containerResourceMatcher}}} ) by ({{selector}})""",
            "cpuLimits" => $$"""sum(kube_pod_container_resource_limits{pod=~"{{pods}}",resource="cpu",namespace="{{ns}}"{{containerResourceMatcher}}} ) by ({{selector}})""",
            "memoryUsage" => $$"""sum(container_memory_working_set_bytes{image!="",{{containerUsageMatcher}},pod=~"{{pods}}",namespace="{{ns}}"}) by ({{selector}})""",
            "memoryRequests" => $$"""sum(kube_pod_container_resource_requests{pod=~"{{pods}}",resource="memory",namespace="{{ns}}"{{containerResourceMatcher}}} ) by ({{selector}})""",
            "memoryLimits" => $$"""sum(kube_pod_container_resource_limits{pod=~"{{pods}}",resource="memory",namespace="{{ns}}"{{containerResourceMatcher}}} ) by ({{selector}})""",
            "fsUsage" => $$"""sum(container_fs_usage_bytes{image!="",{{containerUsageMatcher}},pod=~"{{pods}}",namespace="{{ns}}"}) by ({{selector}})""",
            "fsWrites" => $$"""sum(rate(container_fs_writes_bytes_total{image!="",{{containerUsageMatcher}},pod=~"{{pods}}",namespace="{{ns}}"}[{{rateAccuracy}}])) by ({{selector}})""",
            "fsReads" => $$"""sum(rate(container_fs_reads_bytes_total{image!="",{{containerUsageMatcher}},pod=~"{{pods}}",namespace="{{ns}}"}[{{rateAccuracy}}])) by ({{selector}})""",
            "networkReceive" => $$"""sum(rate(container_network_receive_bytes_total{pod=~"{{pods}}",namespace="{{ns}}"}[{{rateAccuracy}}])) by ({{selector}})""",
            "networkTransmit" => $$"""sum(rate(container_network_transmit_bytes_total{pod=~"{{pods}}",namespace="{{ns}}"}[{{rateAccuracy}}])) by ({{selector}})""",
            _ => throw new InvalidOperationException($"Unsupported pod query '{queryName}'."),
        };
    }

    protected virtual string BuildPvcQuery(string queryName, IReadOnlyDictionary<string, string> options)
    {
        var pvc = GetRequiredOption(options, "pvc");
        var ns = GetRequiredOption(options, "namespace");

        return queryName switch
        {
            "diskUsage" => $$"""sum(kubelet_volume_stats_used_bytes{persistentvolumeclaim="{{pvc}}", namespace="{{ns}}"}) by (persistentvolumeclaim, namespace)""",
            "diskCapacity" => $$"""sum(kubelet_volume_stats_capacity_bytes{persistentvolumeclaim="{{pvc}}", namespace="{{ns}}"}) by (persistentvolumeclaim, namespace)""",
            _ => throw new InvalidOperationException($"Unsupported pvc query '{queryName}'."),
        };
    }

    protected virtual string BuildIngressQuery(string queryName, IReadOnlyDictionary<string, string> options)
    {
        var ingress = GetRequiredOption(options, "ingress");
        var ns = GetRequiredOption(options, "namespace");
        var rateAccuracy = GetRateAccuracy(options);

        return queryName switch
        {
            "bytesSentSuccess" => BytesSent(rateAccuracy, ingress, ns, "^2\\\\d*"),
            "bytesSentFailure" => BytesSent(rateAccuracy, ingress, ns, "^5\\\\d*"),
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

    private static string BytesSent(string rateAccuracy, string ingress, string ns, string statuses)
    {
        return $$"""sum(rate(nginx_ingress_controller_bytes_sent_sum{ingress="{{ingress}}",namespace="{{ns}}",status=~"{{statuses}}"}[{{rateAccuracy}}])) by (ingress, namespace)""";
    }
}
