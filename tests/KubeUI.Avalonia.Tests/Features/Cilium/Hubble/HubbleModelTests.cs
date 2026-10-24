using KubeUI.Cilium.Hubble;
using KubeUI.Cilium.Hubble.Models;
using KubeUI.Avalonia.Features.Cilium.Hubble;
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
    public void Service_map_respects_requested_time_window()
    {
        var builder = new ServiceMapGraphBuilder();
        builder.AddFlow(Flow("old", timestamp: DateTime.UtcNow.AddMinutes(-10)));

        builder.Build(TimeSpan.FromMinutes(15)).EdgeCount.ShouldBe(1);
        builder.Build(TimeSpan.FromMinutes(5)).EdgeCount.ShouldBe(0);
    }

    [Fact]
    public void Service_map_error_filter_includes_dropped_and_error_flows()
    {
        var builder = new ServiceMapGraphBuilder();
        builder.AddFlow(Flow("drop", FlowVerdict.Dropped));
        builder.AddFlow(Flow("error", FlowVerdict.Error));
        builder.AddFlow(Flow("forward", FlowVerdict.Forwarded));

        var graph = builder.Build(TimeSpan.FromMinutes(5), showOnlyErrors: true);

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

        builder.Build(TimeSpan.FromMinutes(5)).NodeCount.ShouldBe(3);
    }

    [Fact]
    public void Service_map_bounds_retained_flow_events()
    {
        var builder = new ServiceMapGraphBuilder(maxRetainedFlows: 2);
        builder.AddFlow(Flow("first"));
        builder.AddFlow(Flow("second"));
        builder.AddFlow(Flow("third"));

        builder.Build(TimeSpan.FromMinutes(5)).Edges.Single().FlowCount.ShouldBe(2);
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
            SourceNamespace = "team-a",
            SourcePod = "client",
            DestinationNamespace = "team-b",
            DestinationPod = "server",
            Protocol = "TCP",
            DestinationPort = 8080
        };
}
