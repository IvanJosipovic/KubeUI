using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Metrics;

public sealed class ResourceMetricsCatalogTests
{
    [Fact]
    public async Task CreateAsync_pod_descriptor_builds_metric_panels_for_pod_name()
    {
        var pod = new V1Pod { Metadata = new V1ObjectMeta { Name = "sample.pod", NamespaceProperty = "default" } };

        var descriptor = await ResourceMetricsCatalog.CreateAsync(null!, pod);

        descriptor.ShouldNotBeNull();
        descriptor.Tabs.Count.ShouldBe(4);
        var cpuRequest = descriptor.Tabs[0].Panels.ShouldHaveSingleItem().Request;
        cpuRequest.Category.ShouldBe(MetricCategory.Pods);
        cpuRequest.Queries.Select(query => query.Name).ShouldBe(["cpuUsage", "cpuRequests", "cpuLimits"]);
        cpuRequest.Queries[0].Options["pods"].ShouldBe(@"sample\.pod");
    }

    [Fact]
    public async Task CreateAsync_node_descriptor_filters_series_by_node_or_instance_address()
    {
        var node = new V1Node
        {
            Metadata = new V1ObjectMeta { Name = "node-a" },
            Status = new V1NodeStatus
            {
                Addresses = [new V1NodeAddress { Type = "InternalIP", Address = "10.0.0.1" }],
            },
        };

        var descriptor = await ResourceMetricsCatalog.CreateAsync(null!, node);

        descriptor.ShouldNotBeNull();
        descriptor.Tabs.Count.ShouldBe(3);
        var filter = descriptor.Tabs[0].Panels.Single().Filter;
        filter(new MetricSeries { Name = "cpuUsage", Labels = new Dictionary<string, string> { ["node"] = "node-a" } }).ShouldBeTrue();
        filter(new MetricSeries { Name = "cpuUsage", Labels = new Dictionary<string, string> { ["instance"] = "10.0.0.1:9100" } }).ShouldBeTrue();
        filter(new MetricSeries { Name = "cpuUsage", Labels = new Dictionary<string, string> { ["node"] = "other" } }).ShouldBeFalse();
    }

    [Fact]
    public async Task CreateAsync_namespace_pvc_and_ingress_build_expected_tab_counts()
    {
        var namespaceDescriptor = await ResourceMetricsCatalog.CreateAsync(null!, new V1Namespace { Metadata = new V1ObjectMeta { Name = "default" } });
        var pvcDescriptor = await ResourceMetricsCatalog.CreateAsync(null!, new V1PersistentVolumeClaim { Metadata = new V1ObjectMeta { Name = "data", NamespaceProperty = "default" } });
        var ingressDescriptor = await ResourceMetricsCatalog.CreateAsync(null!, new V1Ingress { Metadata = new V1ObjectMeta { Name = "web", NamespaceProperty = "default" } });

        namespaceDescriptor?.Tabs.Count.ShouldBe(2);
        pvcDescriptor?.Tabs.Count.ShouldBe(1);
        ingressDescriptor?.Tabs.Count.ShouldBe(2);
        pvcDescriptor!.Tabs[0].Panels.Single().Request.Category.ShouldBe(MetricCategory.Pvc);
    }

    [Fact]
    public async Task CreateAsync_workload_without_selector_returns_empty_state_without_querying_cluster()
    {
        var deployment = new V1Deployment { Metadata = new V1ObjectMeta { NamespaceProperty = "default" } };

        var descriptor = await ResourceMetricsCatalog.CreateAsync(null!, deployment);

        descriptor.ShouldNotBeNull();
        descriptor.Tabs.ShouldBeEmpty();
        descriptor.EmptyState.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateAsync_unsupported_resource_returns_null()
    {
        var descriptor = await ResourceMetricsCatalog.CreateAsync(null!, new object());

        descriptor.ShouldBeNull();
    }
}
