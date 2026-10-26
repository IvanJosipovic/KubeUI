using KubeUI.Avalonia.Features.Cilium.Hubble;
using KubeUI.Cilium.Hubble;
using KubeUI.Cilium.Hubble.Models;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Cilium.Hubble;

public sealed class HubbleModelTests
{
    [Fact]
    public void Cache_key_keeps_distinct_hubble_events_for_the_same_connection()
    {
        var first = Flow("event-1");
        var second = Flow("event-2");

        FlowCacheKey.From(first).ShouldNotBe(FlowCacheKey.From(second));
        FlowCacheKey.From(first).ShouldBe(FlowCacheKey.From(Flow("event-1")));
    }

    [Fact]
    public void Errors_only_filter_includes_error_and_dropped_verdicts()
    {
        var filter = new FlowFilterViewModel { ShowOnlyErrors = true };

        filter.Matches(Flow("dropped", FlowVerdict.Dropped)).ShouldBeTrue();
        filter.Matches(Flow("error", FlowVerdict.Error)).ShouldBeTrue();
        filter.Matches(Flow("forwarded", FlowVerdict.Forwarded)).ShouldBeFalse();
    }

    [Fact]
    public void Flow_mapper_reads_layer_seven_latency()
    {
        var flow = new Flow
        {
            L7 = new Layer7 { LatencyNs = 1_250_000 }
        };

        new FlowViewModel(flow).Latency.ShouldBe(TimeSpan.FromMilliseconds(1.25));
    }

    [Fact]
    public void Flow_mapper_reads_source_and_destination_services()
    {
        var flow = new Flow
        {
            SourceService = new Service { Name = "frontend", Namespace = "team-a" },
            DestinationService = new Service { Name = "backend", Namespace = "team-b" }
        };

        var viewModel = new FlowViewModel(flow);

        viewModel.SourceServiceName.ShouldBe("frontend");
        viewModel.SourceServiceNamespace.ShouldBe("team-a");
        viewModel.DestinationServiceName.ShouldBe("backend");
        viewModel.DestinationServiceNamespace.ShouldBe("team-b");
    }

    [Fact]
    public void Service_map_respects_requested_time_window()
    {
        var builder = new ServiceMapGraphBuilder();
        builder.AddFlow(Flow("old", timestamp: DateTime.UtcNow.AddMinutes(-10)));

        builder.Build(TimeSpan.FromMinutes(15), namespaces: ["team-a", "team-b"]).EdgeCount.ShouldBe(1);
        builder.Build(TimeSpan.FromMinutes(5), namespaces: ["team-a", "team-b"]).EdgeCount.ShouldBe(0);
    }

    [Fact]
    public void Service_map_requires_a_selected_namespace()
    {
        var builder = new ServiceMapGraphBuilder();
        builder.AddFlow(Flow("scoped"));

        builder.Build(TimeSpan.FromMinutes(5)).NodeCount.ShouldBe(0);
        builder.GetFlows(TimeSpan.FromMinutes(5)).ShouldBeEmpty();
    }

    [Fact]
    public void Service_map_node_details_use_the_same_service_aggregation_as_the_graph()
    {
        var builder = new ServiceMapGraphBuilder();
        var flow = Flow("service-flow");
        flow.SourceWorkload = "client";
        flow.SourceWorkloadKind = "Deployment";
        builder.AddFlow(flow);
        var node = builder.Build(TimeSpan.FromMinutes(5), namespaces: ["team-a"]).Nodes.Single(candidate => candidate.Name == "client");

        builder.GetFlowsForNode(
                node,
                TimeSpan.FromMinutes(5),
                aggregateByNamespace: false,
                showOnlyErrors: false,
                namespaces: ["team-a"])
            .ShouldContain(flow);
    }

    [Fact]
    public void Service_map_error_filter_includes_dropped_and_error_flows()
    {
        var builder = new ServiceMapGraphBuilder();
        builder.AddFlow(Flow("drop", FlowVerdict.Dropped));
        builder.AddFlow(Flow("error", FlowVerdict.Error));
        builder.AddFlow(Flow("forward", FlowVerdict.Forwarded));

        var graph = builder.Build(TimeSpan.FromMinutes(5), showOnlyErrors: true, namespaces: ["team-a", "team-b"]);

        graph.EdgeCount.ShouldBe(1);
        graph.Edges[0].FlowCount.ShouldBe(2);
        graph.Edges[0].DeniedCount.ShouldBe(2);
    }

    [Fact]
    public void Service_map_separates_external_endpoints_by_ip()
    {
        var builder = new ServiceMapGraphBuilder();
        var first = Flow("external-a");
        first.DestinationNamespace = string.Empty;
        first.DestinationPod = string.Empty;
        first.DestinationIp = "203.0.113.10";
        var second = Flow("external-b");
        second.DestinationNamespace = string.Empty;
        second.DestinationPod = string.Empty;
        second.DestinationIp = "203.0.113.11";
        builder.AddFlow(first);
        builder.AddFlow(second);

        builder.Build(TimeSpan.FromMinutes(5), namespaces: ["team-a", "team-b"]).NodeCount.ShouldBe(3);
    }

    [Fact]
    public void Service_map_aggregates_workload_replicas_and_tracks_destination_access_points()
    {
        var builder = new ServiceMapGraphBuilder();
        var first = Flow("first");
        first.SourceWorkload = "client";
        first.SourceWorkloadKind = "Deployment";
        first.SourcePort = 53000;
        first.DestinationWorkload = "api";
        first.DestinationWorkloadKind = "Deployment";

        var second = Flow("second");
        second.SourcePod = "client-2";
        second.SourceWorkload = "client";
        second.SourceWorkloadKind = "Deployment";
        second.DestinationPod = "api-2";
        second.DestinationWorkload = "api";
        second.DestinationWorkloadKind = "Deployment";
        second.DestinationPort = 8443;
        second.SourcePort = 33412;

        var third = Flow("third");
        third.SourcePod = "client-3";
        third.SourceWorkload = "client";
        third.SourceWorkloadKind = "Deployment";
        third.DestinationPod = "api-3";
        third.DestinationWorkload = "api";
        third.DestinationWorkloadKind = "Deployment";
        third.Protocol = "UDP";
        third.SourcePort = 53840;

        builder.AddFlow(first);
        builder.AddFlow(second);
        builder.AddFlow(third);

        var graph = builder.Build(TimeSpan.FromMinutes(5), namespaces: ["team-a", "team-b"]);

        graph.NodeCount.ShouldBe(2);
        graph.EdgeCount.ShouldBe(1);
        var edge = graph.Edges.Single();
        edge.FlowCount.ShouldBe(3);
        edge.AccessPoints.Select(accessPoint => (accessPoint.Protocol, accessPoint.Port)).ShouldBe(
            [("TCP", 8080), ("TCP", 8443), ("UDP", 8080)]);
        graph.Nodes.Select(node => node.Name).Order(StringComparer.Ordinal).ShouldBe(["api", "client"]);
        var api = graph.Nodes.Single(node => node.Name == "api");
        api.AccessPoints.Select(accessPoint => (accessPoint.Protocol, accessPoint.Port)).ShouldBe(
            [("TCP", 8080), ("TCP", 8443), ("UDP", 8080)]);
        api.AccessPoints.Select(accessPoint => accessPoint.FlowCount).ShouldBe([1, 1, 1]);
        graph.Nodes.Single(node => node.Name == "client").AccessPoints.ShouldBeEmpty();
        api.AccessPoints.Select(accessPoint => accessPoint.Port)
            .ShouldNotContain(53000);
        api.AccessPoints.Select(accessPoint => accessPoint.Port).ShouldNotContain(33412);
        api.AccessPoints.Select(accessPoint => accessPoint.Port).ShouldNotContain(53840);
    }

    [Fact]
    public void Flow_mapper_keeps_endpoint_dns_names_and_world_identity_for_service_map()
    {
        var flow = new Flow
        {
            Source = new Endpoint { Labels = { "k8s:io.kubernetes.pod.namespace=team-a" } },
            Destination = new Endpoint { Labels = { "reserved:world" } },
            DestinationNames = { "api.example.com" }
        };

        var viewModel = new FlowViewModel(flow);
        viewModel.DestinationIsWorld.ShouldBeTrue();
        viewModel.DestinationNames.ShouldBe(["api.example.com"]);
    }

    [Fact]
    public void Service_map_access_points_use_mapped_destination_port_not_source_port()
    {
        var flow = new Flow
        {
            Time = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
            TrafficDirection = TrafficDirection.Ingress,
            Source = new Endpoint { Namespace = "team-a", PodName = "client" },
            Destination = new Endpoint { Namespace = "team-b", PodName = "dns" },
            L4 = new Layer4
            {
                TCP = new TCP { SourcePort = 53000, DestinationPort = 53 }
            }
        };
        var mappedFlow = new FlowViewModel(flow);
        mappedFlow.SourcePort.ShouldBe(53000);
        mappedFlow.DestinationPort.ShouldBe(53);

        var builder = new ServiceMapGraphBuilder();
        builder.AddFlow(mappedFlow);
        var graph = builder.Build(TimeSpan.FromMinutes(5), namespaces: ["team-a", "team-b"]);

        graph.Nodes.Single(node => node.Name == "dns").AccessPoints
            .Select(accessPoint => accessPoint.Port)
            .ShouldBe([53]);
        graph.Nodes.Single(node => node.Name == "client").AccessPoints.ShouldBeEmpty();
    }

    [Fact]
    public void Service_map_uses_external_dns_name_and_target_access_point()
    {
        var flow = Flow("external-dns");
        flow.DestinationIsWorld = true;
        flow.DestinationNames = ["api.example.com"];
        flow.DestinationPort = 443;
        flow.ApplicationProtocol = "HTTP";
        var builder = new ServiceMapGraphBuilder();
        builder.AddFlow(flow);

        var graph = builder.Build(TimeSpan.FromMinutes(5), namespaces: ["team-a"]);

        var external = graph.Nodes.Single(node => node.Name == "api.example.com");
        external.AccessPoints.Select(accessPoint => (accessPoint.Protocol, accessPoint.Port))
            .ShouldBe([("TCP", 443)]);
        external.AccessPoints.Single().FlowCount.ShouldBe(1);
        external.AccessPoints.Single().ApplicationProtocols.ShouldBe(["HTTP"]);
    }

    [Fact]
    public void Service_map_node_access_points_use_destination_ports_and_ignore_replies()
    {
        var builder = new ServiceMapGraphBuilder();
        var egress = Flow("egress");
        egress.Direction = FlowDirection.Egress;
        egress.DestinationPort = 43123;
        var ingress = Flow("ingress");
        ingress.Direction = FlowDirection.Ingress;
        ingress.DestinationPort = 8080;
        var reply = Flow("reply");
        reply.SourceNamespace = "team-b";
        reply.SourcePod = "server";
        reply.DestinationNamespace = "team-a";
        reply.DestinationPod = "client";
        reply.Direction = FlowDirection.Ingress;
        reply.IsReply = true;
        reply.DestinationPort = 49152;
        builder.AddFlow(egress);
        builder.AddFlow(ingress);
        builder.AddFlow(reply);

        var graph = builder.Build(TimeSpan.FromMinutes(5), namespaces: ["team-a", "team-b"]);

        graph.Nodes.Single(node => node.Name == "server").AccessPoints
            .Select(accessPoint => accessPoint.Port)
            .ShouldBe([8080, 43123]);
        graph.Nodes.Single(node => node.Name == "client").AccessPoints.ShouldBeEmpty();
        graph.Edges.Count.ShouldBe(1);
        graph.Edges.Single().FlowCount.ShouldBe(2);
        builder.GetFlowsForNode(
                graph.Nodes.Single(node => node.Name == "server"),
                TimeSpan.FromMinutes(5),
                aggregateByNamespace: false,
                showOnlyErrors: false,
                namespaces: ["team-a", "team-b"])
            .ShouldNotContain(reply);
    }

    [Fact]
    public void Service_map_prefers_service_nodes_when_service_metadata_is_available()
    {
        var builder = new ServiceMapGraphBuilder();
        var flow = Flow("service-flow");
        flow.SourceServiceName = "frontend";
        flow.SourceServiceNamespace = "team-a";
        flow.DestinationServiceName = "backend";
        flow.DestinationServiceNamespace = "team-b";
        builder.AddFlow(flow);

        var graph = builder.Build(TimeSpan.FromMinutes(5), namespaces: ["team-a", "team-b"]);

        graph.Nodes.Select(node => node.Id).ShouldContain("service/team-a/frontend");
        graph.Nodes.Select(node => node.Id).ShouldContain("service/team-b/backend");
    }

    [Fact]
    public void Service_map_keeps_reverse_service_links_directional()
    {
        var builder = new ServiceMapGraphBuilder();
        var forward = Flow("forward");
        forward.SourceWorkload = "client";
        forward.SourceWorkloadKind = "Deployment";
        forward.DestinationWorkload = "api";
        forward.DestinationWorkloadKind = "Deployment";
        var reverse = Flow("reverse");
        reverse.SourceNamespace = "team-b";
        reverse.SourcePod = "api-1";
        reverse.SourceWorkload = "api";
        reverse.SourceWorkloadKind = "Deployment";
        reverse.DestinationNamespace = "team-a";
        reverse.DestinationPod = "client-1";
        reverse.DestinationWorkload = "client";
        reverse.DestinationWorkloadKind = "Deployment";

        builder.AddFlow(forward);
        builder.AddFlow(reverse);

        var graph = builder.Build(TimeSpan.FromMinutes(5), namespaces: ["team-a", "team-b"]);

        graph.NodeCount.ShouldBe(2);
        graph.EdgeCount.ShouldBe(2);
        graph.Edges.Select(edge => (edge.SourceId, edge.TargetId)).ShouldContain(("workload/team-a/Deployment/client", "workload/team-b/Deployment/api"));
        graph.Edges.Select(edge => (edge.SourceId, edge.TargetId)).ShouldContain(("workload/team-b/Deployment/api", "workload/team-a/Deployment/client"));
    }

    [Fact]
    public void Service_map_bounds_retained_flow_events()
    {
        var builder = new ServiceMapGraphBuilder(maxRetainedFlows: 2);
        builder.AddFlow(Flow("first"));
        builder.AddFlow(Flow("second"));
        builder.AddFlow(Flow("third"));

        builder.Build(TimeSpan.FromMinutes(5), namespaces: ["team-a", "team-b"]).Edges.Single().FlowCount.ShouldBe(2);
    }

    private static FlowViewModel Flow(
        string uuid,
        FlowVerdict verdict = FlowVerdict.Forwarded,
        DateTime? timestamp = null)
        => new()
        {
            Uuid = uuid,
            Timestamp = timestamp ?? DateTime.UtcNow,
            Verdict = verdict,
            Direction = FlowDirection.Ingress,
            SourceNamespace = "team-a",
            SourcePod = "client",
            DestinationNamespace = "team-b",
            DestinationPod = "server",
            Protocol = "TCP",
            DestinationPort = 8080
        };
}
