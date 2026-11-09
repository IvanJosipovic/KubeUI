using k8s.Models;
using KubeUI.App.Features.Clusters.Overview;
using Shouldly;

namespace KubeUI.App.Tests.App.Features.Clusters.Overview;

public sealed class ClusterMetricSnapshotBuilderTests
{
    [Fact]
    public void empty_inputs_produce_zero_totals()
    {
        var snapshot = ClusterMetricSnapshotBuilder.Build([], [], []);

        snapshot.ShouldBe(new ClusterMetricSnapshot(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void sums_pod_resources_node_capacity_and_reported_usage()
    {
        V1Pod pod = new()
        {
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string, ResourceQuantity>
                            {
                                ["cpu"] = new("500m"),
                                ["memory"] = new("1Gi"),
                            },
                            Limits = new Dictionary<string, ResourceQuantity>
                            {
                                ["cpu"] = new("2"),
                                ["memory"] = new("2Gi"),
                            },
                        },
                    },
                    new V1Container
                    {
                        Name = "sidecar",
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string, ResourceQuantity>
                            {
                                ["cpu"] = new("250m"),
                                ["memory"] = new("512Mi"),
                            },
                        },
                    },
                ],
            },
        };
        V1Node node = new()
        {
            Status = new V1NodeStatus
            {
                Capacity = new Dictionary<string, ResourceQuantity>
                {
                    ["pods"] = new("110"),
                    ["cpu"] = new("8"),
                    ["memory"] = new("16Gi"),
                },
                Allocatable = new Dictionary<string, ResourceQuantity>
                {
                    ["cpu"] = new("7500m"),
                    ["memory"] = new("15Gi"),
                },
            },
        };
        PodMetrics metrics = new()
        {
            Containers =
            [
                new ContainerMetrics
                {
                    Name = "app",
                    Usage = new Dictionary<string, ResourceQuantity>
                    {
                        ["cpu"] = new("100m"),
                        ["memory"] = new("256Mi"),
                    },
                },
            ],
        };

        var snapshot = ClusterMetricSnapshotBuilder.Build([pod], [node], [metrics]);

        snapshot.TotalPods.ShouldBe(1);
        snapshot.MaxPods.ShouldBe(110);
        snapshot.CpuCapacity.ShouldBe(8);
        snapshot.CpuAllocatable.ShouldBe(7.5);
        snapshot.CpuRequests.ShouldBe(0.75);
        snapshot.CpuLimits.ShouldBe(2);
        snapshot.CpuUsage.ShouldBe(0.1);
        snapshot.MemoryCapacityGi.ShouldBe(16);
        snapshot.MemoryAllocatableGi.ShouldBe(15);
        snapshot.MemoryRequestsGi.ShouldBe(1.5);
        snapshot.MemoryLimitsGi.ShouldBe(2);
        snapshot.MemoryUsageGi.ShouldBe(0.25);
    }

    [Fact]
    public void missing_resources_status_and_containers_are_treated_as_zero()
    {
        V1Pod pod = new() { Spec = new V1PodSpec { Containers = [] } };
        V1Pod podWithoutSpec = new();
        V1Node node = new() { Status = new V1NodeStatus() };
        PodMetrics metricsWithoutContainers = new();

        var snapshot = ClusterMetricSnapshotBuilder.Build(
            [pod, podWithoutSpec],
            [node],
            [metricsWithoutContainers]);

        snapshot.TotalPods.ShouldBe(2);
        snapshot.MaxPods.ShouldBe(0);
        snapshot.CpuCapacity.ShouldBe(0);
        snapshot.CpuAllocatable.ShouldBe(0);
        snapshot.CpuRequests.ShouldBe(0);
        snapshot.CpuLimits.ShouldBe(0);
        snapshot.CpuUsage.ShouldBe(0);
        snapshot.MemoryCapacityGi.ShouldBe(0);
        snapshot.MemoryAllocatableGi.ShouldBe(0);
        snapshot.MemoryRequestsGi.ShouldBe(0);
        snapshot.MemoryLimitsGi.ShouldBe(0);
        snapshot.MemoryUsageGi.ShouldBe(0);
    }
}
