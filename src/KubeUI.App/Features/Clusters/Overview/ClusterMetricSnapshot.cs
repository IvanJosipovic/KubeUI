using k8s.Models;

namespace KubeUI.App.Features.Clusters.Overview;

/// <summary>Immutable cluster-wide CPU, memory, and pod totals shown by the overview gauges.</summary>
/// <param name="TotalPods">Number of pods currently known to the cluster runtime.</param>
/// <param name="MaxPods">Sum of node pod capacity.</param>
/// <param name="CpuCapacity">Sum of node CPU capacity in cores.</param>
/// <param name="CpuAllocatable">Sum of node allocatable CPU in cores.</param>
/// <param name="CpuRequests">Sum of pod container CPU requests in cores.</param>
/// <param name="CpuLimits">Sum of pod container CPU limits in cores.</param>
/// <param name="CpuUsage">Sum of reported pod container CPU usage in cores.</param>
/// <param name="MemoryCapacityGi">Sum of node memory capacity in gibibytes.</param>
/// <param name="MemoryAllocatableGi">Sum of node allocatable memory in gibibytes.</param>
/// <param name="MemoryRequestsGi">Sum of pod container memory requests in gibibytes.</param>
/// <param name="MemoryLimitsGi">Sum of pod container memory limits in gibibytes.</param>
/// <param name="MemoryUsageGi">Sum of reported pod container memory usage in gibibytes.</param>
public sealed record ClusterMetricSnapshot(
    int TotalPods,
    double MaxPods,
    double CpuCapacity,
    double CpuAllocatable,
    double CpuRequests,
    double CpuLimits,
    double CpuUsage,
    double MemoryCapacityGi,
    double MemoryAllocatableGi,
    double MemoryRequestsGi,
    double MemoryLimitsGi,
    double MemoryUsageGi);

/// <summary>Calculates overview gauge values from pods, nodes, and pod metrics.</summary>
public static class ClusterMetricSnapshotBuilder
{
    private const double BytesPerMebibyte = 1048576d;
    private const double MebibytesPerGibibyte = 1024d;

    /// <summary>Builds an immutable snapshot of cluster resource totals.</summary>
    /// <param name="pods">Pods whose container requests and limits are summed.</param>
    /// <param name="nodes">Nodes whose capacity and allocatable values are summed.</param>
    /// <param name="podMetrics">Reported pod metrics; empty when metrics are unavailable.</param>
    /// <returns>The computed snapshot.</returns>
    public static ClusterMetricSnapshot Build(
        IReadOnlyList<V1Pod> pods,
        IReadOnlyList<V1Node> nodes,
        IReadOnlyList<PodMetrics> podMetrics)
    {
        ArgumentNullException.ThrowIfNull(pods);
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(podMetrics);

        var cpuRequests = 0d;
        var cpuLimits = 0d;
        var memoryRequests = 0d;
        var memoryLimits = 0d;
        for (var podIndex = 0; podIndex < pods.Count; podIndex++)
        {
            var containers = pods[podIndex].Spec?.Containers;
            if (containers is null)
            {
                continue;
            }

            for (var containerIndex = 0; containerIndex < containers.Count; containerIndex++)
            {
                var resources = containers[containerIndex].Resources;
                if (resources is null)
                {
                    continue;
                }

                if (resources.Requests?.TryGetValue("cpu", out var cpuRequest) == true)
                {
                    cpuRequests += cpuRequest.ToDouble();
                }

                if (resources.Limits?.TryGetValue("cpu", out var cpuLimit) == true)
                {
                    cpuLimits += cpuLimit.ToDouble();
                }

                if (resources.Requests?.TryGetValue("memory", out var memoryRequest) == true)
                {
                    memoryRequests += memoryRequest.ToDouble();
                }

                if (resources.Limits?.TryGetValue("memory", out var memoryLimit) == true)
                {
                    memoryLimits += memoryLimit.ToDouble();
                }
            }
        }

        var cpuUsage = 0d;
        var memoryUsage = 0d;
        for (var metricsIndex = 0; metricsIndex < podMetrics.Count; metricsIndex++)
        {
            var containers = podMetrics[metricsIndex].Containers;
            if (containers is null)
            {
                continue;
            }

            for (var containerIndex = 0; containerIndex < containers.Count; containerIndex++)
            {
                var usage = containers[containerIndex].Usage;
                if (usage?.TryGetValue("cpu", out var cpu) == true)
                {
                    cpuUsage += cpu.ToDouble();
                }

                if (usage?.TryGetValue("memory", out var memory) == true)
                {
                    memoryUsage += memory.ToDouble();
                }
            }
        }

        var maxPods = 0d;
        var cpuCapacity = 0d;
        var cpuAllocatable = 0d;
        var memoryCapacity = 0d;
        var memoryAllocatable = 0d;
        for (var nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
        {
            var status = nodes[nodeIndex].Status;
            if (status?.Capacity is { } capacity)
            {
                if (capacity.TryGetValue("pods", out var podsValue))
                {
                    maxPods += podsValue.ToDouble();
                }

                if (capacity.TryGetValue("cpu", out var cpuValue))
                {
                    cpuCapacity += cpuValue.ToDouble();
                }

                if (capacity.TryGetValue("memory", out var memoryValue))
                {
                    memoryCapacity += memoryValue.ToDouble();
                }
            }

            if (status?.Allocatable is { } allocatable)
            {
                if (allocatable.TryGetValue("cpu", out var cpuValue))
                {
                    cpuAllocatable += cpuValue.ToDouble();
                }

                if (allocatable.TryGetValue("memory", out var memoryValue))
                {
                    memoryAllocatable += memoryValue.ToDouble();
                }
            }
        }

        return new ClusterMetricSnapshot(
            pods.Count,
            maxPods,
            cpuCapacity,
            cpuAllocatable,
            cpuRequests,
            cpuLimits,
            cpuUsage,
            ToGibibytes(memoryCapacity),
            ToGibibytes(memoryAllocatable),
            ToGibibytes(memoryRequests),
            ToGibibytes(memoryLimits),
            ToGibibytes(memoryUsage));
    }

    private static double ToGibibytes(double bytes)
    {
        return bytes / BytesPerMebibyte / MebibytesPerGibibyte;
    }
}
