using System.Text.RegularExpressions;
using k8s.Models;
using KubeUI.Kubernetes;

namespace KubeUI.Documentation.Tests.Infra;

internal sealed class WalkthroughPrometheusQueryClient : IPrometheusQueryClient
{
    private readonly V1Pod[] _pods = KubeUIWalkthroughRecorder.CreateDemoResources().Resources
        .OfType<V1Pod>()
        .OrderBy(pod => pod.Metadata.Name, StringComparer.Ordinal)
        .ToArray();

    public Task PrepareAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task<PrometheusClientQueryRangeResponse?> QueryRangeAsync(
        Cluster cluster,
        ResolvedPrometheusEndpoint endpoint,
        string query,
        DateTimeOffset start,
        DateTimeOffset end,
        int stepSeconds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<PrometheusClientQueryRangeResponse?>(
            Query(query, start, end, stepSeconds, cancellationToken));
    }

    internal PrometheusClientQueryRangeResponse Query(
        string query,
        DateTimeOffset start,
        DateTimeOffset end,
        int stepSeconds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (stepSeconds <= 0 || start > end)
        {
            if (stepSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stepSeconds));
            }

            throw new ArgumentOutOfRangeException(nameof(start), "Prometheus range start must not follow its end.");
        }

        var metric = GetMetric(query);
        var selector = Regex.Match(query, "pod=~\"(?<pod>[^\"]+)\".*namespace=\"(?<namespace>[^\"]+)\"");
        if (!selector.Success)
        {
            throw new InvalidOperationException($"Missing Pod and namespace selectors in Prometheus query: {query}");
        }

        var podPattern = selector.Groups["pod"].Value.Replace(@"\\", @"\", StringComparison.Ordinal);
        var ns = selector.Groups["namespace"].Value;
        var results = new List<PrometheusClientQueryRangeResponse.ResultObject>();
        foreach (var pod in _pods)
        {
            if (pod.Metadata.NamespaceProperty != ns
                || !Regex.IsMatch(pod.Metadata.Name, $"\\A(?:{podPattern})\\z"))
            {
                continue;
            }

            var values = new List<(DateTimeOffset Timestamp, double Value)>();
            for (var time = start; time <= end; time = time.AddSeconds(stepSeconds))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var minute = (int)(time.ToUnixTimeSeconds() / 60);
                values.Add((time, SampleValue(metric, pod.Metadata.Name, minute)));
            }

            results.Add(new PrometheusClientQueryRangeResponse.ResultObject
            {
                Metric = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["pod"] = pod.Metadata.Name,
                    ["namespace"] = ns,
                },
                Values = values,
            });
        }

        return new PrometheusClientQueryRangeResponse
        {
            Status = "success",
            Data = new PrometheusClientQueryRangeResponse.DataObject
            {
                ResultType = "matrix",
                Result = results.ToArray(),
            },
        };
    }

    public Task ResetAsync() => Task.CompletedTask;

    internal static double SampleValue(string metric, string podName, int minute)
    {
        var podIndex = podName == WalkthroughDemoResources.FeaturedPodName
            ? 0
            : podName.Aggregate(0, static (total, letter) => total + letter) % 12 + 1;
        return metric switch
        {
            "cpuUsage" => 0.18 + podIndex * 0.007 + Math.Abs(minute % 11) * 0.004,
            "cpuRequests" => 0.35 + podIndex * 0.01,
            "cpuLimits" => 0.65 + podIndex * 0.02,
            "memoryUsage" => (96 + podIndex * 4 + Math.Abs(minute % 11) * 2) * 1024d * 1024d,
            "memoryRequests" => (160 + podIndex * 4) * 1024d * 1024d,
            "memoryLimits" => (320 + podIndex * 8) * 1024d * 1024d,
            "networkReceive" => (1.2 + podIndex * 0.07 + Math.Abs(minute % 11) * 0.03) * 1024d * 1024d,
            "networkTransmit" => (0.8 + podIndex * 0.05 + Math.Abs(minute % 9) * 0.02) * 1024d * 1024d,
            "fsUsage" => (48 + podIndex * 3 + Math.Abs(minute % 11)) * 1024d * 1024d,
            "fsReads" => (14 + podIndex * 0.2 + Math.Abs(minute % 7) * 0.5) * 1024d * 1024d,
            "fsWrites" => (8 + podIndex * 0.2 + Math.Abs(minute % 8) * 0.4) * 1024d * 1024d,
            _ => throw new ArgumentOutOfRangeException(nameof(metric), metric, "Unsupported monitoring metric."),
        };
    }

    private static string GetMetric(string query)
    {
        if (query.Contains("container_cpu_usage_seconds_total", StringComparison.Ordinal))
        {
            return "cpuUsage";
        }

        if (query.Contains("container_memory_working_set_bytes", StringComparison.Ordinal))
        {
            return "memoryUsage";
        }

        if (query.Contains("container_network_receive_bytes_total", StringComparison.Ordinal))
        {
            return "networkReceive";
        }

        if (query.Contains("container_network_transmit_bytes_total", StringComparison.Ordinal))
        {
            return "networkTransmit";
        }

        if (query.Contains("container_fs_usage_bytes", StringComparison.Ordinal))
        {
            return "fsUsage";
        }

        if (query.Contains("container_fs_reads_bytes_total", StringComparison.Ordinal))
        {
            return "fsReads";
        }

        if (query.Contains("container_fs_writes_bytes_total", StringComparison.Ordinal))
        {
            return "fsWrites";
        }

        if (query.Contains("kube_pod_container_resource_requests", StringComparison.Ordinal))
        {
            return query.Contains("resource=\"cpu\"", StringComparison.Ordinal)
                ? "cpuRequests"
                : query.Contains("resource=\"memory\"", StringComparison.Ordinal)
                    ? "memoryRequests"
                    : throw new InvalidOperationException($"Unsupported resource in Prometheus query: {query}");
        }

        if (query.Contains("kube_pod_container_resource_limits", StringComparison.Ordinal))
        {
            return query.Contains("resource=\"cpu\"", StringComparison.Ordinal)
                ? "cpuLimits"
                : query.Contains("resource=\"memory\"", StringComparison.Ordinal)
                    ? "memoryLimits"
                    : throw new InvalidOperationException($"Unsupported resource in Prometheus query: {query}");
        }

        throw new InvalidOperationException($"Unsupported Prometheus query in monitoring walkthrough: {query}");
    }
}
